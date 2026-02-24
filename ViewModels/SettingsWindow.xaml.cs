using System.Windows;

namespace DeepSeekSurveyAnalyzer.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        this.DataContextChanged += SettingsWindow_DataContextChanged;
    }

    private void SettingsWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is ViewModels.SettingsViewModel vm)
        {
            vm.Close = Close;
        }
    }
}