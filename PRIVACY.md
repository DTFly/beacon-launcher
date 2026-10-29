# Privacy Policy

Last updated: 2026-10-29

Beacon Launcher is an open-source desktop launcher for Minecraft: Java Edition. This policy explains what data the launcher handles.

## Summary

- No telemetry, analytics, advertising, or tracking.
- No personal data is sent to the developer.
- Launcher settings are stored locally on the user's device.
- Microsoft account sign-in (when implemented) is handled directly between the user, Microsoft, Xbox Live, and Minecraft Services.

## Local data

The launcher stores its settings in `%APPDATA%\.bl\config.json`:

- launcher/title-bar name
- theme and accent color
- window width and height
- theme version used for config migration

This file does not contain account credentials.

## Microsoft account sign-in (planned)

The current build does not yet include Microsoft sign-in. When it is added, the launcher is designed to use the official Microsoft identity platform (`consumers` tenant) with the delegated scopes `XboxLive.signin offline_access`, and then the official Xbox Live/XSTS and Minecraft Services APIs.

Authentication is performed directly with Microsoft. The developer does not receive the user's password. If the launcher stores an access or refresh token locally to keep the user signed in, that token stays on the user's device and can be removed by signing out or deleting the launcher's local data.

## Network requests

The current build does not send telemetry. Future versions may contact Microsoft, Xbox Live, and Minecraft Services only to sign in and launch the game. The launcher does not operate its own data-collection servers.

## Data sharing

The developer does not sell, rent, or share personal data. Data is only exchanged with the official services required to sign in and launch the game, as described above.

## User control

The launcher's local data can be removed at any time by deleting `%APPDATA%\.bl`. When sign-in is implemented, signing out will remove the locally stored session.

## Changes

If this policy changes, the updated version will be committed to the project repository with a new "last updated" date.

## Contact

Please open an issue in the project's GitHub repository.

## Disclaimer

Not an official Minecraft product. Not approved by or associated with Mojang Studios or Microsoft. "Minecraft" is a trademark of Mojang Synergies AB.
