# DualPC-StreamingMic-Helper

English | [简体中文](README.md)

A lightweight Windows system tray helper for **capture-card-free dual-PC streaming**, designed to send the gaming PC's **selected microphone / audio input device** separately to the streaming PC through NDI® Free Audio.

> Current version: **v1.1.0**
>
> UI languages: **Simplified Chinese / English**
>
> Simplified Chinese is the default. Change languages from the tray menu.

## Why This Project Exists

NDI Screen Capture HX is a convenient way to send your gaming PC's screen and desktop playback audio to another computer over the local network.

However, if your microphone is connected as a separate input device, it usually needs to be sent as an independent audio source.

NDI Free Audio can do this, but it is primarily a command-line utility.

DualPC-StreamingMic-Helper provides a simple Windows tray interface for NDI Free Audio.

It can:

- Use the Windows default microphone or select a specific audio input device.
- Launch NDI Free Audio silently in the background.
- Show the current sending status in the system tray.
- Restart the microphone NDI output.
- Let the user manually select the NDI Free Audio executable.
- Remember the selected language, input device, and NDI Free Audio path.
- Automatically stop the Free Audio process started by this helper when the helper exits.

## Recommended Dual-PC Setup

```text
Gaming PC
├─ NDI Screen Capture HX
│  ├─ Game / Desktop Video
│  └─ Game / Discord / Music / Desktop Playback Audio
│
└─ DualPC-StreamingMic-Helper
   └─ Selected Microphone / Audio Input
             │
             │ Local Network
             ▼
Streaming PC
├─ NDI Video + Desktop Audio from Screen Capture HX
└─ Independent Microphone Audio from "NDI Microphone"
             │
             ▼
      OBS / Other NDI-Compatible Software
```

## v1.1.0 Features

- Runs in the Windows system tray
- Uses the Windows default microphone or a selected input device
- Simplified Chinese / English UI with saved language preference
- Restart immediately after changing devices, or save for the next restart
- NDI source name: `NDI Microphone`
- Manually select and save the path to `NDI FreeAudio.exe`
- No visible CMD or PowerShell window
- Restart the microphone NDI output from the tray menu
- Automatically stop the Free Audio process when exiting
- Prevent multiple instances of the helper from running at the same time

Language, input device, and Free Audio path are stored at:

```text
%APPDATA%\NDI-Mic\helper-settings.json
```

## Requirements

### Gaming PC

Download and install the required NDI software directly from the official NDI website:

- NDI Tools / Screen Capture HX  
  https://ndi.video/tools/

- NDI Free Audio  
  https://ndi.video/tools/free-audio/

**This repository does not include, modify, or redistribute any NDI software.**

### Streaming PC

You need software capable of receiving NDI sources.

For example, OBS Studio can receive NDI sources when used with a compatible NDI input plugin.

The exact setup depends on your streaming software and workflow.

## First-Time Setup

