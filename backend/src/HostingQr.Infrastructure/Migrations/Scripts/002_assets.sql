create table if not exists assets (
    id uuid primary key,
    project_id uuid not null references projects(id) on delete cascade,
    language_code text not null,
    original_file_name text not null,
    stored_file_name text not null unique,
    content_type text not null,
    size_bytes bigint not null,
    sort_order integer not null default 0,
    created_at timestamptz not null default now()
);
