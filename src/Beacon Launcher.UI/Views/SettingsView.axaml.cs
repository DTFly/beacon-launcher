using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using BeaconLaunch.Theme;
using BeaconLaunch.ViewModels;

namespace BeaconLaunch.Views;

public partial class SettingsView : UserControl
{
    private MainViewModel? _viewModel;
    private bool _isSyncingTheme;
    private bool _isSyncingAccentEditor;

    public SettingsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        SyncThemeSelection();
        SyncAccentEditor();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeToViewModel();
        SyncThemeSelection();
        SyncAccentEditor();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeFromViewModel();
        base.OnDetachedFromVisualTree(e);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        SubscribeToViewModel();
        SyncThemeSelection();
        SyncAccentEditor();
    }

    private void SubscribeToViewModel()
    {
        var viewModel = DataContext as MainViewModel;
        if (ReferenceEquals(_viewModel, viewModel))
        {
            return;
        }

        UnsubscribeFromViewModel();
        _viewModel = viewModel;
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private void UnsubscribeFromViewModel()
    {
        if (_viewModel is null)
        {
            return;
        }

        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        _viewModel = null;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainViewModel.Theme) or nameof(MainViewModel.CurrentTheme))
        {
            SyncThemeSelection();
        }

        if (e.PropertyName is nameof(MainViewModel.Theme) or
            nameof(MainViewModel.CurrentTheme) or
            nameof(MainViewModel.AccentColor) or
            nameof(MainViewModel.EffectiveAccentHex))
        {
            SyncAccentEditor();
        }
    }

    private void SettingsSection_Checked(object? sender, RoutedEventArgs e)
    {
        if (PersonalizationPanel is null || sender is not RadioButton { IsChecked: true })
        {
            return;
        }

        PersonalizationPanel.IsVisible = ReferenceEquals(sender, PersonalizationNav);
        GameSettingsPanel.IsVisible = ReferenceEquals(sender, GameSettingsNav);
        JavaPanel.IsVisible = ReferenceEquals(sender, JavaNav);
        OnlinePanel.IsVisible = ReferenceEquals(sender, OnlineNav);
    }

    private void LightTheme_Checked(object? sender, RoutedEventArgs e)
    {
        if (_isSyncingTheme ||
            sender is not RadioButton { IsChecked: true } ||
            DataContext is not MainViewModel viewModel)
        {
            return;
        }

        viewModel.Theme = "Light";
    }

    private void DarkTheme_Checked(object? sender, RoutedEventArgs e)
    {
        if (_isSyncingTheme ||
            sender is not RadioButton { IsChecked: true } ||
            DataContext is not MainViewModel viewModel)
        {
            return;
        }

        viewModel.Theme = "Dark";
    }

    private void SyncThemeSelection()
    {
        if (DataContext is not MainViewModel viewModel ||
            LightThemeOption is null ||
            DarkThemeOption is null)
        {
            return;
        }

        _isSyncingTheme = true;
        try
        {
            var isLight = viewModel.Theme != "Dark";
            LightThemeOption.IsChecked = isLight;
            DarkThemeOption.IsChecked = !isLight;
        }
        finally
        {
            _isSyncingTheme = false;
        }
    }

    private void AccentPicker_ColorChanged(object? sender, Color color)
    {
        if (_isSyncingAccentEditor)
        {
            return;
        }

        UpdateAccentDraft(color);
    }

    private void AccentHexInput_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isSyncingAccentEditor || AccentHexInput is null)
        {
            return;
        }

        AccentHexInput.Classes.Set("invalid", false);

        var text = AccentHexInput.Text?.Trim();
        if (!ColorHex.TryParse(text, out var color) ||
            AccentPicker is null ||
            AccentHexPreview is null ||
            AccentPreviewSwatch is null)
        {
            return;
        }

        _isSyncingAccentEditor = true;
        try
        {
            AccentPicker.Color = color;
            AccentHexPreview.Text = ColorHex.ToHex(color);
            AccentPreviewSwatch.Background = new SolidColorBrush(color);
        }
        finally
        {
            _isSyncingAccentEditor = false;
        }
    }

    private void AccentHexInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        ApplyAccent();
        e.Handled = true;
    }

    private void ApplyAccent_Click(object? sender, RoutedEventArgs e) => ApplyAccent();

    private void FollowTheme_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        viewModel.AccentColor = string.Empty;
        SyncAccentEditor();
    }

    private void ApplyAccent()
    {
        if (DataContext is not MainViewModel viewModel || AccentHexInput is null)
        {
            return;
        }

        var value = AccentHexInput.Text?.Trim() ?? string.Empty;
        if (!ColorHex.TryParse(value, out var color))
        {
            AccentHexInput.Classes.Set("invalid", true);
            return;
        }

        AccentHexInput.Classes.Set("invalid", false);
        viewModel.AccentColor = ColorHex.ToHex(color);
        SyncAccentEditor();
    }

    private void UpdateAccentDraft(Color color)
    {
        if (AccentHexInput is null ||
            AccentHexPreview is null ||
            AccentPreviewSwatch is null)
        {
            return;
        }

        _isSyncingAccentEditor = true;
        try
        {
            AccentHexInput.Text = ColorHex.ToHex(color);
            AccentHexInput.Classes.Set("invalid", false);
            AccentHexPreview.Text = ColorHex.ToHex(color);
            AccentPreviewSwatch.Background = new SolidColorBrush(color);
        }
        finally
        {
            _isSyncingAccentEditor = false;
        }
    }

    private void SyncAccentEditor()
    {
        if (DataContext is not MainViewModel viewModel ||
            AccentPicker is null ||
            AccentHexInput is null ||
            AccentHexPreview is null ||
            AccentPreviewSwatch is null)
        {
            return;
        }

        var color = ColorHex.TryParse(viewModel.EffectiveAccentHex, out var parsed)
            ? parsed
            : Colors.RoyalBlue;

        _isSyncingAccentEditor = true;
        try
        {
            AccentPicker.Color = color;
            AccentHexInput.Text = ColorHex.ToHex(color);
            AccentHexInput.Classes.Set("invalid", false);
            AccentHexPreview.Text = ColorHex.ToHex(color);
            AccentPreviewSwatch.Background = new SolidColorBrush(color);
        }
        finally
        {
            _isSyncingAccentEditor = false;
        }
    }
}
