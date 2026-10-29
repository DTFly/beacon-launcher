using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using BeaconLaunch;
using BeaconLaunch.Controls;
using BeaconLaunch.Core.Configuration;
using BeaconLaunch.ViewModels;
using BeaconLaunch.Views;

namespace UiSnapshot;

/// <summary>
/// Renders the launcher offscreen with Avalonia's headless platform, so UI checks
/// never create a window on the desktop. Uses a throwaway config directory.
/// </summary>
internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine(
                "Usage: UiSnapshot <output.png> [--settings] [--section personalization|game|java|online] " +
                "[--theme light|dark] [--accent #RRGGBB] [--scroll <px|bottom>] " +
                "[--picker-click x,y] [--hover-control <name>] [--hover-point x,y] [--width 960] [--height 700] [--config-dir <path>]");
            return 1;
        }

        var outputPath = Path.GetFullPath(args[0]);
        var openSettings = args.Contains("--settings", StringComparer.OrdinalIgnoreCase);
        var section = GetOption(args, "--section");
        var theme = GetOption(args, "--theme");
        var width = GetOption(args, "--width");
        var height = GetOption(args, "--height");
        var customAccent = GetOption(args, "--accent");
        var scroll = GetOption(args, "--scroll");
        var pickerClick = GetOption(args, "--picker-click");
        var hoverControl = GetOption(args, "--hover-control");
        var hoverPointOption = GetOption(args, "--hover-point");
        var configDirectoryOption = GetOption(args, "--config-dir");

        var configDirectory = configDirectoryOption ??
                              Path.Combine(
                                  Path.GetTempPath(),
                                  "beacon-ui-snapshot-" + Guid.NewGuid().ToString("N")[..8]);
        var ownsConfigDirectory = configDirectoryOption is null;

        try
        {
            AppBuilder.Configure<App>()
                .UseSkia()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions
                {
                    UseHeadlessDrawing = false,
                    OverlayPopups = true,
                })
                .SetupWithoutStarting();

            var viewModel = new MainViewModel(new ConfigService(configDirectory));

            if (string.Equals(theme, "dark", StringComparison.OrdinalIgnoreCase))
            {
                viewModel.Theme = "Dark";
            }
            else if (string.Equals(theme, "light", StringComparison.OrdinalIgnoreCase))
            {
                viewModel.Theme = "Light";
            }

            if (!string.IsNullOrWhiteSpace(customAccent))
            {
                viewModel.AccentColor = customAccent;
            }

            var window = new MainWindow { DataContext = viewModel };
            if (double.TryParse(width, out var previewWidth))
            {
                window.Width = previewWidth;
            }

            if (double.TryParse(height, out var previewHeight))
            {
                window.Height = previewHeight;
            }

            window.Show();

            if (openSettings || section is not null)
            {
                window.FindControl<RadioButton>("SettingsNavButton")!.IsChecked = true;
                Dispatcher.UIThread.RunJobs();
            }

            var settingsView = window.FindControl<SettingsView>("SettingsViewControl");
            var sectionName = section?.ToLowerInvariant() switch
            {
                "personalization" => "PersonalizationNav",
                "game" => "GameSettingsNav",
                "java" => "JavaNav",
                "online" => "OnlineNav",
                _ => null,
            };

            if (sectionName is not null &&
                settingsView?.FindControl<RadioButton>(sectionName) is { } sectionButton)
            {
                sectionButton.IsChecked = true;
                Dispatcher.UIThread.RunJobs();
            }

            Dispatcher.UIThread.RunJobs();

            if (scroll is not null && settingsView is not null)
            {
                var scrollViewer = settingsView.FindControl<ScrollViewer>("SettingsScroll");
                if (scrollViewer is not null)
                {
                    if (string.Equals(scroll, "bottom", StringComparison.OrdinalIgnoreCase))
                    {
                        scrollViewer.Offset = new Vector(0, scrollViewer.Extent.Height);
                    }
                    else if (double.TryParse(scroll, out var scrollOffset))
                    {
                        scrollViewer.Offset = new Vector(0, scrollOffset);
                    }

                    Dispatcher.UIThread.RunJobs();
                }
            }

            if (hoverPointOption is not null)
            {
                var parts = hoverPointOption.Split(',');
                if (parts.Length == 2 &&
                    double.TryParse(parts[0], out var hoverX) &&
                    double.TryParse(parts[1], out var hoverY))
                {
                    window.MouseMove(new Point(hoverX, hoverY), RawInputModifiers.None);
                    Dispatcher.UIThread.RunJobs();
                    Console.WriteLine($"HoverPoint={hoverX},{hoverY}");
                }
            }

            if (hoverControl is not null && settingsView is not null)
            {
                var target = settingsView.FindControl<Control>(hoverControl);
                if (target is not null &&
                    target.TranslatePoint(new Point(target.Bounds.Width - 14, target.Bounds.Height / 2), window) is { } hoverPoint)
                {
                    window.MouseMove(hoverPoint, RawInputModifiers.None);
                    Dispatcher.UIThread.RunJobs();
                    Console.WriteLine($"HoverControl={hoverControl} at {hoverPoint}");
                }
            }

            if (pickerClick is not null && settingsView is not null)
            {
                var numbers = pickerClick.Split(',');
                if (numbers.Length == 2 &&
                    double.TryParse(numbers[0], out var pickerX) &&
                    double.TryParse(numbers[1], out var pickerY) &&
                    settingsView.FindControl<HsvColorPicker>("AccentPicker") is { } picker &&
                    picker.TranslatePoint(new Point(pickerX, pickerY), window) is { } clickPoint)
                {
                    window.MouseDown(clickPoint, MouseButton.Left, RawInputModifiers.None);
                    window.MouseUp(clickPoint, MouseButton.Left, RawInputModifiers.None);
                    Dispatcher.UIThread.RunJobs();
                    var hex = settingsView.FindControl<TextBox>("AccentHexInput")?.Text;
                    Console.WriteLine($"PickerClick={pickerX},{pickerY} Hex={hex}");
                }
            }

            // Selection changes animate their background for ~120 ms; let them settle
            // so the capture does not catch a half-transitioned brush.
            Thread.Sleep(300);
            Dispatcher.UIThread.RunJobs();

            AvaloniaHeadlessPlatform.ForceRenderTimerTick(2);
            Dispatcher.UIThread.RunJobs();

            if (settingsView is not null)
            {
                foreach (var name in new[] { "PersonalizationNav", "GameSettingsNav", "JavaNav", "OnlineNav" })
                {
                    var radio = settingsView.FindControl<RadioButton>(name);
                    Console.WriteLine($"{name}: IsChecked={radio?.IsChecked}");
                }
            }

            Console.WriteLine($"Theme={viewModel.Theme} Accent={viewModel.EffectiveAccentHex}");

            var frame = window.CaptureRenderedFrame();
            if (frame is null)
            {
                Console.Error.WriteLine("Failed to capture a rendered frame.");
                return 2;
            }

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            frame.Save(outputPath, PngBitmapEncoderOptions.Default);
            Console.WriteLine($"Saved {outputPath} ({frame.PixelSize.Width}x{frame.PixelSize.Height})");

            window.Close();

            var savedConfig = new ConfigService(configDirectory).Load();
            Console.WriteLine(
                $"SavedWindow={savedConfig.WindowWidth}x{savedConfig.WindowHeight} " +
                $"Theme={savedConfig.Theme} Accent='{savedConfig.AccentColor}'");

            return 0;
        }
        finally
        {
            if (ownsConfigDirectory)
            {
                try
                {
                    if (Directory.Exists(configDirectory))
                    {
                        Directory.Delete(configDirectory, recursive: true);
                    }
                }
                catch
                {
                    // A leftover temp directory is harmless.
                }
            }
        }
    }

    private static string? GetOption(string[] args, string name)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }
}
