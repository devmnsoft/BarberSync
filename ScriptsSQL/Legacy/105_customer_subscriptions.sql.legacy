CREATE TABLE IF NOT EXISTS customer_subscription_plans (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    branch_id UUID,
    name VARCHAR(120) NOT NULL,
    description TEXT,
    billing_cycle VARCHAR(20) NOT NULL,
    price NUMERIC(12,2) NOT NULL,
    auto_renew BOOLEAN NOT NULL DEFAULT TRUE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS customer_subscriptions (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    branch_id UUID,
    plan_id UUID NOT NULL REFERENCES customer_subscription_plans(id),
    client_id UUID NOT NULL,
    status VARCHAR(30) NOT NULL,
    started_at TIMESTAMP NOT NULL,
    next_renewal_at TIMESTAMP,
    canceled_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS customer_subscription_benefits (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    plan_id UUID NOT NULL REFERENCES customer_subscription_plans(id),
    benefit_type VARCHAR(40) NOT NULL,
    benefit_value NUMERIC(12,2),
    usage_limit INT,
    expires_in_days INT
);

CREATE TABLE IF NOT EXISTS customer_subscription_usage (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    subscription_id UUID NOT NULL REFERENCES customer_subscriptions(id),
    service_id UUID,
    used_at TIMESTAMP NOT NULL DEFAULT NOW(),
    amount_used NUMERIC(12,2) NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS customer_subscription_invoices (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    subscription_id UUID NOT NULL REFERENCES customer_subscriptions(id),
    reference_period VARCHAR(20) NOT NULL,
    amount NUMERIC(12,2) NOT NULL,
    status VARCHAR(20) NOT NULL,
    due_at TIMESTAMP,
    paid_at TIMESTAMP
);

CREATE TABLE IF NOT EXISTS customer_subscription_events (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    subscription_id UUID NOT NULL REFERENCES customer_subscriptions(id),
    event_type VARCHAR(40) NOT NULL,
    payload JSONB,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS customer_subscription_pauses (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    subscription_id UUID NOT NULL REFERENCES customer_subscriptions(id),
    paused_at TIMESTAMP NOT NULL,
    resume_at TIMESTAMP,
    reason VARCHAR(255)
);
