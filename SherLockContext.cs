using System.Drawing.Text;
using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;

namespace SherLock;

public partial class SherLockContext : ApplicationContext
{
    private NotifyIcon _capsIcon = null!;
    private NotifyIcon _numIcon = null!;
    private Timer _timer = null!;

    [LibraryImport("user32.dll", EntryPoint = "GetKeyState")]
    private static partial short GetKeyState(int nVirtKey);

    private const int VkCapital = 0x14;
    private const int VkNumlock = 0x90;

    private bool _lastCapsState;
    private bool _lastNumState;

    public SherLockContext()
    {
        InitializeIcons();
        InitializeTimer();
        UpdateIcons(force: true);
    }

    private void InitializeIcons()
    {
        var contextMenu = new ContextMenuStrip();
        var exitItem = new ToolStripMenuItem("退出");
        exitItem.Click += ExitItem_Click;
        contextMenu.Items.Add(exitItem);

        _capsIcon = new NotifyIcon
        {
            Text = "Caps Lock Status",
            Visible = true,
            ContextMenuStrip = contextMenu
        };

        _numIcon = new NotifyIcon
        {
            Text = "Num Lock Status",
            Visible = true,
            ContextMenuStrip = contextMenu
        };
    }

    private void InitializeTimer()
    {
        _timer = new Timer();
        _timer.Interval = 200;
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        UpdateIcons();
    }

    private void UpdateIcons(bool force = false)
    {
        var capsActive = (GetKeyState(VkCapital) & 0x0001) != 0;
        var numActive = (GetKeyState(VkNumlock) & 0x0001) != 0;

        if (force || capsActive != _lastCapsState)
        {
            var oldIcon = _capsIcon.Icon;
            _capsIcon.Icon = CreateIcon("C", capsActive);
            _lastCapsState = capsActive;
            oldIcon?.Dispose();
        }

        if (force || numActive != _lastNumState)
        {
            var oldIcon = _numIcon.Icon;
            _numIcon.Icon = CreateIcon("N", numActive);
            _lastNumState = numActive;
            oldIcon?.Dispose();
        }
    }

    [LibraryImport("user32.dll", EntryPoint = "DestroyIcon")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial void DestroyIcon(IntPtr handle);

    private static Icon CreateIcon(string text, bool isActive)
    {
        var bgColor = isActive ? Color.SeaGreen : Color.DimGray;
        var textColor = Color.White;

        using var bitmap = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bitmap);

        g.Clear(bgColor);

        g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

        using var brush = new SolidBrush(textColor);
        using var font = new Font("Segoe UI", 11, FontStyle.Bold, GraphicsUnit.Pixel);
        var textSize = g.MeasureString(text, font);

        var x = (16 - textSize.Width) / 2;
        var y = (16 - textSize.Height) / 2 + 1;

        g.DrawString(text, font, brush, x, y);

        var hIcon = bitmap.GetHicon();
        try
        {
            using var tempIcon = Icon.FromHandle(hIcon);
            return (Icon)tempIcon.Clone();
        }
        finally
        {
            DestroyIcon(hIcon);
        }
    }

    private void ExitItem_Click(object? sender, EventArgs e)
    {
        _capsIcon.Visible = false;
        _numIcon.Visible = false;
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _capsIcon.Dispose();
            _numIcon.Dispose();
            _timer.Dispose();
        }
        base.Dispose(disposing);
    }
}
