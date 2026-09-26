# DualPC-StreamingMic-Helper

English | [简体中文](README.md)

A lightweight Windows system tray helper for **capture-card-free dual-PC streaming**, designed to send the gaming PC's **Windows default microphone** separately to the streaming PC through NDI® Free Audio.

> Current version: **v1.0.0**
>
> v1.0.0 UI language: **Simplified Chinese**
>
> English UI support is planned for a future release.

## Why This Project Exists

NDI Screen Capture HX is a convenient way to send your gaming PC's screen and desktop playback audio to another computer over the local network.

However, if your microphone is connected as a separate input device, it usually needs to be sent as an independent audio source.

NDI Free Audio can do this, but it is primarily a command-line utility.

DualPC-StreamingMic-Helper provides a simple Windows tray interface for NDI Free Audio.

It can:

- Use the current Windows default microphone.
- Launch NDI Free Audio silently in the background.
- Show the current sending status in the system tray.
- Restart the microphone NDI output.
- Let the user manually select the NDI Free Audio executable.
- Remember the selected NDI Free Audio path.
- Automatically stop the Free Audio process started by this helper when the helper exits.

## Recommended Dual-PC Setup

```text
Gaming PC
├─ NDI Screen Capture HX
│  ├─ Game / Desktop Video
│  └─ Game / Discord / Music / Desktop Playback Audio
│
└─ DualPC-StreamingMic-Helper
   └─ Windows Default Microphone
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

## v1.0.0 Features

- Runs in the Windows system tray
- Uses the Windows default microphone
- NDI source name: `NDI Microphone`
- Manually select and save the path to `NDI FreeAudio.exe`
- No visible CMD or PowerShell window
- Restart the microphone NDI output from the tray menu
- Automatically stop the Free Audio process when exiting
- Prevent multiple instances of the helper from running at the same time

The saved Free Audio path is stored at:

```text
%APPDATA%\NDI-Mic\freeaudio_path.txt
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

1. In Windows, set the microphone you want to use for streaming as your **default recording/input device**.

2. Launch:

```text
DualPC-StreamingMic-Helper.exe
```

3. On first launch, the system tray icon will indicate that the NDI Free Audio path has not yet been configured.

4. Right-click the tray icon.

5. Select:

```text
设置 NDI Free Audio 路径...
```

6. Click the browse button and select your installed:

```text
NDI FreeAudio.exe
```

7. Save the setting.

8. The helper will launch NDI Free Audio and create an NDI audio source named:

```text
NDI Microphone
```

The selected Free Audio path will be remembered automatically.

If an NDI update changes the installation path, simply open the tray menu and select the Free Audio executable again.

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
= Windows Default Microphone
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

## Windows Security Notice

Prebuilt releases of this project may not be signed with a commercial code-signing certificate.

Because of this, Windows SmartScreen or Smart App Control may display a warning or block the executable on some systems.

If you do not trust the prebuilt executable, you can inspect:

```text
src/Program.cs
```

and build the application yourself from source.

**Disabling Windows security features globally just to run this project is not recommended.**

## Project Status

### v1.0.0

- Simplified Chinese UI
- Basic system tray functionality
- Windows default microphone support
- Manual NDI Free Audio executable selection
- Saved Free Audio path
- Background Free Audio process management

### Planned

- English UI support
- Improved status messages
- Additional usability improvements without making the application unnecessarily complex

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
