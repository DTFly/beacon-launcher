using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace BeaconLaunch.Controls;

/// <summary>
/// A saturation/value square with a hue strip. Dragging either area raises
/// <see cref="ColorChanged"/>; this is a real free-form picker, not a palette.
/// </summary>
public sealed class HsvColorPicker : Control
{
    private const double HueStripHeight = 14;
    private const double StripGap = 10;
    private const double BottomPadding = 4;

    public static readonly StyledProperty<Color> ColorProperty =
        AvaloniaProperty.Register<HsvColorPicker, Color>(
            nameof(Color),
            Color.FromRgb(0x25, 0x63, 0xEB));

    public static readonly StyledProperty<IBrush?> OutlineBrushProperty =
        AvaloniaProperty.Register<HsvColorPicker, IBrush?>(nameof(OutlineBrush));

    private double _hue;
    private double _saturation = 1;
    private double _value = 1;
    private bool _isApplyingHsv;
    private bool _draggingSpectrum;
    private bool _draggingHue;

    static HsvColorPicker()
    {
        AffectsRender<HsvColorPicker>(ColorProperty, OutlineBrushProperty);
    }

    public HsvColorPicker()
    {
        Cursor = new Cursor(StandardCursorType.Cross);
        Focusable = false;

        var hsv = Color.ToHsv();
        _hue = hsv.H;
        _saturation = hsv.S;
        _value = hsv.V;
    }

    public Color Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Border brush for the spectrum and hue strip.</summary>
    public IBrush? OutlineBrush
    {
        get => GetValue(OutlineBrushProperty);
        set => SetValue(OutlineBrushProperty, value);
    }

    public event EventHandler<Color>? ColorChanged;

    public override void Render(DrawingContext context)
    {
        if (!TryGetSpectrumRect(out var spectrumRect) || !TryGetHueRect(out var hueRect))
        {
            return;
        }

        var outline = new Pen(OutlineBrush ?? Brushes.Transparent, 1);

        var hueColor = new HsvColor(1, _hue, 1, 1).ToRgb();
        context.DrawRectangle(new SolidColorBrush(hueColor), null, spectrumRect, 8, 8);
        context.DrawRectangle(
            CreateLinearGradient(
                Colors.White,
                Color.FromArgb(0, 255, 255, 255),
                horizontal: true),
            null,
            spectrumRect,
            8,
            8);
        context.DrawRectangle(
            CreateLinearGradient(
                Color.FromArgb(0, 0, 0, 0),
                Colors.Black,
                horizontal: false),
            null,
            spectrumRect,
            8,
            8);
        context.DrawRectangle(null, outline, spectrumRect, 8, 8);

        context.DrawRectangle(CreateHueBrush(), null, hueRect, 7, 7);
        context.DrawRectangle(null, outline, hueRect, 7, 7);

        var thumbCenter = new Point(
            Math.Clamp(_saturation * spectrumRect.Width, 7, spectrumRect.Width - 7),
            Math.Clamp((1 - _value) * spectrumRect.Height, 7, spectrumRect.Height - 7));
        context.DrawEllipse(
            null,
            new Pen(new SolidColorBrush(Color.FromArgb(0x66, 0, 0, 0)), 4),
            thumbCenter,
            7,
            7);
        context.DrawEllipse(
            new SolidColorBrush(new HsvColor(1, _hue, _saturation, _value).ToRgb()),
            new Pen(Brushes.White, 2),
            thumbCenter,
            6,
            6);

        var hueThumbX = Math.Clamp(_hue / 360 * hueRect.Width, 8, hueRect.Width - 8);
        var hueThumb = new Rect(hueThumbX - 4, hueRect.Y - 3, 8, HueStripHeight + 6);
        context.DrawRectangle(
            Brushes.White,
            new Pen(new SolidColorBrush(Color.FromArgb(0x66, 0, 0, 0)), 1),
            hueThumb,
            4,
            4);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != ColorProperty)
        {
            return;
        }

