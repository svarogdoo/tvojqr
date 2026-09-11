alter table projects
add column if not exists time_zone_configured boolean not null default false;

update projects
set time_zone_configured = true
where time_zone <> 'UTC';
