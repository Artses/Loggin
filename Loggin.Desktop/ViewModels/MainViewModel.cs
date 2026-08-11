using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Loggin.Desktop.Models;
using Loggin.Desktop.Services;

namespace Loggin.Desktop.ViewModels;

public sealed class MainViewModel : ObservableObject, IDisposable
{
    private readonly ApiClient _api = new();
    private readonly SettingsService _settingsService = new();
    private readonly CredentialService _credentialService = new();
    private CancellationTokenSource? _monitorCts;
    private AppSettings _settings = AppSettings.Default();
    private string _activePage = "Dashboard";
    private bool _isAuthenticated;
    private bool _isBusy;
    private bool _isRegisterMode;
    private string _statusMessage = "Inicializando…";
    private ConnectionStatus _connectionStatus = ConnectionStatus.Offline;
    private string _userName = "Visitante";
    private string _email = "";
    private string _password = "";
    private string _registrationName = "";
    private CollectorDto? _selectedCollector;
    private LogDto? _selectedLog;
    private string _collectorName = "";
    private string _collectorUrl = "";
    private string _logPath = "";
    private CollectorDto? _logCollector;
    private string _searchText = "";
    private DateTimeOffset? _lastSync;

    public ObservableCollection<CollectorDto> Collectors { get; } = [];
    public ObservableCollection<LogDto> Logs { get; } = [];
    public ObservableCollection<LogLineDto> LogLines { get; } = [];
    public ObservableCollection<double> ActivityValues { get; } = [];
    public IReadOnlyList<ThemePreference> ThemeOptions { get; } = Enum.GetValues<ThemePreference>();
    public IReadOnlyList<string> Palettes { get; } = ["Padrão", "Oceano", "Floresta", "Pôr do sol", "Personalizada"];

