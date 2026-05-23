CREATE TABLE IF NOT EXISTS dre_statements(id UUID PRIMARY KEY, tenant_id UUID NOT NULL, period_start DATE, period_end DATE, net_revenue NUMERIC(14,2), operating_profit NUMERIC(14,2));
