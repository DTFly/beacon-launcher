# Beacon Launcher

Beacon Launcher is an open-source launcher for Minecraft: Java Edition, built with [Avalonia UI](https://avaloniaui.net/) 12 and .NET 10.

[中文说明](README.zh-CN.md)

> **Status:** early development. The window shell, settings, Light/Dark themes, custom accent picker, and config persistence are implemented. Microsoft account sign-in, Java management, game downloads, and multiplayer are planned.

<p align="center">
  <img src="docs/screenshot-light.png" width="49%" alt="Light theme settings" />
  <img src="docs/screenshot-dark.png" width="49%" alt="Dark theme and accent picker" />
</p>

## Features

- Frameless, resizable window with a custom title bar.
- Remembers the last window size (not position) and starts centered.
- Light and Dark themes built on semantic color tokens.
- Free-form HSV accent picker with hex input and a "follow theme" reset.
- Atomic local settings in `%APPDATA%\.bl\config.json`.
- Offscreen UI snapshot tool for development.
- No telemetry.

## Requirements

- Windows 10/11 x64 (current publish target)
- .NET 10 SDK
- NuGet access to restore Avalonia 12 packages

## Build and run

```powershell
dotnet build "Beacon Launcher.sln" -c Release
dotnet run --project "src/Beacon Launcher.UI/Beacon Launcher.UI.csproj"
```

Publish a self-contained single-file executable:

```powershell
dotnet publish "src/Beacon Launcher.UI/Beacon Launcher.UI.csproj" `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:PublishTrimmed=true -p:TrimMode=partial `
  -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true `
  -o artifacts/publish
```

## Project layout

```
src/Beacon Launcher.Core/   configuration and persistence
src/Beacon Launcher.UI/     Avalonia app, themes, controls, views
tools/UiSnapshot/           offscreen UI renderer
docs/                       screenshots
```

## Offscreen UI snapshots

`tools/UiSnapshot` renders the real UI with Avalonia's headless platform and Skia. It never creates a desktop window and uses a throwaway config directory, so it does not touch `%APPDATA%\.bl\config.json`.

```powershell
dotnet run --project tools/UiSnapshot -c Release -- docs/screenshot.png `
  --settings --section personalization --width 1280 --height 800

dotnet run --project tools/UiSnapshot -c Release -- docs/dark.png `
  --theme dark --settings --section personalization --scroll bottom
```

## Microsoft account sign-in

Microsoft sign-in is planned as the launcher's authentication path. It will use the official Microsoft identity platform (`consumers` tenant) with the delegated scopes `XboxLive.signin offline_access`, then the official Xbox Live/XSTS and Minecraft Services APIs.

The launcher is a public client: it does not use a client secret, uses a localhost redirect URI with PKCE, and does not bypass authentication, ownership checks, license checks, or any other safety feature. It does not distribute game files.

## Privacy

See [PRIVACY.md](PRIVACY.md). The current build stores settings locally and does not collect telemetry.

## License

MIT. See [LICENSE](LICENSE).

## Disclaimer

Not an official Minecraft product. Not approved by or associated with Mojang Studios or Microsoft. "Minecraft" is a trademark of Mojang Synergies AB.
