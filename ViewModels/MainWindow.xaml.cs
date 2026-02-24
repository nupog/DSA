using System.Windows;
using DeepSeekSurveyAnalyzer.ViewModels;

namespace DeepSeekSurveyAnalyzer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ListBox_DragEnter(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void ListBox_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (DataContext is MainViewModel vm)
            {
                foreach (var file in files.Where(f => f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ||
                                                       f.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) ||
                                                       f.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)))
                {
                    vm.SelectedFiles.Add(file);
                }
            }
        }
        e.Handled = true;
    }
}