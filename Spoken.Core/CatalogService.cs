using System.Net.Http;
using System.Text.Json;
using System.Security.Cryptography;
using System.IO.Compression;

namespace Spoken.Core;

public class CatalogService
{
    private readonly HttpClient _httpClient;
    private readonly string _catalogUrl;
    private readonly string _cacheDir;

    public CatalogService(string catalogUrl = "https://api.example.com/bible-translations/catalog.json", string? cacheDir = null)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Spoken Bible App/1.0");
        _catalogUrl = catalogUrl;
        _cacheDir = cacheDir ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Spoken", "translations");
        Directory.CreateDirectory(_cacheDir);
    }

    public async Task<CatalogManifest?> FetchCatalogAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(_catalogUrl, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<CatalogManifest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to fetch catalog: {ex.Message}");
            return null;
        }
    }

    public async Task<List<TranslationInfo>> GetAvailableTranslationsAsync(CancellationToken ct = default)
    {
        var catalog = await FetchCatalogAsync(ct);
        return catalog?.Translations ?? new List<TranslationInfo>();
    }

    public async Task<bool> DownloadTranslationAsync(TranslationInfo translation, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        try
        {
            var localPath = Path.Combine(_cacheDir, $"{translation.Code.ToLowerInvariant()}.zip");
            
            // Check if already downloaded and verified
            if (File.Exists(localPath) && await VerifyChecksumAsync(localPath, translation.Checksum))
            {
                return true;
            }

            // Download
            using var response = await _httpClient.GetAsync(translation.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();
            
            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var fileStream = File.Create(localPath);
            
            var buffer = new byte[8192];
            var totalRead = 0L;
            int bytesRead;
            
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, ct);
                totalRead += bytesRead;
                
                if (totalBytes > 0)
                    progress?.Report((double)totalRead / totalBytes);
            }

            // Verify checksum
            if (!await VerifyChecksumAsync(localPath, translation.Checksum))
            {
                File.Delete(localPath);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Download failed for {translation.Code}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteTranslationAsync(string translationCode)
    {
        try
        {
            var localPath = Path.Combine(_cacheDir, $"{translationCode.ToLowerInvariant()}.zip");
            if (File.Exists(localPath))
            {
                File.Delete(localPath);
            }
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Delete failed for {translationCode}: {ex.Message}");
            return false;
        }
    }

    public List<string> GetInstalledTranslations()
    {
        if (!Directory.Exists(_cacheDir))
            return new List<string>();
            
        return Directory.GetFiles(_cacheDir, "*.zip")
            .Select(f => Path.GetFileNameWithoutExtension(f).ToUpperInvariant())
            .ToList();
    }

    private async Task<bool> VerifyChecksumAsync(string filePath, string expectedSha256)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            using var sha256 = SHA256.Create();
            var hash = await sha256.ComputeHashAsync(stream);
            var actualHash = Convert.ToHexString(hash).ToLowerInvariant();
            return string.Equals(actualHash, expectedSha256.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

public class CatalogManifest
{
    public int Version { get; set; } = 1;
    public DateTime LastUpdated { get; set; }
    public List<TranslationInfo> Translations { get; set; } = new();
}

public class TranslationInfo
{
    public required string Code { get; set; } // "ESV", "NIV", etc.
    public required string Name { get; set; } // "English Standard Version"
    public required string Language { get; set; } // "English", "Spanish", etc.
    public string? Description { get; set; }
    public string? Copyright { get; set; }
    public string? Attribution { get; set; }
    public required string DownloadUrl { get; set; }
    public required string Checksum { get; set; } // SHA-256
    public long SizeBytes { get; set; }
    public DateTime? PublishedDate { get; set; }
    public List<string> Features { get; set; } = new(); // "poetry", "footnotes", etc.
}