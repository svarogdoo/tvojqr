using Dapper;
using HostingQr.Application.Abstractions;
using HostingQr.Domain.Projects;
using HostingQr.Infrastructure.Data;

namespace HostingQr.Infrastructure.Projects;

public sealed class ProjectRepository : IProjectRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProjectRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ProjectWithSlug>> ListByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select
                p.id,
                p.owner_user_id as OwnerUserId,
                p.name,
                p.status,
                s.slug as Slug,
                p.created_at as CreatedAt,
                p.updated_at as UpdatedAt
            from projects p
            inner join slugs s on s.project_id = p.id and s.is_primary = true
            where p.owner_user_id = @OwnerUserId
            order by p.updated_at desc;
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { OwnerUserId = ownerUserId }, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync<ProjectWithSlug>(command);
        return rows.ToArray();
    }

    public async Task<ProjectWithSlug?> GetByIdAsync(Guid ownerUserId, Guid projectId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select
                p.id,
                p.owner_user_id as OwnerUserId,
                p.name,
                p.status,
                s.slug as Slug,
                p.created_at as CreatedAt,
                p.updated_at as UpdatedAt
            from projects p
            inner join slugs s on s.project_id = p.id and s.is_primary = true
            where p.owner_user_id = @OwnerUserId and p.id = @ProjectId;
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { OwnerUserId = ownerUserId, ProjectId = projectId }, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ProjectWithSlug>(command);
    }

    public async Task<PublicProject?> GetPublicBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select
                p.id as ProjectId,
                p.name,
                s.slug as Slug,
                u.display_name as OwnerDisplayName,
                p.status
            from slugs s
            inner join projects p on p.id = s.project_id
            inner join users u on u.id = p.owner_user_id
            where s.slug = @Slug and s.is_primary = true;
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Slug = slug }, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<PublicProject>(command);
    }

    public async Task<ProjectWithSlug> CreateAsync(Guid ownerUserId, string name, string slug, CancellationToken cancellationToken = default)
    {
        Guid projectId = Guid.NewGuid();
        Guid slugId = Guid.NewGuid();

        const string projectSql = """
            insert into projects (id, owner_user_id, name, status)
            values (@Id, @OwnerUserId, @Name, @Status);
            """;

        const string slugSql = """
            insert into slugs (id, project_id, slug, is_primary)
            values (@Id, @ProjectId, @Slug, true);
            """;

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(projectSql, new { Id = projectId, OwnerUserId = ownerUserId, Name = name, Status = ProjectStatus.Active }, transaction, cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(slugSql, new { Id = slugId, ProjectId = projectId, Slug = slug }, transaction, cancellationToken: cancellationToken));

        transaction.Commit();

        return (await GetByIdAsync(ownerUserId, projectId, cancellationToken))!;
    }

    public async Task<ProjectWithSlug?> UpdateAsync(Guid ownerUserId, Guid projectId, string name, string slug, CancellationToken cancellationToken = default)
    {
        const string projectSql = """
            update projects
            set name = @Name,
                updated_at = now()
            where id = @ProjectId and owner_user_id = @OwnerUserId;
            """;

        const string slugSql = """
            update slugs
            set slug = @Slug
            where project_id = @ProjectId and is_primary = true;
            """;

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        int affectedProjects = await connection.ExecuteAsync(new CommandDefinition(projectSql, new
        {
            ProjectId = projectId,
            OwnerUserId = ownerUserId,
            Name = name,
        }, transaction, cancellationToken: cancellationToken));

        if (affectedProjects == 0)
        {
            transaction.Rollback();
            return null;
        }

        await connection.ExecuteAsync(new CommandDefinition(slugSql, new
        {
            ProjectId = projectId,
            Slug = slug,
        }, transaction, cancellationToken: cancellationToken));

        transaction.Commit();

        return await GetByIdAsync(ownerUserId, projectId, cancellationToken);
    }

    public async Task<ProjectWithSlug?> UpdateStatusAsync(Guid ownerUserId, Guid projectId, string status, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update projects
            set status = @Status,
                updated_at = now()
            where id = @ProjectId and owner_user_id = @OwnerUserId;
            """;

        using var connection = _connectionFactory.CreateConnection();
        int affected = await connection.ExecuteAsync(new CommandDefinition(sql, new { Status = status, ProjectId = projectId, OwnerUserId = ownerUserId }, cancellationToken: cancellationToken));
        if (affected == 0)
        {
            return null;
        }

        return await GetByIdAsync(ownerUserId, projectId, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid ownerUserId, Guid projectId, CancellationToken cancellationToken = default)
    {
        const string deleteAssetsSql = "delete from assets where project_id = @ProjectId;";
        const string deleteSlugsSql = "delete from slugs where project_id = @ProjectId;";
        const string deleteProjectSql = "delete from projects where id = @ProjectId and owner_user_id = @OwnerUserId;";

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(deleteAssetsSql, new { ProjectId = projectId }, transaction, cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(deleteSlugsSql, new { ProjectId = projectId }, transaction, cancellationToken: cancellationToken));
        int affected = await connection.ExecuteAsync(new CommandDefinition(deleteProjectSql, new { ProjectId = projectId, OwnerUserId = ownerUserId }, transaction, cancellationToken: cancellationToken));

        transaction.Commit();
        return affected > 0;
    }
}
