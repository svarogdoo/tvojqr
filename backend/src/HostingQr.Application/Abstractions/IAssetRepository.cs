using HostingQr.Domain.Assets;

namespace HostingQr.Application.Abstractions;

public interface IAssetRepository
{
    Task<IReadOnlyList<Asset>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Asset>> CreateAsync(Guid projectId, string languageCode, IReadOnlyList<CreateAssetRecord> assets, CancellationToken cancellationToken = default);
}

public sealed record CreateAssetRecord(string OriginalFileName, string StoredFileName, string ContentType, long SizeBytes, int SortOrder);
