CREATE TABLE IF NOT EXISTS benefit_clubs (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    branch_id UUID,
    name VARCHAR(120) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);
CREATE TABLE IF NOT EXISTS benefit_tiers (
    id UUID PRIMARY KEY,
    club_id UUID NOT NULL REFERENCES benefit_clubs(id),
    name VARCHAR(50) NOT NULL,
    priority INT NOT NULL,
    min_points INT NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS benefit_rules (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    tier_id UUID NOT NULL REFERENCES benefit_tiers(id),
    rule_type VARCHAR(40) NOT NULL,
    rule_value NUMERIC(12,2),
    requires_active_subscription BOOLEAN NOT NULL DEFAULT FALSE,
    valid_until TIMESTAMP
);
CREATE TABLE IF NOT EXISTS customer_benefit_memberships (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    client_id UUID NOT NULL,
    club_id UUID NOT NULL REFERENCES benefit_clubs(id),
    tier_id UUID NOT NULL REFERENCES benefit_tiers(id),
    status VARCHAR(20) NOT NULL,
    started_at TIMESTAMP NOT NULL DEFAULT NOW()
);
CREATE TABLE IF NOT EXISTS benefit_redemptions (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    membership_id UUID NOT NULL REFERENCES customer_benefit_memberships(id),
    rule_id UUID NOT NULL REFERENCES benefit_rules(id),
    redeemed_at TIMESTAMP NOT NULL DEFAULT NOW(),
    context VARCHAR(50)
);
CREATE TABLE IF NOT EXISTS benefit_eligibility_logs (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    client_id UUID NOT NULL,
    rule_id UUID,
    eligible BOOLEAN NOT NULL,
    reason VARCHAR(255),
    checked_at TIMESTAMP NOT NULL DEFAULT NOW()
);
