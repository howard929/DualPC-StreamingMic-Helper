# DualPC-StreamingMic-Helper v1.0.0

首个公开版本。

## 功能

- Windows 系统托盘运行
- 使用 Windows 默认麦克风
- NDI Source 名称：`NDI Microphone`
- 手动选择并记住 NDI Free Audio 路径
- 后台启动 NDI Free Audio，无命令行窗口
- 支持重新启动麦克风 NDI 输出
- 退出程序时自动关闭对应的 Free Audio 进程
- 防止重复启动

## 当前语言

- 简体中文

English UI support is planned for a future release.

## Requirements

- Windows
- NDI Free Audio（用户需自行从 NDI 官方网站安装）
- 如果自行从源码编译：.NET 8 SDK

## Important

This project does not include or redistribute NDI software.

NDI® is a registered trademark of Vizrt NDI AB.

This project is independent and is not affiliated with, endorsed by, or sponsored by Vizrt NDI AB.

## Windows Security

The prebuilt executable may be unsigned. Some Windows systems may show SmartScreen or Smart App Control warnings.

Users who prefer not to run the prebuilt executable can inspect and build the source code themselves.
