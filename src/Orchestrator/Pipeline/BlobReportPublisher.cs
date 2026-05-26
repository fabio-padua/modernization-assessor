using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace ModernizationAssessor.Orchestrator.Pipeline;

/// <summary>
/// Optional component that uploads the generated report artifacts to a blob container.
/// Activated when <c>ASSESSOR_OUTPUT_BLOB_URL</c> is set to a full container URL such as
/// <c>https://stmodassessor.blob.core.windows.net/reports</c>.
/// </summary>
internal sealed class BlobReportPublisher
{
    private readonly BlobContainerClient _container;
    private readonly ILogger<BlobReportPublisher> _logger;

    public BlobReportPublisher(Uri containerUri, TokenCredential credential, ILogger<BlobReportPublisher> logger)
    {
        _container = new BlobContainerClient(containerUri, credential);
        _logger = logger;
    }

    public async Task<IReadOnlyList<Uri>> PublishAsync(string customerName, IEnumerable<string> filePaths, CancellationToken ct)
    {
        var safeCustomer = SanitizeForPath(customerName);
        var stamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
        var uploadedUris = new List<Uri>();

        foreach (var path in filePaths)
        {
            var name = Path.GetFileName(path);
            var blobName = $"{safeCustomer}/{stamp}/{name}";
            var blob = _container.GetBlobClient(blobName);

            await using var stream = File.OpenRead(path);
            var contentType = path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                ? "application/json"
                : path.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                    ? "text/markdown"
                    : "application/octet-stream";

            await blob.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            }, ct);

            _logger.LogInformation("Uploaded {Local} -> {Blob}", path, blob.Uri);
            uploadedUris.Add(blob.Uri);
        }

        return uploadedUris;
    }

    private static string SanitizeForPath(string value)
    {
        var trimmed = value.Trim().ToLowerInvariant();
        var allowed = trimmed.Select(c => char.IsLetterOrDigit(c) || c == '-' ? c : '-').ToArray();
        var slug = new string(allowed).Trim('-');
        return string.IsNullOrEmpty(slug) ? "unknown" : slug;
    }
}
