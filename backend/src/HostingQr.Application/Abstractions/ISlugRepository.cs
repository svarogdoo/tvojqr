namespace HostingQr.Application.Abstractions;

public interface ISlugRepository
{
    Task<bool> ExistsAsync(string slug, CancellationToken cancellationToken = default);
}