    public AppSettings Settings { get => _settings; private set { _settings = value; Raise(); } }
    public string ActivePage { get => _activePage; set { if (Set(ref _activePage, value)) RaisePageVisibility(); } }
    public bool IsAuthenticated { get => _isAuthenticated; private set { if (Set(ref _isAuthenticated, value)) { Raise(nameof(IsLoginVisible)); Raise(nameof(IsShellVisible)); } } }
    public bool IsLoginVisible => !IsAuthenticated;
    public bool IsShellVisible => IsAuthenticated;
    public bool IsBusy { get => _isBusy; private set => Set(ref _isBusy, value); }
    public bool IsRegisterMode { get => _isRegisterMode; set { if (Set(ref _isRegisterMode, value)) Raise(nameof(AuthActionText)); } }
    public string AuthActionText => IsRegisterMode ? "Criar conta" : "Entrar";
    public string StatusMessage { get => _statusMessage; private set => Set(ref _statusMessage, value); }
    public ConnectionStatus ConnectionStatus { get => _connectionStatus; private set { if (Set(ref _connectionStatus, value)) Raise(nameof(ConnectionText)); } }
    public string ConnectionText => ConnectionStatus switch
    {
        ConnectionStatus.Online => "Conectado",
        ConnectionStatus.Connecting => "Conectando",
        ConnectionStatus.Reconnecting => "Reconectando",
        _ => "Offline"
    };
    public string UserName { get => _userName; private set => Set(ref _userName, value); }
    public string Email { get => _email; set => Set(ref _email, value); }
    public string Password { get => _password; set => Set(ref _password, value); }
    public string RegistrationName { get => _registrationName; set => Set(ref _registrationName, value); }
    public string CollectorName { get => _collectorName; set => Set(ref _collectorName, value); }
    public string CollectorUrl { get => _collectorUrl; set => Set(ref _collectorUrl, value); }
    public string LogPath { get => _logPath; set => Set(ref _logPath, value); }
    public CollectorDto? LogCollector { get => _logCollector; set => Set(ref _logCollector, value); }
    public string SearchText { get => _searchText; set { if (Set(ref _searchText, value)) Raise(nameof(FilteredLogLines)); } }
    public IEnumerable<LogLineDto> FilteredLogLines => string.IsNullOrWhiteSpace(SearchText)
        ? LogLines
        : LogLines.Where(line => line.Line.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
    public DateTimeOffset? LastSync { get => _lastSync; private set { if (Set(ref _lastSync, value)) Raise(nameof(LastSyncText)); } }
    public string LastSyncText => LastSync is null ? "Nunca sincronizado" : $"Sincronizado às {LastSync:HH:mm:ss}";
    public string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.1";
    public int CollectorCount => Collectors.Count;
    public int LogCount => Logs.Count;
    public int CurrentLineCount => LogLines.Count;
    public bool IsDashboardVisible => ActivePage == "Dashboard";
    public bool IsCollectorsVisible => ActivePage == "Collectors";
    public bool IsLogsVisible => ActivePage == "Logs";
    public bool IsSettingsVisible => ActivePage == "Settings";

    public CollectorDto? SelectedCollector
    {
        get => _selectedCollector;
        set
        {
            if (!Set(ref _selectedCollector, value)) return;
            if (value is not null)
            {
                CollectorName = value.Name;
                CollectorUrl = value.Url;
            }
            else
            {
                CollectorName = "";
                CollectorUrl = "";
            }
            DeleteCollectorCommand?.RaiseCanExecuteChanged();
        }
    }

    public LogDto? SelectedLog
    {
        get => _selectedLog;
        set
        {
            if (!Set(ref _selectedLog, value)) return;
            if (value is not null)
            {
                LogPath = value.Path;
                LogCollector = Collectors.FirstOrDefault(c => c.Id == value.CollectorId);
            }
            else
            {
                LogPath = "";
            }
            FetchLogCommand?.RaiseCanExecuteChanged();
            DeleteLogCommand?.RaiseCanExecuteChanged();
        }
    }

    public ICommand NavigateDashboardCommand { get; }
    public ICommand NavigateCollectorsCommand { get; }
    public ICommand NavigateLogsCommand { get; }
    public ICommand NavigateSettingsCommand { get; }
    public ICommand AuthenticateCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand SaveCollectorCommand { get; }
    public ICommand NewCollectorCommand { get; }
    public AsyncCommand DeleteCollectorCommand { get; }
    public ICommand SaveLogCommand { get; }
    public RelayCommand NewLogCommand { get; }
    public AsyncCommand DeleteLogCommand { get; }
    public AsyncCommand FetchLogCommand { get; }
    public ICommand SaveSettingsCommand { get; }
    public ICommand ResetSettingsCommand { get; }

    public event EventHandler? SettingsRequested;

    public MainViewModel()
    {
        NavigateDashboardCommand = new RelayCommand(() => ActivePage = "Dashboard");
        NavigateCollectorsCommand = new RelayCommand(() => ActivePage = "Collectors");
        NavigateLogsCommand = new RelayCommand(() => ActivePage = "Logs");
        NavigateSettingsCommand = new RelayCommand(() => { ActivePage = "Settings"; SettingsRequested?.Invoke(this, EventArgs.Empty); });
        AuthenticateCommand = new AsyncCommand(AuthenticateAsync);
        LogoutCommand = new RelayCommand(Logout);
        RefreshCommand = new AsyncCommand(() => RefreshAllAsync(CancellationToken.None));
        SaveCollectorCommand = new AsyncCommand(SaveCollectorAsync);
        NewCollectorCommand = new RelayCommand(ClearCollectorForm);
        DeleteCollectorCommand = new AsyncCommand(DeleteCollectorAsync, () => SelectedCollector is not null);
        SaveLogCommand = new AsyncCommand(SaveLogAsync);
        NewLogCommand = new RelayCommand(ClearLogForm);
        DeleteLogCommand = new AsyncCommand(DeleteLogAsync, () => SelectedLog is not null);
        FetchLogCommand = new AsyncCommand(FetchSelectedLogAsync, () => SelectedLog is not null);
        SaveSettingsCommand = new AsyncCommand(SaveSettingsAsync);
        ResetSettingsCommand = new AsyncCommand(ResetSettingsAsync);
    }

    public async Task InitializeAsync(bool startMinimized)
    {
        Settings = await _settingsService.LoadAsync();
        ThemeService.Apply(Settings);
        _api.Configure(Settings);
        try
        {
            var token = _credentialService.ReadToken();
            if (!string.IsNullOrWhiteSpace(token))
            {
                _api.Token = token;
                ConnectionStatus = ConnectionStatus.Connecting;
                await _api.VerifyAsync(CancellationToken.None);
                IsAuthenticated = true;
                UserName = "Usuário";
                StartMonitor();
                await RefreshAllAsync(CancellationToken.None);
            }
            else
            {
                ConnectionStatus = ConnectionStatus.Offline;
                StatusMessage = "Entre para acessar seu ambiente.";
            }
        }
        catch (Exception ex)
        {
            _credentialService.DeleteToken();
            _api.Token = null;
            ConnectionStatus = ConnectionStatus.Offline;
            StatusMessage = "Sua sessão expirou. Entre novamente.";
            LocalLog.Error("Falha ao restaurar sessão.", ex);
        }
    }

    private async Task AuthenticateAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
            IsRegisterMode && string.IsNullOrWhiteSpace(RegistrationName))
        {
            StatusMessage = "Preencha os campos obrigatórios.";
            return;
        }
        await RunOperationAsync(async ct =>
        {
            _api.Configure(Settings);
            await _settingsService.SaveAsync(Settings, ct);
            ConnectionStatus = ConnectionStatus.Connecting;
            var auth = IsRegisterMode
                ? await _api.RegisterAsync(RegistrationName.Trim(), Email.Trim(), Password, ct)
                : await _api.LoginAsync(Email.Trim(), Password, ct);
            _api.Token = auth.Token;
            _credentialService.SaveToken(auth.Token);
            UserName = auth.Name;
            Password = "";
            IsAuthenticated = true;
            ConnectionStatus = ConnectionStatus.Online;
            StatusMessage = $"Bem-vindo, {auth.Name}.";
            StartMonitor();
            await RefreshAllAsync(ct);
        }, "Não foi possível autenticar.");
    }

    private async Task RefreshAllAsync(CancellationToken ct)
    {
        if (!IsAuthenticated) return;
        await RunOperationAsync(RefreshCoreAsync, "Não foi possível sincronizar.", ct);
    }

    private async Task RefreshCoreAsync(CancellationToken token)
    {
        var selectedLogId = SelectedLog?.Id;
        var selectedCollectorId = SelectedCollector?.Id;

        var collectorsTask = _api.GetCollectorsAsync(token);
        var logsTask = _api.GetLogsAsync(token);
        await Task.WhenAll(collectorsTask, logsTask);
        Replace(Collectors, await collectorsTask);
        Replace(Logs, await logsTask);

        if (selectedCollectorId is { } collectorId)
            SelectedCollector = Collectors.FirstOrDefault(c => c.Id == collectorId);
        if (selectedLogId is { } logId)
            SelectedLog = Logs.FirstOrDefault(l => l.Id == logId);

        LogCollector ??= Collectors.FirstOrDefault();
        LastSync = DateTimeOffset.Now;
        ConnectionStatus = ConnectionStatus.Online;
        StatusMessage = "Dados atualizados.";
        RaiseCounts();
    }

    private async Task SaveCollectorAsync()
    {
        if (string.IsNullOrWhiteSpace(CollectorName) || string.IsNullOrWhiteSpace(CollectorUrl))
        {
            StatusMessage = "Informe nome e endereço do coletor.";
            return;
        }
        await RunOperationAsync(async ct =>
        {
            if (SelectedCollector is null)
                await _api.AddCollectorAsync(CollectorName.Trim(), CollectorUrl.Trim(), ct);
            else
                await _api.UpdateCollectorAsync(SelectedCollector.Id, CollectorName.Trim(), CollectorUrl.Trim(), ct);
            ClearCollectorForm();
            await RefreshAllAsync(ct);
            StatusMessage = "Coletor salvo com sucesso.";
        }, "Não foi possível salvar o coletor.");
    }

    private async Task DeleteCollectorAsync()
    {
        if (SelectedCollector is null) return;
        if (System.Windows.MessageBox.Show($"Excluir o coletor “{SelectedCollector.Name}”?", "Confirmar exclusão",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        await RunOperationAsync(async ct =>
        {
            await _api.DeleteCollectorAsync(SelectedCollector.Id, ct);
            ClearCollectorForm();
            await RefreshAllAsync(ct);
            StatusMessage = "Coletor excluído.";
        }, "Não foi possível excluir o coletor.");
    }

    private async Task SaveLogAsync()
    {
        if (LogCollector is null || string.IsNullOrWhiteSpace(LogPath))
        {
            StatusMessage = "Selecione o coletor e informe o caminho do log.";
            return;
        }
        await RunOperationAsync(async ct =>
        {
            if (SelectedLog is null)
                await _api.AddLogAsync(LogCollector.Id, LogPath.Trim(), ct);
            else
                await _api.UpdateLogAsync(SelectedLog.Id, LogCollector.Id, LogPath.Trim(), ct);
            ClearLogForm();
            await RefreshAllAsync(ct);
            StatusMessage = "Fonte de log salva com sucesso.";
        }, "Não foi possível salvar a fonte de log.");
    }

    private async Task DeleteLogAsync()
    {
        if (SelectedLog is null) return;
        if (System.Windows.MessageBox.Show($"Excluir a fonte “{SelectedLog.Path}”?", "Confirmar exclusão",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        await RunOperationAsync(async ct =>
        {
            await _api.DeleteLogAsync(SelectedLog.Id, ct);
            ClearLogForm();
            await RefreshAllAsync(ct);
            StatusMessage = "Fonte de log excluída.";
        }, "Não foi possível excluir a fonte de log.");
    }

    private async Task FetchSelectedLogAsync()
    {
        if (SelectedLog is null) return;
        await RunOperationAsync(FetchSelectedLogCoreAsync, "Não foi possível coletar as linhas.");
    }

    private async Task FetchSelectedLogCoreAsync(CancellationToken ct)
    {
        if (SelectedLog is null) 
            return;

        var lines = await _api.FetchLogAsync(SelectedLog.Id, SelectedLog.CollectorId, ct);

        Replace(LogLines, lines.OrderByDescending(l => l.TimeStamp));
        Raise(nameof(FilteredLogLines));
        Raise(nameof(CurrentLineCount));
        ActivityValues.Add(lines.Count);

        while (ActivityValues.Count > 30) 
            ActivityValues.RemoveAt(0);

        StatusMessage = $"{lines.Count} linhas recebidas.";
    }

    public void ApplyPalette(string palette)
    {
        (Settings.PrimaryColor, Settings.SecondaryColor, Settings.AccentColor) = palette switch
        {
            "Oceano" => ("#2563EB", "#06B6D4", "#38BDF8"),
            "Floresta" => ("#15803D", "#65A30D", "#EAB308"),
            "Pôr do sol" => ("#E11D48", "#F97316", "#FBBF24"),
            _ => ("#6D5EF5", "#20C7B7", "#F59E0B")
        };
        Raise(nameof(Settings));
        ThemeService.Apply(Settings);
    }

    public bool PreviewTheme()
    {
        if (!ThemeService.IsValidColor(Settings.PrimaryColor) ||
            !ThemeService.IsValidColor(Settings.SecondaryColor) ||
            !ThemeService.IsValidColor(Settings.AccentColor))
        {
            StatusMessage = "Use cores hexadecimais válidas, por exemplo #6D5EF5.";
            return false;
        }
        ThemeService.Apply(Settings);
        return true;
    }

    private async Task SaveSettingsAsync()
    {
        if (!PreviewTheme()) return;
        await RunOperationAsync(async ct =>
        {
            _api.Configure(Settings);
            StartupService.SetEnabled(Settings.StartWithWindows, Settings.StartMinimized);
            await _settingsService.SaveAsync(Settings, ct);
            StatusMessage = "Configurações salvas.";
            StartMonitor();
        }, "Não foi possível salvar as configurações.");
    }

    private async Task ResetSettingsAsync()
    {
        Settings = AppSettings.Default();
        ThemeService.Apply(Settings);
        StartupService.SetEnabled(false, false);
        await _settingsService.SaveAsync(Settings);
        _api.Configure(Settings);
        StatusMessage = "Configurações padrão restauradas.";
    }

    private void StartMonitor()
    {
        _monitorCts?.Cancel();
        _monitorCts?.Dispose();
        _monitorCts = new CancellationTokenSource();
        _ = MonitorAsync(_monitorCts.Token);
    }

    private async Task MonitorAsync(CancellationToken ct)
    {
        var failures = 0;
        while (!ct.IsCancellationRequested && IsAuthenticated)
        {
            var delay = failures == 0
                ? TimeSpan.FromSeconds(Math.Clamp(Settings.RefreshIntervalSeconds, 2, 3600))
                : TimeSpan.FromSeconds(Math.Min(60, Math.Pow(2, failures)));
            try
            {
                await Task.Delay(delay, ct);
                await RefreshCoreAsync(ct);
                if (SelectedLog is not null)
                    await FetchSelectedLogCoreAsync(ct);
                failures = 0;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                failures++;
                ConnectionStatus = Settings.AutoReconnect ? ConnectionStatus.Reconnecting : ConnectionStatus.Offline;
                StatusMessage = Settings.AutoReconnect
                    ? $"Conexão perdida. Nova tentativa em {Math.Min(60, Math.Pow(2, failures)):0}s."
                    : "Conexão perdida.";
                LocalLog.Error("Falha no monitoramento.", ex);
                if (!Settings.AutoReconnect) break;
            }
        }
    }

    private async Task RunOperationAsync(Func<CancellationToken, Task> operation, string fallback,
        CancellationToken externalToken = default)
    {
        if (IsBusy && externalToken == default) return;
        IsBusy = true;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(Settings.TimeoutSeconds + 2, 5, 305)));
        try { await operation(timeout.Token); }
        catch (OperationCanceledException) when (!externalToken.IsCancellationRequested)
        {
            ConnectionStatus = ConnectionStatus.Offline;
            StatusMessage = "A operação excedeu o tempo limite.";
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == HttpStatusCode.Unauthorized && IsAuthenticated)
                Logout("Sua sessão expirou.");
            else
                StatusMessage = ex.Message;
            LocalLog.Error(fallback, ex);
        }
        catch (HttpRequestException ex)
        {
            ConnectionStatus = Settings.AutoReconnect ? ConnectionStatus.Reconnecting : ConnectionStatus.Offline;
            StatusMessage = $"{fallback} Verifique o backend.";
            LocalLog.Error(fallback, ex);
        }
        catch (Exception ex)
        {
            StatusMessage = $"{fallback} {ex.Message}";
            LocalLog.Error(fallback, ex);
        }
        finally { IsBusy = false; }
    }

    private void Logout() => Logout("Sessão encerrada.");
    private void Logout(string message)
    {
        _monitorCts?.Cancel();
        _credentialService.DeleteToken();
        _api.Token = null;
        IsAuthenticated = false;
        UserName = "Visitante";
        ConnectionStatus = ConnectionStatus.Offline;
        StatusMessage = message;
        Collectors.Clear();
        Logs.Clear();
        LogLines.Clear();
        RaiseCounts();
    }

    private void ClearCollectorForm()
    {
        SelectedCollector = null;
        CollectorName = "";
        CollectorUrl = "";
    }

    private void ClearLogForm()
    {
        SelectedLog = null;
        LogPath = "";
        LogCollector = Collectors.FirstOrDefault();
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source) target.Add(item);
    }

    private void RaiseCounts()
    {
        Raise(nameof(CollectorCount));
        Raise(nameof(LogCount));
        Raise(nameof(CurrentLineCount));
    }

    private void RaisePageVisibility()
    {
        Raise(nameof(IsDashboardVisible));
        Raise(nameof(IsCollectorsVisible));
        Raise(nameof(IsLogsVisible));
        Raise(nameof(IsSettingsVisible));
    }

    public void Dispose()
    {
        _monitorCts?.Cancel();
        _monitorCts?.Dispose();
        _api.Dispose();
    }
}
