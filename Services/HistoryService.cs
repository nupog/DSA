using System.Text.Json;
using Microsoft.Data.Sqlite;
using DeepSeekSurveyAnalyzer.Models;
using DeepSeekSurveyAnalyzer.Services.Abstractions;
using System.IO;

namespace DeepSeekSurveyAnalyzer.Services;

public class HistoryService : IHistoryService
{
    private readonly string _connectionString;

    public HistoryService()
    {
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                    "DeepSeekSurveyAnalyzer", "history.db");
        var dbDir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dbDir))
        {
            Directory.CreateDirectory(dbDir);
        }
        _connectionString = $"Data Source={dbPath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS History (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Timestamp TEXT NOT NULL,
                Prompt TEXT NOT NULL,
                Reasoning TEXT,
                Answer TEXT,
                Model TEXT,
                Files TEXT
            )";
        command.ExecuteNonQuery();
    }

    public async Task SaveEntryAsync(HistoryEntry entry)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO History (Timestamp, Prompt, Reasoning, Answer, Model, Files)
                VALUES ($timestamp, $prompt, $reasoning, $answer, $model, $files)";
            command.Parameters.AddWithValue("$timestamp", entry.Timestamp.ToString("o"));
            command.Parameters.AddWithValue("$prompt", entry.Prompt);
            command.Parameters.AddWithValue("$reasoning", entry.Reasoning ?? "");
            command.Parameters.AddWithValue("$answer", entry.Answer ?? "");
            command.Parameters.AddWithValue("$model", entry.Model ?? "");
            command.Parameters.AddWithValue("$files", JsonSerializer.Serialize(entry.Files));
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            LoggingService.LogError(ex, "Ошибка сохранения истории");
        }
    }

    public async Task<List<HistoryEntry>> LoadEntriesAsync(int limit = 50)
    {
        var entries = new List<HistoryEntry>();
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM History ORDER BY Timestamp DESC LIMIT $limit";
            command.Parameters.AddWithValue("$limit", limit);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                entries.Add(new HistoryEntry
                {
                    Id = reader.GetInt32(0),
                    Timestamp = DateTime.Parse(reader.GetString(1)),
                    Prompt = reader.GetString(2),
                    Reasoning = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Answer = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Model = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Files = reader.IsDBNull(6) || string.IsNullOrWhiteSpace(reader.GetString(6))
                        ? new List<string>()
                        : (JsonSerializer.Deserialize<List<string>>(reader.GetString(6)) ?? new List<string>())
                });
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError(ex, "Ошибка загрузки истории");
        }
        return entries;
    }
}