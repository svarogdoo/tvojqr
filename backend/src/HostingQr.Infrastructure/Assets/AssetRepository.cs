using Dapper;
using HostingQr.Application.Abstractions;
using HostingQr.Domain.Assets;
using HostingQr.Infrastructure.Data;

namespace HostingQr.Infrastructure.Assets;

public sealed class AssetRepository : IAssetRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AssetRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Asset>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select
                id,
                project_id as ProjectId,
                language_code as LanguageCode,
                original_file_name as OriginalFileName,
                stored_file_name as StoredFileName,
                content_type as ContentType,
                size_bytes as SizeBytes,
                sort_order as SortOrder,
                created_at as CreatedAt
            from assets
            where project_id = @ProjectId
            order by sort_order asc, created_at asc;
            """;

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<Asset>(new CommandDefinition(sql, new { ProjectId = projectId }, cancellationToken: cancellationToken));
        return rows.ToArray();
    }

    public async Task<Asset?> GetByIdAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select
                id,
                project_id as ProjectId,
                language_code as LanguageCode,
                original_file_name as OriginalFileName,
                stored_file_name as StoredFileName,
                content_type as ContentType,
                size_bytes as SizeBytes,
                sort_order as SortOrder,
                created_at as CreatedAt
            from assets
            where id = @AssetId;
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Asset>(new CommandDefinition(sql, new { AssetId = assetId }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<Asset>> CreateAsync(Guid projectId, string languageCode, IReadOnlyList<CreateAssetRecord> assets, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into assets (id, project_id, language_code, original_file_name, stored_file_name, content_type, size_bytes, sort_order)
            values (@Id, @ProjectId, @LanguageCode, @OriginalFileName, @StoredFileName, @ContentType, @SizeBytes, @SortOrder);
            """;

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        foreach (CreateAssetRecord asset in assets)
        {
            await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                LanguageCode = languageCode,
                asset.OriginalFileName,
                asset.StoredFileName,
                asset.ContentType,
                asset.SizeBytes,
                asset.SortOrder,
            }, transaction, cancellationToken: cancellationToken));
        }

        transaction.Commit();
        return await ListByProjectAsync(projectId, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        const string sql = "delete from assets where id = @AssetId;";

        using var connection = _connectionFactory.CreateConnection();
        int affected = await connection.ExecuteAsync(new CommandDefinition(sql, new { AssetId = assetId }, cancellationToken: cancellationToken));
        return affected > 0;
    }

    public async Task UpdateSortOrderAsync(Guid projectId, IReadOnlyList<Guid> assetIds, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update assets
            set sort_order = @SortOrder
            where id = @AssetId and project_id = @ProjectId;
            """;

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        for (int i = 0; i < assetIds.Count; i++)
        {
            await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                ProjectId = projectId,
                AssetId = assetIds[i],
                SortOrder = i,
            }, transaction, cancellationToken: cancellationToken));
        }

        transaction.Commit();
    }
}
