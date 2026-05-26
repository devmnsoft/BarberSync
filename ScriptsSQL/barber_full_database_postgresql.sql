CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE EXTENSION IF NOT EXISTS citext;

CREATE SCHEMA IF NOT EXISTS barber;
SET search_path TO barber, public;

CREATE OR REPLACE FUNCTION barber.set_updated_at() RETURNS trigger AS $$ BEGIN NEW.updated_at = now(); RETURN NEW; END; $$ LANGUAGE plpgsql;
CREATE OR REPLACE FUNCTION barber.generate_slug(input text) RETURNS text AS $$ SELECT lower(regexp_replace(trim(input), '[^a-zA-Z0-9]+', '-', 'g')); $$ LANGUAGE sql IMMUTABLE;
CREATE OR REPLACE FUNCTION barber.normalize_document(input text) RETURNS text AS $$ SELECT regexp_replace(coalesce(input,''), '[^0-9]', '', 'g'); $$ LANGUAGE sql IMMUTABLE;
CREATE OR REPLACE FUNCTION barber.calculate_audit_hash() RETURNS trigger AS $$ BEGIN NEW.hash = encode(digest(coalesce(NEW.payload::text,''),'sha256'),'hex'); RETURN NEW; END; $$ LANGUAGE plpgsql;
CREATE OR REPLACE FUNCTION barber.verify_audit_chain() RETURNS boolean AS $$ SELECT true; $$ LANGUAGE sql;
CREATE OR REPLACE FUNCTION barber.calculate_commission(amount numeric, pct numeric) RETURNS numeric AS $$ SELECT round((amount * pct) / 100.0,2); $$ LANGUAGE sql IMMUTABLE;
CREATE OR REPLACE FUNCTION barber.calculate_cashback(amount numeric, pct numeric) RETURNS numeric AS $$ SELECT round((amount * pct) / 100.0,2); $$ LANGUAGE sql IMMUTABLE;
CREATE OR REPLACE FUNCTION barber.check_module_active(p_tenant_id uuid, p_module text) RETURNS boolean AS $$ SELECT true; $$ LANGUAGE sql;
CREATE OR REPLACE FUNCTION barber.has_permission(p_user_id uuid, p_permission text) RETURNS boolean AS $$ SELECT true; $$ LANGUAGE sql;
CREATE OR REPLACE FUNCTION barber.write_audit_log() RETURNS trigger AS $$ BEGIN INSERT INTO barber.audit_logs(entity_name, entity_id, action, payload) VALUES (TG_TABLE_NAME, coalesce(NEW.id, OLD.id), TG_OP, to_jsonb(coalesce(NEW, OLD))); RETURN coalesce(NEW, OLD); END; $$ LANGUAGE plpgsql;
CREATE OR REPLACE FUNCTION barber.update_stock_balance() RETURNS trigger AS $$ BEGIN RETURN NEW; END; $$ LANGUAGE plpgsql;
CREATE OR REPLACE FUNCTION barber.update_wallet_balance() RETURNS trigger AS $$ BEGIN RETURN NEW; END; $$ LANGUAGE plpgsql;

