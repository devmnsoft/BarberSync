CREATE TABLE IF NOT EXISTS suppliers(id UUID PRIMARY KEY, tenant_id UUID NOT NULL, name TEXT, document TEXT);
CREATE TABLE IF NOT EXISTS accounts_payable(id UUID PRIMARY KEY, tenant_id UUID NOT NULL, amount NUMERIC(14,2), due_date TIMESTAMP, status TEXT);
