CREATE TABLE IF NOT EXISTS franchise_fee_rules(id UUID PRIMARY KEY, tenant_id UUID NOT NULL, initial_fee NUMERIC(14,2), royalty_percent NUMERIC(5,2));
CREATE TABLE IF NOT EXISTS franchise_royalties(id UUID PRIMARY KEY, branch_id UUID NOT NULL, period_month DATE, amount NUMERIC(14,2), status TEXT);
CREATE TABLE IF NOT EXISTS franchise_marketing_fund(id UUID PRIMARY KEY, branch_id UUID NOT NULL, period_month DATE, amount NUMERIC(14,2));
CREATE TABLE IF NOT EXISTS franchise_compliance_standards(id UUID PRIMARY KEY, tenant_id UUID NOT NULL, code TEXT NOT NULL, description TEXT);
CREATE TABLE IF NOT EXISTS franchise_audits(id UUID PRIMARY KEY, branch_id UUID NOT NULL, audit_date DATE, score NUMERIC(5,2));
CREATE TABLE IF NOT EXISTS franchise_audit_items(id UUID PRIMARY KEY, audit_id UUID NOT NULL, standard_id UUID NOT NULL, passed BOOLEAN);
CREATE TABLE IF NOT EXISTS franchise_manuals(id UUID PRIMARY KEY, tenant_id UUID NOT NULL, title TEXT NOT NULL, version TEXT);
CREATE TABLE IF NOT EXISTS franchise_unit_health_scores(id UUID PRIMARY KEY, branch_id UUID NOT NULL, health_score NUMERIC(5,2), calculated_at TIMESTAMP);
CREATE TABLE IF NOT EXISTS franchise_action_plans(id UUID PRIMARY KEY, branch_id UUID NOT NULL, action TEXT NOT NULL, due_date DATE);
