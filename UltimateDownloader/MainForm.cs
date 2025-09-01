namespace UltimateDownloader;

public partial class MainForm : Form
{
    private NotifyIcon trayIcon;
    private ContextMenuStrip trayMenu;

    public MainForm()
    {
        InitializeComponent();

        trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("Open", null, OnOpen);
        trayMenu.Items.Add("Exit", null, OnExit);

        trayIcon = new NotifyIcon
        {
            Icon = new Icon("UltimateDownloader.ico"), // Add an .ico file to your project
            ContextMenuStrip = trayMenu,
            Text = "Ultimate Downloader",
            Visible = true
        };

        this.Resize += new EventHandler(Form_Resize);
    }

    private void Form_Resize(object sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Minimized)
        {
            Hide();
        }
    }

    private void OnOpen(object sender, EventArgs e)
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    private void OnExit(object sender, EventArgs e)
    {
        trayIcon.Visible = false;
        Application.Exit();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        trayIcon.Visible = false;
        base.OnFormClosing(e);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Hide();
        WindowState = FormWindowState.Minimized;
    }
}
