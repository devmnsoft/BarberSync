-- Commercial platform expansion (white label, marketplace, support, analytics, exports)
create table if not exists tenant_branding (
  tenant_id uuid primary key,
  commercial_name text not null,
  logo_url text,
  slogan text,
  primary_color text,
  secondary_color text,
  theme_mode text not null default 'light',
  cover_image_url text,
  app_name text,
  kiosk_welcome_text text,
  public_booking_title text
);
create table if not exists tenant_domains (
  tenant_id uuid primary key,
  subdomain text not null unique,
  custom_domain text,
  verified boolean not null default false
);
create table if not exists tenant_public_pages (
  tenant_id uuid primary key,
  slug text not null unique,
  headline text,
  description text,
  enable_coupons boolean not null default true,
  enable_cashback boolean not null default true
);
create table if not exists marketplace_profiles (
  tenant_id uuid primary key,
  tenant_slug text not null,
  business_name text not null,
  city text,
  district text,
  rating numeric(3,2) default 0,
  min_price numeric(10,2) default 0,
  featured boolean not null default false
);
create table if not exists support_tickets (
  id uuid primary key,
  tenant_id uuid not null,
  subject text not null,
  category text not null,
  priority text not null,
  status text not null,
  opened_at timestamptz not null default now()
);
create table if not exists product_events (
  id uuid primary key,
  tenant_id uuid not null,
  event_name text not null,
  occurred_at timestamptz not null default now()
);
create table if not exists export_jobs (
  id uuid primary key,
  tenant_id uuid not null,
  resource text not null,
  status text not null,
  requested_at timestamptz not null default now(),
  finished_at timestamptz
);
