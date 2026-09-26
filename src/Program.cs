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
                "NDI Microphone 已经在运行。",
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

internal sealed class TrayApplicationContext : ApplicationContext
{
    private const string NdiSourceName = "NDI Microphone";
    private const string AudioInput = "default";

    private static readonly string SettingsDirectory =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NDI-Mic"
        );

    private static readonly string SettingsFile =
        Path.Combine(
            SettingsDirectory,
            "freeaudio_path.txt"
        );

    private readonly NotifyIcon trayIcon;
    private readonly ContextMenuStrip trayMenu;
    private readonly ToolStripMenuItem statusItem;
    private readonly ToolStripMenuItem restartItem;
    private readonly ToolStripMenuItem setPathItem;
    private readonly ToolStripMenuItem exitItem;
    private readonly System.Windows.Forms.Timer processTimer;

    private Process? freeAudioProcess;
    private string? configuredPath;
    private bool isExiting = false;

    public TrayApplicationContext()
    {
        configuredPath = LoadConfiguredPath();

        trayMenu = new ContextMenuStrip();

        statusItem = new ToolStripMenuItem(
            "✕ NDI Microphone 未发送"
        );
        statusItem.Enabled = false;
        trayMenu.Items.Add(statusItem);

        trayMenu.Items.Add(new ToolStripSeparator());

        restartItem = new ToolStripMenuItem(
            "重新启动 NDI 麦克风"
        );
        restartItem.Click += (_, _) =>
        {
            RestartFreeAudio();
        };
        trayMenu.Items.Add(restartItem);

        setPathItem = new ToolStripMenuItem(
            "设置 NDI Free Audio 路径..."
        );
        setPathItem.Click += (_, _) =>
        {
            ConfigureFreeAudioPath();
        };
        trayMenu.Items.Add(setPathItem);

        trayMenu.Items.Add(new ToolStripSeparator());

        exitItem = new ToolStripMenuItem(
            "退出 NDI 麦克风"
        );
        exitItem.Click += (_, _) =>
        {
            ExitApplication();
        };
        trayMenu.Items.Add(exitItem);

        trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Warning,
            Text = "NDI Microphone - Stopped",
            ContextMenuStrip = trayMenu,
            Visible = true
        };

        trayIcon.DoubleClick += (_, _) =>
        {
            ShowCurrentStatus();
        };

        processTimer = new System.Windows.Forms.Timer
        {
            Interval = 1000
        };

        processTimer.Tick += (_, _) =>
        {
            CheckFreeAudioProcess();
        };

        processTimer.Start();

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
                "请右键托盘图标，设置 NDI Free Audio 路径。",
                ToolTipIcon.Warning
            );
        }
    }

    private void StartFreeAudio()
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            SetStoppedStatus();

            MessageBox.Show(
                "还没有设置 NDI Free Audio 的路径。\n\n" +
                "请右键托盘图标，然后选择：\n" +
                "“设置 NDI Free Audio 路径...”",
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
                "之前保存的 NDI Free Audio 路径已经不存在：\n\n" +
                configuredPath +
                "\n\n请重新设置路径。",
                "NDI Microphone",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            return;
        }

        StopFreeAudio();

        try
        {
            ProcessStartInfo startInfo =
                new ProcessStartInfo
                {
                    FileName = configuredPath,

                    Arguments =
                        $"-input {AudioInput} " +
                        $"-input_name \"{NdiSourceName}\"",

                    UseShellExecute = false,

                    CreateNoWindow = true,

                    WindowStyle =
                        ProcessWindowStyle.Hidden,

                    WorkingDirectory =
                        Path.GetDirectoryName(configuredPath) ?? string.Empty
                };

            freeAudioProcess = Process.Start(startInfo);

            if (freeAudioProcess == null)
            {
                throw new InvalidOperationException(
                    "无法启动 NDI Free Audio。"
                );
            }

            SetRunningStatus();

            trayIcon.ShowBalloonTip(
                2500,
                "NDI Microphone",
                "默认麦克风正在通过 NDI 发送。",
                ToolTipIcon.Info
            );
        }
        catch (Exception ex)
        {
            freeAudioProcess = null;

            SetStoppedStatus();

            MessageBox.Show(
                "NDI Free Audio 启动失败。\n\n" +
                ex.Message,
                "NDI Microphone",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void StopFreeAudio()
    {
        if (freeAudioProcess == null)
        {
            return;
        }

        try
        {
            if (!freeAudioProcess.HasExited)
            {
                freeAudioProcess.Kill(
                    entireProcessTree: true
                );

                freeAudioProcess.WaitForExit(
                    2000
                );
            }
        }
        catch
        {
        }

        try
        {
            freeAudioProcess.Dispose();
        }
        catch
        {
        }

        freeAudioProcess = null;
    }

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

        StopFreeAudio();
        StartFreeAudio();
    }

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
                        "NDI Free Audio 已停止运行。",
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

    private void ConfigureFreeAudioPath()
    {
        using FreeAudioPathForm dialog =
            new FreeAudioPathForm(
                configuredPath ?? string.Empty
            );

        DialogResult result = dialog.ShowDialog();

        if (result != DialogResult.OK)
        {
            return;
        }

        string selectedPath =
            dialog.SelectedPath.Trim().Trim('"');

        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            MessageBox.Show(
                "请输入 NDI Free Audio 的路径。",
                "NDI Microphone",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            return;
        }

        if (!File.Exists(selectedPath))
        {
            MessageBox.Show(
                "找不到这个文件：\n\n" +
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
                "请选择 NDI FreeAudio.exe。",
                "NDI Microphone",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            return;
        }

        configuredPath = selectedPath;

        try
        {
            SaveConfiguredPath(
                configuredPath
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "无法保存设置：\n\n" +
                ex.Message,
                "NDI Microphone",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            return;
        }

        StopFreeAudio();
        StartFreeAudio();
    }

    private static void SaveConfiguredPath(string path)
    {
        Directory.CreateDirectory(
            SettingsDirectory
        );

        File.WriteAllText(
            SettingsFile,
            path
        );
    }

    private static string? LoadConfiguredPath()
    {
        try
        {
            if (!File.Exists(SettingsFile))
            {
                return null;
            }

            string path =
                File.ReadAllText(
                    SettingsFile
                ).Trim();

            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            return path;
        }
        catch
        {
            return null;
        }
    }

    private void SetRunningStatus()
    {
        statusItem.Text =
            "✓ NDI Microphone 正在发送";

        trayIcon.Icon =
            SystemIcons.Information;

        trayIcon.Text =
            "NDI Microphone - Running";

        restartItem.Enabled = true;
    }

    private void SetStoppedStatus()
    {
        statusItem.Text =
            "✕ NDI Microphone 未发送";

        trayIcon.Icon =
            SystemIcons.Warning;

        trayIcon.Text =
            "NDI Microphone - Stopped";

        restartItem.Enabled =
            !string.IsNullOrWhiteSpace(
                configuredPath
            );
    }

    private void ShowCurrentStatus()
    {
        string processStatus;

        if (
            freeAudioProcess != null &&
            !freeAudioProcess.HasExited
        )
        {
            processStatus =
                "状态：正在发送";
        }
        else
        {
            processStatus =
                "状态：未发送";
        }

        string pathStatus =
            string.IsNullOrWhiteSpace(
                configuredPath
            )
                ? "未设置"
                : configuredPath;

        MessageBox.Show(
            processStatus +
            "\n\n" +
            "NDI Source：\n" +
            NdiSourceName +
            "\n\n" +
            "麦克风：\n" +
            "Windows 默认麦克风" +
            "\n\n" +
            "NDI Free Audio：\n" +
            pathStatus,
            "NDI Microphone",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }

    private void ExitApplication()
    {
        isExiting = true;

        processTimer.Stop();

        StopFreeAudio();

        trayIcon.Visible = false;
        trayIcon.Dispose();

        trayMenu.Dispose();
        processTimer.Dispose();

        ExitThread();
    }

    protected override void Dispose(bool disposing)
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

internal sealed class FreeAudioPathForm : Form
{
    private readonly TextBox pathTextBox;

    public string SelectedPath =>
        pathTextBox.Text;

    public FreeAudioPathForm(string currentPath)
    {
        Text =
            "设置 NDI Free Audio 路径";

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
                    "请选择或输入 NDI FreeAudio.exe 的完整路径："
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
                Text = "浏览..."
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
                Text = "保存",
                DialogResult = DialogResult.OK
            };

        Button cancelButton =
            new Button
            {
                Left = 525,
                Top = 100,
                Width = 90,
                Height = 30,
                Text = "取消",
                DialogResult = DialogResult.Cancel
            };

        Controls.Add(descriptionLabel);
        Controls.Add(pathTextBox);
        Controls.Add(browseButton);
        Controls.Add(saveButton);
        Controls.Add(cancelButton);

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
                    "选择 NDI FreeAudio.exe",

                Filter =
                    "NDI Free Audio|NDI FreeAudio.exe|" +
                    "EXE 文件|*.exe|" +
                    "所有文件|*.*",

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
