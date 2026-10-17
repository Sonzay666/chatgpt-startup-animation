using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace Codex550COverlay;

public partial class App : Application
{
    private const string AppUserModelId = "OpenAI.Codex_2p2nqsd0c76g0!App";
    private static readonly string LogPath = Path.Combine(Path.GetTempPath(), "ChatGPT-550C-launch.log");

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            Log("Native launcher started.");
            Process.Start(new ProcessStartInfo("explorer.exe", $"shell:AppsFolder\\{AppUserModelId}") { UseShellExecute = true });
            Log($"Activated installed Windows app: {AppUserModelId}");

            var appWindow = await WaitForChatGptWindowAsync(TimeSpan.FromSeconds(40));
            if (appWindow == IntPtr.Zero)
                throw new InvalidOperationException("ChatGPT Windows app did not expose a window within 40 seconds.");

            ShowWindow(appWindow, 9); // SW_RESTORE
            RECT bounds = default;
            var restored = false;
            for (var attempt = 0; attempt < 40; attempt++)
            {
                SetForegroundWindow(appWindow);
                if (!IsIconic(appWindow) && GetWindowRect(appWindow, out bounds) &&
                    bounds.Right - bounds.Left >= 500 && bounds.Bottom - bounds.Top >= 350)
                {
                    restored = true;
                    break;
                }
                await Task.Delay(100);
            }
            if (!restored)
                throw new InvalidOperationException("Could not read the ChatGPT window bounds.");

            Log($"Using ChatGPT window bounds: {bounds.Left},{bounds.Top},{bounds.Right - bounds.Left},{bounds.Bottom - bounds.Top}");
            var window = new MainWindow(bounds);
            MainWindow = window;
            window.Closed += (_, _) => Shutdown();
            window.Show();
        }
        catch (Exception ex)
        {
            Log("ERROR: " + ex);
            MessageBox.Show(
                $"无法启动 ChatGPT 桌面版。\n{ex.Message}\n\n详细日志：{LogPath}",
                "ChatGPT 550C 启动器", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    private static async Task<IntPtr> WaitForChatGptWindowAsync(TimeSpan timeout)
    {
        var end = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < end)
        {
            foreach (var process in Process.GetProcessesByName("ChatGPT"))
            {
                try
                {
                    var path = process.MainModule?.FileName;
                    if (process.MainWindowHandle != IntPtr.Zero && path?.Contains("WindowsApps", StringComparison.OrdinalIgnoreCase) == true)
                        return process.MainWindowHandle;
                }
                catch { }
            }
            await Task.Delay(250);
        }
        return IntPtr.Zero;
    }

    internal static void Log(string message)
    {
        try { File.AppendAllText(LogPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}{Environment.NewLine}"); } catch { }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
