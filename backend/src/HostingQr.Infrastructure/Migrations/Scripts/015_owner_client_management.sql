create table client_billing_profiles (
    user_id uuid primary key references users(id) on delete cascade,
    billing_cycle text not null,
    next_invoice_date date null,
    is_active boolean not null default true,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    constraint ck_client_billing_profiles_cycle check (billing_cycle in ('monthly', 'annual'))
);

create table invoices (
    id uuid primary key,
    client_user_id uuid not null references users(id) on delete cascade,
    uploaded_by_user_id uuid not null references users(id),
    invoice_date date not null,
    original_file_name text not null,
    content_type text not null,
    storage_key text not null unique,
    size_bytes bigint not null,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    constraint ck_invoices_pdf_content_type check (content_type = 'application/pdf'),
    constraint ck_invoices_size check (size_bytes > 0)
);

create index ix_invoices_client_date on invoices (client_user_id, invoice_date desc, created_at desc);

create table project_invitations (
    id uuid primary key,
    project_id uuid not null references projects(id) on delete cascade,
    inviter_user_id uuid not null references users(id),
    invited_email_normalized text not null,
    token_hash text not null unique,
    expires_at timestamptz not null,
    accepted_at timestamptz null,
    revoked_at timestamptz null,
    created_at timestamptz not null default now(),
    constraint ck_project_invitations_email_normalized check (
        invited_email_normalized = lower(trim(invited_email_normalized))
        and invited_email_normalized <> ''
    ),
    constraint ck_project_invitations_expiry check (expires_at > created_at),
    constraint ck_project_invitations_terminal_state check (accepted_at is null or revoked_at is null)
);

create index ix_project_invitations_project_active
    on project_invitations (project_id, expires_at)
    where accepted_at is null and revoked_at is null;

create index ix_project_invitations_email_active
    on project_invitations (invited_email_normalized, expires_at)
    where accepted_at is null and revoked_at is null;
