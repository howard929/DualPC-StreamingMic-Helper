using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        bool createdNew;

        using Mutex mutex = new Mutex(
            true,
            @"Local\NDI-Mic-Tray-App",
            out createdNew
        );

        if (!createdNew)
        {
            MessageBox.Show(
                Ui.T("NDI Microphone 已经在运行。"),
                "NDI Microphone",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using TrayApplicationContext context = new TrayApplicationContext();
        Application.Run(context);
    }
}

internal sealed partial class TrayApplicationContext : ApplicationContext
{
    // =========================================================
    // 基本设置
    // =========================================================

    private const string NdiSourceName = "NDI Microphone";

    // =========================================================
    // 托盘相关
    // =========================================================

    private readonly NotifyIcon trayIcon;
    private readonly ContextMenuStrip trayMenu;

    private readonly ToolStripMenuItem statusItem;
    private readonly ToolStripMenuItem restartItem;
    private readonly ToolStripMenuItem setPathItem;
    private readonly ToolStripMenuItem exitItem;

    private readonly System.Windows.Forms.Timer processTimer;


    // =========================================================
    // NDI Free Audio
    // =========================================================

    private Process? freeAudioProcess;

    private string? configuredPath;

    private bool isExiting = false;


    // =========================================================
    // 初始化
    // =========================================================

    public TrayApplicationContext()
    {
        configuredPath = Settings.Current.FreeAudioPath;

        trayMenu = new ContextMenuStrip();


        // 状态
        statusItem = new ToolStripMenuItem(
            Ui.T("✕ NDI Microphone 未发送")
        );

        statusItem.Enabled = false;

        trayMenu.Items.Add(statusItem);


        trayMenu.Items.Add(
            new ToolStripSeparator()
        );


        // 重新启动
        restartItem = new ToolStripMenuItem(
            Ui.T("重新启动 NDI 麦克风")
        );

        restartItem.Click += (_, _) =>
        {
            RestartFreeAudio();
        };

        trayMenu.Items.Add(restartItem);


        // 设置 Free Audio 路径
        setPathItem = new ToolStripMenuItem(
            Ui.T("设置 NDI Free Audio 路径...")
        );

        setPathItem.Click += (_, _) =>
        {
            ConfigureFreeAudioPath();
        };

        trayMenu.Items.Add(setPathItem);


        trayMenu.Items.Add(
            new ToolStripSeparator()
        );


        // 退出
        exitItem = new ToolStripMenuItem(
            Ui.T("退出 NDI 麦克风")
        );

        exitItem.Click += (_, _) =>
        {
            ExitApplication();
        };

        trayMenu.Items.Add(exitItem);


        // 托盘图标
        trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Warning,
            Text = "NDI Microphone - Stopped",
            ContextMenuStrip = trayMenu,
            Visible = true
        };


        // 双击托盘图标显示状态信息
        trayIcon.DoubleClick += (_, _) =>
        {
            ShowCurrentStatus();
        };


        // 每秒检查一次 Free Audio 是否仍然运行
        processTimer = new System.Windows.Forms.Timer
        {
            Interval = 1000
        };

        processTimer.Tick += (_, _) =>
        {
            CheckFreeAudioProcess();
        };

        processTimer.Start();
        InitializeOptions();


        // =====================================================
        // 启动时：
        //
        // 只读取用户之前保存的路径。
        //
        // 不搜索磁盘。
        // 不扫描 Program Files。
        // 不检查 PATH。
        // =====================================================

