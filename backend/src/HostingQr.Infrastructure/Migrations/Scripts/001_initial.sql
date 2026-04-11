create table if not exists users (
    id uuid primary key,
    email text not null unique,
    display_name text not null,
    created_at timestamptz not null default now()
);

create table if not exists projects (
    id uuid primary key,
    owner_user_id uuid not null references users(id),
    name text not null,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);

create table if not exists slugs (
    id uuid primary key,
    project_id uuid not null references projects(id) on delete cascade,
    slug text not null unique,
    is_primary boolean not null default true,
    created_at timestamptz not null default now(),
    constraint uq_slugs_project_primary unique (project_id)
);

insert into users (id, email, display_name)
values ('${DevelopmentUserId}', '${DevelopmentUserEmail}', '${DevelopmentUserDisplayName}')
on conflict (id) do update set
    email = excluded.email,
    display_name = excluded.display_name;
