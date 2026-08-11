using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Loggin.Desktop.ViewModels;
using Forms = System.Windows.Forms;

namespace Loggin.Desktop;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();
    private readonly Forms.NotifyIcon _notifyIcon;
    private bool _allowClose;
    private bool _trayHintShown;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;

        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "loggin.ico");
        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = File.Exists(iconPath) ? new Icon(iconPath) : SystemIcons.Application,
            Text = "Loggin Desktop",
            Visible = true,
            ContextMenuStrip = BuildTrayMenu()
        };
        _notifyIcon.DoubleClick += (_, _) => RestoreWindow();
        Loaded += OnLoaded;
        Closing += OnClosing;
        StateChanged += OnStateChanged;
        _vm_Highlight = HighlightNav;
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.ActivePage) || e.PropertyName == nameof(MainViewModel.IsShellVisible))
                Dispatcher.Invoke(() => HighlightNav(_viewModel.ActivePage));
        };
    }

    private readonly Action<string> _vm_Highlight;

    private void HighlightNav(string page)
    {
        if (!IsLoaded) return;
        var activeStyle = (Style)FindResource("NavButtonActive");
        var normalStyle = (Style)FindResource("NavButton");
        var map = new System.Collections.Generic.Dictionary<string, System.Windows.Controls.Button>
        {
            [MainViewModel.PageDashboard] = NavDashboard,
            [MainViewModel.PageCollectors] = NavCollectors,
            [MainViewModel.PageLogs] = NavLogs,
            [MainViewModel.PageSettings] = NavSettings
        };
        foreach (var (key, btn) in map)
            btn.Style = key == page ? activeStyle : normalStyle;
    }

    private void NavButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender == NavDashboard) _viewModel.ActivePage = MainViewModel.PageDashboard;
        else if (sender == NavCollectors) _viewModel.ActivePage = MainViewModel.PageCollectors;
        else if (sender == NavLogs) _viewModel.ActivePage = MainViewModel.PageLogs;
        else if (sender == NavSettings) _viewModel.ActivePage = MainViewModel.PageSettings;
    }

    private async void OnLoaded(object o, RoutedEventArgs e)
    {
        var minimized = Environment.GetCommandLineArgs().Contains("--minimized", StringComparer.OrdinalIgnoreCase);
        await _viewModel.InitializeAsync(minimized);
        HighlightNav(_viewModel.ActivePage);
        if (minimized || _viewModel.Settings.StartMinimized)
            HideToTray(showHint: false);
    }

    private Forms.ContextMenuStrip BuildTrayMenu()
    {
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Abrir Loggin", null, (_, _) => Dispatcher.Invoke(RestoreWindow));
        menu.Items.Add("Configurações", null, (_, _) => Dispatcher.Invoke(() =>
        {
            RestoreWindow();
            _viewModel.ActivePage = MainViewModel.PageSettings;
        }));
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Encerrar", null, (_, _) => Dispatcher.Invoke(ExitApplication));
        return menu;
    }

    private void PasswordInput_OnPasswordChanged(object sender, RoutedEventArgs e) =>
        _viewModel.Password = ((PasswordBox)sender).Password;

    private void ToggleAuthMode_OnClick(object sender, RoutedEventArgs e)
    {
        _viewModel.IsRegisterMode = !_viewModel.IsRegisterMode;
        PasswordInput.Clear();
    }

    private void PreviewTheme_OnClick(object sender, RoutedEventArgs e) => _viewModel.PreviewTheme();

    private void Palette_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (((System.Windows.Controls.ComboBox)sender).SelectedItem is string palette && palette != "Personalizada")
            _viewModel.ApplyPalette(palette);
    }

    private void OnStateChanged(object? o, EventArgs e)
    {
        if (WindowState == WindowState.Minimized && _viewModel.Settings.MinimizeToTray)
            HideToTray(showHint: true);
    }

    private void OnClosing(object? o, System.ComponentModel.CancelEventArgs e)
    {
        if (_allowClose) return;
        if (_viewModel.Settings.CloseToTray)
        {
            e.Cancel = true;
            HideToTray(showHint: true);
            return;
        }
        ExitApplication();
    }

    private void HideToTray(bool showHint)
    {
        Hide();
        if (showHint && !_trayHintShown)
        {
            _notifyIcon.ShowBalloonTip(2500, "Loggin continua ativo",
                "Use o ícone da bandeja para restaurar ou encerrar.", Forms.ToolTipIcon.Info);
            _trayHintShown = true;
        }
    }

    private void RestoreWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void ExitApplication()
    {
        _allowClose = true;
        _viewModel.Dispose();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        Close();
        System.Windows.Application.Current.Shutdown();
    }

    private void LogRow_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is Border border && border.DataContext is LogLineViewModel logLine)
        {
            logLine.IsExpanded = !logLine.IsExpanded;
            e.Handled = true;
        }
    }
}
