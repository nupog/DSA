using System.Windows;
using DeepSeekSurveyAnalyzer.ViewModels;

namespace DeepSeekSurveyAnalyzer.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.Close = Close;
    }
}