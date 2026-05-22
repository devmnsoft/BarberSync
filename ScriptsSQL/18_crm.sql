create table if not exists client_segments (id uuid primary key, tenant_id uuid not null, branch_id uuid, name text not null, rules_json jsonb);
create table if not exists client_relationship_events (id uuid primary key, tenant_id uuid not null, client_id uuid not null, event_type text not null, event_date timestamptz not null, metadata jsonb);
create table if not exists client_reactivation_campaigns (id uuid primary key, tenant_id uuid not null, segment_id uuid, name text not null, starts_at date, ends_at date, status text);
create table if not exists client_lifecycle_status (id uuid primary key, tenant_id uuid not null, client_id uuid not null, status text not null, days_without_visit int not null);
create table if not exists client_commercial_scores (id uuid primary key, tenant_id uuid not null, client_id uuid not null, score int not null, lifetime_value numeric(12,2), updated_at timestamptz default now());
