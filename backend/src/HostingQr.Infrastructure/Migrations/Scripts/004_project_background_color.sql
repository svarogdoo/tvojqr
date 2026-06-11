alter table projects
add column if not exists background_color text not null default '#f8f7f3';
