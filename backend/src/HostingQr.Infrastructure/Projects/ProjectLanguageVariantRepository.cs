using Dapper;
using HostingQr.Application.Abstractions;
using HostingQr.Domain.Projects;
using HostingQr.Infrastructure.Data;

namespace HostingQr.Infrastructure.Projects;

public sealed class ProjectLanguageVariantRepository : IProjectLanguageVariantRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProjectLanguageVariantRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ProjectLanguageVariant>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select
                id,
                project_id as ProjectId,
                language_code as LanguageCode,
                display_name as DisplayName,
                is_default as IsDefault,
                sort_order as SortOrder,
                created_at as CreatedAt
            from project_language_variants
            where project_id = @ProjectId
            order by sort_order asc, created_at asc;
            """;

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<ProjectLanguageVariant>(new CommandDefinition(sql, new { ProjectId = projectId }, cancellationToken: cancellationToken));
        return rows.ToArray();
    }

    public async Task<ProjectLanguageVariant> CreateAsync(Guid projectId, string languageCode, string displayName, bool isDefault, int sortOrder, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into project_language_variants (id, project_id, language_code, display_name, is_default, sort_order)
            values (@Id, @ProjectId, @LanguageCode, @DisplayName, @IsDefault, @SortOrder);
            """;

        Guid id = Guid.NewGuid();
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id, ProjectId = projectId, LanguageCode = languageCode, DisplayName = displayName, IsDefault = isDefault, SortOrder = sortOrder }, cancellationToken: cancellationToken));
        return (await ListByProjectAsync(projectId, cancellationToken)).Single(language => language.Id == id);
    }

    public async Task<bool> DeleteAsync(Guid projectId, string languageCode, CancellationToken cancellationToken = default)
    {
        const string sql = """
            delete from project_language_variants
            where project_id = @ProjectId and language_code = @LanguageCode and is_default = false;
            """;

        using var connection = _connectionFactory.CreateConnection();
        int affected = await connection.ExecuteAsync(new CommandDefinition(sql, new { ProjectId = projectId, LanguageCode = languageCode }, cancellationToken: cancellationToken));
        return affected > 0;
    }
}
