using System.Windows;
using DeepSeekSurveyAnalyzer.ViewModels;

namespace DeepSeekSurveyAnalyzer.Views;

public partial class PromptSettingsWindow : Window
{
    public PromptSettingsWindow(PromptSettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseAction = () => Close();
    }
}