CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE SCHEMA IF NOT EXISTS barber;

CREATE TABLE IF NOT EXISTS barber.tenants (
  id uuid primary key default gen_random_uuid(), tenant_id uuid not null default gen_random_uuid(), branch_id uuid null,
  slug varchar(120) unique not null, name varchar(160) not null,
  created_at timestamptz not null default now(), updated_at timestamptz null, created_by uuid null, updated_by uuid null,
  status varchar(30) not null default 'ACTIVE', is_deleted boolean not null default false
);

DO $$ DECLARE t text; BEGIN
  FOREACH t IN ARRAY ARRAY['companies','branches','company_branding','public_site_settings','public_site_sections','public_site_banners','public_site_social_links','public_booking_settings','public_booking_services','public_booking_professionals','kiosk_settings','kiosk_devices','kiosk_service_settings','kiosk_payment_settings','kiosk_messages','kiosk_accessibility_settings','mobile_app_settings','tenant_theme_settings','tenant_terms','tenant_privacy_policies']
  LOOP
    EXECUTE format('CREATE TABLE IF NOT EXISTS barber.%I (id uuid primary key default gen_random_uuid(), tenant_id uuid not null, branch_id uuid null, payload jsonb not null default ''{}''::jsonb, created_at timestamptz not null default now(), updated_at timestamptz null, created_by uuid null, updated_by uuid null, status varchar(30) not null default ''ACTIVE'', is_deleted boolean not null default false);', t);
  END LOOP;
END $$;

INSERT INTO barber.tenants (tenant_id,slug,name,status)
VALUES (gen_random_uuid(),'barbearia-elite-demo','Barbearia Elite Demo','ACTIVE')
ON CONFLICT (slug) DO NOTHING;
