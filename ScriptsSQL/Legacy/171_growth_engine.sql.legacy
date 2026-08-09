CREATE TABLE IF NOT EXISTS growth_opportunity_types (
    id UUID PRIMARY KEY,
    code VARCHAR(80) NOT NULL UNIQUE,
    name VARCHAR(140) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS growth_opportunities (
    id UUID PRIMARY KEY,
    tenant_id VARCHAR(80) NOT NULL,
    branch_id VARCHAR(80) NOT NULL,
    opportunity_type_id UUID NOT NULL REFERENCES growth_opportunity_types(id),
    title VARCHAR(200) NOT NULL,
    expected_impact NUMERIC(14,2) NOT NULL DEFAULT 0,
    priority VARCHAR(20) NOT NULL,
    status VARCHAR(20) NOT NULL,
    detected_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
