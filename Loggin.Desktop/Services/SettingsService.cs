using System.Text.Json;
using System.IO;

namespace Loggin.Desktop.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _directory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Artses", "Loggin");
    private string FilePath => Path.Combine(_directory, "settings.json");

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(FilePath))
                return AppSettings.Default();

            await using var stream = File.OpenRead(FilePath);
            return await JsonSerializer.DeserializeAsync<AppSettings>(stream, JsonOptions, cancellationToken)
                   ?? AppSettings.Default();
        }
        catch (Exception ex)
        {
            LocalLog.Error("Não foi possível carregar as configurações.", ex);
            return AppSettings.Default();
        }
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_directory);
        var temporaryPath = FilePath + ".tmp";
        await using (var stream = File.Create(temporaryPath))
            await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
        File.Move(temporaryPath, FilePath, true);
    }
}
