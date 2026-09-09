alter table projects
add column if not exists menu_type text not null default 'image';

alter table projects
add column if not exists time_zone text not null default 'UTC';

do $$
begin
    if not exists (
        select 1 from pg_constraint where conname = 'ck_projects_menu_type'
    ) then
        alter table projects
        add constraint ck_projects_menu_type check (menu_type in ('image', 'digital'));
    end if;
end $$;

create table if not exists menu_categories (
    id uuid primary key,
    project_id uuid not null references projects(id) on delete cascade,
    sort_order integer not null default 0
);

create table if not exists menu_category_translations (
    category_id uuid not null references menu_categories(id) on delete cascade,
    language_variant_id uuid not null references project_language_variants(id) on delete cascade,
    name text not null,
    primary key (category_id, language_variant_id)
);

create table if not exists menu_category_schedules (
    id uuid primary key,
    category_id uuid not null references menu_categories(id) on delete cascade,
    day_of_week smallint not null check (day_of_week between 0 and 6),
    starts_at time not null,
    ends_at time not null,
    constraint uq_menu_category_schedule unique (category_id, day_of_week, starts_at, ends_at)
);

create table if not exists menu_items (
    id uuid primary key,
    category_id uuid not null references menu_categories(id) on delete cascade,
    price_text text not null default '',
    is_out_of_stock boolean not null default false,
    sort_order integer not null default 0
);

create table if not exists menu_item_translations (
    item_id uuid not null references menu_items(id) on delete cascade,
    language_variant_id uuid not null references project_language_variants(id) on delete cascade,
    name text not null,
    description text not null default '',
    primary key (item_id, language_variant_id)
);

create index if not exists ix_menu_categories_project on menu_categories(project_id, sort_order);
create index if not exists ix_menu_items_category on menu_items(category_id, sort_order);
create index if not exists ix_menu_category_schedules_category on menu_category_schedules(category_id);
