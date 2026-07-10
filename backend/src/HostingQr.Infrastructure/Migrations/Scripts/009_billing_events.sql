create table if not exists billing_events (
    id uuid primary key default gen_random_uuid(),
    provider text not null,
    provider_event_id text not null,
    event_type text not null,
    user_id uuid null references users(id) on delete set null,
    tier text null,
    subscription_id text null,
    order_id text null,
    customer_id text null,
    processed_action text not null,
    entitlement_active boolean null,
    entitlement_ends_at timestamptz null,
    raw_payload jsonb not null,
    received_at timestamptz not null default now(),
    processed_at timestamptz not null default now(),
    constraint uq_billing_events_provider_event unique (provider, provider_event_id)
);

create index if not exists ix_billing_events_user_id on billing_events (user_id);
create index if not exists ix_billing_events_event_type on billing_events (event_type);
create index if not exists ix_billing_events_received_at on billing_events (received_at desc);
