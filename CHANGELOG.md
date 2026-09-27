# Changelog

## v1.1.0

- 新增简体中文 / English 界面切换，默认中文，语言选择自动保存。
- 新增输入设备选择，保留 Windows Default 默认行为。
- 从 NDI Free Audio 读取输入设备列表，支持中文设备名称。
- 更换设备时可立即重启，或保存选择并在下次重启生效。
- 状态窗口显示当前输入和待生效选择。
- 保存语言、设备和 Free Audio 路径，兼容 v1.0.0 路径配置。
- 补强进程停止失败处理，避免继续启动重复发送进程。
- 保留 `NDI Microphone` 源名、系统托盘图标、单实例与无命令行窗口行为。

English UI, input-device selection, persistent preferences, and restart handling are now available. See [v1.1.0 release notes](RELEASE_NOTES_v1.1.0.md).

## v1.0.0

首个公开版本：简体中文托盘界面、Windows 默认麦克风、手动路径保存、后台运行、重启和退出清理、单实例。

See [v1.0.0 release notes](RELEASE_NOTES_v1.0.0.md).
