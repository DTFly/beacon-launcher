using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using BeaconLaunch.Core.Configuration;
using BeaconLaunch.ViewModels;

namespace BeaconLaunch.Views;

public partial class MainWindow : Window
{
    private bool _windowSizeApplied;

    public MainWindow()
    {
        InitializeComponent();

        SettingsNavButton.PropertyChanged += SettingsNavButton_PropertyChanged;
        SettingsViewControl.IsVisible = SettingsNavButton.IsChecked == true;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        ApplySavedWindowSize();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (WindowState != WindowState.Normal ||
            DataContext is not MainViewModel viewModel)
        {
            return;
        }

        viewModel.SaveWindowSize(ClientSize.Width, ClientSize.Height);
    }

    private void ApplySavedWindowSize()
    {
        if (_windowSizeApplied ||
            DataContext is not MainViewModel viewModel ||
            viewModel.SavedWindowWidth < LauncherConfig.MinWindowWidth ||
            viewModel.SavedWindowHeight < LauncherConfig.MinWindowHeight)
        {
            return;
        }

        _windowSizeApplied = true;
        Width = viewModel.SavedWindowWidth;
        Height = viewModel.SavedWindowHeight;
    }

    private void SettingsNavButton_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ToggleButton.IsCheckedProperty)
        {
            SettingsViewControl.IsVisible = SettingsNavButton.IsChecked == true;
        }
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (WindowState == WindowState.Normal)
        {
            BeginMoveDrag(e);
        }
    }

    private void MinimizeButton_Click(object? sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void CloseButton_Click(object? sender, RoutedEventArgs e) => Close();
}
