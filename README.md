# DualPC-StreamingMic-Helper

一个面向 Windows 的轻量托盘小工具，用于在**无采集卡双机直播**工作流中，把游戏电脑的 **Windows 默认麦克风**通过 NDI® Free Audio 单独发送到直播电脑。

> 当前版本：**v1.0.0**
>
> v1.0.0 UI：**简体中文**
>
> English UI support is planned for a future release.

## 为什么做这个工具

NDI Screen Capture HX 很适合把游戏电脑的屏幕和“电脑正在播放的声音”发送到直播电脑。

但如果麦克风是另一个独立输入设备，通常需要再单独发送一路音频。NDI Free Audio 可以完成这个工作，但它是命令行工具。

本项目给 NDI Free Audio 提供一个简单的 Windows 托盘前端：

- 使用 Windows 当前默认麦克风。
- 后台启动 NDI Free Audio，不保留命令行窗口。
- 托盘显示运行状态。
- 可以重新启动麦克风发送。
- 可以手动选择 NDI Free Audio 的路径。
- 自动记住用户选择的路径。
- 退出本工具时，同时结束由本工具启动的 Free Audio 进程。

## 推荐的双机结构

```text
游戏电脑
├─ NDI Screen Capture HX
│  ├─ 游戏 / 桌面画面
│  └─ 游戏 / Discord / 音乐等电脑播放声音
│
└─ DualPC-StreamingMic-Helper
   └─ Windows 默认麦克风
             │
             │ Local Network
             ▼
直播电脑
├─ Screen Capture HX 的 NDI 视频/桌面音频
└─ NDI Microphone 的独立麦克风音频
             │
             ▼
      OBS / 其他 NDI 接收软件
```

## v1.0.0 功能

- Windows 系统托盘运行
- Windows 默认麦克风
- NDI Source 名称：`NDI Microphone`
- 手动选择并记住 `NDI FreeAudio.exe`
- 无 CMD / PowerShell 窗口
- 重新启动 NDI 麦克风
- 退出时自动结束 Free Audio
- 防止重复启动本工具

配置文件保存于：

```text
%APPDATA%\NDI-Mic\freeaudio_path.txt
```

## 使用前准备

### 游戏电脑

从 NDI 官方网站自行获取并安装所需 NDI 软件：

- NDI Tools / Screen Capture HX  
  https://ndi.video/tools/
- NDI Free Audio  
  https://ndi.video/tools/free-audio/

**本仓库不包含、修改或重新分发任何 NDI 软件。**

### 直播电脑

需要可以接收 NDI 的软件。

例如 OBS Studio 可配合支持 NDI 输入的插件使用。请根据你自己的直播软件和工作流进行设置。

## 第一次使用 DualPC-StreamingMic-Helper

1. 在 Windows 中先把你真正要用于直播的麦克风设为**默认录音设备**。
2. 启动 `DualPC-StreamingMic-Helper.exe`。
3. 第一次启动时，托盘图标会提示尚未设置 NDI Free Audio 路径。
4. 右键托盘图标。
5. 选择 **“设置 NDI Free Audio 路径...”**。
6. 点“浏览...”并选择你已经安装的 `NDI FreeAudio.exe`。
7. 保存。
8. 程序会启动 Free Audio，并创建名为：

```text
NDI Microphone
```

的 NDI 音频源。

以后再次启动时，程序会直接使用之前保存的路径。

如果 NDI 更新后安装路径改变，只需要右键托盘图标重新设置路径。

## Screen Capture HX 建议设置

具体选项可能会随 NDI 版本变化，以下仅作为常见双机直播参考：

- Frame Rate：60 fps
- Capture：完整游戏显示器
- Audio Source：选择你游戏电脑真正用于听声音的播放设备
- Video Bandwidth / Codec：可根据网络环境使用 HEVC
- 如果是普通 SDR 工作流，一般没有必要使用 HEVC 10-bit

这样通常可以形成两路独立 NDI：

```text
Screen Capture HX
= 游戏画面 + 游戏电脑播放声音

NDI Microphone
= 默认麦克风
```

## Stream Deck（可选）

如果你使用 Stream Deck，可以建立一个 Multi Action，同时启动：

1. NDI Screen Capture HX
2. DualPC-StreamingMic-Helper.exe

这样只有直播时才开启两套发送工具。

## 编译源码

### 要求

- Windows
- .NET 8 SDK

官方 .NET 下载：

https://dotnet.microsoft.com/download/dotnet/8.0

进入 `src` 文件夹后执行：

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

编译结果通常位于：

```text
src\bin\Release\net8.0-windows\win-x64\publish\
```

## Windows 安全提示

本项目的公开二进制文件可能没有商业代码签名证书。

因此某些 Windows 设备上的 SmartScreen 或 Smart App Control 可能会对新下载或新编译的 EXE 给出提示或阻止运行。

如果你不信任预编译二进制文件，请直接查看 `src/Program.cs` 并自行从源码编译。

**不建议为了运行本项目而关闭整个 Windows 安全功能。**

## 项目状态

### v1.0.0

- 简体中文 UI
- 基础托盘功能
- 默认麦克风
- 手动指定 Free Audio 路径

### Planned

- English UI support
- 更清晰的状态提示
- 在不增加复杂性的前提下继续改善易用性

## License

本项目是 **source-available（源码公开）** 项目，不是 OSI 定义下的 Open Source 软件。

简要规则：

- 可以免费使用，包括用于有收入的直播和商业内容制作。
- 可以学习、修改、自行编译。
- 可以免费分发，但必须保留作者 Credit、LICENSE 和原始 GitHub 链接。
- 修改版必须明确注明修改来源。
- 不允许出售、收费再分发、改名后售卖，或嵌入收费软件/付费服务中商业分发，除非获得作者书面授权。

完整条款见：

- `LICENSE`
- `LICENSE_CN.md`

## NDI® Trademark Notice

NDI® is a registered trademark of Vizrt NDI AB.

DualPC-StreamingMic-Helper is an independent community project and is not affiliated with, endorsed by, or sponsored by Vizrt NDI AB.

This project does not distribute NDI software. Users should obtain NDI Tools and NDI Free Audio directly from the official NDI website.

## Credits

Original project author: **HowardWang**

Canonical repository:

https://github.com/howard929/DualPC-StreamingMic-Helper
