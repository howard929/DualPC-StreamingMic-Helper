# DualPC-StreamingMic-Helper v1.1.0

新增双语界面和音频输入设备选择，保留轻量托盘工作方式。

## 新增功能

- 简体中文 / English 切换，默认中文，记住上次选择
- 一级语言菜单始终显示 `语言 / Language`
- `音频设备 / Audio Device` 可选择 Windows Default 或一个指定输入
- 设备列表来自 NDI Free Audio，支持中文设备名称
- 更换设备时可选择立即重启，由 Helper 自动完成
- 取消立即重启时仍保存选择，下次重启生效
- 双击托盘可查看当前输入和待生效选择
- 语言、输入设备和 Free Audio 路径统一保存

## 保留功能

- Windows 系统托盘运行，无 CMD / PowerShell 窗口
- NDI Source 名称：`NDI Microphone`
- 手动选择并记住 NDI Free Audio 路径，不扫描安装目录
- 重启麦克风发送、退出清理及防止重复启动
- 沿用现有托盘状态图标，不增加输出设备管理

## 当前语言

- 简体中文（默认）
- English

## 升级说明

先退出 v1.0.0，再启动新版。首次升级继承旧版保存的 Free Audio 路径，默认保持 Windows Default。

新设置保存到 `%APPDATA%\NDI-Mic\helper-settings.json`，旧路径文件保留不变。已完成编译、自动验证和作者功能验收。

## English Summary

- Added Simplified Chinese / English UI with saved language preference.
- Added input-device selection from Free Audio's own device list.
- Device changes can restart immediately or apply on the next restart.
- Preserved Windows Default, the `NDI Microphone` source name, hidden launch, and existing tray behavior.
- Existing v1.0.0 Free Audio paths are imported automatically.

## Requirements

- Windows x64
- NDI Free Audio（用户需自行从 NDI 官方网站安装）
- 如果自行从源码编译：.NET 8 SDK
- 成品为自包含单文件，不需要单独安装 .NET Runtime

## Important

This project does not include or redistribute NDI software.

NDI® is a registered trademark of Vizrt NDI AB.

This project is independent and is not affiliated with, endorsed by, or sponsored by Vizrt NDI AB.

## Windows Security

The prebuilt executable may be unsigned. Some Windows systems may show SmartScreen or Smart App Control warnings.

Users who prefer not to run the prebuilt executable can inspect and build the source code themselves.
