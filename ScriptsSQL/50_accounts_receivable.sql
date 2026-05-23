CREATE TABLE IF NOT EXISTS accounts_receivable(id UUID PRIMARY KEY, tenant_id UUID NOT NULL, amount NUMERIC(14,2), due_date TIMESTAMP, status TEXT);