CREATE TABLE IF NOT EXISTS barber.tenants (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_tenants_tenant_id ON barber.tenants(tenant_id);
CREATE INDEX IF NOT EXISTS idx_tenants_branch_id ON barber.tenants(branch_id);
CREATE INDEX IF NOT EXISTS idx_tenants_status ON barber.tenants(status);
CREATE INDEX IF NOT EXISTS idx_tenants_created_at ON barber.tenants(created_at);
CREATE INDEX IF NOT EXISTS idx_tenants_tenant_status ON barber.tenants(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_tenants_tenant_branch ON barber.tenants(tenant_id, branch_id);
CREATE TRIGGER trg_tenants_updated_at BEFORE UPDATE ON barber.tenants FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.companies (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_companies_tenant_id ON barber.companies(tenant_id);
CREATE INDEX IF NOT EXISTS idx_companies_branch_id ON barber.companies(branch_id);
CREATE INDEX IF NOT EXISTS idx_companies_status ON barber.companies(status);
CREATE INDEX IF NOT EXISTS idx_companies_created_at ON barber.companies(created_at);
CREATE INDEX IF NOT EXISTS idx_companies_tenant_status ON barber.companies(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_companies_tenant_branch ON barber.companies(tenant_id, branch_id);
CREATE TRIGGER trg_companies_updated_at BEFORE UPDATE ON barber.companies FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.branches (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_branches_tenant_id ON barber.branches(tenant_id);
CREATE INDEX IF NOT EXISTS idx_branches_branch_id ON barber.branches(branch_id);
CREATE INDEX IF NOT EXISTS idx_branches_status ON barber.branches(status);
CREATE INDEX IF NOT EXISTS idx_branches_created_at ON barber.branches(created_at);
CREATE INDEX IF NOT EXISTS idx_branches_tenant_status ON barber.branches(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_branches_tenant_branch ON barber.branches(tenant_id, branch_id);
CREATE TRIGGER trg_branches_updated_at BEFORE UPDATE ON barber.branches FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.tenant_settings (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_tenant_settings_tenant_id ON barber.tenant_settings(tenant_id);
CREATE INDEX IF NOT EXISTS idx_tenant_settings_branch_id ON barber.tenant_settings(branch_id);
CREATE INDEX IF NOT EXISTS idx_tenant_settings_status ON barber.tenant_settings(status);
CREATE INDEX IF NOT EXISTS idx_tenant_settings_created_at ON barber.tenant_settings(created_at);
CREATE INDEX IF NOT EXISTS idx_tenant_settings_tenant_status ON barber.tenant_settings(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_tenant_settings_tenant_branch ON barber.tenant_settings(tenant_id, branch_id);
CREATE TRIGGER trg_tenant_settings_updated_at BEFORE UPDATE ON barber.tenant_settings FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.company_branding (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_company_branding_tenant_id ON barber.company_branding(tenant_id);
CREATE INDEX IF NOT EXISTS idx_company_branding_branch_id ON barber.company_branding(branch_id);
CREATE INDEX IF NOT EXISTS idx_company_branding_status ON barber.company_branding(status);
CREATE INDEX IF NOT EXISTS idx_company_branding_created_at ON barber.company_branding(created_at);
CREATE INDEX IF NOT EXISTS idx_company_branding_tenant_status ON barber.company_branding(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_company_branding_tenant_branch ON barber.company_branding(tenant_id, branch_id);
CREATE TRIGGER trg_company_branding_updated_at BEFORE UPDATE ON barber.company_branding FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.subscription_plans (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_subscription_plans_tenant_id ON barber.subscription_plans(tenant_id);
CREATE INDEX IF NOT EXISTS idx_subscription_plans_branch_id ON barber.subscription_plans(branch_id);
CREATE INDEX IF NOT EXISTS idx_subscription_plans_status ON barber.subscription_plans(status);
CREATE INDEX IF NOT EXISTS idx_subscription_plans_created_at ON barber.subscription_plans(created_at);
CREATE INDEX IF NOT EXISTS idx_subscription_plans_tenant_status ON barber.subscription_plans(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_subscription_plans_tenant_branch ON barber.subscription_plans(tenant_id, branch_id);
CREATE TRIGGER trg_subscription_plans_updated_at BEFORE UPDATE ON barber.subscription_plans FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.subscription_features (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_subscription_features_tenant_id ON barber.subscription_features(tenant_id);
CREATE INDEX IF NOT EXISTS idx_subscription_features_branch_id ON barber.subscription_features(branch_id);
CREATE INDEX IF NOT EXISTS idx_subscription_features_status ON barber.subscription_features(status);
CREATE INDEX IF NOT EXISTS idx_subscription_features_created_at ON barber.subscription_features(created_at);
CREATE INDEX IF NOT EXISTS idx_subscription_features_tenant_status ON barber.subscription_features(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_subscription_features_tenant_branch ON barber.subscription_features(tenant_id, branch_id);
CREATE TRIGGER trg_subscription_features_updated_at BEFORE UPDATE ON barber.subscription_features FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.subscriptions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_subscriptions_tenant_id ON barber.subscriptions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_subscriptions_branch_id ON barber.subscriptions(branch_id);
CREATE INDEX IF NOT EXISTS idx_subscriptions_status ON barber.subscriptions(status);
CREATE INDEX IF NOT EXISTS idx_subscriptions_created_at ON barber.subscriptions(created_at);
CREATE INDEX IF NOT EXISTS idx_subscriptions_tenant_status ON barber.subscriptions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_subscriptions_tenant_branch ON barber.subscriptions(tenant_id, branch_id);
CREATE TRIGGER trg_subscriptions_updated_at BEFORE UPDATE ON barber.subscriptions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.invoices (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_invoices_tenant_id ON barber.invoices(tenant_id);
CREATE INDEX IF NOT EXISTS idx_invoices_branch_id ON barber.invoices(branch_id);
CREATE INDEX IF NOT EXISTS idx_invoices_status ON barber.invoices(status);
CREATE INDEX IF NOT EXISTS idx_invoices_created_at ON barber.invoices(created_at);
CREATE INDEX IF NOT EXISTS idx_invoices_tenant_status ON barber.invoices(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_invoices_tenant_branch ON barber.invoices(tenant_id, branch_id);
CREATE TRIGGER trg_invoices_updated_at BEFORE UPDATE ON barber.invoices FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.tenant_usage (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_tenant_usage_tenant_id ON barber.tenant_usage(tenant_id);
CREATE INDEX IF NOT EXISTS idx_tenant_usage_branch_id ON barber.tenant_usage(branch_id);
CREATE INDEX IF NOT EXISTS idx_tenant_usage_status ON barber.tenant_usage(status);
CREATE INDEX IF NOT EXISTS idx_tenant_usage_created_at ON barber.tenant_usage(created_at);
CREATE INDEX IF NOT EXISTS idx_tenant_usage_tenant_status ON barber.tenant_usage(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_tenant_usage_tenant_branch ON barber.tenant_usage(tenant_id, branch_id);
CREATE TRIGGER trg_tenant_usage_updated_at BEFORE UPDATE ON barber.tenant_usage FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.platform_modules (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_platform_modules_tenant_id ON barber.platform_modules(tenant_id);
CREATE INDEX IF NOT EXISTS idx_platform_modules_branch_id ON barber.platform_modules(branch_id);
CREATE INDEX IF NOT EXISTS idx_platform_modules_status ON barber.platform_modules(status);
CREATE INDEX IF NOT EXISTS idx_platform_modules_created_at ON barber.platform_modules(created_at);
CREATE INDEX IF NOT EXISTS idx_platform_modules_tenant_status ON barber.platform_modules(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_platform_modules_tenant_branch ON barber.platform_modules(tenant_id, branch_id);
CREATE TRIGGER trg_platform_modules_updated_at BEFORE UPDATE ON barber.platform_modules FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.tenant_modules (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_tenant_modules_tenant_id ON barber.tenant_modules(tenant_id);
CREATE INDEX IF NOT EXISTS idx_tenant_modules_branch_id ON barber.tenant_modules(branch_id);
CREATE INDEX IF NOT EXISTS idx_tenant_modules_status ON barber.tenant_modules(status);
CREATE INDEX IF NOT EXISTS idx_tenant_modules_created_at ON barber.tenant_modules(created_at);
CREATE INDEX IF NOT EXISTS idx_tenant_modules_tenant_status ON barber.tenant_modules(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_tenant_modules_tenant_branch ON barber.tenant_modules(tenant_id, branch_id);
CREATE TRIGGER trg_tenant_modules_updated_at BEFORE UPDATE ON barber.tenant_modules FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.feature_flags (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_feature_flags_tenant_id ON barber.feature_flags(tenant_id);
CREATE INDEX IF NOT EXISTS idx_feature_flags_branch_id ON barber.feature_flags(branch_id);
CREATE INDEX IF NOT EXISTS idx_feature_flags_status ON barber.feature_flags(status);
CREATE INDEX IF NOT EXISTS idx_feature_flags_created_at ON barber.feature_flags(created_at);
CREATE INDEX IF NOT EXISTS idx_feature_flags_tenant_status ON barber.feature_flags(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_feature_flags_tenant_branch ON barber.feature_flags(tenant_id, branch_id);
CREATE TRIGGER trg_feature_flags_updated_at BEFORE UPDATE ON barber.feature_flags FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.users (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_users_tenant_id ON barber.users(tenant_id);
CREATE INDEX IF NOT EXISTS idx_users_branch_id ON barber.users(branch_id);
CREATE INDEX IF NOT EXISTS idx_users_status ON barber.users(status);
CREATE INDEX IF NOT EXISTS idx_users_created_at ON barber.users(created_at);
CREATE INDEX IF NOT EXISTS idx_users_tenant_status ON barber.users(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_users_tenant_branch ON barber.users(tenant_id, branch_id);
CREATE TRIGGER trg_users_updated_at BEFORE UPDATE ON barber.users FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.roles (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_roles_tenant_id ON barber.roles(tenant_id);
CREATE INDEX IF NOT EXISTS idx_roles_branch_id ON barber.roles(branch_id);
CREATE INDEX IF NOT EXISTS idx_roles_status ON barber.roles(status);
CREATE INDEX IF NOT EXISTS idx_roles_created_at ON barber.roles(created_at);
CREATE INDEX IF NOT EXISTS idx_roles_tenant_status ON barber.roles(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_roles_tenant_branch ON barber.roles(tenant_id, branch_id);
CREATE TRIGGER trg_roles_updated_at BEFORE UPDATE ON barber.roles FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.permissions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_permissions_tenant_id ON barber.permissions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_permissions_branch_id ON barber.permissions(branch_id);
CREATE INDEX IF NOT EXISTS idx_permissions_status ON barber.permissions(status);
CREATE INDEX IF NOT EXISTS idx_permissions_created_at ON barber.permissions(created_at);
CREATE INDEX IF NOT EXISTS idx_permissions_tenant_status ON barber.permissions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_permissions_tenant_branch ON barber.permissions(tenant_id, branch_id);
CREATE TRIGGER trg_permissions_updated_at BEFORE UPDATE ON barber.permissions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.user_roles (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_user_roles_tenant_id ON barber.user_roles(tenant_id);
CREATE INDEX IF NOT EXISTS idx_user_roles_branch_id ON barber.user_roles(branch_id);
CREATE INDEX IF NOT EXISTS idx_user_roles_status ON barber.user_roles(status);
CREATE INDEX IF NOT EXISTS idx_user_roles_created_at ON barber.user_roles(created_at);
CREATE INDEX IF NOT EXISTS idx_user_roles_tenant_status ON barber.user_roles(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_user_roles_tenant_branch ON barber.user_roles(tenant_id, branch_id);
CREATE TRIGGER trg_user_roles_updated_at BEFORE UPDATE ON barber.user_roles FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.role_permissions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_role_permissions_tenant_id ON barber.role_permissions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_role_permissions_branch_id ON barber.role_permissions(branch_id);
CREATE INDEX IF NOT EXISTS idx_role_permissions_status ON barber.role_permissions(status);
CREATE INDEX IF NOT EXISTS idx_role_permissions_created_at ON barber.role_permissions(created_at);
CREATE INDEX IF NOT EXISTS idx_role_permissions_tenant_status ON barber.role_permissions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_role_permissions_tenant_branch ON barber.role_permissions(tenant_id, branch_id);
CREATE TRIGGER trg_role_permissions_updated_at BEFORE UPDATE ON barber.role_permissions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.user_permission_overrides (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_user_permission_overrides_tenant_id ON barber.user_permission_overrides(tenant_id);
CREATE INDEX IF NOT EXISTS idx_user_permission_overrides_branch_id ON barber.user_permission_overrides(branch_id);
CREATE INDEX IF NOT EXISTS idx_user_permission_overrides_status ON barber.user_permission_overrides(status);
CREATE INDEX IF NOT EXISTS idx_user_permission_overrides_created_at ON barber.user_permission_overrides(created_at);
CREATE INDEX IF NOT EXISTS idx_user_permission_overrides_tenant_status ON barber.user_permission_overrides(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_user_permission_overrides_tenant_branch ON barber.user_permission_overrides(tenant_id, branch_id);
CREATE TRIGGER trg_user_permission_overrides_updated_at BEFORE UPDATE ON barber.user_permission_overrides FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.refresh_tokens (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_tenant_id ON barber.refresh_tokens(tenant_id);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_branch_id ON barber.refresh_tokens(branch_id);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_status ON barber.refresh_tokens(status);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_created_at ON barber.refresh_tokens(created_at);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_tenant_status ON barber.refresh_tokens(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_tenant_branch ON barber.refresh_tokens(tenant_id, branch_id);
CREATE TRIGGER trg_refresh_tokens_updated_at BEFORE UPDATE ON barber.refresh_tokens FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.login_attempts (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_login_attempts_tenant_id ON barber.login_attempts(tenant_id);
CREATE INDEX IF NOT EXISTS idx_login_attempts_branch_id ON barber.login_attempts(branch_id);
CREATE INDEX IF NOT EXISTS idx_login_attempts_status ON barber.login_attempts(status);
CREATE INDEX IF NOT EXISTS idx_login_attempts_created_at ON barber.login_attempts(created_at);
CREATE INDEX IF NOT EXISTS idx_login_attempts_tenant_status ON barber.login_attempts(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_login_attempts_tenant_branch ON barber.login_attempts(tenant_id, branch_id);
CREATE TRIGGER trg_login_attempts_updated_at BEFORE UPDATE ON barber.login_attempts FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.password_reset_tokens (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_password_reset_tokens_tenant_id ON barber.password_reset_tokens(tenant_id);
CREATE INDEX IF NOT EXISTS idx_password_reset_tokens_branch_id ON barber.password_reset_tokens(branch_id);
CREATE INDEX IF NOT EXISTS idx_password_reset_tokens_status ON barber.password_reset_tokens(status);
CREATE INDEX IF NOT EXISTS idx_password_reset_tokens_created_at ON barber.password_reset_tokens(created_at);
CREATE INDEX IF NOT EXISTS idx_password_reset_tokens_tenant_status ON barber.password_reset_tokens(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_password_reset_tokens_tenant_branch ON barber.password_reset_tokens(tenant_id, branch_id);
CREATE TRIGGER trg_password_reset_tokens_updated_at BEFORE UPDATE ON barber.password_reset_tokens FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.mfa_tokens (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_mfa_tokens_tenant_id ON barber.mfa_tokens(tenant_id);
CREATE INDEX IF NOT EXISTS idx_mfa_tokens_branch_id ON barber.mfa_tokens(branch_id);
CREATE INDEX IF NOT EXISTS idx_mfa_tokens_status ON barber.mfa_tokens(status);
CREATE INDEX IF NOT EXISTS idx_mfa_tokens_created_at ON barber.mfa_tokens(created_at);
CREATE INDEX IF NOT EXISTS idx_mfa_tokens_tenant_status ON barber.mfa_tokens(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_mfa_tokens_tenant_branch ON barber.mfa_tokens(tenant_id, branch_id);
CREATE TRIGGER trg_mfa_tokens_updated_at BEFORE UPDATE ON barber.mfa_tokens FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.active_sessions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_active_sessions_tenant_id ON barber.active_sessions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_active_sessions_branch_id ON barber.active_sessions(branch_id);
CREATE INDEX IF NOT EXISTS idx_active_sessions_status ON barber.active_sessions(status);
CREATE INDEX IF NOT EXISTS idx_active_sessions_created_at ON barber.active_sessions(created_at);
CREATE INDEX IF NOT EXISTS idx_active_sessions_tenant_status ON barber.active_sessions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_active_sessions_tenant_branch ON barber.active_sessions(tenant_id, branch_id);
CREATE TRIGGER trg_active_sessions_updated_at BEFORE UPDATE ON barber.active_sessions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.api_keys (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_api_keys_tenant_id ON barber.api_keys(tenant_id);
CREATE INDEX IF NOT EXISTS idx_api_keys_branch_id ON barber.api_keys(branch_id);
CREATE INDEX IF NOT EXISTS idx_api_keys_status ON barber.api_keys(status);
CREATE INDEX IF NOT EXISTS idx_api_keys_created_at ON barber.api_keys(created_at);
CREATE INDEX IF NOT EXISTS idx_api_keys_tenant_status ON barber.api_keys(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_api_keys_tenant_branch ON barber.api_keys(tenant_id, branch_id);
CREATE TRIGGER trg_api_keys_updated_at BEFORE UPDATE ON barber.api_keys FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.clients (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_clients_tenant_id ON barber.clients(tenant_id);
CREATE INDEX IF NOT EXISTS idx_clients_branch_id ON barber.clients(branch_id);
CREATE INDEX IF NOT EXISTS idx_clients_status ON barber.clients(status);
CREATE INDEX IF NOT EXISTS idx_clients_created_at ON barber.clients(created_at);
CREATE INDEX IF NOT EXISTS idx_clients_tenant_status ON barber.clients(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_clients_tenant_branch ON barber.clients(tenant_id, branch_id);
CREATE TRIGGER trg_clients_updated_at BEFORE UPDATE ON barber.clients FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.client_profiles (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_client_profiles_tenant_id ON barber.client_profiles(tenant_id);
CREATE INDEX IF NOT EXISTS idx_client_profiles_branch_id ON barber.client_profiles(branch_id);
CREATE INDEX IF NOT EXISTS idx_client_profiles_status ON barber.client_profiles(status);
CREATE INDEX IF NOT EXISTS idx_client_profiles_created_at ON barber.client_profiles(created_at);
CREATE INDEX IF NOT EXISTS idx_client_profiles_tenant_status ON barber.client_profiles(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_client_profiles_tenant_branch ON barber.client_profiles(tenant_id, branch_id);
CREATE TRIGGER trg_client_profiles_updated_at BEFORE UPDATE ON barber.client_profiles FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.client_documents (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_client_documents_tenant_id ON barber.client_documents(tenant_id);
CREATE INDEX IF NOT EXISTS idx_client_documents_branch_id ON barber.client_documents(branch_id);
CREATE INDEX IF NOT EXISTS idx_client_documents_status ON barber.client_documents(status);
CREATE INDEX IF NOT EXISTS idx_client_documents_created_at ON barber.client_documents(created_at);
CREATE INDEX IF NOT EXISTS idx_client_documents_tenant_status ON barber.client_documents(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_client_documents_tenant_branch ON barber.client_documents(tenant_id, branch_id);
CREATE TRIGGER trg_client_documents_updated_at BEFORE UPDATE ON barber.client_documents FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.client_preferences (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_client_preferences_tenant_id ON barber.client_preferences(tenant_id);
CREATE INDEX IF NOT EXISTS idx_client_preferences_branch_id ON barber.client_preferences(branch_id);
CREATE INDEX IF NOT EXISTS idx_client_preferences_status ON barber.client_preferences(status);
CREATE INDEX IF NOT EXISTS idx_client_preferences_created_at ON barber.client_preferences(created_at);
CREATE INDEX IF NOT EXISTS idx_client_preferences_tenant_status ON barber.client_preferences(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_client_preferences_tenant_branch ON barber.client_preferences(tenant_id, branch_id);
CREATE TRIGGER trg_client_preferences_updated_at BEFORE UPDATE ON barber.client_preferences FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.customer_wallets (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_customer_wallets_tenant_id ON barber.customer_wallets(tenant_id);
CREATE INDEX IF NOT EXISTS idx_customer_wallets_branch_id ON barber.customer_wallets(branch_id);
CREATE INDEX IF NOT EXISTS idx_customer_wallets_status ON barber.customer_wallets(status);
CREATE INDEX IF NOT EXISTS idx_customer_wallets_created_at ON barber.customer_wallets(created_at);
CREATE INDEX IF NOT EXISTS idx_customer_wallets_tenant_status ON barber.customer_wallets(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_customer_wallets_tenant_branch ON barber.customer_wallets(tenant_id, branch_id);
CREATE TRIGGER trg_customer_wallets_updated_at BEFORE UPDATE ON barber.customer_wallets FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.wallet_transactions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_wallet_transactions_tenant_id ON barber.wallet_transactions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_wallet_transactions_branch_id ON barber.wallet_transactions(branch_id);
CREATE INDEX IF NOT EXISTS idx_wallet_transactions_status ON barber.wallet_transactions(status);
CREATE INDEX IF NOT EXISTS idx_wallet_transactions_created_at ON barber.wallet_transactions(created_at);
CREATE INDEX IF NOT EXISTS idx_wallet_transactions_tenant_status ON barber.wallet_transactions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_wallet_transactions_tenant_branch ON barber.wallet_transactions(tenant_id, branch_id);
CREATE TRIGGER trg_wallet_transactions_updated_at BEFORE UPDATE ON barber.wallet_transactions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.family_accounts (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_family_accounts_tenant_id ON barber.family_accounts(tenant_id);
CREATE INDEX IF NOT EXISTS idx_family_accounts_branch_id ON barber.family_accounts(branch_id);
CREATE INDEX IF NOT EXISTS idx_family_accounts_status ON barber.family_accounts(status);
CREATE INDEX IF NOT EXISTS idx_family_accounts_created_at ON barber.family_accounts(created_at);
CREATE INDEX IF NOT EXISTS idx_family_accounts_tenant_status ON barber.family_accounts(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_family_accounts_tenant_branch ON barber.family_accounts(tenant_id, branch_id);
CREATE TRIGGER trg_family_accounts_updated_at BEFORE UPDATE ON barber.family_accounts FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.family_members (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_family_members_tenant_id ON barber.family_members(tenant_id);
CREATE INDEX IF NOT EXISTS idx_family_members_branch_id ON barber.family_members(branch_id);
CREATE INDEX IF NOT EXISTS idx_family_members_status ON barber.family_members(status);
CREATE INDEX IF NOT EXISTS idx_family_members_created_at ON barber.family_members(created_at);
CREATE INDEX IF NOT EXISTS idx_family_members_tenant_status ON barber.family_members(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_family_members_tenant_branch ON barber.family_members(tenant_id, branch_id);
CREATE TRIGGER trg_family_members_updated_at BEFORE UPDATE ON barber.family_members FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.customer_subscriptions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_customer_subscriptions_tenant_id ON barber.customer_subscriptions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_customer_subscriptions_branch_id ON barber.customer_subscriptions(branch_id);
CREATE INDEX IF NOT EXISTS idx_customer_subscriptions_status ON barber.customer_subscriptions(status);
CREATE INDEX IF NOT EXISTS idx_customer_subscriptions_created_at ON barber.customer_subscriptions(created_at);
CREATE INDEX IF NOT EXISTS idx_customer_subscriptions_tenant_status ON barber.customer_subscriptions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_customer_subscriptions_tenant_branch ON barber.customer_subscriptions(tenant_id, branch_id);
CREATE TRIGGER trg_customer_subscriptions_updated_at BEFORE UPDATE ON barber.customer_subscriptions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.customer_subscription_plans (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_customer_subscription_plans_tenant_id ON barber.customer_subscription_plans(tenant_id);
CREATE INDEX IF NOT EXISTS idx_customer_subscription_plans_branch_id ON barber.customer_subscription_plans(branch_id);
CREATE INDEX IF NOT EXISTS idx_customer_subscription_plans_status ON barber.customer_subscription_plans(status);
CREATE INDEX IF NOT EXISTS idx_customer_subscription_plans_created_at ON barber.customer_subscription_plans(created_at);
CREATE INDEX IF NOT EXISTS idx_customer_subscription_plans_tenant_status ON barber.customer_subscription_plans(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_customer_subscription_plans_tenant_branch ON barber.customer_subscription_plans(tenant_id, branch_id);
CREATE TRIGGER trg_customer_subscription_plans_updated_at BEFORE UPDATE ON barber.customer_subscription_plans FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.benefit_clubs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_benefit_clubs_tenant_id ON barber.benefit_clubs(tenant_id);
CREATE INDEX IF NOT EXISTS idx_benefit_clubs_branch_id ON barber.benefit_clubs(branch_id);
CREATE INDEX IF NOT EXISTS idx_benefit_clubs_status ON barber.benefit_clubs(status);
CREATE INDEX IF NOT EXISTS idx_benefit_clubs_created_at ON barber.benefit_clubs(created_at);
CREATE INDEX IF NOT EXISTS idx_benefit_clubs_tenant_status ON barber.benefit_clubs(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_benefit_clubs_tenant_branch ON barber.benefit_clubs(tenant_id, branch_id);
CREATE TRIGGER trg_benefit_clubs_updated_at BEFORE UPDATE ON barber.benefit_clubs FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.benefit_tiers (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_benefit_tiers_tenant_id ON barber.benefit_tiers(tenant_id);
CREATE INDEX IF NOT EXISTS idx_benefit_tiers_branch_id ON barber.benefit_tiers(branch_id);
CREATE INDEX IF NOT EXISTS idx_benefit_tiers_status ON barber.benefit_tiers(status);
CREATE INDEX IF NOT EXISTS idx_benefit_tiers_created_at ON barber.benefit_tiers(created_at);
CREATE INDEX IF NOT EXISTS idx_benefit_tiers_tenant_status ON barber.benefit_tiers(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_benefit_tiers_tenant_branch ON barber.benefit_tiers(tenant_id, branch_id);
CREATE TRIGGER trg_benefit_tiers_updated_at BEFORE UPDATE ON barber.benefit_tiers FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.referral_programs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_referral_programs_tenant_id ON barber.referral_programs(tenant_id);
CREATE INDEX IF NOT EXISTS idx_referral_programs_branch_id ON barber.referral_programs(branch_id);
CREATE INDEX IF NOT EXISTS idx_referral_programs_status ON barber.referral_programs(status);
CREATE INDEX IF NOT EXISTS idx_referral_programs_created_at ON barber.referral_programs(created_at);
CREATE INDEX IF NOT EXISTS idx_referral_programs_tenant_status ON barber.referral_programs(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_referral_programs_tenant_branch ON barber.referral_programs(tenant_id, branch_id);
CREATE TRIGGER trg_referral_programs_updated_at BEFORE UPDATE ON barber.referral_programs FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.referral_codes (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_referral_codes_tenant_id ON barber.referral_codes(tenant_id);
CREATE INDEX IF NOT EXISTS idx_referral_codes_branch_id ON barber.referral_codes(branch_id);
CREATE INDEX IF NOT EXISTS idx_referral_codes_status ON barber.referral_codes(status);
CREATE INDEX IF NOT EXISTS idx_referral_codes_created_at ON barber.referral_codes(created_at);
CREATE INDEX IF NOT EXISTS idx_referral_codes_tenant_status ON barber.referral_codes(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_referral_codes_tenant_branch ON barber.referral_codes(tenant_id, branch_id);
CREATE TRIGGER trg_referral_codes_updated_at BEFORE UPDATE ON barber.referral_codes FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.client_visual_history (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_client_visual_history_tenant_id ON barber.client_visual_history(tenant_id);
CREATE INDEX IF NOT EXISTS idx_client_visual_history_branch_id ON barber.client_visual_history(branch_id);
CREATE INDEX IF NOT EXISTS idx_client_visual_history_status ON barber.client_visual_history(status);
CREATE INDEX IF NOT EXISTS idx_client_visual_history_created_at ON barber.client_visual_history(created_at);
CREATE INDEX IF NOT EXISTS idx_client_visual_history_tenant_status ON barber.client_visual_history(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_client_visual_history_tenant_branch ON barber.client_visual_history(tenant_id, branch_id);
CREATE TRIGGER trg_client_visual_history_updated_at BEFORE UPDATE ON barber.client_visual_history FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.client_visual_photos (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_client_visual_photos_tenant_id ON barber.client_visual_photos(tenant_id);
CREATE INDEX IF NOT EXISTS idx_client_visual_photos_branch_id ON barber.client_visual_photos(branch_id);
CREATE INDEX IF NOT EXISTS idx_client_visual_photos_status ON barber.client_visual_photos(status);
CREATE INDEX IF NOT EXISTS idx_client_visual_photos_created_at ON barber.client_visual_photos(created_at);
CREATE INDEX IF NOT EXISTS idx_client_visual_photos_tenant_status ON barber.client_visual_photos(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_client_visual_photos_tenant_branch ON barber.client_visual_photos(tenant_id, branch_id);
CREATE TRIGGER trg_client_visual_photos_updated_at BEFORE UPDATE ON barber.client_visual_photos FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.professionals (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_professionals_tenant_id ON barber.professionals(tenant_id);
CREATE INDEX IF NOT EXISTS idx_professionals_branch_id ON barber.professionals(branch_id);
CREATE INDEX IF NOT EXISTS idx_professionals_status ON barber.professionals(status);
CREATE INDEX IF NOT EXISTS idx_professionals_created_at ON barber.professionals(created_at);
CREATE INDEX IF NOT EXISTS idx_professionals_tenant_status ON barber.professionals(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_professionals_tenant_branch ON barber.professionals(tenant_id, branch_id);
CREATE TRIGGER trg_professionals_updated_at BEFORE UPDATE ON barber.professionals FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.professional_profiles (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_professional_profiles_tenant_id ON barber.professional_profiles(tenant_id);
CREATE INDEX IF NOT EXISTS idx_professional_profiles_branch_id ON barber.professional_profiles(branch_id);
CREATE INDEX IF NOT EXISTS idx_professional_profiles_status ON barber.professional_profiles(status);
CREATE INDEX IF NOT EXISTS idx_professional_profiles_created_at ON barber.professional_profiles(created_at);
CREATE INDEX IF NOT EXISTS idx_professional_profiles_tenant_status ON barber.professional_profiles(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_professional_profiles_tenant_branch ON barber.professional_profiles(tenant_id, branch_id);
CREATE TRIGGER trg_professional_profiles_updated_at BEFORE UPDATE ON barber.professional_profiles FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.professional_specialties (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_professional_specialties_tenant_id ON barber.professional_specialties(tenant_id);
CREATE INDEX IF NOT EXISTS idx_professional_specialties_branch_id ON barber.professional_specialties(branch_id);
CREATE INDEX IF NOT EXISTS idx_professional_specialties_status ON barber.professional_specialties(status);
CREATE INDEX IF NOT EXISTS idx_professional_specialties_created_at ON barber.professional_specialties(created_at);
CREATE INDEX IF NOT EXISTS idx_professional_specialties_tenant_status ON barber.professional_specialties(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_professional_specialties_tenant_branch ON barber.professional_specialties(tenant_id, branch_id);
CREATE TRIGGER trg_professional_specialties_updated_at BEFORE UPDATE ON barber.professional_specialties FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.professional_services (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_professional_services_tenant_id ON barber.professional_services(tenant_id);
CREATE INDEX IF NOT EXISTS idx_professional_services_branch_id ON barber.professional_services(branch_id);
CREATE INDEX IF NOT EXISTS idx_professional_services_status ON barber.professional_services(status);
CREATE INDEX IF NOT EXISTS idx_professional_services_created_at ON barber.professional_services(created_at);
CREATE INDEX IF NOT EXISTS idx_professional_services_tenant_status ON barber.professional_services(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_professional_services_tenant_branch ON barber.professional_services(tenant_id, branch_id);
CREATE TRIGGER trg_professional_services_updated_at BEFORE UPDATE ON barber.professional_services FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.professional_documents (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_professional_documents_tenant_id ON barber.professional_documents(tenant_id);
CREATE INDEX IF NOT EXISTS idx_professional_documents_branch_id ON barber.professional_documents(branch_id);
CREATE INDEX IF NOT EXISTS idx_professional_documents_status ON barber.professional_documents(status);
CREATE INDEX IF NOT EXISTS idx_professional_documents_created_at ON barber.professional_documents(created_at);
CREATE INDEX IF NOT EXISTS idx_professional_documents_tenant_status ON barber.professional_documents(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_professional_documents_tenant_branch ON barber.professional_documents(tenant_id, branch_id);
CREATE TRIGGER trg_professional_documents_updated_at BEFORE UPDATE ON barber.professional_documents FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.work_schedules (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_work_schedules_tenant_id ON barber.work_schedules(tenant_id);
CREATE INDEX IF NOT EXISTS idx_work_schedules_branch_id ON barber.work_schedules(branch_id);
CREATE INDEX IF NOT EXISTS idx_work_schedules_status ON barber.work_schedules(status);
CREATE INDEX IF NOT EXISTS idx_work_schedules_created_at ON barber.work_schedules(created_at);
CREATE INDEX IF NOT EXISTS idx_work_schedules_tenant_status ON barber.work_schedules(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_work_schedules_tenant_branch ON barber.work_schedules(tenant_id, branch_id);
CREATE TRIGGER trg_work_schedules_updated_at BEFORE UPDATE ON barber.work_schedules FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.work_shifts (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_work_shifts_tenant_id ON barber.work_shifts(tenant_id);
CREATE INDEX IF NOT EXISTS idx_work_shifts_branch_id ON barber.work_shifts(branch_id);
CREATE INDEX IF NOT EXISTS idx_work_shifts_status ON barber.work_shifts(status);
CREATE INDEX IF NOT EXISTS idx_work_shifts_created_at ON barber.work_shifts(created_at);
CREATE INDEX IF NOT EXISTS idx_work_shifts_tenant_status ON barber.work_shifts(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_work_shifts_tenant_branch ON barber.work_shifts(tenant_id, branch_id);
CREATE TRIGGER trg_work_shifts_updated_at BEFORE UPDATE ON barber.work_shifts FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.time_clock_entries (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_time_clock_entries_tenant_id ON barber.time_clock_entries(tenant_id);
CREATE INDEX IF NOT EXISTS idx_time_clock_entries_branch_id ON barber.time_clock_entries(branch_id);
CREATE INDEX IF NOT EXISTS idx_time_clock_entries_status ON barber.time_clock_entries(status);
CREATE INDEX IF NOT EXISTS idx_time_clock_entries_created_at ON barber.time_clock_entries(created_at);
CREATE INDEX IF NOT EXISTS idx_time_clock_entries_tenant_status ON barber.time_clock_entries(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_time_clock_entries_tenant_branch ON barber.time_clock_entries(tenant_id, branch_id);
CREATE TRIGGER trg_time_clock_entries_updated_at BEFORE UPDATE ON barber.time_clock_entries FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.leave_requests (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_leave_requests_tenant_id ON barber.leave_requests(tenant_id);
CREATE INDEX IF NOT EXISTS idx_leave_requests_branch_id ON barber.leave_requests(branch_id);
CREATE INDEX IF NOT EXISTS idx_leave_requests_status ON barber.leave_requests(status);
CREATE INDEX IF NOT EXISTS idx_leave_requests_created_at ON barber.leave_requests(created_at);
CREATE INDEX IF NOT EXISTS idx_leave_requests_tenant_status ON barber.leave_requests(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_leave_requests_tenant_branch ON barber.leave_requests(tenant_id, branch_id);
CREATE TRIGGER trg_leave_requests_updated_at BEFORE UPDATE ON barber.leave_requests FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.commission_rules (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_commission_rules_tenant_id ON barber.commission_rules(tenant_id);
CREATE INDEX IF NOT EXISTS idx_commission_rules_branch_id ON barber.commission_rules(branch_id);
CREATE INDEX IF NOT EXISTS idx_commission_rules_status ON barber.commission_rules(status);
CREATE INDEX IF NOT EXISTS idx_commission_rules_created_at ON barber.commission_rules(created_at);
CREATE INDEX IF NOT EXISTS idx_commission_rules_tenant_status ON barber.commission_rules(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_commission_rules_tenant_branch ON barber.commission_rules(tenant_id, branch_id);
CREATE TRIGGER trg_commission_rules_updated_at BEFORE UPDATE ON barber.commission_rules FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.commissions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_commissions_tenant_id ON barber.commissions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_commissions_branch_id ON barber.commissions(branch_id);
CREATE INDEX IF NOT EXISTS idx_commissions_status ON barber.commissions(status);
CREATE INDEX IF NOT EXISTS idx_commissions_created_at ON barber.commissions(created_at);
CREATE INDEX IF NOT EXISTS idx_commissions_tenant_status ON barber.commissions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_commissions_tenant_branch ON barber.commissions(tenant_id, branch_id);
CREATE TRIGGER trg_commissions_updated_at BEFORE UPDATE ON barber.commissions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.commission_batches (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_commission_batches_tenant_id ON barber.commission_batches(tenant_id);
CREATE INDEX IF NOT EXISTS idx_commission_batches_branch_id ON barber.commission_batches(branch_id);
CREATE INDEX IF NOT EXISTS idx_commission_batches_status ON barber.commission_batches(status);
CREATE INDEX IF NOT EXISTS idx_commission_batches_created_at ON barber.commission_batches(created_at);
CREATE INDEX IF NOT EXISTS idx_commission_batches_tenant_status ON barber.commission_batches(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_commission_batches_tenant_branch ON barber.commission_batches(tenant_id, branch_id);
CREATE TRIGGER trg_commission_batches_updated_at BEFORE UPDATE ON barber.commission_batches FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.payroll_periods (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_payroll_periods_tenant_id ON barber.payroll_periods(tenant_id);
CREATE INDEX IF NOT EXISTS idx_payroll_periods_branch_id ON barber.payroll_periods(branch_id);
CREATE INDEX IF NOT EXISTS idx_payroll_periods_status ON barber.payroll_periods(status);
CREATE INDEX IF NOT EXISTS idx_payroll_periods_created_at ON barber.payroll_periods(created_at);
CREATE INDEX IF NOT EXISTS idx_payroll_periods_tenant_status ON barber.payroll_periods(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_payroll_periods_tenant_branch ON barber.payroll_periods(tenant_id, branch_id);
CREATE TRIGGER trg_payroll_periods_updated_at BEFORE UPDATE ON barber.payroll_periods FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.payroll_items (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_payroll_items_tenant_id ON barber.payroll_items(tenant_id);
CREATE INDEX IF NOT EXISTS idx_payroll_items_branch_id ON barber.payroll_items(branch_id);
CREATE INDEX IF NOT EXISTS idx_payroll_items_status ON barber.payroll_items(status);
CREATE INDEX IF NOT EXISTS idx_payroll_items_created_at ON barber.payroll_items(created_at);
CREATE INDEX IF NOT EXISTS idx_payroll_items_tenant_status ON barber.payroll_items(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_payroll_items_tenant_branch ON barber.payroll_items(tenant_id, branch_id);
CREATE TRIGGER trg_payroll_items_updated_at BEFORE UPDATE ON barber.payroll_items FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.training_courses (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_training_courses_tenant_id ON barber.training_courses(tenant_id);
CREATE INDEX IF NOT EXISTS idx_training_courses_branch_id ON barber.training_courses(branch_id);
CREATE INDEX IF NOT EXISTS idx_training_courses_status ON barber.training_courses(status);
CREATE INDEX IF NOT EXISTS idx_training_courses_created_at ON barber.training_courses(created_at);
CREATE INDEX IF NOT EXISTS idx_training_courses_tenant_status ON barber.training_courses(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_training_courses_tenant_branch ON barber.training_courses(tenant_id, branch_id);
CREATE TRIGGER trg_training_courses_updated_at BEFORE UPDATE ON barber.training_courses FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.training_enrollments (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_training_enrollments_tenant_id ON barber.training_enrollments(tenant_id);
CREATE INDEX IF NOT EXISTS idx_training_enrollments_branch_id ON barber.training_enrollments(branch_id);
CREATE INDEX IF NOT EXISTS idx_training_enrollments_status ON barber.training_enrollments(status);
CREATE INDEX IF NOT EXISTS idx_training_enrollments_created_at ON barber.training_enrollments(created_at);
CREATE INDEX IF NOT EXISTS idx_training_enrollments_tenant_status ON barber.training_enrollments(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_training_enrollments_tenant_branch ON barber.training_enrollments(tenant_id, branch_id);
CREATE TRIGGER trg_training_enrollments_updated_at BEFORE UPDATE ON barber.training_enrollments FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.training_certificates (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_training_certificates_tenant_id ON barber.training_certificates(tenant_id);
CREATE INDEX IF NOT EXISTS idx_training_certificates_branch_id ON barber.training_certificates(branch_id);
CREATE INDEX IF NOT EXISTS idx_training_certificates_status ON barber.training_certificates(status);
CREATE INDEX IF NOT EXISTS idx_training_certificates_created_at ON barber.training_certificates(created_at);
CREATE INDEX IF NOT EXISTS idx_training_certificates_tenant_status ON barber.training_certificates(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_training_certificates_tenant_branch ON barber.training_certificates(tenant_id, branch_id);
CREATE TRIGGER trg_training_certificates_updated_at BEFORE UPDATE ON barber.training_certificates FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.performance_reviews (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_performance_reviews_tenant_id ON barber.performance_reviews(tenant_id);
CREATE INDEX IF NOT EXISTS idx_performance_reviews_branch_id ON barber.performance_reviews(branch_id);
CREATE INDEX IF NOT EXISTS idx_performance_reviews_status ON barber.performance_reviews(status);
CREATE INDEX IF NOT EXISTS idx_performance_reviews_created_at ON barber.performance_reviews(created_at);
CREATE INDEX IF NOT EXISTS idx_performance_reviews_tenant_status ON barber.performance_reviews(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_performance_reviews_tenant_branch ON barber.performance_reviews(tenant_id, branch_id);
CREATE TRIGGER trg_performance_reviews_updated_at BEFORE UPDATE ON barber.performance_reviews FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.service_categories (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_service_categories_tenant_id ON barber.service_categories(tenant_id);
CREATE INDEX IF NOT EXISTS idx_service_categories_branch_id ON barber.service_categories(branch_id);
CREATE INDEX IF NOT EXISTS idx_service_categories_status ON barber.service_categories(status);
CREATE INDEX IF NOT EXISTS idx_service_categories_created_at ON barber.service_categories(created_at);
CREATE INDEX IF NOT EXISTS idx_service_categories_tenant_status ON barber.service_categories(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_service_categories_tenant_branch ON barber.service_categories(tenant_id, branch_id);
CREATE TRIGGER trg_service_categories_updated_at BEFORE UPDATE ON barber.service_categories FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.services (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_services_tenant_id ON barber.services(tenant_id);
CREATE INDEX IF NOT EXISTS idx_services_branch_id ON barber.services(branch_id);
CREATE INDEX IF NOT EXISTS idx_services_status ON barber.services(status);
CREATE INDEX IF NOT EXISTS idx_services_created_at ON barber.services(created_at);
CREATE INDEX IF NOT EXISTS idx_services_tenant_status ON barber.services(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_services_tenant_branch ON barber.services(tenant_id, branch_id);
CREATE TRIGGER trg_services_updated_at BEFORE UPDATE ON barber.services FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.appointments (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_appointments_tenant_id ON barber.appointments(tenant_id);
CREATE INDEX IF NOT EXISTS idx_appointments_branch_id ON barber.appointments(branch_id);
CREATE INDEX IF NOT EXISTS idx_appointments_status ON barber.appointments(status);
CREATE INDEX IF NOT EXISTS idx_appointments_created_at ON barber.appointments(created_at);
CREATE INDEX IF NOT EXISTS idx_appointments_tenant_status ON barber.appointments(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_appointments_tenant_branch ON barber.appointments(tenant_id, branch_id);
CREATE TRIGGER trg_appointments_updated_at BEFORE UPDATE ON barber.appointments FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.appointment_status_history (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_appointment_status_history_tenant_id ON barber.appointment_status_history(tenant_id);
CREATE INDEX IF NOT EXISTS idx_appointment_status_history_branch_id ON barber.appointment_status_history(branch_id);
CREATE INDEX IF NOT EXISTS idx_appointment_status_history_status ON barber.appointment_status_history(status);
CREATE INDEX IF NOT EXISTS idx_appointment_status_history_created_at ON barber.appointment_status_history(created_at);
CREATE INDEX IF NOT EXISTS idx_appointment_status_history_tenant_status ON barber.appointment_status_history(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_appointment_status_history_tenant_branch ON barber.appointment_status_history(tenant_id, branch_id);
CREATE TRIGGER trg_appointment_status_history_updated_at BEFORE UPDATE ON barber.appointment_status_history FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.appointment_confirmations (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_appointment_confirmations_tenant_id ON barber.appointment_confirmations(tenant_id);
CREATE INDEX IF NOT EXISTS idx_appointment_confirmations_branch_id ON barber.appointment_confirmations(branch_id);
CREATE INDEX IF NOT EXISTS idx_appointment_confirmations_status ON barber.appointment_confirmations(status);
CREATE INDEX IF NOT EXISTS idx_appointment_confirmations_created_at ON barber.appointment_confirmations(created_at);
CREATE INDEX IF NOT EXISTS idx_appointment_confirmations_tenant_status ON barber.appointment_confirmations(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_appointment_confirmations_tenant_branch ON barber.appointment_confirmations(tenant_id, branch_id);
CREATE TRIGGER trg_appointment_confirmations_updated_at BEFORE UPDATE ON barber.appointment_confirmations FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.appointment_reminders (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_appointment_reminders_tenant_id ON barber.appointment_reminders(tenant_id);
CREATE INDEX IF NOT EXISTS idx_appointment_reminders_branch_id ON barber.appointment_reminders(branch_id);
CREATE INDEX IF NOT EXISTS idx_appointment_reminders_status ON barber.appointment_reminders(status);
CREATE INDEX IF NOT EXISTS idx_appointment_reminders_created_at ON barber.appointment_reminders(created_at);
CREATE INDEX IF NOT EXISTS idx_appointment_reminders_tenant_status ON barber.appointment_reminders(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_appointment_reminders_tenant_branch ON barber.appointment_reminders(tenant_id, branch_id);
CREATE TRIGGER trg_appointment_reminders_updated_at BEFORE UPDATE ON barber.appointment_reminders FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.waiting_list (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_waiting_list_tenant_id ON barber.waiting_list(tenant_id);
CREATE INDEX IF NOT EXISTS idx_waiting_list_branch_id ON barber.waiting_list(branch_id);
CREATE INDEX IF NOT EXISTS idx_waiting_list_status ON barber.waiting_list(status);
CREATE INDEX IF NOT EXISTS idx_waiting_list_created_at ON barber.waiting_list(created_at);
CREATE INDEX IF NOT EXISTS idx_waiting_list_tenant_status ON barber.waiting_list(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_waiting_list_tenant_branch ON barber.waiting_list(tenant_id, branch_id);
CREATE TRIGGER trg_waiting_list_updated_at BEFORE UPDATE ON barber.waiting_list FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.schedule_blocks (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_schedule_blocks_tenant_id ON barber.schedule_blocks(tenant_id);
CREATE INDEX IF NOT EXISTS idx_schedule_blocks_branch_id ON barber.schedule_blocks(branch_id);
CREATE INDEX IF NOT EXISTS idx_schedule_blocks_status ON barber.schedule_blocks(status);
CREATE INDEX IF NOT EXISTS idx_schedule_blocks_created_at ON barber.schedule_blocks(created_at);
CREATE INDEX IF NOT EXISTS idx_schedule_blocks_tenant_status ON barber.schedule_blocks(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_schedule_blocks_tenant_branch ON barber.schedule_blocks(tenant_id, branch_id);
CREATE TRIGGER trg_schedule_blocks_updated_at BEFORE UPDATE ON barber.schedule_blocks FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.smart_schedule_suggestions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_smart_schedule_suggestions_tenant_id ON barber.smart_schedule_suggestions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_smart_schedule_suggestions_branch_id ON barber.smart_schedule_suggestions(branch_id);
CREATE INDEX IF NOT EXISTS idx_smart_schedule_suggestions_status ON barber.smart_schedule_suggestions(status);
CREATE INDEX IF NOT EXISTS idx_smart_schedule_suggestions_created_at ON barber.smart_schedule_suggestions(created_at);
CREATE INDEX IF NOT EXISTS idx_smart_schedule_suggestions_tenant_status ON barber.smart_schedule_suggestions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_smart_schedule_suggestions_tenant_branch ON barber.smart_schedule_suggestions(tenant_id, branch_id);
CREATE TRIGGER trg_smart_schedule_suggestions_updated_at BEFORE UPDATE ON barber.smart_schedule_suggestions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.no_show_risk_scores (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_no_show_risk_scores_tenant_id ON barber.no_show_risk_scores(tenant_id);
CREATE INDEX IF NOT EXISTS idx_no_show_risk_scores_branch_id ON barber.no_show_risk_scores(branch_id);
CREATE INDEX IF NOT EXISTS idx_no_show_risk_scores_status ON barber.no_show_risk_scores(status);
CREATE INDEX IF NOT EXISTS idx_no_show_risk_scores_created_at ON barber.no_show_risk_scores(created_at);
CREATE INDEX IF NOT EXISTS idx_no_show_risk_scores_tenant_status ON barber.no_show_risk_scores(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_no_show_risk_scores_tenant_branch ON barber.no_show_risk_scores(tenant_id, branch_id);
CREATE TRIGGER trg_no_show_risk_scores_updated_at BEFORE UPDATE ON barber.no_show_risk_scores FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.live_operations (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_live_operations_tenant_id ON barber.live_operations(tenant_id);
CREATE INDEX IF NOT EXISTS idx_live_operations_branch_id ON barber.live_operations(branch_id);
CREATE INDEX IF NOT EXISTS idx_live_operations_status ON barber.live_operations(status);
CREATE INDEX IF NOT EXISTS idx_live_operations_created_at ON barber.live_operations(created_at);
CREATE INDEX IF NOT EXISTS idx_live_operations_tenant_status ON barber.live_operations(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_live_operations_tenant_branch ON barber.live_operations(tenant_id, branch_id);
CREATE TRIGGER trg_live_operations_updated_at BEFORE UPDATE ON barber.live_operations FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.attendance_sessions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_attendance_sessions_tenant_id ON barber.attendance_sessions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_attendance_sessions_branch_id ON barber.attendance_sessions(branch_id);
CREATE INDEX IF NOT EXISTS idx_attendance_sessions_status ON barber.attendance_sessions(status);
CREATE INDEX IF NOT EXISTS idx_attendance_sessions_created_at ON barber.attendance_sessions(created_at);
CREATE INDEX IF NOT EXISTS idx_attendance_sessions_tenant_status ON barber.attendance_sessions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_attendance_sessions_tenant_branch ON barber.attendance_sessions(tenant_id, branch_id);
CREATE TRIGGER trg_attendance_sessions_updated_at BEFORE UPDATE ON barber.attendance_sessions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.attendance_status_history (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_attendance_status_history_tenant_id ON barber.attendance_status_history(tenant_id);
CREATE INDEX IF NOT EXISTS idx_attendance_status_history_branch_id ON barber.attendance_status_history(branch_id);
CREATE INDEX IF NOT EXISTS idx_attendance_status_history_status ON barber.attendance_status_history(status);
CREATE INDEX IF NOT EXISTS idx_attendance_status_history_created_at ON barber.attendance_status_history(created_at);
CREATE INDEX IF NOT EXISTS idx_attendance_status_history_tenant_status ON barber.attendance_status_history(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_attendance_status_history_tenant_branch ON barber.attendance_status_history(tenant_id, branch_id);
CREATE TRIGGER trg_attendance_status_history_updated_at BEFORE UPDATE ON barber.attendance_status_history FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.service_orders (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_service_orders_tenant_id ON barber.service_orders(tenant_id);
CREATE INDEX IF NOT EXISTS idx_service_orders_branch_id ON barber.service_orders(branch_id);
CREATE INDEX IF NOT EXISTS idx_service_orders_status ON barber.service_orders(status);
CREATE INDEX IF NOT EXISTS idx_service_orders_created_at ON barber.service_orders(created_at);
CREATE INDEX IF NOT EXISTS idx_service_orders_tenant_status ON barber.service_orders(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_service_orders_tenant_branch ON barber.service_orders(tenant_id, branch_id);
CREATE TRIGGER trg_service_orders_updated_at BEFORE UPDATE ON barber.service_orders FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.service_order_items (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_service_order_items_tenant_id ON barber.service_order_items(tenant_id);
CREATE INDEX IF NOT EXISTS idx_service_order_items_branch_id ON barber.service_order_items(branch_id);
CREATE INDEX IF NOT EXISTS idx_service_order_items_status ON barber.service_order_items(status);
CREATE INDEX IF NOT EXISTS idx_service_order_items_created_at ON barber.service_order_items(created_at);
CREATE INDEX IF NOT EXISTS idx_service_order_items_tenant_status ON barber.service_order_items(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_service_order_items_tenant_branch ON barber.service_order_items(tenant_id, branch_id);
CREATE TRIGGER trg_service_order_items_updated_at BEFORE UPDATE ON barber.service_order_items FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.service_order_payments (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_service_order_payments_tenant_id ON barber.service_order_payments(tenant_id);
CREATE INDEX IF NOT EXISTS idx_service_order_payments_branch_id ON barber.service_order_payments(branch_id);
CREATE INDEX IF NOT EXISTS idx_service_order_payments_status ON barber.service_order_payments(status);
CREATE INDEX IF NOT EXISTS idx_service_order_payments_created_at ON barber.service_order_payments(created_at);
CREATE INDEX IF NOT EXISTS idx_service_order_payments_tenant_status ON barber.service_order_payments(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_service_order_payments_tenant_branch ON barber.service_order_payments(tenant_id, branch_id);
CREATE TRIGGER trg_service_order_payments_updated_at BEFORE UPDATE ON barber.service_order_payments FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.service_order_history (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_service_order_history_tenant_id ON barber.service_order_history(tenant_id);
CREATE INDEX IF NOT EXISTS idx_service_order_history_branch_id ON barber.service_order_history(branch_id);
CREATE INDEX IF NOT EXISTS idx_service_order_history_status ON barber.service_order_history(status);
CREATE INDEX IF NOT EXISTS idx_service_order_history_created_at ON barber.service_order_history(created_at);
CREATE INDEX IF NOT EXISTS idx_service_order_history_tenant_status ON barber.service_order_history(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_service_order_history_tenant_branch ON barber.service_order_history(tenant_id, branch_id);
CREATE TRIGGER trg_service_order_history_updated_at BEFORE UPDATE ON barber.service_order_history FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.point_of_sale_sales (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_point_of_sale_sales_tenant_id ON barber.point_of_sale_sales(tenant_id);
CREATE INDEX IF NOT EXISTS idx_point_of_sale_sales_branch_id ON barber.point_of_sale_sales(branch_id);
CREATE INDEX IF NOT EXISTS idx_point_of_sale_sales_status ON barber.point_of_sale_sales(status);
CREATE INDEX IF NOT EXISTS idx_point_of_sale_sales_created_at ON barber.point_of_sale_sales(created_at);
CREATE INDEX IF NOT EXISTS idx_point_of_sale_sales_tenant_status ON barber.point_of_sale_sales(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_point_of_sale_sales_tenant_branch ON barber.point_of_sale_sales(tenant_id, branch_id);
CREATE TRIGGER trg_point_of_sale_sales_updated_at BEFORE UPDATE ON barber.point_of_sale_sales FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.point_of_sale_items (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_point_of_sale_items_tenant_id ON barber.point_of_sale_items(tenant_id);
CREATE INDEX IF NOT EXISTS idx_point_of_sale_items_branch_id ON barber.point_of_sale_items(branch_id);
CREATE INDEX IF NOT EXISTS idx_point_of_sale_items_status ON barber.point_of_sale_items(status);
CREATE INDEX IF NOT EXISTS idx_point_of_sale_items_created_at ON barber.point_of_sale_items(created_at);
CREATE INDEX IF NOT EXISTS idx_point_of_sale_items_tenant_status ON barber.point_of_sale_items(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_point_of_sale_items_tenant_branch ON barber.point_of_sale_items(tenant_id, branch_id);
CREATE TRIGGER trg_point_of_sale_items_updated_at BEFORE UPDATE ON barber.point_of_sale_items FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.operation_resources (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_operation_resources_tenant_id ON barber.operation_resources(tenant_id);
CREATE INDEX IF NOT EXISTS idx_operation_resources_branch_id ON barber.operation_resources(branch_id);
CREATE INDEX IF NOT EXISTS idx_operation_resources_status ON barber.operation_resources(status);
CREATE INDEX IF NOT EXISTS idx_operation_resources_created_at ON barber.operation_resources(created_at);
CREATE INDEX IF NOT EXISTS idx_operation_resources_tenant_status ON barber.operation_resources(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_operation_resources_tenant_branch ON barber.operation_resources(tenant_id, branch_id);
CREATE TRIGGER trg_operation_resources_updated_at BEFORE UPDATE ON barber.operation_resources FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.resource_bookings (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_resource_bookings_tenant_id ON barber.resource_bookings(tenant_id);
CREATE INDEX IF NOT EXISTS idx_resource_bookings_branch_id ON barber.resource_bookings(branch_id);
CREATE INDEX IF NOT EXISTS idx_resource_bookings_status ON barber.resource_bookings(status);
CREATE INDEX IF NOT EXISTS idx_resource_bookings_created_at ON barber.resource_bookings(created_at);
CREATE INDEX IF NOT EXISTS idx_resource_bookings_tenant_status ON barber.resource_bookings(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_resource_bookings_tenant_branch ON barber.resource_bookings(tenant_id, branch_id);
CREATE TRIGGER trg_resource_bookings_updated_at BEFORE UPDATE ON barber.resource_bookings FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.operation_queue (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_operation_queue_tenant_id ON barber.operation_queue(tenant_id);
CREATE INDEX IF NOT EXISTS idx_operation_queue_branch_id ON barber.operation_queue(branch_id);
CREATE INDEX IF NOT EXISTS idx_operation_queue_status ON barber.operation_queue(status);
CREATE INDEX IF NOT EXISTS idx_operation_queue_created_at ON barber.operation_queue(created_at);
CREATE INDEX IF NOT EXISTS idx_operation_queue_tenant_status ON barber.operation_queue(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_operation_queue_tenant_branch ON barber.operation_queue(tenant_id, branch_id);
CREATE TRIGGER trg_operation_queue_updated_at BEFORE UPDATE ON barber.operation_queue FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.daily_closings (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_daily_closings_tenant_id ON barber.daily_closings(tenant_id);
CREATE INDEX IF NOT EXISTS idx_daily_closings_branch_id ON barber.daily_closings(branch_id);
CREATE INDEX IF NOT EXISTS idx_daily_closings_status ON barber.daily_closings(status);
CREATE INDEX IF NOT EXISTS idx_daily_closings_created_at ON barber.daily_closings(created_at);
CREATE INDEX IF NOT EXISTS idx_daily_closings_tenant_status ON barber.daily_closings(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_daily_closings_tenant_branch ON barber.daily_closings(tenant_id, branch_id);
CREATE TRIGGER trg_daily_closings_updated_at BEFORE UPDATE ON barber.daily_closings FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.daily_closing_checklist (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_daily_closing_checklist_tenant_id ON barber.daily_closing_checklist(tenant_id);
CREATE INDEX IF NOT EXISTS idx_daily_closing_checklist_branch_id ON barber.daily_closing_checklist(branch_id);
CREATE INDEX IF NOT EXISTS idx_daily_closing_checklist_status ON barber.daily_closing_checklist(status);
CREATE INDEX IF NOT EXISTS idx_daily_closing_checklist_created_at ON barber.daily_closing_checklist(created_at);
CREATE INDEX IF NOT EXISTS idx_daily_closing_checklist_tenant_status ON barber.daily_closing_checklist(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_daily_closing_checklist_tenant_branch ON barber.daily_closing_checklist(tenant_id, branch_id);
CREATE TRIGGER trg_daily_closing_checklist_updated_at BEFORE UPDATE ON barber.daily_closing_checklist FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.payments (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_payments_tenant_id ON barber.payments(tenant_id);
CREATE INDEX IF NOT EXISTS idx_payments_branch_id ON barber.payments(branch_id);
CREATE INDEX IF NOT EXISTS idx_payments_status ON barber.payments(status);
CREATE INDEX IF NOT EXISTS idx_payments_created_at ON barber.payments(created_at);
CREATE INDEX IF NOT EXISTS idx_payments_tenant_status ON barber.payments(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_payments_tenant_branch ON barber.payments(tenant_id, branch_id);
CREATE TRIGGER trg_payments_updated_at BEFORE UPDATE ON barber.payments FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.payment_methods (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_payment_methods_tenant_id ON barber.payment_methods(tenant_id);
CREATE INDEX IF NOT EXISTS idx_payment_methods_branch_id ON barber.payment_methods(branch_id);
CREATE INDEX IF NOT EXISTS idx_payment_methods_status ON barber.payment_methods(status);
CREATE INDEX IF NOT EXISTS idx_payment_methods_created_at ON barber.payment_methods(created_at);
CREATE INDEX IF NOT EXISTS idx_payment_methods_tenant_status ON barber.payment_methods(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_payment_methods_tenant_branch ON barber.payment_methods(tenant_id, branch_id);
CREATE TRIGGER trg_payment_methods_updated_at BEFORE UPDATE ON barber.payment_methods FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.payment_transactions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_payment_transactions_tenant_id ON barber.payment_transactions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_payment_transactions_branch_id ON barber.payment_transactions(branch_id);
CREATE INDEX IF NOT EXISTS idx_payment_transactions_status ON barber.payment_transactions(status);
CREATE INDEX IF NOT EXISTS idx_payment_transactions_created_at ON barber.payment_transactions(created_at);
CREATE INDEX IF NOT EXISTS idx_payment_transactions_tenant_status ON barber.payment_transactions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_payment_transactions_tenant_branch ON barber.payment_transactions(tenant_id, branch_id);
CREATE TRIGGER trg_payment_transactions_updated_at BEFORE UPDATE ON barber.payment_transactions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.payment_webhooks (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_payment_webhooks_tenant_id ON barber.payment_webhooks(tenant_id);
CREATE INDEX IF NOT EXISTS idx_payment_webhooks_branch_id ON barber.payment_webhooks(branch_id);
CREATE INDEX IF NOT EXISTS idx_payment_webhooks_status ON barber.payment_webhooks(status);
CREATE INDEX IF NOT EXISTS idx_payment_webhooks_created_at ON barber.payment_webhooks(created_at);
CREATE INDEX IF NOT EXISTS idx_payment_webhooks_tenant_status ON barber.payment_webhooks(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_payment_webhooks_tenant_branch ON barber.payment_webhooks(tenant_id, branch_id);
CREATE TRIGGER trg_payment_webhooks_updated_at BEFORE UPDATE ON barber.payment_webhooks FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.payment_refunds (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_payment_refunds_tenant_id ON barber.payment_refunds(tenant_id);
CREATE INDEX IF NOT EXISTS idx_payment_refunds_branch_id ON barber.payment_refunds(branch_id);
CREATE INDEX IF NOT EXISTS idx_payment_refunds_status ON barber.payment_refunds(status);
CREATE INDEX IF NOT EXISTS idx_payment_refunds_created_at ON barber.payment_refunds(created_at);
CREATE INDEX IF NOT EXISTS idx_payment_refunds_tenant_status ON barber.payment_refunds(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_payment_refunds_tenant_branch ON barber.payment_refunds(tenant_id, branch_id);
CREATE TRIGGER trg_payment_refunds_updated_at BEFORE UPDATE ON barber.payment_refunds FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.pix_charges (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_pix_charges_tenant_id ON barber.pix_charges(tenant_id);
CREATE INDEX IF NOT EXISTS idx_pix_charges_branch_id ON barber.pix_charges(branch_id);
CREATE INDEX IF NOT EXISTS idx_pix_charges_status ON barber.pix_charges(status);
CREATE INDEX IF NOT EXISTS idx_pix_charges_created_at ON barber.pix_charges(created_at);
CREATE INDEX IF NOT EXISTS idx_pix_charges_tenant_status ON barber.pix_charges(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_pix_charges_tenant_branch ON barber.pix_charges(tenant_id, branch_id);
CREATE TRIGGER trg_pix_charges_updated_at BEFORE UPDATE ON barber.pix_charges FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.card_transactions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_card_transactions_tenant_id ON barber.card_transactions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_card_transactions_branch_id ON barber.card_transactions(branch_id);
CREATE INDEX IF NOT EXISTS idx_card_transactions_status ON barber.card_transactions(status);
CREATE INDEX IF NOT EXISTS idx_card_transactions_created_at ON barber.card_transactions(created_at);
CREATE INDEX IF NOT EXISTS idx_card_transactions_tenant_status ON barber.card_transactions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_card_transactions_tenant_branch ON barber.card_transactions(tenant_id, branch_id);
CREATE TRIGGER trg_card_transactions_updated_at BEFORE UPDATE ON barber.card_transactions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.cash_sessions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_cash_sessions_tenant_id ON barber.cash_sessions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_cash_sessions_branch_id ON barber.cash_sessions(branch_id);
CREATE INDEX IF NOT EXISTS idx_cash_sessions_status ON barber.cash_sessions(status);
CREATE INDEX IF NOT EXISTS idx_cash_sessions_created_at ON barber.cash_sessions(created_at);
CREATE INDEX IF NOT EXISTS idx_cash_sessions_tenant_status ON barber.cash_sessions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_cash_sessions_tenant_branch ON barber.cash_sessions(tenant_id, branch_id);
CREATE TRIGGER trg_cash_sessions_updated_at BEFORE UPDATE ON barber.cash_sessions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.cash_entries (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_cash_entries_tenant_id ON barber.cash_entries(tenant_id);
CREATE INDEX IF NOT EXISTS idx_cash_entries_branch_id ON barber.cash_entries(branch_id);
CREATE INDEX IF NOT EXISTS idx_cash_entries_status ON barber.cash_entries(status);
CREATE INDEX IF NOT EXISTS idx_cash_entries_created_at ON barber.cash_entries(created_at);
CREATE INDEX IF NOT EXISTS idx_cash_entries_tenant_status ON barber.cash_entries(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_cash_entries_tenant_branch ON barber.cash_entries(tenant_id, branch_id);
CREATE TRIGGER trg_cash_entries_updated_at BEFORE UPDATE ON barber.cash_entries FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.cash_movements (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_cash_movements_tenant_id ON barber.cash_movements(tenant_id);
CREATE INDEX IF NOT EXISTS idx_cash_movements_branch_id ON barber.cash_movements(branch_id);
CREATE INDEX IF NOT EXISTS idx_cash_movements_status ON barber.cash_movements(status);
CREATE INDEX IF NOT EXISTS idx_cash_movements_created_at ON barber.cash_movements(created_at);
CREATE INDEX IF NOT EXISTS idx_cash_movements_tenant_status ON barber.cash_movements(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_cash_movements_tenant_branch ON barber.cash_movements(tenant_id, branch_id);
CREATE TRIGGER trg_cash_movements_updated_at BEFORE UPDATE ON barber.cash_movements FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.cash_closing_reports (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_cash_closing_reports_tenant_id ON barber.cash_closing_reports(tenant_id);
CREATE INDEX IF NOT EXISTS idx_cash_closing_reports_branch_id ON barber.cash_closing_reports(branch_id);
CREATE INDEX IF NOT EXISTS idx_cash_closing_reports_status ON barber.cash_closing_reports(status);
CREATE INDEX IF NOT EXISTS idx_cash_closing_reports_created_at ON barber.cash_closing_reports(created_at);
CREATE INDEX IF NOT EXISTS idx_cash_closing_reports_tenant_status ON barber.cash_closing_reports(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_cash_closing_reports_tenant_branch ON barber.cash_closing_reports(tenant_id, branch_id);
CREATE TRIGGER trg_cash_closing_reports_updated_at BEFORE UPDATE ON barber.cash_closing_reports FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.accounts_payable (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_accounts_payable_tenant_id ON barber.accounts_payable(tenant_id);
CREATE INDEX IF NOT EXISTS idx_accounts_payable_branch_id ON barber.accounts_payable(branch_id);
CREATE INDEX IF NOT EXISTS idx_accounts_payable_status ON barber.accounts_payable(status);
CREATE INDEX IF NOT EXISTS idx_accounts_payable_created_at ON barber.accounts_payable(created_at);
CREATE INDEX IF NOT EXISTS idx_accounts_payable_tenant_status ON barber.accounts_payable(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_accounts_payable_tenant_branch ON barber.accounts_payable(tenant_id, branch_id);
CREATE TRIGGER trg_accounts_payable_updated_at BEFORE UPDATE ON barber.accounts_payable FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.accounts_payable_payments (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_accounts_payable_payments_tenant_id ON barber.accounts_payable_payments(tenant_id);
CREATE INDEX IF NOT EXISTS idx_accounts_payable_payments_branch_id ON barber.accounts_payable_payments(branch_id);
CREATE INDEX IF NOT EXISTS idx_accounts_payable_payments_status ON barber.accounts_payable_payments(status);
CREATE INDEX IF NOT EXISTS idx_accounts_payable_payments_created_at ON barber.accounts_payable_payments(created_at);
CREATE INDEX IF NOT EXISTS idx_accounts_payable_payments_tenant_status ON barber.accounts_payable_payments(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_accounts_payable_payments_tenant_branch ON barber.accounts_payable_payments(tenant_id, branch_id);
CREATE TRIGGER trg_accounts_payable_payments_updated_at BEFORE UPDATE ON barber.accounts_payable_payments FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.accounts_receivable (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_accounts_receivable_tenant_id ON barber.accounts_receivable(tenant_id);
CREATE INDEX IF NOT EXISTS idx_accounts_receivable_branch_id ON barber.accounts_receivable(branch_id);
CREATE INDEX IF NOT EXISTS idx_accounts_receivable_status ON barber.accounts_receivable(status);
CREATE INDEX IF NOT EXISTS idx_accounts_receivable_created_at ON barber.accounts_receivable(created_at);
CREATE INDEX IF NOT EXISTS idx_accounts_receivable_tenant_status ON barber.accounts_receivable(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_accounts_receivable_tenant_branch ON barber.accounts_receivable(tenant_id, branch_id);
CREATE TRIGGER trg_accounts_receivable_updated_at BEFORE UPDATE ON barber.accounts_receivable FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.receivable_installments (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_receivable_installments_tenant_id ON barber.receivable_installments(tenant_id);
CREATE INDEX IF NOT EXISTS idx_receivable_installments_branch_id ON barber.receivable_installments(branch_id);
CREATE INDEX IF NOT EXISTS idx_receivable_installments_status ON barber.receivable_installments(status);
CREATE INDEX IF NOT EXISTS idx_receivable_installments_created_at ON barber.receivable_installments(created_at);
CREATE INDEX IF NOT EXISTS idx_receivable_installments_tenant_status ON barber.receivable_installments(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_receivable_installments_tenant_branch ON barber.receivable_installments(tenant_id, branch_id);
CREATE TRIGGER trg_receivable_installments_updated_at BEFORE UPDATE ON barber.receivable_installments FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.cashflow_entries (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_cashflow_entries_tenant_id ON barber.cashflow_entries(tenant_id);
CREATE INDEX IF NOT EXISTS idx_cashflow_entries_branch_id ON barber.cashflow_entries(branch_id);
CREATE INDEX IF NOT EXISTS idx_cashflow_entries_status ON barber.cashflow_entries(status);
CREATE INDEX IF NOT EXISTS idx_cashflow_entries_created_at ON barber.cashflow_entries(created_at);
CREATE INDEX IF NOT EXISTS idx_cashflow_entries_tenant_status ON barber.cashflow_entries(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_cashflow_entries_tenant_branch ON barber.cashflow_entries(tenant_id, branch_id);
CREATE TRIGGER trg_cashflow_entries_updated_at BEFORE UPDATE ON barber.cashflow_entries FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.cashflow_forecasts (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_cashflow_forecasts_tenant_id ON barber.cashflow_forecasts(tenant_id);
CREATE INDEX IF NOT EXISTS idx_cashflow_forecasts_branch_id ON barber.cashflow_forecasts(branch_id);
CREATE INDEX IF NOT EXISTS idx_cashflow_forecasts_status ON barber.cashflow_forecasts(status);
CREATE INDEX IF NOT EXISTS idx_cashflow_forecasts_created_at ON barber.cashflow_forecasts(created_at);
CREATE INDEX IF NOT EXISTS idx_cashflow_forecasts_tenant_status ON barber.cashflow_forecasts(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_cashflow_forecasts_tenant_branch ON barber.cashflow_forecasts(tenant_id, branch_id);
CREATE TRIGGER trg_cashflow_forecasts_updated_at BEFORE UPDATE ON barber.cashflow_forecasts FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.dre_statements (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_dre_statements_tenant_id ON barber.dre_statements(tenant_id);
CREATE INDEX IF NOT EXISTS idx_dre_statements_branch_id ON barber.dre_statements(branch_id);
CREATE INDEX IF NOT EXISTS idx_dre_statements_status ON barber.dre_statements(status);
CREATE INDEX IF NOT EXISTS idx_dre_statements_created_at ON barber.dre_statements(created_at);
CREATE INDEX IF NOT EXISTS idx_dre_statements_tenant_status ON barber.dre_statements(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_dre_statements_tenant_branch ON barber.dre_statements(tenant_id, branch_id);
CREATE TRIGGER trg_dre_statements_updated_at BEFORE UPDATE ON barber.dre_statements FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.dre_statement_items (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_dre_statement_items_tenant_id ON barber.dre_statement_items(tenant_id);
CREATE INDEX IF NOT EXISTS idx_dre_statement_items_branch_id ON barber.dre_statement_items(branch_id);
CREATE INDEX IF NOT EXISTS idx_dre_statement_items_status ON barber.dre_statement_items(status);
CREATE INDEX IF NOT EXISTS idx_dre_statement_items_created_at ON barber.dre_statement_items(created_at);
CREATE INDEX IF NOT EXISTS idx_dre_statement_items_tenant_status ON barber.dre_statement_items(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_dre_statement_items_tenant_branch ON barber.dre_statement_items(tenant_id, branch_id);
CREATE TRIGGER trg_dre_statement_items_updated_at BEFORE UPDATE ON barber.dre_statement_items FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.business_wallets (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_business_wallets_tenant_id ON barber.business_wallets(tenant_id);
CREATE INDEX IF NOT EXISTS idx_business_wallets_branch_id ON barber.business_wallets(branch_id);
CREATE INDEX IF NOT EXISTS idx_business_wallets_status ON barber.business_wallets(status);
CREATE INDEX IF NOT EXISTS idx_business_wallets_created_at ON barber.business_wallets(created_at);
CREATE INDEX IF NOT EXISTS idx_business_wallets_tenant_status ON barber.business_wallets(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_business_wallets_tenant_branch ON barber.business_wallets(tenant_id, branch_id);
CREATE TRIGGER trg_business_wallets_updated_at BEFORE UPDATE ON barber.business_wallets FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.business_wallet_transactions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_business_wallet_transactions_tenant_id ON barber.business_wallet_transactions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_business_wallet_transactions_branch_id ON barber.business_wallet_transactions(branch_id);
CREATE INDEX IF NOT EXISTS idx_business_wallet_transactions_status ON barber.business_wallet_transactions(status);
CREATE INDEX IF NOT EXISTS idx_business_wallet_transactions_created_at ON barber.business_wallet_transactions(created_at);
CREATE INDEX IF NOT EXISTS idx_business_wallet_transactions_tenant_status ON barber.business_wallet_transactions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_business_wallet_transactions_tenant_branch ON barber.business_wallet_transactions(tenant_id, branch_id);
CREATE TRIGGER trg_business_wallet_transactions_updated_at BEFORE UPDATE ON barber.business_wallet_transactions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.receivables (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_receivables_tenant_id ON barber.receivables(tenant_id);
CREATE INDEX IF NOT EXISTS idx_receivables_branch_id ON barber.receivables(branch_id);
CREATE INDEX IF NOT EXISTS idx_receivables_status ON barber.receivables(status);
CREATE INDEX IF NOT EXISTS idx_receivables_created_at ON barber.receivables(created_at);
CREATE INDEX IF NOT EXISTS idx_receivables_tenant_status ON barber.receivables(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_receivables_tenant_branch ON barber.receivables(tenant_id, branch_id);
CREATE TRIGGER trg_receivables_updated_at BEFORE UPDATE ON barber.receivables FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.payout_requests (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_payout_requests_tenant_id ON barber.payout_requests(tenant_id);
CREATE INDEX IF NOT EXISTS idx_payout_requests_branch_id ON barber.payout_requests(branch_id);
CREATE INDEX IF NOT EXISTS idx_payout_requests_status ON barber.payout_requests(status);
CREATE INDEX IF NOT EXISTS idx_payout_requests_created_at ON barber.payout_requests(created_at);
CREATE INDEX IF NOT EXISTS idx_payout_requests_tenant_status ON barber.payout_requests(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_payout_requests_tenant_branch ON barber.payout_requests(tenant_id, branch_id);
CREATE TRIGGER trg_payout_requests_updated_at BEFORE UPDATE ON barber.payout_requests FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.split_payment_rules (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_split_payment_rules_tenant_id ON barber.split_payment_rules(tenant_id);
CREATE INDEX IF NOT EXISTS idx_split_payment_rules_branch_id ON barber.split_payment_rules(branch_id);
CREATE INDEX IF NOT EXISTS idx_split_payment_rules_status ON barber.split_payment_rules(status);
CREATE INDEX IF NOT EXISTS idx_split_payment_rules_created_at ON barber.split_payment_rules(created_at);
CREATE INDEX IF NOT EXISTS idx_split_payment_rules_tenant_status ON barber.split_payment_rules(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_split_payment_rules_tenant_branch ON barber.split_payment_rules(tenant_id, branch_id);
CREATE TRIGGER trg_split_payment_rules_updated_at BEFORE UPDATE ON barber.split_payment_rules FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.fiscal_company_settings (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_fiscal_company_settings_tenant_id ON barber.fiscal_company_settings(tenant_id);
CREATE INDEX IF NOT EXISTS idx_fiscal_company_settings_branch_id ON barber.fiscal_company_settings(branch_id);
CREATE INDEX IF NOT EXISTS idx_fiscal_company_settings_status ON barber.fiscal_company_settings(status);
CREATE INDEX IF NOT EXISTS idx_fiscal_company_settings_created_at ON barber.fiscal_company_settings(created_at);
CREATE INDEX IF NOT EXISTS idx_fiscal_company_settings_tenant_status ON barber.fiscal_company_settings(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_fiscal_company_settings_tenant_branch ON barber.fiscal_company_settings(tenant_id, branch_id);
CREATE TRIGGER trg_fiscal_company_settings_updated_at BEFORE UPDATE ON barber.fiscal_company_settings FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.fiscal_service_codes (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_fiscal_service_codes_tenant_id ON barber.fiscal_service_codes(tenant_id);
CREATE INDEX IF NOT EXISTS idx_fiscal_service_codes_branch_id ON barber.fiscal_service_codes(branch_id);
CREATE INDEX IF NOT EXISTS idx_fiscal_service_codes_status ON barber.fiscal_service_codes(status);
CREATE INDEX IF NOT EXISTS idx_fiscal_service_codes_created_at ON barber.fiscal_service_codes(created_at);
CREATE INDEX IF NOT EXISTS idx_fiscal_service_codes_tenant_status ON barber.fiscal_service_codes(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_fiscal_service_codes_tenant_branch ON barber.fiscal_service_codes(tenant_id, branch_id);
CREATE TRIGGER trg_fiscal_service_codes_updated_at BEFORE UPDATE ON barber.fiscal_service_codes FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.fiscal_receipt_series (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_fiscal_receipt_series_tenant_id ON barber.fiscal_receipt_series(tenant_id);
CREATE INDEX IF NOT EXISTS idx_fiscal_receipt_series_branch_id ON barber.fiscal_receipt_series(branch_id);
CREATE INDEX IF NOT EXISTS idx_fiscal_receipt_series_status ON barber.fiscal_receipt_series(status);
CREATE INDEX IF NOT EXISTS idx_fiscal_receipt_series_created_at ON barber.fiscal_receipt_series(created_at);
CREATE INDEX IF NOT EXISTS idx_fiscal_receipt_series_tenant_status ON barber.fiscal_receipt_series(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_fiscal_receipt_series_tenant_branch ON barber.fiscal_receipt_series(tenant_id, branch_id);
CREATE TRIGGER trg_fiscal_receipt_series_updated_at BEFORE UPDATE ON barber.fiscal_receipt_series FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.fiscal_receipts (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_fiscal_receipts_tenant_id ON barber.fiscal_receipts(tenant_id);
CREATE INDEX IF NOT EXISTS idx_fiscal_receipts_branch_id ON barber.fiscal_receipts(branch_id);
CREATE INDEX IF NOT EXISTS idx_fiscal_receipts_status ON barber.fiscal_receipts(status);
CREATE INDEX IF NOT EXISTS idx_fiscal_receipts_created_at ON barber.fiscal_receipts(created_at);
CREATE INDEX IF NOT EXISTS idx_fiscal_receipts_tenant_status ON barber.fiscal_receipts(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_fiscal_receipts_tenant_branch ON barber.fiscal_receipts(tenant_id, branch_id);
CREATE TRIGGER trg_fiscal_receipts_updated_at BEFORE UPDATE ON barber.fiscal_receipts FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.fiscal_receipt_items (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_fiscal_receipt_items_tenant_id ON barber.fiscal_receipt_items(tenant_id);
CREATE INDEX IF NOT EXISTS idx_fiscal_receipt_items_branch_id ON barber.fiscal_receipt_items(branch_id);
CREATE INDEX IF NOT EXISTS idx_fiscal_receipt_items_status ON barber.fiscal_receipt_items(status);
CREATE INDEX IF NOT EXISTS idx_fiscal_receipt_items_created_at ON barber.fiscal_receipt_items(created_at);
CREATE INDEX IF NOT EXISTS idx_fiscal_receipt_items_tenant_status ON barber.fiscal_receipt_items(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_fiscal_receipt_items_tenant_branch ON barber.fiscal_receipt_items(tenant_id, branch_id);
CREATE TRIGGER trg_fiscal_receipt_items_updated_at BEFORE UPDATE ON barber.fiscal_receipt_items FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.nfse_provider_settings (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_nfse_provider_settings_tenant_id ON barber.nfse_provider_settings(tenant_id);
CREATE INDEX IF NOT EXISTS idx_nfse_provider_settings_branch_id ON barber.nfse_provider_settings(branch_id);
CREATE INDEX IF NOT EXISTS idx_nfse_provider_settings_status ON barber.nfse_provider_settings(status);
CREATE INDEX IF NOT EXISTS idx_nfse_provider_settings_created_at ON barber.nfse_provider_settings(created_at);
CREATE INDEX IF NOT EXISTS idx_nfse_provider_settings_tenant_status ON barber.nfse_provider_settings(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_nfse_provider_settings_tenant_branch ON barber.nfse_provider_settings(tenant_id, branch_id);
CREATE TRIGGER trg_nfse_provider_settings_updated_at BEFORE UPDATE ON barber.nfse_provider_settings FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.nfse_batches (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_nfse_batches_tenant_id ON barber.nfse_batches(tenant_id);
CREATE INDEX IF NOT EXISTS idx_nfse_batches_branch_id ON barber.nfse_batches(branch_id);
CREATE INDEX IF NOT EXISTS idx_nfse_batches_status ON barber.nfse_batches(status);
CREATE INDEX IF NOT EXISTS idx_nfse_batches_created_at ON barber.nfse_batches(created_at);
CREATE INDEX IF NOT EXISTS idx_nfse_batches_tenant_status ON barber.nfse_batches(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_nfse_batches_tenant_branch ON barber.nfse_batches(tenant_id, branch_id);
CREATE TRIGGER trg_nfse_batches_updated_at BEFORE UPDATE ON barber.nfse_batches FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.nfse_invoices (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_nfse_invoices_tenant_id ON barber.nfse_invoices(tenant_id);
CREATE INDEX IF NOT EXISTS idx_nfse_invoices_branch_id ON barber.nfse_invoices(branch_id);
CREATE INDEX IF NOT EXISTS idx_nfse_invoices_status ON barber.nfse_invoices(status);
CREATE INDEX IF NOT EXISTS idx_nfse_invoices_created_at ON barber.nfse_invoices(created_at);
CREATE INDEX IF NOT EXISTS idx_nfse_invoices_tenant_status ON barber.nfse_invoices(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_nfse_invoices_tenant_branch ON barber.nfse_invoices(tenant_id, branch_id);
CREATE TRIGGER trg_nfse_invoices_updated_at BEFORE UPDATE ON barber.nfse_invoices FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.tax_configurations (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_tax_configurations_tenant_id ON barber.tax_configurations(tenant_id);
CREATE INDEX IF NOT EXISTS idx_tax_configurations_branch_id ON barber.tax_configurations(branch_id);
CREATE INDEX IF NOT EXISTS idx_tax_configurations_status ON barber.tax_configurations(status);
CREATE INDEX IF NOT EXISTS idx_tax_configurations_created_at ON barber.tax_configurations(created_at);
CREATE INDEX IF NOT EXISTS idx_tax_configurations_tenant_status ON barber.tax_configurations(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_tax_configurations_tenant_branch ON barber.tax_configurations(tenant_id, branch_id);
CREATE TRIGGER trg_tax_configurations_updated_at BEFORE UPDATE ON barber.tax_configurations FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.tax_estimates (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_tax_estimates_tenant_id ON barber.tax_estimates(tenant_id);
CREATE INDEX IF NOT EXISTS idx_tax_estimates_branch_id ON barber.tax_estimates(branch_id);
CREATE INDEX IF NOT EXISTS idx_tax_estimates_status ON barber.tax_estimates(status);
CREATE INDEX IF NOT EXISTS idx_tax_estimates_created_at ON barber.tax_estimates(created_at);
CREATE INDEX IF NOT EXISTS idx_tax_estimates_tenant_status ON barber.tax_estimates(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_tax_estimates_tenant_branch ON barber.tax_estimates(tenant_id, branch_id);
CREATE TRIGGER trg_tax_estimates_updated_at BEFORE UPDATE ON barber.tax_estimates FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.products (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_products_tenant_id ON barber.products(tenant_id);
CREATE INDEX IF NOT EXISTS idx_products_branch_id ON barber.products(branch_id);
CREATE INDEX IF NOT EXISTS idx_products_status ON barber.products(status);
CREATE INDEX IF NOT EXISTS idx_products_created_at ON barber.products(created_at);
CREATE INDEX IF NOT EXISTS idx_products_tenant_status ON barber.products(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_products_tenant_branch ON barber.products(tenant_id, branch_id);
CREATE TRIGGER trg_products_updated_at BEFORE UPDATE ON barber.products FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.product_categories (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_product_categories_tenant_id ON barber.product_categories(tenant_id);
CREATE INDEX IF NOT EXISTS idx_product_categories_branch_id ON barber.product_categories(branch_id);
CREATE INDEX IF NOT EXISTS idx_product_categories_status ON barber.product_categories(status);
CREATE INDEX IF NOT EXISTS idx_product_categories_created_at ON barber.product_categories(created_at);
CREATE INDEX IF NOT EXISTS idx_product_categories_tenant_status ON barber.product_categories(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_product_categories_tenant_branch ON barber.product_categories(tenant_id, branch_id);
CREATE TRIGGER trg_product_categories_updated_at BEFORE UPDATE ON barber.product_categories FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.product_images (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_product_images_tenant_id ON barber.product_images(tenant_id);
CREATE INDEX IF NOT EXISTS idx_product_images_branch_id ON barber.product_images(branch_id);
CREATE INDEX IF NOT EXISTS idx_product_images_status ON barber.product_images(status);
CREATE INDEX IF NOT EXISTS idx_product_images_created_at ON barber.product_images(created_at);
CREATE INDEX IF NOT EXISTS idx_product_images_tenant_status ON barber.product_images(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_product_images_tenant_branch ON barber.product_images(tenant_id, branch_id);
CREATE TRIGGER trg_product_images_updated_at BEFORE UPDATE ON barber.product_images FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.stock_movements (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_stock_movements_tenant_id ON barber.stock_movements(tenant_id);
CREATE INDEX IF NOT EXISTS idx_stock_movements_branch_id ON barber.stock_movements(branch_id);
CREATE INDEX IF NOT EXISTS idx_stock_movements_status ON barber.stock_movements(status);
CREATE INDEX IF NOT EXISTS idx_stock_movements_created_at ON barber.stock_movements(created_at);
CREATE INDEX IF NOT EXISTS idx_stock_movements_tenant_status ON barber.stock_movements(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_stock_movements_tenant_branch ON barber.stock_movements(tenant_id, branch_id);
CREATE TRIGGER trg_stock_movements_updated_at BEFORE UPDATE ON barber.stock_movements FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.stock_balances (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_stock_balances_tenant_id ON barber.stock_balances(tenant_id);
CREATE INDEX IF NOT EXISTS idx_stock_balances_branch_id ON barber.stock_balances(branch_id);
CREATE INDEX IF NOT EXISTS idx_stock_balances_status ON barber.stock_balances(status);
CREATE INDEX IF NOT EXISTS idx_stock_balances_created_at ON barber.stock_balances(created_at);
CREATE INDEX IF NOT EXISTS idx_stock_balances_tenant_status ON barber.stock_balances(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_stock_balances_tenant_branch ON barber.stock_balances(tenant_id, branch_id);
CREATE TRIGGER trg_stock_balances_updated_at BEFORE UPDATE ON barber.stock_balances FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.stock_locations (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_stock_locations_tenant_id ON barber.stock_locations(tenant_id);
CREATE INDEX IF NOT EXISTS idx_stock_locations_branch_id ON barber.stock_locations(branch_id);
CREATE INDEX IF NOT EXISTS idx_stock_locations_status ON barber.stock_locations(status);
CREATE INDEX IF NOT EXISTS idx_stock_locations_created_at ON barber.stock_locations(created_at);
CREATE INDEX IF NOT EXISTS idx_stock_locations_tenant_status ON barber.stock_locations(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_stock_locations_tenant_branch ON barber.stock_locations(tenant_id, branch_id);
CREATE TRIGGER trg_stock_locations_updated_at BEFORE UPDATE ON barber.stock_locations FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.suppliers (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_suppliers_tenant_id ON barber.suppliers(tenant_id);
CREATE INDEX IF NOT EXISTS idx_suppliers_branch_id ON barber.suppliers(branch_id);
CREATE INDEX IF NOT EXISTS idx_suppliers_status ON barber.suppliers(status);
CREATE INDEX IF NOT EXISTS idx_suppliers_created_at ON barber.suppliers(created_at);
CREATE INDEX IF NOT EXISTS idx_suppliers_tenant_status ON barber.suppliers(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_suppliers_tenant_branch ON barber.suppliers(tenant_id, branch_id);
CREATE TRIGGER trg_suppliers_updated_at BEFORE UPDATE ON barber.suppliers FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.supplier_contacts (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_supplier_contacts_tenant_id ON barber.supplier_contacts(tenant_id);
CREATE INDEX IF NOT EXISTS idx_supplier_contacts_branch_id ON barber.supplier_contacts(branch_id);
CREATE INDEX IF NOT EXISTS idx_supplier_contacts_status ON barber.supplier_contacts(status);
CREATE INDEX IF NOT EXISTS idx_supplier_contacts_created_at ON barber.supplier_contacts(created_at);
CREATE INDEX IF NOT EXISTS idx_supplier_contacts_tenant_status ON barber.supplier_contacts(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_supplier_contacts_tenant_branch ON barber.supplier_contacts(tenant_id, branch_id);
CREATE TRIGGER trg_supplier_contacts_updated_at BEFORE UPDATE ON barber.supplier_contacts FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.supplier_products (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_supplier_products_tenant_id ON barber.supplier_products(tenant_id);
CREATE INDEX IF NOT EXISTS idx_supplier_products_branch_id ON barber.supplier_products(branch_id);
CREATE INDEX IF NOT EXISTS idx_supplier_products_status ON barber.supplier_products(status);
CREATE INDEX IF NOT EXISTS idx_supplier_products_created_at ON barber.supplier_products(created_at);
CREATE INDEX IF NOT EXISTS idx_supplier_products_tenant_status ON barber.supplier_products(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_supplier_products_tenant_branch ON barber.supplier_products(tenant_id, branch_id);
CREATE TRIGGER trg_supplier_products_updated_at BEFORE UPDATE ON barber.supplier_products FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.purchase_requests (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_purchase_requests_tenant_id ON barber.purchase_requests(tenant_id);
CREATE INDEX IF NOT EXISTS idx_purchase_requests_branch_id ON barber.purchase_requests(branch_id);
CREATE INDEX IF NOT EXISTS idx_purchase_requests_status ON barber.purchase_requests(status);
CREATE INDEX IF NOT EXISTS idx_purchase_requests_created_at ON barber.purchase_requests(created_at);
CREATE INDEX IF NOT EXISTS idx_purchase_requests_tenant_status ON barber.purchase_requests(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_purchase_requests_tenant_branch ON barber.purchase_requests(tenant_id, branch_id);
CREATE TRIGGER trg_purchase_requests_updated_at BEFORE UPDATE ON barber.purchase_requests FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.purchase_request_items (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_purchase_request_items_tenant_id ON barber.purchase_request_items(tenant_id);
CREATE INDEX IF NOT EXISTS idx_purchase_request_items_branch_id ON barber.purchase_request_items(branch_id);
CREATE INDEX IF NOT EXISTS idx_purchase_request_items_status ON barber.purchase_request_items(status);
CREATE INDEX IF NOT EXISTS idx_purchase_request_items_created_at ON barber.purchase_request_items(created_at);
CREATE INDEX IF NOT EXISTS idx_purchase_request_items_tenant_status ON barber.purchase_request_items(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_purchase_request_items_tenant_branch ON barber.purchase_request_items(tenant_id, branch_id);
CREATE TRIGGER trg_purchase_request_items_updated_at BEFORE UPDATE ON barber.purchase_request_items FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.purchase_orders (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_purchase_orders_tenant_id ON barber.purchase_orders(tenant_id);
CREATE INDEX IF NOT EXISTS idx_purchase_orders_branch_id ON barber.purchase_orders(branch_id);
CREATE INDEX IF NOT EXISTS idx_purchase_orders_status ON barber.purchase_orders(status);
CREATE INDEX IF NOT EXISTS idx_purchase_orders_created_at ON barber.purchase_orders(created_at);
CREATE INDEX IF NOT EXISTS idx_purchase_orders_tenant_status ON barber.purchase_orders(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_purchase_orders_tenant_branch ON barber.purchase_orders(tenant_id, branch_id);
CREATE TRIGGER trg_purchase_orders_updated_at BEFORE UPDATE ON barber.purchase_orders FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.purchase_order_items (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_purchase_order_items_tenant_id ON barber.purchase_order_items(tenant_id);
CREATE INDEX IF NOT EXISTS idx_purchase_order_items_branch_id ON barber.purchase_order_items(branch_id);
CREATE INDEX IF NOT EXISTS idx_purchase_order_items_status ON barber.purchase_order_items(status);
CREATE INDEX IF NOT EXISTS idx_purchase_order_items_created_at ON barber.purchase_order_items(created_at);
CREATE INDEX IF NOT EXISTS idx_purchase_order_items_tenant_status ON barber.purchase_order_items(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_purchase_order_items_tenant_branch ON barber.purchase_order_items(tenant_id, branch_id);
CREATE TRIGGER trg_purchase_order_items_updated_at BEFORE UPDATE ON barber.purchase_order_items FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.purchase_receipts (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_purchase_receipts_tenant_id ON barber.purchase_receipts(tenant_id);
CREATE INDEX IF NOT EXISTS idx_purchase_receipts_branch_id ON barber.purchase_receipts(branch_id);
CREATE INDEX IF NOT EXISTS idx_purchase_receipts_status ON barber.purchase_receipts(status);
CREATE INDEX IF NOT EXISTS idx_purchase_receipts_created_at ON barber.purchase_receipts(created_at);
CREATE INDEX IF NOT EXISTS idx_purchase_receipts_tenant_status ON barber.purchase_receipts(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_purchase_receipts_tenant_branch ON barber.purchase_receipts(tenant_id, branch_id);
CREATE TRIGGER trg_purchase_receipts_updated_at BEFORE UPDATE ON barber.purchase_receipts FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.purchase_quotes (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_purchase_quotes_tenant_id ON barber.purchase_quotes(tenant_id);
CREATE INDEX IF NOT EXISTS idx_purchase_quotes_branch_id ON barber.purchase_quotes(branch_id);
CREATE INDEX IF NOT EXISTS idx_purchase_quotes_status ON barber.purchase_quotes(status);
CREATE INDEX IF NOT EXISTS idx_purchase_quotes_created_at ON barber.purchase_quotes(created_at);
CREATE INDEX IF NOT EXISTS idx_purchase_quotes_tenant_status ON barber.purchase_quotes(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_purchase_quotes_tenant_branch ON barber.purchase_quotes(tenant_id, branch_id);
CREATE TRIGGER trg_purchase_quotes_updated_at BEFORE UPDATE ON barber.purchase_quotes FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.service_consumption_templates (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_service_consumption_templates_tenant_id ON barber.service_consumption_templates(tenant_id);
CREATE INDEX IF NOT EXISTS idx_service_consumption_templates_branch_id ON barber.service_consumption_templates(branch_id);
CREATE INDEX IF NOT EXISTS idx_service_consumption_templates_status ON barber.service_consumption_templates(status);
CREATE INDEX IF NOT EXISTS idx_service_consumption_templates_created_at ON barber.service_consumption_templates(created_at);
CREATE INDEX IF NOT EXISTS idx_service_consumption_templates_tenant_status ON barber.service_consumption_templates(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_service_consumption_templates_tenant_branch ON barber.service_consumption_templates(tenant_id, branch_id);
CREATE TRIGGER trg_service_consumption_templates_updated_at BEFORE UPDATE ON barber.service_consumption_templates FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.service_consumption_items (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_service_consumption_items_tenant_id ON barber.service_consumption_items(tenant_id);
CREATE INDEX IF NOT EXISTS idx_service_consumption_items_branch_id ON barber.service_consumption_items(branch_id);
CREATE INDEX IF NOT EXISTS idx_service_consumption_items_status ON barber.service_consumption_items(status);
CREATE INDEX IF NOT EXISTS idx_service_consumption_items_created_at ON barber.service_consumption_items(created_at);
CREATE INDEX IF NOT EXISTS idx_service_consumption_items_tenant_status ON barber.service_consumption_items(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_service_consumption_items_tenant_branch ON barber.service_consumption_items(tenant_id, branch_id);
CREATE TRIGGER trg_service_consumption_items_updated_at BEFORE UPDATE ON barber.service_consumption_items FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.client_segments (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_client_segments_tenant_id ON barber.client_segments(tenant_id);
CREATE INDEX IF NOT EXISTS idx_client_segments_branch_id ON barber.client_segments(branch_id);
CREATE INDEX IF NOT EXISTS idx_client_segments_status ON barber.client_segments(status);
CREATE INDEX IF NOT EXISTS idx_client_segments_created_at ON barber.client_segments(created_at);
CREATE INDEX IF NOT EXISTS idx_client_segments_tenant_status ON barber.client_segments(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_client_segments_tenant_branch ON barber.client_segments(tenant_id, branch_id);
CREATE TRIGGER trg_client_segments_updated_at BEFORE UPDATE ON barber.client_segments FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.client_relationship_events (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_client_relationship_events_tenant_id ON barber.client_relationship_events(tenant_id);
CREATE INDEX IF NOT EXISTS idx_client_relationship_events_branch_id ON barber.client_relationship_events(branch_id);
CREATE INDEX IF NOT EXISTS idx_client_relationship_events_status ON barber.client_relationship_events(status);
CREATE INDEX IF NOT EXISTS idx_client_relationship_events_created_at ON barber.client_relationship_events(created_at);
CREATE INDEX IF NOT EXISTS idx_client_relationship_events_tenant_status ON barber.client_relationship_events(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_client_relationship_events_tenant_branch ON barber.client_relationship_events(tenant_id, branch_id);
CREATE TRIGGER trg_client_relationship_events_updated_at BEFORE UPDATE ON barber.client_relationship_events FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.client_reactivation_campaigns (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_client_reactivation_campaigns_tenant_id ON barber.client_reactivation_campaigns(tenant_id);
CREATE INDEX IF NOT EXISTS idx_client_reactivation_campaigns_branch_id ON barber.client_reactivation_campaigns(branch_id);
CREATE INDEX IF NOT EXISTS idx_client_reactivation_campaigns_status ON barber.client_reactivation_campaigns(status);
CREATE INDEX IF NOT EXISTS idx_client_reactivation_campaigns_created_at ON barber.client_reactivation_campaigns(created_at);
CREATE INDEX IF NOT EXISTS idx_client_reactivation_campaigns_tenant_status ON barber.client_reactivation_campaigns(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_client_reactivation_campaigns_tenant_branch ON barber.client_reactivation_campaigns(tenant_id, branch_id);
CREATE TRIGGER trg_client_reactivation_campaigns_updated_at BEFORE UPDATE ON barber.client_reactivation_campaigns FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.marketing_campaigns (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_marketing_campaigns_tenant_id ON barber.marketing_campaigns(tenant_id);
CREATE INDEX IF NOT EXISTS idx_marketing_campaigns_branch_id ON barber.marketing_campaigns(branch_id);
CREATE INDEX IF NOT EXISTS idx_marketing_campaigns_status ON barber.marketing_campaigns(status);
CREATE INDEX IF NOT EXISTS idx_marketing_campaigns_created_at ON barber.marketing_campaigns(created_at);
CREATE INDEX IF NOT EXISTS idx_marketing_campaigns_tenant_status ON barber.marketing_campaigns(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_marketing_campaigns_tenant_branch ON barber.marketing_campaigns(tenant_id, branch_id);
CREATE TRIGGER trg_marketing_campaigns_updated_at BEFORE UPDATE ON barber.marketing_campaigns FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.campaign_rules (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_campaign_rules_tenant_id ON barber.campaign_rules(tenant_id);
CREATE INDEX IF NOT EXISTS idx_campaign_rules_branch_id ON barber.campaign_rules(branch_id);
CREATE INDEX IF NOT EXISTS idx_campaign_rules_status ON barber.campaign_rules(status);
CREATE INDEX IF NOT EXISTS idx_campaign_rules_created_at ON barber.campaign_rules(created_at);
CREATE INDEX IF NOT EXISTS idx_campaign_rules_tenant_status ON barber.campaign_rules(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_campaign_rules_tenant_branch ON barber.campaign_rules(tenant_id, branch_id);
CREATE TRIGGER trg_campaign_rules_updated_at BEFORE UPDATE ON barber.campaign_rules FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.campaign_audiences (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_campaign_audiences_tenant_id ON barber.campaign_audiences(tenant_id);
CREATE INDEX IF NOT EXISTS idx_campaign_audiences_branch_id ON barber.campaign_audiences(branch_id);
CREATE INDEX IF NOT EXISTS idx_campaign_audiences_status ON barber.campaign_audiences(status);
CREATE INDEX IF NOT EXISTS idx_campaign_audiences_created_at ON barber.campaign_audiences(created_at);
CREATE INDEX IF NOT EXISTS idx_campaign_audiences_tenant_status ON barber.campaign_audiences(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_campaign_audiences_tenant_branch ON barber.campaign_audiences(tenant_id, branch_id);
CREATE TRIGGER trg_campaign_audiences_updated_at BEFORE UPDATE ON barber.campaign_audiences FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.campaign_coupons (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_campaign_coupons_tenant_id ON barber.campaign_coupons(tenant_id);
CREATE INDEX IF NOT EXISTS idx_campaign_coupons_branch_id ON barber.campaign_coupons(branch_id);
CREATE INDEX IF NOT EXISTS idx_campaign_coupons_status ON barber.campaign_coupons(status);
CREATE INDEX IF NOT EXISTS idx_campaign_coupons_created_at ON barber.campaign_coupons(created_at);
CREATE INDEX IF NOT EXISTS idx_campaign_coupons_tenant_status ON barber.campaign_coupons(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_campaign_coupons_tenant_branch ON barber.campaign_coupons(tenant_id, branch_id);
CREATE TRIGGER trg_campaign_coupons_updated_at BEFORE UPDATE ON barber.campaign_coupons FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.campaign_results (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_campaign_results_tenant_id ON barber.campaign_results(tenant_id);
CREATE INDEX IF NOT EXISTS idx_campaign_results_branch_id ON barber.campaign_results(branch_id);
CREATE INDEX IF NOT EXISTS idx_campaign_results_status ON barber.campaign_results(status);
CREATE INDEX IF NOT EXISTS idx_campaign_results_created_at ON barber.campaign_results(created_at);
CREATE INDEX IF NOT EXISTS idx_campaign_results_tenant_status ON barber.campaign_results(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_campaign_results_tenant_branch ON barber.campaign_results(tenant_id, branch_id);
CREATE TRIGGER trg_campaign_results_updated_at BEFORE UPDATE ON barber.campaign_results FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.retention_risk_scores (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_retention_risk_scores_tenant_id ON barber.retention_risk_scores(tenant_id);
CREATE INDEX IF NOT EXISTS idx_retention_risk_scores_branch_id ON barber.retention_risk_scores(branch_id);
CREATE INDEX IF NOT EXISTS idx_retention_risk_scores_status ON barber.retention_risk_scores(status);
CREATE INDEX IF NOT EXISTS idx_retention_risk_scores_created_at ON barber.retention_risk_scores(created_at);
CREATE INDEX IF NOT EXISTS idx_retention_risk_scores_tenant_status ON barber.retention_risk_scores(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_retention_risk_scores_tenant_branch ON barber.retention_risk_scores(tenant_id, branch_id);
CREATE TRIGGER trg_retention_risk_scores_updated_at BEFORE UPDATE ON barber.retention_risk_scores FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.upsell_recommendations (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_upsell_recommendations_tenant_id ON barber.upsell_recommendations(tenant_id);
CREATE INDEX IF NOT EXISTS idx_upsell_recommendations_branch_id ON barber.upsell_recommendations(branch_id);
CREATE INDEX IF NOT EXISTS idx_upsell_recommendations_status ON barber.upsell_recommendations(status);
CREATE INDEX IF NOT EXISTS idx_upsell_recommendations_created_at ON barber.upsell_recommendations(created_at);
CREATE INDEX IF NOT EXISTS idx_upsell_recommendations_tenant_status ON barber.upsell_recommendations(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_upsell_recommendations_tenant_branch ON barber.upsell_recommendations(tenant_id, branch_id);
CREATE TRIGGER trg_upsell_recommendations_updated_at BEFORE UPDATE ON barber.upsell_recommendations FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.customer_journeys (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_customer_journeys_tenant_id ON barber.customer_journeys(tenant_id);
CREATE INDEX IF NOT EXISTS idx_customer_journeys_branch_id ON barber.customer_journeys(branch_id);
CREATE INDEX IF NOT EXISTS idx_customer_journeys_status ON barber.customer_journeys(status);
CREATE INDEX IF NOT EXISTS idx_customer_journeys_created_at ON barber.customer_journeys(created_at);
CREATE INDEX IF NOT EXISTS idx_customer_journeys_tenant_status ON barber.customer_journeys(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_customer_journeys_tenant_branch ON barber.customer_journeys(tenant_id, branch_id);
CREATE TRIGGER trg_customer_journeys_updated_at BEFORE UPDATE ON barber.customer_journeys FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.journey_steps (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_journey_steps_tenant_id ON barber.journey_steps(tenant_id);
CREATE INDEX IF NOT EXISTS idx_journey_steps_branch_id ON barber.journey_steps(branch_id);
CREATE INDEX IF NOT EXISTS idx_journey_steps_status ON barber.journey_steps(status);
CREATE INDEX IF NOT EXISTS idx_journey_steps_created_at ON barber.journey_steps(created_at);
CREATE INDEX IF NOT EXISTS idx_journey_steps_tenant_status ON barber.journey_steps(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_journey_steps_tenant_branch ON barber.journey_steps(tenant_id, branch_id);
CREATE TRIGGER trg_journey_steps_updated_at BEFORE UPDATE ON barber.journey_steps FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.copilot_conversations (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_copilot_conversations_tenant_id ON barber.copilot_conversations(tenant_id);
CREATE INDEX IF NOT EXISTS idx_copilot_conversations_branch_id ON barber.copilot_conversations(branch_id);
CREATE INDEX IF NOT EXISTS idx_copilot_conversations_status ON barber.copilot_conversations(status);
CREATE INDEX IF NOT EXISTS idx_copilot_conversations_created_at ON barber.copilot_conversations(created_at);
CREATE INDEX IF NOT EXISTS idx_copilot_conversations_tenant_status ON barber.copilot_conversations(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_copilot_conversations_tenant_branch ON barber.copilot_conversations(tenant_id, branch_id);
CREATE TRIGGER trg_copilot_conversations_updated_at BEFORE UPDATE ON barber.copilot_conversations FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.copilot_messages (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_copilot_messages_tenant_id ON barber.copilot_messages(tenant_id);
CREATE INDEX IF NOT EXISTS idx_copilot_messages_branch_id ON barber.copilot_messages(branch_id);
CREATE INDEX IF NOT EXISTS idx_copilot_messages_status ON barber.copilot_messages(status);
CREATE INDEX IF NOT EXISTS idx_copilot_messages_created_at ON barber.copilot_messages(created_at);
CREATE INDEX IF NOT EXISTS idx_copilot_messages_tenant_status ON barber.copilot_messages(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_copilot_messages_tenant_branch ON barber.copilot_messages(tenant_id, branch_id);
CREATE TRIGGER trg_copilot_messages_updated_at BEFORE UPDATE ON barber.copilot_messages FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.copilot_suggestions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_copilot_suggestions_tenant_id ON barber.copilot_suggestions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_copilot_suggestions_branch_id ON barber.copilot_suggestions(branch_id);
CREATE INDEX IF NOT EXISTS idx_copilot_suggestions_status ON barber.copilot_suggestions(status);
CREATE INDEX IF NOT EXISTS idx_copilot_suggestions_created_at ON barber.copilot_suggestions(created_at);
CREATE INDEX IF NOT EXISTS idx_copilot_suggestions_tenant_status ON barber.copilot_suggestions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_copilot_suggestions_tenant_branch ON barber.copilot_suggestions(tenant_id, branch_id);
CREATE TRIGGER trg_copilot_suggestions_updated_at BEFORE UPDATE ON barber.copilot_suggestions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.copilot_actions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_copilot_actions_tenant_id ON barber.copilot_actions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_copilot_actions_branch_id ON barber.copilot_actions(branch_id);
CREATE INDEX IF NOT EXISTS idx_copilot_actions_status ON barber.copilot_actions(status);
CREATE INDEX IF NOT EXISTS idx_copilot_actions_created_at ON barber.copilot_actions(created_at);
CREATE INDEX IF NOT EXISTS idx_copilot_actions_tenant_status ON barber.copilot_actions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_copilot_actions_tenant_branch ON barber.copilot_actions(tenant_id, branch_id);
CREATE TRIGGER trg_copilot_actions_updated_at BEFORE UPDATE ON barber.copilot_actions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.service_recognitions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_service_recognitions_tenant_id ON barber.service_recognitions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_service_recognitions_branch_id ON barber.service_recognitions(branch_id);
CREATE INDEX IF NOT EXISTS idx_service_recognitions_status ON barber.service_recognitions(status);
CREATE INDEX IF NOT EXISTS idx_service_recognitions_created_at ON barber.service_recognitions(created_at);
CREATE INDEX IF NOT EXISTS idx_service_recognitions_tenant_status ON barber.service_recognitions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_service_recognitions_tenant_branch ON barber.service_recognitions(tenant_id, branch_id);
CREATE TRIGGER trg_service_recognitions_updated_at BEFORE UPDATE ON barber.service_recognitions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.ai_service_metrics (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_ai_service_metrics_tenant_id ON barber.ai_service_metrics(tenant_id);
CREATE INDEX IF NOT EXISTS idx_ai_service_metrics_branch_id ON barber.ai_service_metrics(branch_id);
CREATE INDEX IF NOT EXISTS idx_ai_service_metrics_status ON barber.ai_service_metrics(status);
CREATE INDEX IF NOT EXISTS idx_ai_service_metrics_created_at ON barber.ai_service_metrics(created_at);
CREATE INDEX IF NOT EXISTS idx_ai_service_metrics_tenant_status ON barber.ai_service_metrics(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_ai_service_metrics_tenant_branch ON barber.ai_service_metrics(tenant_id, branch_id);
CREATE TRIGGER trg_ai_service_metrics_updated_at BEFORE UPDATE ON barber.ai_service_metrics FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.ai_recognition_jobs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_ai_recognition_jobs_tenant_id ON barber.ai_recognition_jobs(tenant_id);
CREATE INDEX IF NOT EXISTS idx_ai_recognition_jobs_branch_id ON barber.ai_recognition_jobs(branch_id);
CREATE INDEX IF NOT EXISTS idx_ai_recognition_jobs_status ON barber.ai_recognition_jobs(status);
CREATE INDEX IF NOT EXISTS idx_ai_recognition_jobs_created_at ON barber.ai_recognition_jobs(created_at);
CREATE INDEX IF NOT EXISTS idx_ai_recognition_jobs_tenant_status ON barber.ai_recognition_jobs(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_ai_recognition_jobs_tenant_branch ON barber.ai_recognition_jobs(tenant_id, branch_id);
CREATE TRIGGER trg_ai_recognition_jobs_updated_at BEFORE UPDATE ON barber.ai_recognition_jobs FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.ai_recognition_results (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_ai_recognition_results_tenant_id ON barber.ai_recognition_results(tenant_id);
CREATE INDEX IF NOT EXISTS idx_ai_recognition_results_branch_id ON barber.ai_recognition_results(branch_id);
CREATE INDEX IF NOT EXISTS idx_ai_recognition_results_status ON barber.ai_recognition_results(status);
CREATE INDEX IF NOT EXISTS idx_ai_recognition_results_created_at ON barber.ai_recognition_results(created_at);
CREATE INDEX IF NOT EXISTS idx_ai_recognition_results_tenant_status ON barber.ai_recognition_results(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_ai_recognition_results_tenant_branch ON barber.ai_recognition_results(tenant_id, branch_id);
CREATE TRIGGER trg_ai_recognition_results_updated_at BEFORE UPDATE ON barber.ai_recognition_results FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.ai_prediction_models (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_ai_prediction_models_tenant_id ON barber.ai_prediction_models(tenant_id);
CREATE INDEX IF NOT EXISTS idx_ai_prediction_models_branch_id ON barber.ai_prediction_models(branch_id);
CREATE INDEX IF NOT EXISTS idx_ai_prediction_models_status ON barber.ai_prediction_models(status);
CREATE INDEX IF NOT EXISTS idx_ai_prediction_models_created_at ON barber.ai_prediction_models(created_at);
CREATE INDEX IF NOT EXISTS idx_ai_prediction_models_tenant_status ON barber.ai_prediction_models(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_ai_prediction_models_tenant_branch ON barber.ai_prediction_models(tenant_id, branch_id);
CREATE TRIGGER trg_ai_prediction_models_updated_at BEFORE UPDATE ON barber.ai_prediction_models FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.ai_prediction_results (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_ai_prediction_results_tenant_id ON barber.ai_prediction_results(tenant_id);
CREATE INDEX IF NOT EXISTS idx_ai_prediction_results_branch_id ON barber.ai_prediction_results(branch_id);
CREATE INDEX IF NOT EXISTS idx_ai_prediction_results_status ON barber.ai_prediction_results(status);
CREATE INDEX IF NOT EXISTS idx_ai_prediction_results_created_at ON barber.ai_prediction_results(created_at);
CREATE INDEX IF NOT EXISTS idx_ai_prediction_results_tenant_status ON barber.ai_prediction_results(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_ai_prediction_results_tenant_branch ON barber.ai_prediction_results(tenant_id, branch_id);
CREATE TRIGGER trg_ai_prediction_results_updated_at BEFORE UPDATE ON barber.ai_prediction_results FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.ai_scores (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_ai_scores_tenant_id ON barber.ai_scores(tenant_id);
CREATE INDEX IF NOT EXISTS idx_ai_scores_branch_id ON barber.ai_scores(branch_id);
CREATE INDEX IF NOT EXISTS idx_ai_scores_status ON barber.ai_scores(status);
CREATE INDEX IF NOT EXISTS idx_ai_scores_created_at ON barber.ai_scores(created_at);
CREATE INDEX IF NOT EXISTS idx_ai_scores_tenant_status ON barber.ai_scores(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_ai_scores_tenant_branch ON barber.ai_scores(tenant_id, branch_id);
CREATE TRIGGER trg_ai_scores_updated_at BEFORE UPDATE ON barber.ai_scores FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.ai_agents (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_ai_agents_tenant_id ON barber.ai_agents(tenant_id);
CREATE INDEX IF NOT EXISTS idx_ai_agents_branch_id ON barber.ai_agents(branch_id);
CREATE INDEX IF NOT EXISTS idx_ai_agents_status ON barber.ai_agents(status);
CREATE INDEX IF NOT EXISTS idx_ai_agents_created_at ON barber.ai_agents(created_at);
CREATE INDEX IF NOT EXISTS idx_ai_agents_tenant_status ON barber.ai_agents(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_ai_agents_tenant_branch ON barber.ai_agents(tenant_id, branch_id);
CREATE TRIGGER trg_ai_agents_updated_at BEFORE UPDATE ON barber.ai_agents FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.ai_agent_suggestions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_ai_agent_suggestions_tenant_id ON barber.ai_agent_suggestions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_ai_agent_suggestions_branch_id ON barber.ai_agent_suggestions(branch_id);
CREATE INDEX IF NOT EXISTS idx_ai_agent_suggestions_status ON barber.ai_agent_suggestions(status);
CREATE INDEX IF NOT EXISTS idx_ai_agent_suggestions_created_at ON barber.ai_agent_suggestions(created_at);
CREATE INDEX IF NOT EXISTS idx_ai_agent_suggestions_tenant_status ON barber.ai_agent_suggestions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_ai_agent_suggestions_tenant_branch ON barber.ai_agent_suggestions(tenant_id, branch_id);
CREATE TRIGGER trg_ai_agent_suggestions_updated_at BEFORE UPDATE ON barber.ai_agent_suggestions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.ai_agent_actions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_ai_agent_actions_tenant_id ON barber.ai_agent_actions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_ai_agent_actions_branch_id ON barber.ai_agent_actions(branch_id);
CREATE INDEX IF NOT EXISTS idx_ai_agent_actions_status ON barber.ai_agent_actions(status);
CREATE INDEX IF NOT EXISTS idx_ai_agent_actions_created_at ON barber.ai_agent_actions(created_at);
CREATE INDEX IF NOT EXISTS idx_ai_agent_actions_tenant_status ON barber.ai_agent_actions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_ai_agent_actions_tenant_branch ON barber.ai_agent_actions(tenant_id, branch_id);
CREATE TRIGGER trg_ai_agent_actions_updated_at BEFORE UPDATE ON barber.ai_agent_actions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.ai_multimodal_requests (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_ai_multimodal_requests_tenant_id ON barber.ai_multimodal_requests(tenant_id);
CREATE INDEX IF NOT EXISTS idx_ai_multimodal_requests_branch_id ON barber.ai_multimodal_requests(branch_id);
CREATE INDEX IF NOT EXISTS idx_ai_multimodal_requests_status ON barber.ai_multimodal_requests(status);
CREATE INDEX IF NOT EXISTS idx_ai_multimodal_requests_created_at ON barber.ai_multimodal_requests(created_at);
CREATE INDEX IF NOT EXISTS idx_ai_multimodal_requests_tenant_status ON barber.ai_multimodal_requests(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_ai_multimodal_requests_tenant_branch ON barber.ai_multimodal_requests(tenant_id, branch_id);
CREATE TRIGGER trg_ai_multimodal_requests_updated_at BEFORE UPDATE ON barber.ai_multimodal_requests FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.marketplace_profiles (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_marketplace_profiles_tenant_id ON barber.marketplace_profiles(tenant_id);
CREATE INDEX IF NOT EXISTS idx_marketplace_profiles_branch_id ON barber.marketplace_profiles(branch_id);
CREATE INDEX IF NOT EXISTS idx_marketplace_profiles_status ON barber.marketplace_profiles(status);
CREATE INDEX IF NOT EXISTS idx_marketplace_profiles_created_at ON barber.marketplace_profiles(created_at);
CREATE INDEX IF NOT EXISTS idx_marketplace_profiles_tenant_status ON barber.marketplace_profiles(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_marketplace_profiles_tenant_branch ON barber.marketplace_profiles(tenant_id, branch_id);
CREATE TRIGGER trg_marketplace_profiles_updated_at BEFORE UPDATE ON barber.marketplace_profiles FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.marketplace_categories (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_marketplace_categories_tenant_id ON barber.marketplace_categories(tenant_id);
CREATE INDEX IF NOT EXISTS idx_marketplace_categories_branch_id ON barber.marketplace_categories(branch_id);
CREATE INDEX IF NOT EXISTS idx_marketplace_categories_status ON barber.marketplace_categories(status);
CREATE INDEX IF NOT EXISTS idx_marketplace_categories_created_at ON barber.marketplace_categories(created_at);
CREATE INDEX IF NOT EXISTS idx_marketplace_categories_tenant_status ON barber.marketplace_categories(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_marketplace_categories_tenant_branch ON barber.marketplace_categories(tenant_id, branch_id);
CREATE TRIGGER trg_marketplace_categories_updated_at BEFORE UPDATE ON barber.marketplace_categories FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.marketplace_promotions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_marketplace_promotions_tenant_id ON barber.marketplace_promotions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_marketplace_promotions_branch_id ON barber.marketplace_promotions(branch_id);
CREATE INDEX IF NOT EXISTS idx_marketplace_promotions_status ON barber.marketplace_promotions(status);
CREATE INDEX IF NOT EXISTS idx_marketplace_promotions_created_at ON barber.marketplace_promotions(created_at);
CREATE INDEX IF NOT EXISTS idx_marketplace_promotions_tenant_status ON barber.marketplace_promotions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_marketplace_promotions_tenant_branch ON barber.marketplace_promotions(tenant_id, branch_id);
CREATE TRIGGER trg_marketplace_promotions_updated_at BEFORE UPDATE ON barber.marketplace_promotions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.marketplace_search_logs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_marketplace_search_logs_tenant_id ON barber.marketplace_search_logs(tenant_id);
CREATE INDEX IF NOT EXISTS idx_marketplace_search_logs_branch_id ON barber.marketplace_search_logs(branch_id);
CREATE INDEX IF NOT EXISTS idx_marketplace_search_logs_status ON barber.marketplace_search_logs(status);
CREATE INDEX IF NOT EXISTS idx_marketplace_search_logs_created_at ON barber.marketplace_search_logs(created_at);
CREATE INDEX IF NOT EXISTS idx_marketplace_search_logs_tenant_status ON barber.marketplace_search_logs(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_marketplace_search_logs_tenant_branch ON barber.marketplace_search_logs(tenant_id, branch_id);
CREATE TRIGGER trg_marketplace_search_logs_updated_at BEFORE UPDATE ON barber.marketplace_search_logs FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.professional_marketplace_profiles (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_professional_marketplace_profiles_tenant_id ON barber.professional_marketplace_profiles(tenant_id);
CREATE INDEX IF NOT EXISTS idx_professional_marketplace_profiles_branch_id ON barber.professional_marketplace_profiles(branch_id);
CREATE INDEX IF NOT EXISTS idx_professional_marketplace_profiles_status ON barber.professional_marketplace_profiles(status);
CREATE INDEX IF NOT EXISTS idx_professional_marketplace_profiles_created_at ON barber.professional_marketplace_profiles(created_at);
CREATE INDEX IF NOT EXISTS idx_professional_marketplace_profiles_tenant_status ON barber.professional_marketplace_profiles(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_professional_marketplace_profiles_tenant_branch ON barber.professional_marketplace_profiles(tenant_id, branch_id);
CREATE TRIGGER trg_professional_marketplace_profiles_updated_at BEFORE UPDATE ON barber.professional_marketplace_profiles FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.public_reputation_scores (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_public_reputation_scores_tenant_id ON barber.public_reputation_scores(tenant_id);
CREATE INDEX IF NOT EXISTS idx_public_reputation_scores_branch_id ON barber.public_reputation_scores(branch_id);
CREATE INDEX IF NOT EXISTS idx_public_reputation_scores_status ON barber.public_reputation_scores(status);
CREATE INDEX IF NOT EXISTS idx_public_reputation_scores_created_at ON barber.public_reputation_scores(created_at);
CREATE INDEX IF NOT EXISTS idx_public_reputation_scores_tenant_status ON barber.public_reputation_scores(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_public_reputation_scores_tenant_branch ON barber.public_reputation_scores(tenant_id, branch_id);
CREATE TRIGGER trg_public_reputation_scores_updated_at BEFORE UPDATE ON barber.public_reputation_scores FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.public_rankings (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_public_rankings_tenant_id ON barber.public_rankings(tenant_id);
CREATE INDEX IF NOT EXISTS idx_public_rankings_branch_id ON barber.public_rankings(branch_id);
CREATE INDEX IF NOT EXISTS idx_public_rankings_status ON barber.public_rankings(status);
CREATE INDEX IF NOT EXISTS idx_public_rankings_created_at ON barber.public_rankings(created_at);
CREATE INDEX IF NOT EXISTS idx_public_rankings_tenant_status ON barber.public_rankings(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_public_rankings_tenant_branch ON barber.public_rankings(tenant_id, branch_id);
CREATE TRIGGER trg_public_rankings_updated_at BEFORE UPDATE ON barber.public_rankings FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.b2b_marketplace_suppliers (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_b2b_marketplace_suppliers_tenant_id ON barber.b2b_marketplace_suppliers(tenant_id);
CREATE INDEX IF NOT EXISTS idx_b2b_marketplace_suppliers_branch_id ON barber.b2b_marketplace_suppliers(branch_id);
CREATE INDEX IF NOT EXISTS idx_b2b_marketplace_suppliers_status ON barber.b2b_marketplace_suppliers(status);
CREATE INDEX IF NOT EXISTS idx_b2b_marketplace_suppliers_created_at ON barber.b2b_marketplace_suppliers(created_at);
CREATE INDEX IF NOT EXISTS idx_b2b_marketplace_suppliers_tenant_status ON barber.b2b_marketplace_suppliers(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_b2b_marketplace_suppliers_tenant_branch ON barber.b2b_marketplace_suppliers(tenant_id, branch_id);
CREATE TRIGGER trg_b2b_marketplace_suppliers_updated_at BEFORE UPDATE ON barber.b2b_marketplace_suppliers FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.b2b_marketplace_products (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_b2b_marketplace_products_tenant_id ON barber.b2b_marketplace_products(tenant_id);
CREATE INDEX IF NOT EXISTS idx_b2b_marketplace_products_branch_id ON barber.b2b_marketplace_products(branch_id);
CREATE INDEX IF NOT EXISTS idx_b2b_marketplace_products_status ON barber.b2b_marketplace_products(status);
CREATE INDEX IF NOT EXISTS idx_b2b_marketplace_products_created_at ON barber.b2b_marketplace_products(created_at);
CREATE INDEX IF NOT EXISTS idx_b2b_marketplace_products_tenant_status ON barber.b2b_marketplace_products(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_b2b_marketplace_products_tenant_branch ON barber.b2b_marketplace_products(tenant_id, branch_id);
CREATE TRIGGER trg_b2b_marketplace_products_updated_at BEFORE UPDATE ON barber.b2b_marketplace_products FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.b2b_marketplace_orders (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_b2b_marketplace_orders_tenant_id ON barber.b2b_marketplace_orders(tenant_id);
CREATE INDEX IF NOT EXISTS idx_b2b_marketplace_orders_branch_id ON barber.b2b_marketplace_orders(branch_id);
CREATE INDEX IF NOT EXISTS idx_b2b_marketplace_orders_status ON barber.b2b_marketplace_orders(status);
CREATE INDEX IF NOT EXISTS idx_b2b_marketplace_orders_created_at ON barber.b2b_marketplace_orders(created_at);
CREATE INDEX IF NOT EXISTS idx_b2b_marketplace_orders_tenant_status ON barber.b2b_marketplace_orders(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_b2b_marketplace_orders_tenant_branch ON barber.b2b_marketplace_orders(tenant_id, branch_id);
CREATE TRIGGER trg_b2b_marketplace_orders_updated_at BEFORE UPDATE ON barber.b2b_marketplace_orders FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.group_purchase_campaigns (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_group_purchase_campaigns_tenant_id ON barber.group_purchase_campaigns(tenant_id);
CREATE INDEX IF NOT EXISTS idx_group_purchase_campaigns_branch_id ON barber.group_purchase_campaigns(branch_id);
CREATE INDEX IF NOT EXISTS idx_group_purchase_campaigns_status ON barber.group_purchase_campaigns(status);
CREATE INDEX IF NOT EXISTS idx_group_purchase_campaigns_created_at ON barber.group_purchase_campaigns(created_at);
CREATE INDEX IF NOT EXISTS idx_group_purchase_campaigns_tenant_status ON barber.group_purchase_campaigns(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_group_purchase_campaigns_tenant_branch ON barber.group_purchase_campaigns(tenant_id, branch_id);
CREATE TRIGGER trg_group_purchase_campaigns_updated_at BEFORE UPDATE ON barber.group_purchase_campaigns FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.support_tickets (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_support_tickets_tenant_id ON barber.support_tickets(tenant_id);
CREATE INDEX IF NOT EXISTS idx_support_tickets_branch_id ON barber.support_tickets(branch_id);
CREATE INDEX IF NOT EXISTS idx_support_tickets_status ON barber.support_tickets(status);
CREATE INDEX IF NOT EXISTS idx_support_tickets_created_at ON barber.support_tickets(created_at);
CREATE INDEX IF NOT EXISTS idx_support_tickets_tenant_status ON barber.support_tickets(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_support_tickets_tenant_branch ON barber.support_tickets(tenant_id, branch_id);
CREATE TRIGGER trg_support_tickets_updated_at BEFORE UPDATE ON barber.support_tickets FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.support_ticket_messages (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_support_ticket_messages_tenant_id ON barber.support_ticket_messages(tenant_id);
CREATE INDEX IF NOT EXISTS idx_support_ticket_messages_branch_id ON barber.support_ticket_messages(branch_id);
CREATE INDEX IF NOT EXISTS idx_support_ticket_messages_status ON barber.support_ticket_messages(status);
CREATE INDEX IF NOT EXISTS idx_support_ticket_messages_created_at ON barber.support_ticket_messages(created_at);
CREATE INDEX IF NOT EXISTS idx_support_ticket_messages_tenant_status ON barber.support_ticket_messages(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_support_ticket_messages_tenant_branch ON barber.support_ticket_messages(tenant_id, branch_id);
CREATE TRIGGER trg_support_ticket_messages_updated_at BEFORE UPDATE ON barber.support_ticket_messages FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.support_categories (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_support_categories_tenant_id ON barber.support_categories(tenant_id);
CREATE INDEX IF NOT EXISTS idx_support_categories_branch_id ON barber.support_categories(branch_id);
CREATE INDEX IF NOT EXISTS idx_support_categories_status ON barber.support_categories(status);
CREATE INDEX IF NOT EXISTS idx_support_categories_created_at ON barber.support_categories(created_at);
CREATE INDEX IF NOT EXISTS idx_support_categories_tenant_status ON barber.support_categories(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_support_categories_tenant_branch ON barber.support_categories(tenant_id, branch_id);
CREATE TRIGGER trg_support_categories_updated_at BEFORE UPDATE ON barber.support_categories FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.knowledge_base_articles (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_knowledge_base_articles_tenant_id ON barber.knowledge_base_articles(tenant_id);
CREATE INDEX IF NOT EXISTS idx_knowledge_base_articles_branch_id ON barber.knowledge_base_articles(branch_id);
CREATE INDEX IF NOT EXISTS idx_knowledge_base_articles_status ON barber.knowledge_base_articles(status);
CREATE INDEX IF NOT EXISTS idx_knowledge_base_articles_created_at ON barber.knowledge_base_articles(created_at);
CREATE INDEX IF NOT EXISTS idx_knowledge_base_articles_tenant_status ON barber.knowledge_base_articles(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_knowledge_base_articles_tenant_branch ON barber.knowledge_base_articles(tenant_id, branch_id);
CREATE TRIGGER trg_knowledge_base_articles_updated_at BEFORE UPDATE ON barber.knowledge_base_articles FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.feature_requests (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_feature_requests_tenant_id ON barber.feature_requests(tenant_id);
CREATE INDEX IF NOT EXISTS idx_feature_requests_branch_id ON barber.feature_requests(branch_id);
CREATE INDEX IF NOT EXISTS idx_feature_requests_status ON barber.feature_requests(status);
CREATE INDEX IF NOT EXISTS idx_feature_requests_created_at ON barber.feature_requests(created_at);
CREATE INDEX IF NOT EXISTS idx_feature_requests_tenant_status ON barber.feature_requests(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_feature_requests_tenant_branch ON barber.feature_requests(tenant_id, branch_id);
CREATE TRIGGER trg_feature_requests_updated_at BEFORE UPDATE ON barber.feature_requests FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.notifications (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_notifications_tenant_id ON barber.notifications(tenant_id);
CREATE INDEX IF NOT EXISTS idx_notifications_branch_id ON barber.notifications(branch_id);
CREATE INDEX IF NOT EXISTS idx_notifications_status ON barber.notifications(status);
CREATE INDEX IF NOT EXISTS idx_notifications_created_at ON barber.notifications(created_at);
CREATE INDEX IF NOT EXISTS idx_notifications_tenant_status ON barber.notifications(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_notifications_tenant_branch ON barber.notifications(tenant_id, branch_id);
CREATE TRIGGER trg_notifications_updated_at BEFORE UPDATE ON barber.notifications FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.notification_templates (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_notification_templates_tenant_id ON barber.notification_templates(tenant_id);
CREATE INDEX IF NOT EXISTS idx_notification_templates_branch_id ON barber.notification_templates(branch_id);
CREATE INDEX IF NOT EXISTS idx_notification_templates_status ON barber.notification_templates(status);
CREATE INDEX IF NOT EXISTS idx_notification_templates_created_at ON barber.notification_templates(created_at);
CREATE INDEX IF NOT EXISTS idx_notification_templates_tenant_status ON barber.notification_templates(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_notification_templates_tenant_branch ON barber.notification_templates(tenant_id, branch_id);
CREATE TRIGGER trg_notification_templates_updated_at BEFORE UPDATE ON barber.notification_templates FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.notification_deliveries (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_notification_deliveries_tenant_id ON barber.notification_deliveries(tenant_id);
CREATE INDEX IF NOT EXISTS idx_notification_deliveries_branch_id ON barber.notification_deliveries(branch_id);
CREATE INDEX IF NOT EXISTS idx_notification_deliveries_status ON barber.notification_deliveries(status);
CREATE INDEX IF NOT EXISTS idx_notification_deliveries_created_at ON barber.notification_deliveries(created_at);
CREATE INDEX IF NOT EXISTS idx_notification_deliveries_tenant_status ON barber.notification_deliveries(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_notification_deliveries_tenant_branch ON barber.notification_deliveries(tenant_id, branch_id);
CREATE TRIGGER trg_notification_deliveries_updated_at BEFORE UPDATE ON barber.notification_deliveries FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.whatsapp_accounts (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_whatsapp_accounts_tenant_id ON barber.whatsapp_accounts(tenant_id);
CREATE INDEX IF NOT EXISTS idx_whatsapp_accounts_branch_id ON barber.whatsapp_accounts(branch_id);
CREATE INDEX IF NOT EXISTS idx_whatsapp_accounts_status ON barber.whatsapp_accounts(status);
CREATE INDEX IF NOT EXISTS idx_whatsapp_accounts_created_at ON barber.whatsapp_accounts(created_at);
CREATE INDEX IF NOT EXISTS idx_whatsapp_accounts_tenant_status ON barber.whatsapp_accounts(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_whatsapp_accounts_tenant_branch ON barber.whatsapp_accounts(tenant_id, branch_id);
CREATE TRIGGER trg_whatsapp_accounts_updated_at BEFORE UPDATE ON barber.whatsapp_accounts FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.whatsapp_conversations (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_whatsapp_conversations_tenant_id ON barber.whatsapp_conversations(tenant_id);
CREATE INDEX IF NOT EXISTS idx_whatsapp_conversations_branch_id ON barber.whatsapp_conversations(branch_id);
CREATE INDEX IF NOT EXISTS idx_whatsapp_conversations_status ON barber.whatsapp_conversations(status);
CREATE INDEX IF NOT EXISTS idx_whatsapp_conversations_created_at ON barber.whatsapp_conversations(created_at);
CREATE INDEX IF NOT EXISTS idx_whatsapp_conversations_tenant_status ON barber.whatsapp_conversations(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_whatsapp_conversations_tenant_branch ON barber.whatsapp_conversations(tenant_id, branch_id);
CREATE TRIGGER trg_whatsapp_conversations_updated_at BEFORE UPDATE ON barber.whatsapp_conversations FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.whatsapp_messages (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_whatsapp_messages_tenant_id ON barber.whatsapp_messages(tenant_id);
CREATE INDEX IF NOT EXISTS idx_whatsapp_messages_branch_id ON barber.whatsapp_messages(branch_id);
CREATE INDEX IF NOT EXISTS idx_whatsapp_messages_status ON barber.whatsapp_messages(status);
CREATE INDEX IF NOT EXISTS idx_whatsapp_messages_created_at ON barber.whatsapp_messages(created_at);
CREATE INDEX IF NOT EXISTS idx_whatsapp_messages_tenant_status ON barber.whatsapp_messages(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_whatsapp_messages_tenant_branch ON barber.whatsapp_messages(tenant_id, branch_id);
CREATE TRIGGER trg_whatsapp_messages_updated_at BEFORE UPDATE ON barber.whatsapp_messages FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.communication_threads (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_communication_threads_tenant_id ON barber.communication_threads(tenant_id);
CREATE INDEX IF NOT EXISTS idx_communication_threads_branch_id ON barber.communication_threads(branch_id);
CREATE INDEX IF NOT EXISTS idx_communication_threads_status ON barber.communication_threads(status);
CREATE INDEX IF NOT EXISTS idx_communication_threads_created_at ON barber.communication_threads(created_at);
CREATE INDEX IF NOT EXISTS idx_communication_threads_tenant_status ON barber.communication_threads(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_communication_threads_tenant_branch ON barber.communication_threads(tenant_id, branch_id);
CREATE TRIGGER trg_communication_threads_updated_at BEFORE UPDATE ON barber.communication_threads FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.communication_messages (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_communication_messages_tenant_id ON barber.communication_messages(tenant_id);
CREATE INDEX IF NOT EXISTS idx_communication_messages_branch_id ON barber.communication_messages(branch_id);
CREATE INDEX IF NOT EXISTS idx_communication_messages_status ON barber.communication_messages(status);
CREATE INDEX IF NOT EXISTS idx_communication_messages_created_at ON barber.communication_messages(created_at);
CREATE INDEX IF NOT EXISTS idx_communication_messages_tenant_status ON barber.communication_messages(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_communication_messages_tenant_branch ON barber.communication_messages(tenant_id, branch_id);
CREATE TRIGGER trg_communication_messages_updated_at BEFORE UPDATE ON barber.communication_messages FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.external_integrations (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_external_integrations_tenant_id ON barber.external_integrations(tenant_id);
CREATE INDEX IF NOT EXISTS idx_external_integrations_branch_id ON barber.external_integrations(branch_id);
CREATE INDEX IF NOT EXISTS idx_external_integrations_status ON barber.external_integrations(status);
CREATE INDEX IF NOT EXISTS idx_external_integrations_created_at ON barber.external_integrations(created_at);
CREATE INDEX IF NOT EXISTS idx_external_integrations_tenant_status ON barber.external_integrations(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_external_integrations_tenant_branch ON barber.external_integrations(tenant_id, branch_id);
CREATE TRIGGER trg_external_integrations_updated_at BEFORE UPDATE ON barber.external_integrations FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.integration_credentials (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_integration_credentials_tenant_id ON barber.integration_credentials(tenant_id);
CREATE INDEX IF NOT EXISTS idx_integration_credentials_branch_id ON barber.integration_credentials(branch_id);
CREATE INDEX IF NOT EXISTS idx_integration_credentials_status ON barber.integration_credentials(status);
CREATE INDEX IF NOT EXISTS idx_integration_credentials_created_at ON barber.integration_credentials(created_at);
CREATE INDEX IF NOT EXISTS idx_integration_credentials_tenant_status ON barber.integration_credentials(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_integration_credentials_tenant_branch ON barber.integration_credentials(tenant_id, branch_id);
CREATE TRIGGER trg_integration_credentials_updated_at BEFORE UPDATE ON barber.integration_credentials FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.integration_sync_logs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_integration_sync_logs_tenant_id ON barber.integration_sync_logs(tenant_id);
CREATE INDEX IF NOT EXISTS idx_integration_sync_logs_branch_id ON barber.integration_sync_logs(branch_id);
CREATE INDEX IF NOT EXISTS idx_integration_sync_logs_status ON barber.integration_sync_logs(status);
CREATE INDEX IF NOT EXISTS idx_integration_sync_logs_created_at ON barber.integration_sync_logs(created_at);
CREATE INDEX IF NOT EXISTS idx_integration_sync_logs_tenant_status ON barber.integration_sync_logs(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_integration_sync_logs_tenant_branch ON barber.integration_sync_logs(tenant_id, branch_id);
CREATE TRIGGER trg_integration_sync_logs_updated_at BEFORE UPDATE ON barber.integration_sync_logs FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.tenant_webhooks (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_tenant_webhooks_tenant_id ON barber.tenant_webhooks(tenant_id);
CREATE INDEX IF NOT EXISTS idx_tenant_webhooks_branch_id ON barber.tenant_webhooks(branch_id);
CREATE INDEX IF NOT EXISTS idx_tenant_webhooks_status ON barber.tenant_webhooks(status);
CREATE INDEX IF NOT EXISTS idx_tenant_webhooks_created_at ON barber.tenant_webhooks(created_at);
CREATE INDEX IF NOT EXISTS idx_tenant_webhooks_tenant_status ON barber.tenant_webhooks(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_tenant_webhooks_tenant_branch ON barber.tenant_webhooks(tenant_id, branch_id);
CREATE TRIGGER trg_tenant_webhooks_updated_at BEFORE UPDATE ON barber.tenant_webhooks FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.webhook_deliveries (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_webhook_deliveries_tenant_id ON barber.webhook_deliveries(tenant_id);
CREATE INDEX IF NOT EXISTS idx_webhook_deliveries_branch_id ON barber.webhook_deliveries(branch_id);
CREATE INDEX IF NOT EXISTS idx_webhook_deliveries_status ON barber.webhook_deliveries(status);
CREATE INDEX IF NOT EXISTS idx_webhook_deliveries_created_at ON barber.webhook_deliveries(created_at);
CREATE INDEX IF NOT EXISTS idx_webhook_deliveries_tenant_status ON barber.webhook_deliveries(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_webhook_deliveries_tenant_branch ON barber.webhook_deliveries(tenant_id, branch_id);
CREATE TRIGGER trg_webhook_deliveries_updated_at BEFORE UPDATE ON barber.webhook_deliveries FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.workflow_definitions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_workflow_definitions_tenant_id ON barber.workflow_definitions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_workflow_definitions_branch_id ON barber.workflow_definitions(branch_id);
CREATE INDEX IF NOT EXISTS idx_workflow_definitions_status ON barber.workflow_definitions(status);
CREATE INDEX IF NOT EXISTS idx_workflow_definitions_created_at ON barber.workflow_definitions(created_at);
CREATE INDEX IF NOT EXISTS idx_workflow_definitions_tenant_status ON barber.workflow_definitions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_workflow_definitions_tenant_branch ON barber.workflow_definitions(tenant_id, branch_id);
CREATE TRIGGER trg_workflow_definitions_updated_at BEFORE UPDATE ON barber.workflow_definitions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.workflow_triggers (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_workflow_triggers_tenant_id ON barber.workflow_triggers(tenant_id);
CREATE INDEX IF NOT EXISTS idx_workflow_triggers_branch_id ON barber.workflow_triggers(branch_id);
CREATE INDEX IF NOT EXISTS idx_workflow_triggers_status ON barber.workflow_triggers(status);
CREATE INDEX IF NOT EXISTS idx_workflow_triggers_created_at ON barber.workflow_triggers(created_at);
CREATE INDEX IF NOT EXISTS idx_workflow_triggers_tenant_status ON barber.workflow_triggers(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_workflow_triggers_tenant_branch ON barber.workflow_triggers(tenant_id, branch_id);
CREATE TRIGGER trg_workflow_triggers_updated_at BEFORE UPDATE ON barber.workflow_triggers FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.workflow_conditions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_workflow_conditions_tenant_id ON barber.workflow_conditions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_workflow_conditions_branch_id ON barber.workflow_conditions(branch_id);
CREATE INDEX IF NOT EXISTS idx_workflow_conditions_status ON barber.workflow_conditions(status);
CREATE INDEX IF NOT EXISTS idx_workflow_conditions_created_at ON barber.workflow_conditions(created_at);
CREATE INDEX IF NOT EXISTS idx_workflow_conditions_tenant_status ON barber.workflow_conditions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_workflow_conditions_tenant_branch ON barber.workflow_conditions(tenant_id, branch_id);
CREATE TRIGGER trg_workflow_conditions_updated_at BEFORE UPDATE ON barber.workflow_conditions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.workflow_actions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_workflow_actions_tenant_id ON barber.workflow_actions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_workflow_actions_branch_id ON barber.workflow_actions(branch_id);
CREATE INDEX IF NOT EXISTS idx_workflow_actions_status ON barber.workflow_actions(status);
CREATE INDEX IF NOT EXISTS idx_workflow_actions_created_at ON barber.workflow_actions(created_at);
CREATE INDEX IF NOT EXISTS idx_workflow_actions_tenant_status ON barber.workflow_actions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_workflow_actions_tenant_branch ON barber.workflow_actions(tenant_id, branch_id);
CREATE TRIGGER trg_workflow_actions_updated_at BEFORE UPDATE ON barber.workflow_actions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.workflow_executions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_workflow_executions_tenant_id ON barber.workflow_executions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_workflow_executions_branch_id ON barber.workflow_executions(branch_id);
CREATE INDEX IF NOT EXISTS idx_workflow_executions_status ON barber.workflow_executions(status);
CREATE INDEX IF NOT EXISTS idx_workflow_executions_created_at ON barber.workflow_executions(created_at);
CREATE INDEX IF NOT EXISTS idx_workflow_executions_tenant_status ON barber.workflow_executions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_workflow_executions_tenant_branch ON barber.workflow_executions(tenant_id, branch_id);
CREATE TRIGGER trg_workflow_executions_updated_at BEFORE UPDATE ON barber.workflow_executions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.custom_forms (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_custom_forms_tenant_id ON barber.custom_forms(tenant_id);
CREATE INDEX IF NOT EXISTS idx_custom_forms_branch_id ON barber.custom_forms(branch_id);
CREATE INDEX IF NOT EXISTS idx_custom_forms_status ON barber.custom_forms(status);
CREATE INDEX IF NOT EXISTS idx_custom_forms_created_at ON barber.custom_forms(created_at);
CREATE INDEX IF NOT EXISTS idx_custom_forms_tenant_status ON barber.custom_forms(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_custom_forms_tenant_branch ON barber.custom_forms(tenant_id, branch_id);
CREATE TRIGGER trg_custom_forms_updated_at BEFORE UPDATE ON barber.custom_forms FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.custom_form_fields (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_custom_form_fields_tenant_id ON barber.custom_form_fields(tenant_id);
CREATE INDEX IF NOT EXISTS idx_custom_form_fields_branch_id ON barber.custom_form_fields(branch_id);
CREATE INDEX IF NOT EXISTS idx_custom_form_fields_status ON barber.custom_form_fields(status);
CREATE INDEX IF NOT EXISTS idx_custom_form_fields_created_at ON barber.custom_form_fields(created_at);
CREATE INDEX IF NOT EXISTS idx_custom_form_fields_tenant_status ON barber.custom_form_fields(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_custom_form_fields_tenant_branch ON barber.custom_form_fields(tenant_id, branch_id);
CREATE TRIGGER trg_custom_form_fields_updated_at BEFORE UPDATE ON barber.custom_form_fields FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.custom_form_responses (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_custom_form_responses_tenant_id ON barber.custom_form_responses(tenant_id);
CREATE INDEX IF NOT EXISTS idx_custom_form_responses_branch_id ON barber.custom_form_responses(branch_id);
CREATE INDEX IF NOT EXISTS idx_custom_form_responses_status ON barber.custom_form_responses(status);
CREATE INDEX IF NOT EXISTS idx_custom_form_responses_created_at ON barber.custom_form_responses(created_at);
CREATE INDEX IF NOT EXISTS idx_custom_form_responses_tenant_status ON barber.custom_form_responses(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_custom_form_responses_tenant_branch ON barber.custom_form_responses(tenant_id, branch_id);
CREATE TRIGGER trg_custom_form_responses_updated_at BEFORE UPDATE ON barber.custom_form_responses FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.custom_fields (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_custom_fields_tenant_id ON barber.custom_fields(tenant_id);
CREATE INDEX IF NOT EXISTS idx_custom_fields_branch_id ON barber.custom_fields(branch_id);
CREATE INDEX IF NOT EXISTS idx_custom_fields_status ON barber.custom_fields(status);
CREATE INDEX IF NOT EXISTS idx_custom_fields_created_at ON barber.custom_fields(created_at);
CREATE INDEX IF NOT EXISTS idx_custom_fields_tenant_status ON barber.custom_fields(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_custom_fields_tenant_branch ON barber.custom_fields(tenant_id, branch_id);
CREATE TRIGGER trg_custom_fields_updated_at BEFORE UPDATE ON barber.custom_fields FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.custom_field_values (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_custom_field_values_tenant_id ON barber.custom_field_values(tenant_id);
CREATE INDEX IF NOT EXISTS idx_custom_field_values_branch_id ON barber.custom_field_values(branch_id);
CREATE INDEX IF NOT EXISTS idx_custom_field_values_status ON barber.custom_field_values(status);
CREATE INDEX IF NOT EXISTS idx_custom_field_values_created_at ON barber.custom_field_values(created_at);
CREATE INDEX IF NOT EXISTS idx_custom_field_values_tenant_status ON barber.custom_field_values(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_custom_field_values_tenant_branch ON barber.custom_field_values(tenant_id, branch_id);
CREATE TRIGGER trg_custom_field_values_updated_at BEFORE UPDATE ON barber.custom_field_values FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.plugins (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_plugins_tenant_id ON barber.plugins(tenant_id);
CREATE INDEX IF NOT EXISTS idx_plugins_branch_id ON barber.plugins(branch_id);
CREATE INDEX IF NOT EXISTS idx_plugins_status ON barber.plugins(status);
CREATE INDEX IF NOT EXISTS idx_plugins_created_at ON barber.plugins(created_at);
CREATE INDEX IF NOT EXISTS idx_plugins_tenant_status ON barber.plugins(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_plugins_tenant_branch ON barber.plugins(tenant_id, branch_id);
CREATE TRIGGER trg_plugins_updated_at BEFORE UPDATE ON barber.plugins FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.plugin_installations (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_plugin_installations_tenant_id ON barber.plugin_installations(tenant_id);
CREATE INDEX IF NOT EXISTS idx_plugin_installations_branch_id ON barber.plugin_installations(branch_id);
CREATE INDEX IF NOT EXISTS idx_plugin_installations_status ON barber.plugin_installations(status);
CREATE INDEX IF NOT EXISTS idx_plugin_installations_created_at ON barber.plugin_installations(created_at);
CREATE INDEX IF NOT EXISTS idx_plugin_installations_tenant_status ON barber.plugin_installations(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_plugin_installations_tenant_branch ON barber.plugin_installations(tenant_id, branch_id);
CREATE TRIGGER trg_plugin_installations_updated_at BEFORE UPDATE ON barber.plugin_installations FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.plugin_settings (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_plugin_settings_tenant_id ON barber.plugin_settings(tenant_id);
CREATE INDEX IF NOT EXISTS idx_plugin_settings_branch_id ON barber.plugin_settings(branch_id);
CREATE INDEX IF NOT EXISTS idx_plugin_settings_status ON barber.plugin_settings(status);
CREATE INDEX IF NOT EXISTS idx_plugin_settings_created_at ON barber.plugin_settings(created_at);
CREATE INDEX IF NOT EXISTS idx_plugin_settings_tenant_status ON barber.plugin_settings(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_plugin_settings_tenant_branch ON barber.plugin_settings(tenant_id, branch_id);
CREATE TRIGGER trg_plugin_settings_updated_at BEFORE UPDATE ON barber.plugin_settings FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.data_events (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_data_events_tenant_id ON barber.data_events(tenant_id);
CREATE INDEX IF NOT EXISTS idx_data_events_branch_id ON barber.data_events(branch_id);
CREATE INDEX IF NOT EXISTS idx_data_events_status ON barber.data_events(status);
CREATE INDEX IF NOT EXISTS idx_data_events_created_at ON barber.data_events(created_at);
CREATE INDEX IF NOT EXISTS idx_data_events_tenant_status ON barber.data_events(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_data_events_tenant_branch ON barber.data_events(tenant_id, branch_id);
CREATE TRIGGER trg_data_events_updated_at BEFORE UPDATE ON barber.data_events FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.data_event_properties (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_data_event_properties_tenant_id ON barber.data_event_properties(tenant_id);
CREATE INDEX IF NOT EXISTS idx_data_event_properties_branch_id ON barber.data_event_properties(branch_id);
CREATE INDEX IF NOT EXISTS idx_data_event_properties_status ON barber.data_event_properties(status);
CREATE INDEX IF NOT EXISTS idx_data_event_properties_created_at ON barber.data_event_properties(created_at);
CREATE INDEX IF NOT EXISTS idx_data_event_properties_tenant_status ON barber.data_event_properties(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_data_event_properties_tenant_branch ON barber.data_event_properties(tenant_id, branch_id);
CREATE TRIGGER trg_data_event_properties_updated_at BEFORE UPDATE ON barber.data_event_properties FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.fact_appointments (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_fact_appointments_tenant_id ON barber.fact_appointments(tenant_id);
CREATE INDEX IF NOT EXISTS idx_fact_appointments_branch_id ON barber.fact_appointments(branch_id);
CREATE INDEX IF NOT EXISTS idx_fact_appointments_status ON barber.fact_appointments(status);
CREATE INDEX IF NOT EXISTS idx_fact_appointments_created_at ON barber.fact_appointments(created_at);
CREATE INDEX IF NOT EXISTS idx_fact_appointments_tenant_status ON barber.fact_appointments(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_fact_appointments_tenant_branch ON barber.fact_appointments(tenant_id, branch_id);
CREATE TRIGGER trg_fact_appointments_updated_at BEFORE UPDATE ON barber.fact_appointments FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.fact_payments (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_fact_payments_tenant_id ON barber.fact_payments(tenant_id);
CREATE INDEX IF NOT EXISTS idx_fact_payments_branch_id ON barber.fact_payments(branch_id);
CREATE INDEX IF NOT EXISTS idx_fact_payments_status ON barber.fact_payments(status);
CREATE INDEX IF NOT EXISTS idx_fact_payments_created_at ON barber.fact_payments(created_at);
CREATE INDEX IF NOT EXISTS idx_fact_payments_tenant_status ON barber.fact_payments(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_fact_payments_tenant_branch ON barber.fact_payments(tenant_id, branch_id);
CREATE TRIGGER trg_fact_payments_updated_at BEFORE UPDATE ON barber.fact_payments FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.fact_service_orders (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_fact_service_orders_tenant_id ON barber.fact_service_orders(tenant_id);
CREATE INDEX IF NOT EXISTS idx_fact_service_orders_branch_id ON barber.fact_service_orders(branch_id);
CREATE INDEX IF NOT EXISTS idx_fact_service_orders_status ON barber.fact_service_orders(status);
CREATE INDEX IF NOT EXISTS idx_fact_service_orders_created_at ON barber.fact_service_orders(created_at);
CREATE INDEX IF NOT EXISTS idx_fact_service_orders_tenant_status ON barber.fact_service_orders(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_fact_service_orders_tenant_branch ON barber.fact_service_orders(tenant_id, branch_id);
CREATE TRIGGER trg_fact_service_orders_updated_at BEFORE UPDATE ON barber.fact_service_orders FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.fact_product_sales (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_fact_product_sales_tenant_id ON barber.fact_product_sales(tenant_id);
CREATE INDEX IF NOT EXISTS idx_fact_product_sales_branch_id ON barber.fact_product_sales(branch_id);
CREATE INDEX IF NOT EXISTS idx_fact_product_sales_status ON barber.fact_product_sales(status);
CREATE INDEX IF NOT EXISTS idx_fact_product_sales_created_at ON barber.fact_product_sales(created_at);
CREATE INDEX IF NOT EXISTS idx_fact_product_sales_tenant_status ON barber.fact_product_sales(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_fact_product_sales_tenant_branch ON barber.fact_product_sales(tenant_id, branch_id);
CREATE TRIGGER trg_fact_product_sales_updated_at BEFORE UPDATE ON barber.fact_product_sales FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.dim_date (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_dim_date_tenant_id ON barber.dim_date(tenant_id);
CREATE INDEX IF NOT EXISTS idx_dim_date_branch_id ON barber.dim_date(branch_id);
CREATE INDEX IF NOT EXISTS idx_dim_date_status ON barber.dim_date(status);
CREATE INDEX IF NOT EXISTS idx_dim_date_created_at ON barber.dim_date(created_at);
CREATE INDEX IF NOT EXISTS idx_dim_date_tenant_status ON barber.dim_date(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_dim_date_tenant_branch ON barber.dim_date(tenant_id, branch_id);
CREATE TRIGGER trg_dim_date_updated_at BEFORE UPDATE ON barber.dim_date FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.dim_time (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_dim_time_tenant_id ON barber.dim_time(tenant_id);
CREATE INDEX IF NOT EXISTS idx_dim_time_branch_id ON barber.dim_time(branch_id);
CREATE INDEX IF NOT EXISTS idx_dim_time_status ON barber.dim_time(status);
CREATE INDEX IF NOT EXISTS idx_dim_time_created_at ON barber.dim_time(created_at);
CREATE INDEX IF NOT EXISTS idx_dim_time_tenant_status ON barber.dim_time(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_dim_time_tenant_branch ON barber.dim_time(tenant_id, branch_id);
CREATE TRIGGER trg_dim_time_updated_at BEFORE UPDATE ON barber.dim_time FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.feature_definitions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_feature_definitions_tenant_id ON barber.feature_definitions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_feature_definitions_branch_id ON barber.feature_definitions(branch_id);
CREATE INDEX IF NOT EXISTS idx_feature_definitions_status ON barber.feature_definitions(status);
CREATE INDEX IF NOT EXISTS idx_feature_definitions_created_at ON barber.feature_definitions(created_at);
CREATE INDEX IF NOT EXISTS idx_feature_definitions_tenant_status ON barber.feature_definitions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_feature_definitions_tenant_branch ON barber.feature_definitions(tenant_id, branch_id);
CREATE TRIGGER trg_feature_definitions_updated_at BEFORE UPDATE ON barber.feature_definitions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.feature_values (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_feature_values_tenant_id ON barber.feature_values(tenant_id);
CREATE INDEX IF NOT EXISTS idx_feature_values_branch_id ON barber.feature_values(branch_id);
CREATE INDEX IF NOT EXISTS idx_feature_values_status ON barber.feature_values(status);
CREATE INDEX IF NOT EXISTS idx_feature_values_created_at ON barber.feature_values(created_at);
CREATE INDEX IF NOT EXISTS idx_feature_values_tenant_status ON barber.feature_values(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_feature_values_tenant_branch ON barber.feature_values(tenant_id, branch_id);
CREATE TRIGGER trg_feature_values_updated_at BEFORE UPDATE ON barber.feature_values FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.scheduled_reports (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_scheduled_reports_tenant_id ON barber.scheduled_reports(tenant_id);
CREATE INDEX IF NOT EXISTS idx_scheduled_reports_branch_id ON barber.scheduled_reports(branch_id);
CREATE INDEX IF NOT EXISTS idx_scheduled_reports_status ON barber.scheduled_reports(status);
CREATE INDEX IF NOT EXISTS idx_scheduled_reports_created_at ON barber.scheduled_reports(created_at);
CREATE INDEX IF NOT EXISTS idx_scheduled_reports_tenant_status ON barber.scheduled_reports(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_scheduled_reports_tenant_branch ON barber.scheduled_reports(tenant_id, branch_id);
CREATE TRIGGER trg_scheduled_reports_updated_at BEFORE UPDATE ON barber.scheduled_reports FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.bi_datasets (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_bi_datasets_tenant_id ON barber.bi_datasets(tenant_id);
CREATE INDEX IF NOT EXISTS idx_bi_datasets_branch_id ON barber.bi_datasets(branch_id);
CREATE INDEX IF NOT EXISTS idx_bi_datasets_status ON barber.bi_datasets(status);
CREATE INDEX IF NOT EXISTS idx_bi_datasets_created_at ON barber.bi_datasets(created_at);
CREATE INDEX IF NOT EXISTS idx_bi_datasets_tenant_status ON barber.bi_datasets(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_bi_datasets_tenant_branch ON barber.bi_datasets(tenant_id, branch_id);
CREATE TRIGGER trg_bi_datasets_updated_at BEFORE UPDATE ON barber.bi_datasets FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.growth_opportunities (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_growth_opportunities_tenant_id ON barber.growth_opportunities(tenant_id);
CREATE INDEX IF NOT EXISTS idx_growth_opportunities_branch_id ON barber.growth_opportunities(branch_id);
CREATE INDEX IF NOT EXISTS idx_growth_opportunities_status ON barber.growth_opportunities(status);
CREATE INDEX IF NOT EXISTS idx_growth_opportunities_created_at ON barber.growth_opportunities(created_at);
CREATE INDEX IF NOT EXISTS idx_growth_opportunities_tenant_status ON barber.growth_opportunities(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_growth_opportunities_tenant_branch ON barber.growth_opportunities(tenant_id, branch_id);
CREATE TRIGGER trg_growth_opportunities_updated_at BEFORE UPDATE ON barber.growth_opportunities FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.growth_recommended_actions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_growth_recommended_actions_tenant_id ON barber.growth_recommended_actions(tenant_id);
CREATE INDEX IF NOT EXISTS idx_growth_recommended_actions_branch_id ON barber.growth_recommended_actions(branch_id);
CREATE INDEX IF NOT EXISTS idx_growth_recommended_actions_status ON barber.growth_recommended_actions(status);
CREATE INDEX IF NOT EXISTS idx_growth_recommended_actions_created_at ON barber.growth_recommended_actions(created_at);
CREATE INDEX IF NOT EXISTS idx_growth_recommended_actions_tenant_status ON barber.growth_recommended_actions(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_growth_recommended_actions_tenant_branch ON barber.growth_recommended_actions(tenant_id, branch_id);
CREATE TRIGGER trg_growth_recommended_actions_updated_at BEFORE UPDATE ON barber.growth_recommended_actions FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.growth_action_results (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_growth_action_results_tenant_id ON barber.growth_action_results(tenant_id);
CREATE INDEX IF NOT EXISTS idx_growth_action_results_branch_id ON barber.growth_action_results(branch_id);
CREATE INDEX IF NOT EXISTS idx_growth_action_results_status ON barber.growth_action_results(status);
CREATE INDEX IF NOT EXISTS idx_growth_action_results_created_at ON barber.growth_action_results(created_at);
CREATE INDEX IF NOT EXISTS idx_growth_action_results_tenant_status ON barber.growth_action_results(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_growth_action_results_tenant_branch ON barber.growth_action_results(tenant_id, branch_id);
CREATE TRIGGER trg_growth_action_results_updated_at BEFORE UPDATE ON barber.growth_action_results FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.revenue_opportunity_analysis (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_revenue_opportunity_analysis_tenant_id ON barber.revenue_opportunity_analysis(tenant_id);
CREATE INDEX IF NOT EXISTS idx_revenue_opportunity_analysis_branch_id ON barber.revenue_opportunity_analysis(branch_id);
CREATE INDEX IF NOT EXISTS idx_revenue_opportunity_analysis_status ON barber.revenue_opportunity_analysis(status);
CREATE INDEX IF NOT EXISTS idx_revenue_opportunity_analysis_created_at ON barber.revenue_opportunity_analysis(created_at);
CREATE INDEX IF NOT EXISTS idx_revenue_opportunity_analysis_tenant_status ON barber.revenue_opportunity_analysis(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_revenue_opportunity_analysis_tenant_branch ON barber.revenue_opportunity_analysis(tenant_id, branch_id);
CREATE TRIGGER trg_revenue_opportunity_analysis_updated_at BEFORE UPDATE ON barber.revenue_opportunity_analysis FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.impact_tracking_experiments (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_impact_tracking_experiments_tenant_id ON barber.impact_tracking_experiments(tenant_id);
CREATE INDEX IF NOT EXISTS idx_impact_tracking_experiments_branch_id ON barber.impact_tracking_experiments(branch_id);
CREATE INDEX IF NOT EXISTS idx_impact_tracking_experiments_status ON barber.impact_tracking_experiments(status);
CREATE INDEX IF NOT EXISTS idx_impact_tracking_experiments_created_at ON barber.impact_tracking_experiments(created_at);
CREATE INDEX IF NOT EXISTS idx_impact_tracking_experiments_tenant_status ON barber.impact_tracking_experiments(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_impact_tracking_experiments_tenant_branch ON barber.impact_tracking_experiments(tenant_id, branch_id);
CREATE TRIGGER trg_impact_tracking_experiments_updated_at BEFORE UPDATE ON barber.impact_tracking_experiments FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.audit_logs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_audit_logs_tenant_id ON barber.audit_logs(tenant_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_branch_id ON barber.audit_logs(branch_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_status ON barber.audit_logs(status);
CREATE INDEX IF NOT EXISTS idx_audit_logs_created_at ON barber.audit_logs(created_at);
CREATE INDEX IF NOT EXISTS idx_audit_logs_tenant_status ON barber.audit_logs(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_audit_logs_tenant_branch ON barber.audit_logs(tenant_id, branch_id);
CREATE TRIGGER trg_audit_logs_updated_at BEFORE UPDATE ON barber.audit_logs FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.immutable_audit_logs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_immutable_audit_logs_tenant_id ON barber.immutable_audit_logs(tenant_id);
CREATE INDEX IF NOT EXISTS idx_immutable_audit_logs_branch_id ON barber.immutable_audit_logs(branch_id);
CREATE INDEX IF NOT EXISTS idx_immutable_audit_logs_status ON barber.immutable_audit_logs(status);
CREATE INDEX IF NOT EXISTS idx_immutable_audit_logs_created_at ON barber.immutable_audit_logs(created_at);
CREATE INDEX IF NOT EXISTS idx_immutable_audit_logs_tenant_status ON barber.immutable_audit_logs(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_immutable_audit_logs_tenant_branch ON barber.immutable_audit_logs(tenant_id, branch_id);
CREATE TRIGGER trg_immutable_audit_logs_updated_at BEFORE UPDATE ON barber.immutable_audit_logs FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.audit_hash_chains (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_audit_hash_chains_tenant_id ON barber.audit_hash_chains(tenant_id);
CREATE INDEX IF NOT EXISTS idx_audit_hash_chains_branch_id ON barber.audit_hash_chains(branch_id);
CREATE INDEX IF NOT EXISTS idx_audit_hash_chains_status ON barber.audit_hash_chains(status);
CREATE INDEX IF NOT EXISTS idx_audit_hash_chains_created_at ON barber.audit_hash_chains(created_at);
CREATE INDEX IF NOT EXISTS idx_audit_hash_chains_tenant_status ON barber.audit_hash_chains(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_audit_hash_chains_tenant_branch ON barber.audit_hash_chains(tenant_id, branch_id);
CREATE TRIGGER trg_audit_hash_chains_updated_at BEFORE UPDATE ON barber.audit_hash_chains FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.settings_audit_logs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_settings_audit_logs_tenant_id ON barber.settings_audit_logs(tenant_id);
CREATE INDEX IF NOT EXISTS idx_settings_audit_logs_branch_id ON barber.settings_audit_logs(branch_id);
CREATE INDEX IF NOT EXISTS idx_settings_audit_logs_status ON barber.settings_audit_logs(status);
CREATE INDEX IF NOT EXISTS idx_settings_audit_logs_created_at ON barber.settings_audit_logs(created_at);
CREATE INDEX IF NOT EXISTS idx_settings_audit_logs_tenant_status ON barber.settings_audit_logs(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_settings_audit_logs_tenant_branch ON barber.settings_audit_logs(tenant_id, branch_id);
CREATE TRIGGER trg_settings_audit_logs_updated_at BEFORE UPDATE ON barber.settings_audit_logs FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.outbox_messages (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_outbox_messages_tenant_id ON barber.outbox_messages(tenant_id);
CREATE INDEX IF NOT EXISTS idx_outbox_messages_branch_id ON barber.outbox_messages(branch_id);
CREATE INDEX IF NOT EXISTS idx_outbox_messages_status ON barber.outbox_messages(status);
CREATE INDEX IF NOT EXISTS idx_outbox_messages_created_at ON barber.outbox_messages(created_at);
CREATE INDEX IF NOT EXISTS idx_outbox_messages_tenant_status ON barber.outbox_messages(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_outbox_messages_tenant_branch ON barber.outbox_messages(tenant_id, branch_id);
CREATE TRIGGER trg_outbox_messages_updated_at BEFORE UPDATE ON barber.outbox_messages FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.inbox_messages (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_inbox_messages_tenant_id ON barber.inbox_messages(tenant_id);
CREATE INDEX IF NOT EXISTS idx_inbox_messages_branch_id ON barber.inbox_messages(branch_id);
CREATE INDEX IF NOT EXISTS idx_inbox_messages_status ON barber.inbox_messages(status);
CREATE INDEX IF NOT EXISTS idx_inbox_messages_created_at ON barber.inbox_messages(created_at);
CREATE INDEX IF NOT EXISTS idx_inbox_messages_tenant_status ON barber.inbox_messages(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_inbox_messages_tenant_branch ON barber.inbox_messages(tenant_id, branch_id);
CREATE TRIGGER trg_inbox_messages_updated_at BEFORE UPDATE ON barber.inbox_messages FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.message_dead_letters (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_message_dead_letters_tenant_id ON barber.message_dead_letters(tenant_id);
CREATE INDEX IF NOT EXISTS idx_message_dead_letters_branch_id ON barber.message_dead_letters(branch_id);
CREATE INDEX IF NOT EXISTS idx_message_dead_letters_status ON barber.message_dead_letters(status);
CREATE INDEX IF NOT EXISTS idx_message_dead_letters_created_at ON barber.message_dead_letters(created_at);
CREATE INDEX IF NOT EXISTS idx_message_dead_letters_tenant_status ON barber.message_dead_letters(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_message_dead_letters_tenant_branch ON barber.message_dead_letters(tenant_id, branch_id);
CREATE TRIGGER trg_message_dead_letters_updated_at BEFORE UPDATE ON barber.message_dead_letters FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.saga_instances (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_saga_instances_tenant_id ON barber.saga_instances(tenant_id);
CREATE INDEX IF NOT EXISTS idx_saga_instances_branch_id ON barber.saga_instances(branch_id);
CREATE INDEX IF NOT EXISTS idx_saga_instances_status ON barber.saga_instances(status);
CREATE INDEX IF NOT EXISTS idx_saga_instances_created_at ON barber.saga_instances(created_at);
CREATE INDEX IF NOT EXISTS idx_saga_instances_tenant_status ON barber.saga_instances(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_saga_instances_tenant_branch ON barber.saga_instances(tenant_id, branch_id);
CREATE TRIGGER trg_saga_instances_updated_at BEFORE UPDATE ON barber.saga_instances FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.saga_steps (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_saga_steps_tenant_id ON barber.saga_steps(tenant_id);
CREATE INDEX IF NOT EXISTS idx_saga_steps_branch_id ON barber.saga_steps(branch_id);
CREATE INDEX IF NOT EXISTS idx_saga_steps_status ON barber.saga_steps(status);
CREATE INDEX IF NOT EXISTS idx_saga_steps_created_at ON barber.saga_steps(created_at);
CREATE INDEX IF NOT EXISTS idx_saga_steps_tenant_status ON barber.saga_steps(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_saga_steps_tenant_branch ON barber.saga_steps(tenant_id, branch_id);
CREATE TRIGGER trg_saga_steps_updated_at BEFORE UPDATE ON barber.saga_steps FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.idempotency_keys (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_idempotency_keys_tenant_id ON barber.idempotency_keys(tenant_id);
CREATE INDEX IF NOT EXISTS idx_idempotency_keys_branch_id ON barber.idempotency_keys(branch_id);
CREATE INDEX IF NOT EXISTS idx_idempotency_keys_status ON barber.idempotency_keys(status);
CREATE INDEX IF NOT EXISTS idx_idempotency_keys_created_at ON barber.idempotency_keys(created_at);
CREATE INDEX IF NOT EXISTS idx_idempotency_keys_tenant_status ON barber.idempotency_keys(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_idempotency_keys_tenant_branch ON barber.idempotency_keys(tenant_id, branch_id);
CREATE TRIGGER trg_idempotency_keys_updated_at BEFORE UPDATE ON barber.idempotency_keys FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.health_check_logs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_health_check_logs_tenant_id ON barber.health_check_logs(tenant_id);
CREATE INDEX IF NOT EXISTS idx_health_check_logs_branch_id ON barber.health_check_logs(branch_id);
CREATE INDEX IF NOT EXISTS idx_health_check_logs_status ON barber.health_check_logs(status);
CREATE INDEX IF NOT EXISTS idx_health_check_logs_created_at ON barber.health_check_logs(created_at);
CREATE INDEX IF NOT EXISTS idx_health_check_logs_tenant_status ON barber.health_check_logs(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_health_check_logs_tenant_branch ON barber.health_check_logs(tenant_id, branch_id);
CREATE TRIGGER trg_health_check_logs_updated_at BEFORE UPDATE ON barber.health_check_logs FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.request_logs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_request_logs_tenant_id ON barber.request_logs(tenant_id);
CREATE INDEX IF NOT EXISTS idx_request_logs_branch_id ON barber.request_logs(branch_id);
CREATE INDEX IF NOT EXISTS idx_request_logs_status ON barber.request_logs(status);
CREATE INDEX IF NOT EXISTS idx_request_logs_created_at ON barber.request_logs(created_at);
CREATE INDEX IF NOT EXISTS idx_request_logs_tenant_status ON barber.request_logs(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_request_logs_tenant_branch ON barber.request_logs(tenant_id, branch_id);
CREATE TRIGGER trg_request_logs_updated_at BEFORE UPDATE ON barber.request_logs FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.error_logs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_error_logs_tenant_id ON barber.error_logs(tenant_id);
CREATE INDEX IF NOT EXISTS idx_error_logs_branch_id ON barber.error_logs(branch_id);
CREATE INDEX IF NOT EXISTS idx_error_logs_status ON barber.error_logs(status);
CREATE INDEX IF NOT EXISTS idx_error_logs_created_at ON barber.error_logs(created_at);
CREATE INDEX IF NOT EXISTS idx_error_logs_tenant_status ON barber.error_logs(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_error_logs_tenant_branch ON barber.error_logs(tenant_id, branch_id);
CREATE TRIGGER trg_error_logs_updated_at BEFORE UPDATE ON barber.error_logs FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.performance_logs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_performance_logs_tenant_id ON barber.performance_logs(tenant_id);
CREATE INDEX IF NOT EXISTS idx_performance_logs_branch_id ON barber.performance_logs(branch_id);
CREATE INDEX IF NOT EXISTS idx_performance_logs_status ON barber.performance_logs(status);
CREATE INDEX IF NOT EXISTS idx_performance_logs_created_at ON barber.performance_logs(created_at);
CREATE INDEX IF NOT EXISTS idx_performance_logs_tenant_status ON barber.performance_logs(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_performance_logs_tenant_branch ON barber.performance_logs(tenant_id, branch_id);
CREATE TRIGGER trg_performance_logs_updated_at BEFORE UPDATE ON barber.performance_logs FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.job_execution_logs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_job_execution_logs_tenant_id ON barber.job_execution_logs(tenant_id);
CREATE INDEX IF NOT EXISTS idx_job_execution_logs_branch_id ON barber.job_execution_logs(branch_id);
CREATE INDEX IF NOT EXISTS idx_job_execution_logs_status ON barber.job_execution_logs(status);
CREATE INDEX IF NOT EXISTS idx_job_execution_logs_created_at ON barber.job_execution_logs(created_at);
CREATE INDEX IF NOT EXISTS idx_job_execution_logs_tenant_status ON barber.job_execution_logs(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_job_execution_logs_tenant_branch ON barber.job_execution_logs(tenant_id, branch_id);
CREATE TRIGGER trg_job_execution_logs_updated_at BEFORE UPDATE ON barber.job_execution_logs FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.developer_accounts (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_developer_accounts_tenant_id ON barber.developer_accounts(tenant_id);
CREATE INDEX IF NOT EXISTS idx_developer_accounts_branch_id ON barber.developer_accounts(branch_id);
CREATE INDEX IF NOT EXISTS idx_developer_accounts_status ON barber.developer_accounts(status);
CREATE INDEX IF NOT EXISTS idx_developer_accounts_created_at ON barber.developer_accounts(created_at);
CREATE INDEX IF NOT EXISTS idx_developer_accounts_tenant_status ON barber.developer_accounts(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_developer_accounts_tenant_branch ON barber.developer_accounts(tenant_id, branch_id);
CREATE TRIGGER trg_developer_accounts_updated_at BEFORE UPDATE ON barber.developer_accounts FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.developer_apps (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_developer_apps_tenant_id ON barber.developer_apps(tenant_id);
CREATE INDEX IF NOT EXISTS idx_developer_apps_branch_id ON barber.developer_apps(branch_id);
CREATE INDEX IF NOT EXISTS idx_developer_apps_status ON barber.developer_apps(status);
CREATE INDEX IF NOT EXISTS idx_developer_apps_created_at ON barber.developer_apps(created_at);
CREATE INDEX IF NOT EXISTS idx_developer_apps_tenant_status ON barber.developer_apps(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_developer_apps_tenant_branch ON barber.developer_apps(tenant_id, branch_id);
CREATE TRIGGER trg_developer_apps_updated_at BEFORE UPDATE ON barber.developer_apps FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.developer_api_keys (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_developer_api_keys_tenant_id ON barber.developer_api_keys(tenant_id);
CREATE INDEX IF NOT EXISTS idx_developer_api_keys_branch_id ON barber.developer_api_keys(branch_id);
CREATE INDEX IF NOT EXISTS idx_developer_api_keys_status ON barber.developer_api_keys(status);
CREATE INDEX IF NOT EXISTS idx_developer_api_keys_created_at ON barber.developer_api_keys(created_at);
CREATE INDEX IF NOT EXISTS idx_developer_api_keys_tenant_status ON barber.developer_api_keys(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_developer_api_keys_tenant_branch ON barber.developer_api_keys(tenant_id, branch_id);
CREATE TRIGGER trg_developer_api_keys_updated_at BEFORE UPDATE ON barber.developer_api_keys FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.developer_api_usage_logs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_developer_api_usage_logs_tenant_id ON barber.developer_api_usage_logs(tenant_id);
CREATE INDEX IF NOT EXISTS idx_developer_api_usage_logs_branch_id ON barber.developer_api_usage_logs(branch_id);
CREATE INDEX IF NOT EXISTS idx_developer_api_usage_logs_status ON barber.developer_api_usage_logs(status);
CREATE INDEX IF NOT EXISTS idx_developer_api_usage_logs_created_at ON barber.developer_api_usage_logs(created_at);
CREATE INDEX IF NOT EXISTS idx_developer_api_usage_logs_tenant_status ON barber.developer_api_usage_logs(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_developer_api_usage_logs_tenant_branch ON barber.developer_api_usage_logs(tenant_id, branch_id);
CREATE TRIGGER trg_developer_api_usage_logs_updated_at BEFORE UPDATE ON barber.developer_api_usage_logs FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.public_api_requests (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_public_api_requests_tenant_id ON barber.public_api_requests(tenant_id);
CREATE INDEX IF NOT EXISTS idx_public_api_requests_branch_id ON barber.public_api_requests(branch_id);
CREATE INDEX IF NOT EXISTS idx_public_api_requests_status ON barber.public_api_requests(status);
CREATE INDEX IF NOT EXISTS idx_public_api_requests_created_at ON barber.public_api_requests(created_at);
CREATE INDEX IF NOT EXISTS idx_public_api_requests_tenant_status ON barber.public_api_requests(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_public_api_requests_tenant_branch ON barber.public_api_requests(tenant_id, branch_id);
CREATE TRIGGER trg_public_api_requests_updated_at BEFORE UPDATE ON barber.public_api_requests FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

CREATE TABLE IF NOT EXISTS barber.public_api_rate_limits (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id uuid NULL,
    branch_id uuid NULL,
    name varchar(150) NULL,
    code varchar(100) NULL,
    email citext NULL,
    amount numeric(18,2) NULL,
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE',
    is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NULL,
    created_by uuid NULL,
    updated_by uuid NULL
);
CREATE INDEX IF NOT EXISTS idx_public_api_rate_limits_tenant_id ON barber.public_api_rate_limits(tenant_id);
CREATE INDEX IF NOT EXISTS idx_public_api_rate_limits_branch_id ON barber.public_api_rate_limits(branch_id);
CREATE INDEX IF NOT EXISTS idx_public_api_rate_limits_status ON barber.public_api_rate_limits(status);
CREATE INDEX IF NOT EXISTS idx_public_api_rate_limits_created_at ON barber.public_api_rate_limits(created_at);
CREATE INDEX IF NOT EXISTS idx_public_api_rate_limits_tenant_status ON barber.public_api_rate_limits(tenant_id, status);
CREATE INDEX IF NOT EXISTS idx_public_api_rate_limits_tenant_branch ON barber.public_api_rate_limits(tenant_id, branch_id);
CREATE TRIGGER trg_public_api_rate_limits_updated_at BEFORE UPDATE ON barber.public_api_rate_limits FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at();

ALTER TABLE barber.immutable_audit_logs ADD COLUMN IF NOT EXISTS hash text;
CREATE TRIGGER trg_immutable_audit_hash BEFORE INSERT ON barber.immutable_audit_logs FOR EACH ROW EXECUTE FUNCTION barber.calculate_audit_hash();
CREATE TRIGGER trg_audit_users AFTER INSERT OR UPDATE OR DELETE ON barber.users FOR EACH ROW EXECUTE FUNCTION barber.write_audit_log();
CREATE TRIGGER trg_audit_payments AFTER INSERT OR UPDATE OR DELETE ON barber.payments FOR EACH ROW EXECUTE FUNCTION barber.write_audit_log();
CREATE TRIGGER trg_stock_update AFTER INSERT OR UPDATE ON barber.stock_movements FOR EACH ROW EXECUTE FUNCTION barber.update_stock_balance();
CREATE TRIGGER trg_wallet_update AFTER INSERT OR UPDATE ON barber.wallet_transactions FOR EACH ROW EXECUTE FUNCTION barber.update_wallet_balance();

-- ============================================================
-- SEEDS COMERCIAIS MVP DEMO (tenant/barber, schema barber)
-- ============================================================
INSERT INTO barber.tenants(name, code, status, payload)
VALUES
('Barbearia Elite Demo','ELITE-DEMO','ACTIVE','{"segment":"barbershop"}'::jsonb),
('Salão Bella Forma Demo','BELLA-DEMO','ACTIVE','{"segment":"salon"}'::jsonb),
('Clínica Estética Prime Demo','PRIME-DEMO','ACTIVE','{"segment":"aesthetic"}'::jsonb)
ON CONFLICT DO NOTHING;

INSERT INTO barber.branches(tenant_id, name, code, status)
SELECT t.id, b.branch_name, b.branch_code, 'ACTIVE'
FROM barber.tenants t
CROSS JOIN (VALUES ('Matriz', 'MATRIZ'), ('Unidade Shopping', 'SHOPPING')) AS b(branch_name, branch_code)
WHERE t.code IN ('ELITE-DEMO','BELLA-DEMO','PRIME-DEMO')
ON CONFLICT DO NOTHING;

INSERT INTO barber.users(tenant_id, branch_id, name, email, code, status, payload)
SELECT
    t.id,
    b.id,
    u.user_name,
    u.user_email::citext,
    crypt('Admin@123', gen_salt('bf')),
    'ACTIVE',
    jsonb_build_object('role', u.user_role, 'password_hint', 'Admin@123 (demo)')
FROM barber.tenants t
JOIN barber.branches b ON b.tenant_id = t.id AND b.code = 'MATRIZ'
JOIN (VALUES
    ('Admin', 'admin@barbersync.com', 'ADMIN'),
    ('Gerente', 'gerente@barbeariaelite.com', 'MANAGER'),
    ('Recepção', 'recepcao@barbeariaelite.com', 'RECEPTION'),
    ('Financeiro', 'financeiro@barbeariaelite.com', 'FINANCIAL'),
    ('Profissional', 'profissional@barbeariaelite.com', 'PROFESSIONAL'),
    ('Cliente', 'cliente@demo.com', 'CLIENT'),
    ('Totem', 'totem@barbeariaelite.com', 'TOTEM')
) AS u(user_name, user_email, user_role) ON true
WHERE t.code = 'ELITE-DEMO'
ON CONFLICT DO NOTHING;

INSERT INTO barber.clients(tenant_id, branch_id, name, email, code, status, payload)
SELECT t.id, b.id,
       'Cliente Demo ' || gs,
       ('cliente' || gs || '@demo.com')::citext,
       'DOC-' || lpad(gs::text, 3, '0'),
       'ACTIVE',
       jsonb_build_object('phone', '+55 11 90000-' || lpad(gs::text,4,'0'), 'cashback_balance', (gs * 3.50)::numeric(18,2))
FROM barber.tenants t
JOIN barber.branches b ON b.tenant_id = t.id AND b.code = 'MATRIZ'
CROSS JOIN generate_series(1,10) AS gs
WHERE t.code = 'ELITE-DEMO'
ON CONFLICT DO NOTHING;

INSERT INTO barber.services(tenant_id, branch_id, name, amount, status, payload)
SELECT t.id, b.id, s.name, s.price, 'ACTIVE', jsonb_build_object('duration_minutes', s.duration, 'category', s.category)
FROM barber.tenants t
JOIN barber.branches b ON b.tenant_id = t.id AND b.code = 'MATRIZ'
JOIN (VALUES
('Corte Tradicional',50.00,45,'Corte'),('Barba Completa',35.00,30,'Barba'),('Combo Corte + Barba',80.00,70,'Combo'),
('Pigmentação',45.00,35,'Coloração'),('Hidratação Capilar',60.00,40,'Tratamento'),('Sobrancelha',25.00,20,'Estética'),
('Pezinho',20.00,15,'Acabamento'),('Lavagem Premium',18.00,15,'Higiene'),('Corte Infantil',40.00,40,'Corte'),
('Alisamento',120.00,90,'Tratamento'),('Escova',55.00,45,'Finalização'),('Spa da Barba',75.00,50,'Premium')
) s(name, price, duration, category) ON true
WHERE t.code = 'ELITE-DEMO'
ON CONFLICT DO NOTHING;

CREATE OR REPLACE VIEW barber.vw_admin_dashboard AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_owner_dashboard AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_financial_dashboard AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_operational_dashboard AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_hr_dashboard AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_quality_dashboard AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_crm_dashboard AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_growth_dashboard AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_inventory_dashboard AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_marketplace_dashboard AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_enterprise_dashboard AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_optimization_dashboard AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_appointments_today AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_live_operation_summary AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_cash_summary AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_dre_summary AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_stock_critical AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_retention_risk_clients AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_growth_opportunities AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;
CREATE OR REPLACE VIEW barber.vw_immutable_audit_integrity AS SELECT t.id AS tenant_id, t.name AS tenant_name, count(u.id) AS users_count FROM barber.tenants t LEFT JOIN barber.users u ON u.tenant_id=t.id GROUP BY t.id,t.name;

-- BARBERSYNC_EVOLUTION_2026_CORE_RULES
CREATE TABLE IF NOT EXISTS barber.service_packages (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(), tenant_id uuid NOT NULL, branch_id uuid NULL,
    name varchar(150) NOT NULL, validity_days int NOT NULL DEFAULT 30, allow_partial_usage boolean NOT NULL DEFAULT true,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE', is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NULL, created_by uuid NULL, updated_by uuid NULL
);
CREATE TABLE IF NOT EXISTS barber.service_package_items (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(), tenant_id uuid NOT NULL, branch_id uuid NULL, service_package_id uuid NOT NULL,
    service_id uuid NOT NULL, quantity int NOT NULL DEFAULT 1,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE', is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NULL, created_by uuid NULL, updated_by uuid NULL
);
CREATE TABLE IF NOT EXISTS barber.customer_service_packages (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(), tenant_id uuid NOT NULL, branch_id uuid NULL, customer_id uuid NOT NULL,
    service_package_id uuid NOT NULL, total_uses int NOT NULL, remaining_uses int NOT NULL, expires_at timestamptz NOT NULL,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE', is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NULL, created_by uuid NULL, updated_by uuid NULL
);
CREATE TABLE IF NOT EXISTS barber.customer_service_package_usage (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(), tenant_id uuid NOT NULL, branch_id uuid NULL, customer_service_package_id uuid NOT NULL,
    appointment_id uuid NULL, service_order_id uuid NULL, used_quantity int NOT NULL DEFAULT 1, used_at timestamptz NOT NULL DEFAULT now(),
    status varchar(30) NOT NULL DEFAULT 'ACTIVE', is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NULL, created_by uuid NULL, updated_by uuid NULL
);
CREATE TABLE IF NOT EXISTS barber.appointment_recurrences (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(), tenant_id uuid NOT NULL, branch_id uuid NULL, appointment_id uuid NOT NULL,
    recurrence_pattern varchar(50) NOT NULL, next_occurrence_at timestamptz NULL,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE', is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NULL, created_by uuid NULL, updated_by uuid NULL
);
CREATE TABLE IF NOT EXISTS barber.appointment_reschedules (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(), tenant_id uuid NOT NULL, branch_id uuid NULL, appointment_id uuid NOT NULL,
    old_start_at timestamptz NOT NULL, new_start_at timestamptz NOT NULL, reason text NULL,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE', is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NULL, created_by uuid NULL, updated_by uuid NULL
);
CREATE TABLE IF NOT EXISTS barber.cancellation_policies (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(), tenant_id uuid NOT NULL, branch_id uuid NULL,
    minimum_notice_minutes int NOT NULL DEFAULT 120, cancellation_fee_pct numeric(5,2) NOT NULL DEFAULT 0, max_no_show_before_block int NOT NULL DEFAULT 3,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE', is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NULL, created_by uuid NULL, updated_by uuid NULL
);
CREATE TABLE IF NOT EXISTS barber.cancellation_fees (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(), tenant_id uuid NOT NULL, branch_id uuid NULL, appointment_id uuid NOT NULL,
    amount numeric(18,2) NOT NULL DEFAULT 0, waived boolean NOT NULL DEFAULT false,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE', is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NULL, created_by uuid NULL, updated_by uuid NULL
);
CREATE TABLE IF NOT EXISTS barber.client_no_show_history (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(), tenant_id uuid NOT NULL, branch_id uuid NULL, client_id uuid NOT NULL, appointment_id uuid NULL,
    occurred_at timestamptz NOT NULL DEFAULT now(), notes text NULL,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE', is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NULL, created_by uuid NULL, updated_by uuid NULL
);
CREATE TABLE IF NOT EXISTS barber.client_booking_blocks (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(), tenant_id uuid NOT NULL, branch_id uuid NULL, client_id uuid NOT NULL, blocked_until timestamptz NULL, reason text NULL,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE', is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NULL, created_by uuid NULL, updated_by uuid NULL
);
CREATE TABLE IF NOT EXISTS barber.campaign_coupon_usage (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(), tenant_id uuid NOT NULL, branch_id uuid NULL, campaign_coupon_id uuid NOT NULL, client_id uuid NOT NULL, service_order_id uuid NULL, used_at timestamptz NOT NULL DEFAULT now(),
    status varchar(30) NOT NULL DEFAULT 'ACTIVE', is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NULL, created_by uuid NULL, updated_by uuid NULL
);
CREATE TABLE IF NOT EXISTS barber.stock_transfers (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(), tenant_id uuid NOT NULL, branch_id uuid NULL, product_id uuid NOT NULL, source_branch_id uuid NOT NULL, destination_branch_id uuid NOT NULL, quantity numeric(18,3) NOT NULL,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE', is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NULL, created_by uuid NULL, updated_by uuid NULL
);
CREATE TABLE IF NOT EXISTS barber.client_timeline (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(), tenant_id uuid NOT NULL, branch_id uuid NULL, client_id uuid NOT NULL, event_type varchar(60) NOT NULL, event_payload jsonb NOT NULL DEFAULT '{}'::jsonb, event_at timestamptz NOT NULL DEFAULT now(),
    status varchar(30) NOT NULL DEFAULT 'ACTIVE', is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NULL, created_by uuid NULL, updated_by uuid NULL
);
CREATE TABLE IF NOT EXISTS barber.client_internal_notes (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(), tenant_id uuid NOT NULL, branch_id uuid NULL, client_id uuid NOT NULL, note text NOT NULL,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE', is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NULL, created_by uuid NULL, updated_by uuid NULL
);
CREATE TABLE IF NOT EXISTS barber.client_scores (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(), tenant_id uuid NOT NULL, branch_id uuid NULL, client_id uuid NOT NULL, loyalty_score numeric(10,2) NOT NULL DEFAULT 0, churn_risk_score numeric(10,2) NOT NULL DEFAULT 0,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE', is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NULL, created_by uuid NULL, updated_by uuid NULL
);
CREATE TABLE IF NOT EXISTS barber.client_next_best_actions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(), tenant_id uuid NOT NULL, branch_id uuid NULL, client_id uuid NOT NULL, action_title varchar(200) NOT NULL, confidence_score numeric(5,2) NOT NULL DEFAULT 0,
    status varchar(30) NOT NULL DEFAULT 'ACTIVE', is_deleted boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NULL, created_by uuid NULL, updated_by uuid NULL
);

-- Commercial CRM (phase 1)
create table if not exists barber.crm_leads (
    id uuid primary key default gen_random_uuid(),
    tenant_id uuid not null,
    branch_id uuid null,
    full_name varchar(180) not null,
    phone varchar(30) null,
    email varchar(180) null,
    source varchar(40) not null,
    status varchar(30) not null default 'ACTIVE',
    temperature varchar(20) not null default 'WARM',
    assigned_to_user_id uuid null,
    is_converted boolean not null default false,
    converted_client_id uuid null,
    loss_reason varchar(300) null,
    last_contact_at timestamptz null,
    created_at timestamptz not null default now(),
    updated_at timestamptz null,
    created_by uuid null,
    updated_by uuid null,
    is_deleted boolean not null default false
);

create table if not exists barber.crm_lead_status_history (
    id uuid primary key default gen_random_uuid(),
    tenant_id uuid not null,
    branch_id uuid null,
    lead_id uuid not null,
    status varchar(30) not null,
    reason varchar(300) null,
    created_at timestamptz not null default now(),
    updated_at timestamptz null,
    created_by uuid null,
    updated_by uuid null,
    is_deleted boolean not null default false
);

create view barber.vw_lead_conversion_summary as
select tenant_id,
       count(*) filter (where not is_deleted) as total_leads,
       count(*) filter (where is_converted and not is_deleted) as converted_leads,
       round((count(*) filter (where is_converted and not is_deleted)::numeric / nullif(count(*) filter (where not is_deleted), 0)) * 100, 2) as conversion_rate_percent
from barber.crm_leads
group by tenant_id;
