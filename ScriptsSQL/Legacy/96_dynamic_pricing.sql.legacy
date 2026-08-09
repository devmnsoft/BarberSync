CREATE TABLE IF NOT EXISTS pricing_rules(id UUID PRIMARY KEY, tenant_id UUID NOT NULL, service_id UUID NOT NULL, min_price NUMERIC(12,2), max_price NUMERIC(12,2));
CREATE TABLE IF NOT EXISTS dynamic_price_suggestions(id UUID PRIMARY KEY, service_id UUID NOT NULL, current_price NUMERIC(12,2), suggested_price NUMERIC(12,2), reason TEXT);
CREATE TABLE IF NOT EXISTS service_price_history(id UUID PRIMARY KEY, service_id UUID NOT NULL, old_price NUMERIC(12,2), new_price NUMERIC(12,2), changed_at TIMESTAMP NOT NULL);
CREATE TABLE IF NOT EXISTS service_price_approvals(id UUID PRIMARY KEY, suggestion_id UUID NOT NULL, approved_by UUID, approved_at TIMESTAMP);
CREATE TABLE IF NOT EXISTS price_simulations(id UUID PRIMARY KEY, service_id UUID NOT NULL, price NUMERIC(12,2), expected_margin NUMERIC(6,2), expected_demand NUMERIC(6,2));
CREATE TABLE IF NOT EXISTS professional_service_prices(id UUID PRIMARY KEY, professional_id UUID NOT NULL, service_id UUID NOT NULL, price NUMERIC(12,2));
CREATE TABLE IF NOT EXISTS branch_service_prices(id UUID PRIMARY KEY, branch_id UUID NOT NULL, service_id UUID NOT NULL, price NUMERIC(12,2));
