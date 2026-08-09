CREATE TABLE IF NOT EXISTS supplier_profiles(id UUID PRIMARY KEY, tenant_id UUID NOT NULL, name TEXT NOT NULL, category TEXT);
CREATE TABLE IF NOT EXISTS supplier_contacts(id UUID PRIMARY KEY, supplier_id UUID NOT NULL, contact_name TEXT, phone TEXT, email TEXT);
CREATE TABLE IF NOT EXISTS supplier_categories(id UUID PRIMARY KEY, tenant_id UUID NOT NULL, name TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS supplier_products(id UUID PRIMARY KEY, supplier_id UUID NOT NULL, product_name TEXT NOT NULL, unit_cost NUMERIC(12,2));
CREATE TABLE IF NOT EXISTS supplier_contracts(id UUID PRIMARY KEY, supplier_id UUID NOT NULL, expires_at DATE, terms TEXT);
CREATE TABLE IF NOT EXISTS supplier_documents(id UUID PRIMARY KEY, supplier_id UUID NOT NULL, document_name TEXT NOT NULL, url TEXT);
CREATE TABLE IF NOT EXISTS supplier_ratings(id UUID PRIMARY KEY, supplier_id UUID NOT NULL, score NUMERIC(4,2));
CREATE TABLE IF NOT EXISTS supplier_performance_metrics(id UUID PRIMARY KEY, supplier_id UUID NOT NULL, on_time_rate NUMERIC(5,2), quality_rate NUMERIC(5,2));
