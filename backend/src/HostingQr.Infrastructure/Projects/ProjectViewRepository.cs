using Dapper;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Projects;
using HostingQr.Infrastructure.Data;

namespace HostingQr.Infrastructure.Projects;

public sealed class ProjectViewRepository : IProjectViewRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProjectViewRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task IncrementAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into project_view_counts (project_id, total_views, last_viewed_at)
            values (@ProjectId, 1, now())
            on conflict (project_id) do update set
                total_views = project_view_counts.total_views + 1,
                last_viewed_at = now();
            """;

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { ProjectId = projectId }, cancellationToken: cancellationToken));
    }

    public async Task<ProjectViewStats?> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select
                project_id as ProjectId,
                total_views as TotalViews,
                last_viewed_at as LastViewedAt
            from project_view_counts
            where project_id = @ProjectId;
            """;

        using var connection = _connectionFactory.CreateConnection();
        ProjectViewStatsRow? row = await connection.QuerySingleOrDefaultAsync<ProjectViewStatsRow>(new CommandDefinition(sql, new { ProjectId = projectId }, cancellationToken: cancellationToken));
        return row?.ToStats();
    }

    public async Task<IReadOnlyDictionary<Guid, ProjectViewStats>> GetByProjectIdsAsync(IReadOnlyList<Guid> projectIds, CancellationToken cancellationToken = default)
    {
        if (projectIds.Count == 0)
        {
            return new Dictionary<Guid, ProjectViewStats>();
        }

        const string sql = """
            select
                project_id as ProjectId,
                total_views as TotalViews,
                last_viewed_at as LastViewedAt
            from project_view_counts
            where project_id = any(@ProjectIds);
            """;

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<ProjectViewStatsRow>(new CommandDefinition(sql, new { ProjectIds = projectIds.ToArray() }, cancellationToken: cancellationToken));
        return rows.ToDictionary(row => row.ProjectId, row => row.ToStats());
    }

    private sealed class ProjectViewStatsRow
    {
        public Guid ProjectId { get; init; }

        public long TotalViews { get; init; }

        public DateTime? LastViewedAt { get; init; }

        public ProjectViewStats ToStats() => new(
            ProjectId,
            TotalViews,
            LastViewedAt is null ? null : new DateTimeOffset(DateTime.SpecifyKind(LastViewedAt.Value, DateTimeKind.Utc)));
    }
}
