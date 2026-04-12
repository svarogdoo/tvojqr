alter table projects
    add column if not exists status text not null default 'active';

update projects
set status = 'active'
where status is null;