1. Download `DualPC-StreamingMic-Helper.exe` from [Releases](https://github.com/howard929/DualPC-StreamingMic-Helper/releases). Exit the previous helper before upgrading.
2. Launch the helper. If you plan to use Windows Default, set your intended microphone as the Windows default recording/input device.
3. Right-click the tray icon and choose **语言 / Language → English**.
4. If no Free Audio path is saved, choose **Set NDI Free Audio path...**.
5. Click **Browse...**, select your installed `NDI FreeAudio.exe`, and save.
6. Free Audio starts in the background with the source name:

```text
NDI Microphone
```

The selected Free Audio path will be remembered. If an NDI update changes the installation path, select the executable again from the tray menu. The helper does not scan installation folders.

## Language and Audio Devices

- **语言 / Language**: choose Simplified Chinese or English. Changes apply immediately and persist; this top-level menu always remains bilingual.
- **音频设备 / Audio Device**: choose Windows Default or one specific input. Names come from Free Audio itself; output-device management is not included.

Changing the device opens a restart prompt:

- **Restart** stops the current Free Audio process, saves the selection, and starts with the new input.
- **Cancel** still saves the selection, keeps the current session running, and applies the new input on the next restart.

Double-click the tray icon to see the current input, Free Audio path, and any pending device selection. Sending status reflects the Free Audio process state; it does not confirm audio reception on the other PC.

Missing or ambiguous device names require reselection. The helper does not silently substitute another microphone. Keep devices connected while switching, as hot-plugging may change the device list.

## Upgrading From v1.0.0

Exit the previous helper before launching v1.1.0. When the new settings file does not exist, the helper imports the path from `%APPDATA%\NDI-Mic\freeaudio_path.txt` and defaults to Simplified Chinese and Windows Default.

The old path file remains unchanged. New preferences are saved in `helper-settings.json`; returning to v1.0.0 uses the original path file.

## Screen Capture HX Suggested Setup

The exact interface and available settings may change between NDI versions.

For a typical dual-PC streaming setup, the following settings are a useful starting point:

- Frame Rate: `60 fps`
- Capture: your full gaming display
- Audio Source: the playback device you actually use for game / Discord / music audio
- Codec / Bandwidth: HEVC can be useful for reducing network bandwidth
- For a normal SDR streaming workflow, HEVC 10-bit is usually unnecessary

This gives you two separate NDI sources:

```text
Screen Capture HX
= Game Video + Desktop Playback Audio

NDI Microphone
= Selected Microphone / Audio Input
```

This keeps desktop audio and microphone audio separate on the streaming PC.

## Optional: Stream Deck

If you use a Stream Deck, you can create a Multi Action that launches:

1. NDI Screen Capture HX
2. `DualPC-StreamingMic-Helper.exe`

This allows you to start both NDI senders with a single button.

When you are not streaming, neither helper needs to be running.

## Building From Source

### Requirements

- Windows
- .NET 8 SDK

Official .NET 8 download:

https://dotnet.microsoft.com/download/dotnet/8.0

Open a terminal inside the `src` directory and run:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The compiled executable will normally be located at:

```text
src\bin\Release\net8.0-windows\win-x64\publish\
```

Alternatively, run `Build.cmd` in `src`. The published Windows x64 executable is self-contained and does not require a separate .NET Runtime installation.

After building, run `Verify.ps1` from the repository root using PowerShell 7 to check migration, persistence, parsing, and translations. Results are written to `verification-output/`, without changing personal settings.

## Windows Security Notice

Prebuilt releases of this project may not be signed with a commercial code-signing certificate.

Because of this, Windows SmartScreen or Smart App Control may display a warning or block the executable on some systems.

If you do not trust the prebuilt executable, you can inspect:

```text
src/
```

and build the application yourself from source.

**Disabling Windows security features globally just to run this project is not recommended.**

## Project Status

### v1.1.0

- Simplified Chinese / English UI
- Input-device selection with Windows Default preserved
- Device-change restart prompt and pending selection display
- Persistent language, input device, and Free Audio path
- Existing tray, single-instance, and background process management retained

Compilation, automated checks, and author functional acceptance are complete. See [v1.1.0 release notes](RELEASE_NOTES_v1.1.0.md).

### Future Direction

Continue improving usability without unnecessary complexity. Additional features will be discussed separately.

## License

This project is **source-available**.

It is **not** Open Source Software under the OSI definition.

In short:

- You may use the software free of charge.
- Monetized livestreaming and commercial content production are allowed.
- You may inspect, study, modify, and compile the source code.
- You may redistribute the original or modified software free of charge.
- Redistribution must retain the original author credit, license, and canonical GitHub repository link.
- Modified versions must clearly state that they are modified versions of this project.
- Selling the software, selling modified versions, paid redistribution, commercial repackaging, or integrating the software into paid products or services is not allowed without prior written permission from the copyright holder.

See the full license terms:

- [LICENSE](LICENSE)
- [中文许可证说明](LICENSE_CN.md)

## NDI® Trademark Notice

NDI® is a registered trademark of Vizrt NDI AB.

DualPC-StreamingMic-Helper is an independent community project and is not affiliated with, endorsed by, or sponsored by Vizrt NDI AB.

This project does not distribute NDI software.

Users should obtain NDI Tools and NDI Free Audio directly from the official NDI website.

## Credits

Original project author: **HowardWang**

Canonical repository:

https://github.com/howard929/DualPC-StreamingMic-Helper

