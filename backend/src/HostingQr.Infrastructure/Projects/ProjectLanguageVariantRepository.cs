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

    public async Task<ProjectLanguageVariant> UpdateAsync(Guid projectId, string currentLanguageCode, string languageCode, string displayName, CancellationToken cancellationToken = default)
    {
        const string currentVariantSql = """
            select
                id,
                project_id as ProjectId,
                language_code as LanguageCode,
                display_name as DisplayName,
                is_default as IsDefault,
                sort_order as SortOrder,
                created_at as CreatedAt
            from project_language_variants
            where project_id = @ProjectId and language_code = @CurrentLanguageCode and is_default = false
            limit 1;
            """;

        const string updateVariantSql = """
            update project_language_variants
            set language_code = @LanguageCode,
                display_name = @DisplayName
            where project_id = @ProjectId and language_code = @CurrentLanguageCode and is_default = false;
            """;

        const string updateAssetsSql = """
            update assets
            set language_code = @NewLanguageCode
            where project_id = @ProjectId and language_code = @OldLanguageCode;
            """;

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        ProjectLanguageVariant currentVariant = await connection.QuerySingleAsync<ProjectLanguageVariant>(new CommandDefinition(currentVariantSql, new { ProjectId = projectId, CurrentLanguageCode = currentLanguageCode }, transaction, cancellationToken: cancellationToken));

        if (currentVariant.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                update project_language_variants
                set display_name = @DisplayName
                where project_id = @ProjectId and language_code = @CurrentLanguageCode and is_default = false;
                """, new { ProjectId = projectId, CurrentLanguageCode = currentLanguageCode, DisplayName = displayName }, transaction, cancellationToken: cancellationToken));
            transaction.Commit();
            return (await ListByProjectAsync(projectId, cancellationToken)).Single(language => language.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase) && !language.IsDefault);
        }

        await connection.ExecuteAsync(new CommandDefinition(updateVariantSql, new { ProjectId = projectId, CurrentLanguageCode = currentLanguageCode, LanguageCode = languageCode, DisplayName = displayName }, transaction, cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(updateAssetsSql, new { ProjectId = projectId, OldLanguageCode = currentLanguageCode, NewLanguageCode = languageCode }, transaction, cancellationToken: cancellationToken));
        transaction.Commit();

        return (await ListByProjectAsync(projectId, cancellationToken)).Single(language => language.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase) && !language.IsDefault);
    }

    public async Task<ProjectLanguageVariant> UpdateDefaultAsync(Guid projectId, string languageCode, string displayName, CancellationToken cancellationToken = default)
    {
        const string currentDefaultSql = """
            select
                id,
                project_id as ProjectId,
                language_code as LanguageCode,
                display_name as DisplayName,
                is_default as IsDefault,
                sort_order as SortOrder,
                created_at as CreatedAt
            from project_language_variants
            where project_id = @ProjectId and is_default = true
            limit 1;
            """;

        const string targetVariantSql = """
            select
                id,
                project_id as ProjectId,
                language_code as LanguageCode,
                display_name as DisplayName,
                is_default as IsDefault,
                sort_order as SortOrder,
                created_at as CreatedAt
            from project_language_variants
            where project_id = @ProjectId and language_code = @LanguageCode
            limit 1;
            """;

        const string unsetCurrentDefaultSql = """
            update project_language_variants
            set is_default = false
            where project_id = @ProjectId and is_default = true;
            """;

        const string setTargetDefaultSql = """
            update project_language_variants
            set is_default = true,
                display_name = @DisplayName
            where project_id = @ProjectId and language_code = @LanguageCode;
            """;

        const string renameCurrentDefaultSql = """
            update project_language_variants
            set language_code = @LanguageCode,
                display_name = @DisplayName
            where project_id = @ProjectId and is_default = true;
            """;

        const string updateAssetsSql = """
            update assets
            set language_code = @NewLanguageCode
            where project_id = @ProjectId and language_code = @OldLanguageCode;
            """;

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        ProjectLanguageVariant currentDefault = await connection.QuerySingleAsync<ProjectLanguageVariant>(new CommandDefinition(currentDefaultSql, new { ProjectId = projectId }, transaction, cancellationToken: cancellationToken));
        ProjectLanguageVariant? targetVariant = await connection.QuerySingleOrDefaultAsync<ProjectLanguageVariant>(new CommandDefinition(targetVariantSql, new { ProjectId = projectId, LanguageCode = languageCode }, transaction, cancellationToken: cancellationToken));

        if (currentDefault.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
        {
            await connection.ExecuteAsync(new CommandDefinition(setTargetDefaultSql, new { ProjectId = projectId, LanguageCode = languageCode, DisplayName = displayName }, transaction, cancellationToken: cancellationToken));
            transaction.Commit();
            return (await ListByProjectAsync(projectId, cancellationToken)).Single(language => language.IsDefault);
        }

        if (targetVariant is not null)
        {
            await connection.ExecuteAsync(new CommandDefinition(unsetCurrentDefaultSql, new { ProjectId = projectId }, transaction, cancellationToken: cancellationToken));
            await connection.ExecuteAsync(new CommandDefinition(setTargetDefaultSql, new { ProjectId = projectId, LanguageCode = languageCode, DisplayName = displayName }, transaction, cancellationToken: cancellationToken));
            transaction.Commit();
            return (await ListByProjectAsync(projectId, cancellationToken)).Single(language => language.IsDefault);
        }

        await connection.ExecuteAsync(new CommandDefinition(renameCurrentDefaultSql, new { ProjectId = projectId, LanguageCode = languageCode, DisplayName = displayName }, transaction, cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(updateAssetsSql, new { ProjectId = projectId, OldLanguageCode = currentDefault.LanguageCode, NewLanguageCode = languageCode }, transaction, cancellationToken: cancellationToken));
        transaction.Commit();

        return (await ListByProjectAsync(projectId, cancellationToken)).Single(language => language.IsDefault);
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
