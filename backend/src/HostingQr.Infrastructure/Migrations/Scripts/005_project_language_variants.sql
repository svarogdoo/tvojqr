update assets
set language_code = 'en'
where language_code = 'default';

create table if not exists project_language_variants (
    id uuid primary key,
    project_id uuid not null references projects(id) on delete cascade,
    language_code text not null,
    display_name text not null,
    is_default boolean not null default false,
    sort_order integer not null default 0,
    created_at timestamptz not null default now(),
    constraint uq_project_language_code unique (project_id, language_code)
);

create unique index if not exists ux_project_language_default
on project_language_variants(project_id)
where is_default = true;

insert into project_language_variants (id, project_id, language_code, display_name, is_default, sort_order)
select gen_random_uuid(), p.id, 'en', 'English', true, 0
from projects p
where not exists (
    select 1 from project_language_variants lv where lv.project_id = p.id
);