        var color = change.GetNewValue<Color>();
        if (!_isApplyingHsv)
        {
            var hsv = color.ToHsv();
            _hue = hsv.H;
            _saturation = hsv.S;
            _value = hsv.V;
        }

        ColorChanged?.Invoke(this, color);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var point = e.GetPosition(this);
        if (TryGetHueRect(out var hueRect) && hueRect.Contains(point))
        {
            _draggingHue = true;
            _draggingSpectrum = false;
            UpdateFromHue(point.X);
        }
        else if (TryGetSpectrumRect(out var spectrumRect) && spectrumRect.Contains(point))
        {
            _draggingSpectrum = true;
            _draggingHue = false;
            UpdateFromSpectrum(point);
        }
        else
        {
            return;
        }

        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (e.Pointer.Captured != this)
        {
            return;
        }

        var point = e.GetPosition(this);
        if (_draggingSpectrum)
        {
            UpdateFromSpectrum(point);
            e.Handled = true;
        }
        else if (_draggingHue)
        {
            UpdateFromHue(point.X);
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        var wasDragging = _draggingSpectrum || _draggingHue;
        _draggingSpectrum = false;
        _draggingHue = false;
        if (e.Pointer.Captured == this)
        {
            e.Pointer.Capture(null);
        }

        if (wasDragging)
        {
            e.Handled = true;
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _draggingSpectrum = false;
        _draggingHue = false;
    }

    private void UpdateFromSpectrum(Point point)
    {
        if (!TryGetSpectrumRect(out var spectrumRect))
        {
            return;
        }

        _saturation = Math.Clamp(point.X / spectrumRect.Width, 0, 1);
        _value = Math.Clamp(1 - point.Y / spectrumRect.Height, 0, 1);
        ApplyHsv();
    }

    private void UpdateFromHue(double x)
    {
        var width = Bounds.Width;
        if (width <= 0)
        {
            return;
        }

        _hue = Math.Clamp(x / width, 0, 1) * 360;
        ApplyHsv();
    }

    private void ApplyHsv()
    {
        var color = new HsvColor(1, _hue, _saturation, _value).ToRgb();
        _isApplyingHsv = true;
        try
        {
            SetCurrentValue(ColorProperty, color);
        }
        finally
        {
            _isApplyingHsv = false;
        }

        InvalidateVisual();
    }

    private bool TryGetSpectrumRect(out Rect rect)
    {
        rect = new Rect(
            0,
            0,
            Bounds.Width,
            Bounds.Height - HueStripHeight - StripGap - BottomPadding);
        return rect.Width > 0 && rect.Height > 0;
    }

    private bool TryGetHueRect(out Rect rect)
    {
        rect = new Rect(
            0,
            Bounds.Height - HueStripHeight - BottomPadding,
            Bounds.Width,
            HueStripHeight);
        return rect.Width > 0 && rect.Height > 0;
    }

    private LinearGradientBrush CreateHueBrush()
    {
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
            GradientStops = new GradientStops
            {
                new GradientStop(Color.FromRgb(255, 0, 0), 0),
                new GradientStop(Color.FromRgb(255, 255, 0), 1d / 6),
                new GradientStop(Color.FromRgb(0, 255, 0), 2d / 6),
                new GradientStop(Color.FromRgb(0, 255, 255), 3d / 6),
                new GradientStop(Color.FromRgb(0, 0, 255), 4d / 6),
                new GradientStop(Color.FromRgb(255, 0, 255), 5d / 6),
                new GradientStop(Color.FromRgb(255, 0, 0), 1),
            },
        };
    }

    private static LinearGradientBrush CreateLinearGradient(Color start, Color end, bool horizontal)
    {
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = horizontal
                ? new RelativePoint(1, 0, RelativeUnit.Relative)
                : new RelativePoint(0, 1, RelativeUnit.Relative),
            GradientStops = new GradientStops
            {
                new GradientStop(start, 0),
                new GradientStop(end, 1),
            },
        };
    }
}
