alter table projects
add column if not exists currency_code text not null default 'EUR';

do $$
begin
    if not exists (
        select 1 from pg_constraint where conname = 'ck_projects_currency_code'
    ) then
        alter table projects
        add constraint ck_projects_currency_code check (currency_code ~ '^[A-Z]{3}$');
    end if;
end $$;
