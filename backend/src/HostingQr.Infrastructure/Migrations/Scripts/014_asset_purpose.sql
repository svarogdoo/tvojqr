alter table assets
    add column if not exists purpose text not null default 'menu_content';

alter table assets
    drop constraint if exists assets_purpose_check;

alter table assets
    add constraint assets_purpose_check
    check (purpose in ('menu_content', 'digital_menu_cover'));

create index if not exists ix_assets_project_purpose
    on assets (project_id, purpose);
