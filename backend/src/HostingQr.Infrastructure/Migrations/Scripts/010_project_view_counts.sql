create table if not exists project_view_counts (
    project_id uuid primary key references projects(id) on delete cascade,
    total_views bigint not null default 0,
    last_viewed_at timestamptz null
);

create index if not exists ix_project_view_counts_total_views on project_view_counts (total_views desc);
