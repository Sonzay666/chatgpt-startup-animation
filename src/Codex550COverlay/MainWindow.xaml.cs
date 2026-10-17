using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Codex550COverlay;

public partial class MainWindow : Window
{
    private static readonly IntPtr HwndTopmost = new(-1);
    private const uint SwpShowWindow = 0x0040;
    private readonly App.RECT _bounds;
    private readonly string _profile = Path.Combine(Path.GetTempPath(), "Codex550COverlay-" + Environment.ProcessId);
    private bool _closed;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

    public MainWindow(App.RECT bounds)
    {
        InitializeComponent();
        _bounds = bounds;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var handle = new WindowInteropHelper(this).Handle;
            SetWindowPos(handle, HwndTopmost, _bounds.Left, _bounds.Top,
                _bounds.Right - _bounds.Left, _bounds.Bottom - _bounds.Top, SwpShowWindow);
            Opacity = 1;

            var env = await CoreWebView2Environment.CreateAsync(null, _profile);
            await AnimationView.EnsureCoreWebView2Async(env);
            AnimationView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            AnimationView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            AnimationView.CoreWebView2.WebMessageReceived += (_, args) =>
            {
                if (args.TryGetWebMessageAsString() == "skip") Dispatcher.BeginInvoke(Close);
            };
            await AnimationView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
                document.addEventListener('click', e => { if (e.target.closest('#skip')) chrome.webview.postMessage('skip'); }, true);
                window.addEventListener('keydown', e => { if (e.key === 'Escape') chrome.webview.postMessage('skip'); }, true);
            ");

            var html = Path.Combine(AppContext.BaseDirectory, "CODEX-550C-intro.html");
            if (!File.Exists(html)) throw new FileNotFoundException("The local animation HTML was not found.", html);

            AnimationView.CoreWebView2.NavigationCompleted += (_, _) =>
            {
                if (_closed) return;
                App.Log($"Animation rendered in native overlay over the {App.ProductName} window.");
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(17000) };
                timer.Tick += (_, _) => { timer.Stop(); Close(); };
                timer.Start();
            };
            AnimationView.CoreWebView2.Navigate(new Uri(html).AbsoluteUri);
        }
        catch (Exception ex)
        {
            App.Log("OVERLAY ERROR: " + ex);
            MessageBox.Show(
                $"动画无法播放。请确认系统已安装 Microsoft Edge WebView2 Runtime。\n\n详细日志：{Path.Combine(Path.GetTempPath(), App.ProductName + "-550C-launch.log")}",
                $"{App.ProductName} 550C 启动器", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _closed = true;
        App.Log($"Native animation overlay closed; {App.ProductName} remains open.");
        try { AnimationView.Dispose(); } catch { }
        try { Directory.Delete(_profile, true); } catch { }
        base.OnClosed(e);
    }
}
