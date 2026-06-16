create table if not exists user_entitlements (
    user_id uuid primary key references users(id) on delete cascade,
    tier text not null,
    is_active boolean not null default true,
    granted_manually boolean not null default false,
    starts_at timestamptz not null default now(),
    ends_at timestamptz null,
    updated_at timestamptz not null default now(),
    constraint ck_user_entitlements_tier check (tier in ('free', 'standard', 'world_cup', 'plus'))
);

create index if not exists ix_user_entitlements_active on user_entitlements (user_id, is_active);
