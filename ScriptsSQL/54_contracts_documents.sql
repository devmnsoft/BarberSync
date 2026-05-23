CREATE TABLE IF NOT EXISTS documents(id UUID PRIMARY KEY, tenant_id UUID NOT NULL, category TEXT, name TEXT, expires_at TIMESTAMP, status TEXT);
CREATE TABLE IF NOT EXISTS contracts(id UUID PRIMARY KEY, tenant_id UUID NOT NULL, type TEXT, title TEXT, status TEXT);
