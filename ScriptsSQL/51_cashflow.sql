CREATE TABLE IF NOT EXISTS cashflow_entries(id UUID PRIMARY KEY, tenant_id UUID NOT NULL, type TEXT, amount NUMERIC(14,2), entry_date TIMESTAMP);