        if (
            !string.IsNullOrWhiteSpace(configuredPath) &&
            File.Exists(configuredPath)
        )
        {
            StartFreeAudio();
        }
        else
        {
            configuredPath = null;

            SetStoppedStatus();

            trayIcon.ShowBalloonTip(
                4000,
                "NDI Microphone",
                Ui.T("请右键托盘图标，设置 NDI Free Audio 路径。"),
                ToolTipIcon.Warning
            );
        }
    }


    // =========================================================
    // 启动 Free Audio
    // =========================================================

    private async void StartFreeAudio()
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            SetStoppedStatus();

            MessageBox.Show(
                Ui.T("还没有设置 NDI Free Audio 的路径。\n\n") +
                Ui.T("请右键托盘图标，然后选择：\n") +
                Ui.T("“设置 NDI Free Audio 路径...”"),
                "NDI Microphone",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            return;
        }


        if (!File.Exists(configuredPath))
        {
            SetStoppedStatus();

            MessageBox.Show(
                Ui.T("之前保存的 NDI Free Audio 路径已经不存在：\n\n") +
                configuredPath +
                Ui.T("\n\n请重新设置路径。"),
                "NDI Microphone",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            return;
        }


        if (isExiting || startBusy) return;
        startBusy = true;
        setPathItem.Enabled = false;
        deviceMenu.Enabled = false;
        restartItem.Enabled = false;
        try
        {
            if (!StopFreeAudio()) return;
            SetStoppedStatus();
            string input = await ResolveInput();
            if (isExiting) return;
            ProcessStartInfo startInfo =
                new ProcessStartInfo
                {
                    FileName = configuredPath,



                    UseShellExecute = false,

                    CreateNoWindow = true,

                    WindowStyle =
                        ProcessWindowStyle.Hidden,

                    WorkingDirectory =
                        Path.GetDirectoryName(
                            configuredPath
                        ) ?? string.Empty
                };


            startInfo.ArgumentList.Add("-input");
            startInfo.ArgumentList.Add(input);
            startInfo.ArgumentList.Add("-input_name");
            startInfo.ArgumentList.Add(NdiSourceName);
            freeAudioProcess = Process.Start(startInfo);
            activeDevice = Settings.Current.InputDevice;


            if (freeAudioProcess == null)
            {
                throw new InvalidOperationException(
                    Ui.T("无法启动 NDI Free Audio。")
                );
            }


            SetRunningStatus();


            trayIcon.ShowBalloonTip(
                2500,
                "NDI Microphone",
                Ui.T("所选输入正在通过 NDI 发送。", "The selected input is sending through NDI."),
                ToolTipIcon.Info
            );
        }
        catch (Exception ex)
        {
            if (isExiting) return;
            freeAudioProcess?.Dispose();
            freeAudioProcess = null;

            SetStoppedStatus();


            MessageBox.Show(
                Ui.T("NDI Free Audio 启动失败。\n\n") +
                ex.Message,
                "NDI Microphone",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
        finally
        {
            startBusy = false;
            if (!isExiting)
            {
                setPathItem.Enabled = true;
                deviceMenu.Enabled = true;
                restartItem.Enabled = !string.IsNullOrWhiteSpace(configuredPath);
            }
        }
    }


    // =========================================================
    // 停止 Free Audio
    // =========================================================

    private bool StopFreeAudio()
    {
        if (freeAudioProcess == null) return true;
        try
        {
            if (!freeAudioProcess.HasExited)
            {
                freeAudioProcess.Kill(entireProcessTree: true);
                if (!freeAudioProcess.WaitForExit(2000)) throw new TimeoutException();
            }
            freeAudioProcess.Dispose();
            freeAudioProcess = null;
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(Ui.T("无法停止 Free Audio；不会启动新的发送进程。\n", "Could not stop Free Audio; no new sender will be started.\n") + ex.Message, NdiSourceName);
            return false;
        }
    }

    // =========================================================
    // 重启 Free Audio
    // =========================================================

    private void RestartFreeAudio()
    {
        if (
            string.IsNullOrWhiteSpace(configuredPath) ||
            !File.Exists(configuredPath)
        )
        {
            ConfigureFreeAudioPath();

            return;
        }


        StartFreeAudio();
    }


    // =========================================================
    // 检查进程
    // =========================================================

    private void CheckFreeAudioProcess()
    {
        if (freeAudioProcess == null)
        {
            return;
        }


        try
        {
            if (freeAudioProcess.HasExited)
            {
                freeAudioProcess.Dispose();

                freeAudioProcess = null;


                SetStoppedStatus();


                if (!isExiting)
                {
                    trayIcon.ShowBalloonTip(
                        3500,
                        "NDI Microphone",
                        Ui.T("NDI Free Audio 已停止运行。"),
                        ToolTipIcon.Warning
                    );
                }
            }
        }
        catch
        {
            freeAudioProcess = null;

            SetStoppedStatus();
        }
    }


    // =========================================================
    // 设置 Free Audio 路径
    // =========================================================

    private void ConfigureFreeAudioPath()
    {
        if (startBusy || isExiting) return;
        using FreeAudioPathForm dialog =
            new FreeAudioPathForm(
                configuredPath ?? string.Empty
            );


        DialogResult result =
            dialog.ShowDialog();


        if (isExiting || result != DialogResult.OK)
        {
            return;
        }


        string selectedPath =
            dialog.SelectedPath.Trim().Trim('"');


        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            MessageBox.Show(
                Ui.T("请输入 NDI Free Audio 的路径。"),
                "NDI Microphone",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            return;
        }


        if (!File.Exists(selectedPath))
        {
            MessageBox.Show(
                Ui.T("找不到这个文件：\n\n") +
                selectedPath,
                "NDI Microphone",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            return;
        }


        if (
            !string.Equals(
                Path.GetExtension(selectedPath),
                ".exe",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            MessageBox.Show(
                Ui.T("请选择 NDI FreeAudio.exe。"),
                "NDI Microphone",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            return;
        }


        string? previousPath = configuredPath;
        configuredPath = selectedPath;


        try
        {
            SaveConfiguredPath(
                configuredPath
            );
        }
        catch (Exception ex)
        {
            configuredPath = previousPath;
            Settings.Current.FreeAudioPath = previousPath;
            MessageBox.Show(
                Ui.T("无法保存设置：\n\n") +
                ex.Message,
                "NDI Microphone",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            return;
        }


        // 路径修改后立即使用新路径重启
        StartFreeAudio();
    }


    // =========================================================
    // 保存路径
    // =========================================================

    private static void SaveConfiguredPath(string path)
    {
        Settings.Current.FreeAudioPath = path;
        Settings.Save();
    }

    private void SetRunningStatus()
    {
        statusItem.Text =
            Ui.T("✓ NDI Microphone 正在发送");

        trayIcon.Icon =
            SystemIcons.Information;

        trayIcon.Text =
            "NDI Microphone - Running";

        restartItem.Enabled = true;
    }


    // =========================================================
    // 停止状态
    // =========================================================

    private void SetStoppedStatus()
    {
        statusItem.Text =
            Ui.T("✕ NDI Microphone 未发送");

        trayIcon.Icon =
            SystemIcons.Warning;

        trayIcon.Text =
            "NDI Microphone - Stopped";

        restartItem.Enabled =
            !string.IsNullOrWhiteSpace(
                configuredPath
            );
    }


    // =========================================================
    // 双击托盘状态
    // =========================================================

    private void ShowCurrentStatus()
    {
        string processStatus;


        if (
            freeAudioProcess != null &&
            !freeAudioProcess.HasExited
        )
        {
            processStatus =
                Ui.T("状态：正在发送");
        }
        else
        {
            processStatus =
                Ui.T("状态：未发送");
        }


        string pathStatus =
            string.IsNullOrWhiteSpace(
                configuredPath
            )
                ? Ui.T("未设置")
                : configuredPath;


        MessageBox.Show(
            processStatus +
            "\n\n" +
            "NDI Source：\n" +
            NdiSourceName +
            "\n\n" +
            Ui.T("麦克风：\n") +
            DeviceDescription() +
            "\n\n" +
            "NDI Free Audio：\n" +
            pathStatus,
            "NDI Microphone",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }


    // =========================================================
    // 退出
    // =========================================================

    private void ExitApplication()
    {
        isExiting = true;


        processTimer.Stop();

        if (!StopFreeAudio()) { isExiting = false; processTimer.Start(); return; }
        try { AudioDevices.StopQueries(); }
        catch (Exception ex)
        {
            isExiting = false;
            processTimer.Start();
            MessageBox.Show(Ui.T("无法关闭设备查询进程：", "Could not stop the device query: ") + ex.Message, NdiSourceName);
            return;
        }

        trayIcon.Visible = false;

        trayIcon.Dispose();

        trayMenu.Dispose();

        processTimer.Dispose();


        ExitThread();
    }


    protected override void Dispose(
        bool disposing
    )
    {
        if (disposing)
        {
            if (!isExiting)
            {
                StopFreeAudio();
            }
        }


        base.Dispose(disposing);
    }
}


// =============================================================
// NDI Free Audio 路径设置窗口
//
// 用户可以：
// 1. 直接输入路径
// 2. 粘贴路径
// 3. 点击“浏览...”选择 EXE
//
// 不进行任何自动文件扫描。
// =============================================================

internal sealed class FreeAudioPathForm : Form
{
    private readonly TextBox pathTextBox;

    public string SelectedPath =>
        pathTextBox.Text;


    public FreeAudioPathForm(
        string currentPath
    )
    {
        Text =
            Ui.T("设置 NDI Free Audio 路径");

        Width = 650;

        Height = 190;

        FormBorderStyle =
            FormBorderStyle.FixedDialog;

        MaximizeBox = false;

        MinimizeBox = false;

        ShowInTaskbar = false;

        StartPosition =
            FormStartPosition.CenterScreen;


        Label descriptionLabel =
            new Label
            {
                Left = 15,
                Top = 15,
                Width = 600,
                Height = 35,

                Text =
                    Ui.T("请选择或输入 NDI FreeAudio.exe 的完整路径：")
            };


        pathTextBox =
            new TextBox
            {
                Left = 15,
                Top = 55,
                Width = 500,
                Text = currentPath
            };


        Button browseButton =
            new Button
            {
                Left = 525,
                Top = 53,
                Width = 90,
                Height = 27,
                Text = Ui.T("浏览...")
            };


        browseButton.Click += (_, _) =>
        {
            BrowseForFreeAudio();
        };


        Button saveButton =
            new Button
            {
                Left = 425,
                Top = 100,
                Width = 90,
                Height = 30,
                Text = Ui.T("保存"),
                DialogResult = DialogResult.OK
            };


        Button cancelButton =
            new Button
            {
                Left = 525,
                Top = 100,
                Width = 90,
                Height = 30,
                Text = Ui.T("取消"),
                DialogResult = DialogResult.Cancel
            };


        Controls.Add(
            descriptionLabel
        );

        Controls.Add(
            pathTextBox
        );

        Controls.Add(
            browseButton
        );

        Controls.Add(
            saveButton
        );

        Controls.Add(
            cancelButton
        );


        AcceptButton =
            saveButton;

        CancelButton =
            cancelButton;
    }


    private void BrowseForFreeAudio()
    {
        using OpenFileDialog dialog =
            new OpenFileDialog
            {
                Title =
                    Ui.T("选择 NDI FreeAudio.exe"),

                Filter =
                    "NDI Free Audio|NDI FreeAudio.exe|" +
                    Ui.T("EXE 文件|*.exe|", "EXE files|*.exe|") +
                    Ui.T("所有文件|*.*", "All files|*.*"),

                CheckFileExists = true,

                Multiselect = false
            };


        if (
            !string.IsNullOrWhiteSpace(
                pathTextBox.Text
            )
        )
        {
            try
            {
                string? directory =
                    Path.GetDirectoryName(
                        pathTextBox.Text
                    );


                if (
                    !string.IsNullOrWhiteSpace(
                        directory
                    ) &&
                    Directory.Exists(
                        directory
                    )
                )
                {
                    dialog.InitialDirectory =
                        directory;
                }
            }
            catch
            {
            }
        }


        if (
            dialog.ShowDialog() ==
            DialogResult.OK
        )
        {
            pathTextBox.Text =
                dialog.FileName;
        }
    }
}
