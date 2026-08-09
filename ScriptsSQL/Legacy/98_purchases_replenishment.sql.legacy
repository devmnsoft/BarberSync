CREATE TABLE IF NOT EXISTS purchase_requests(id UUID PRIMARY KEY, tenant_id UUID NOT NULL, branch_id UUID NOT NULL, status TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS purchase_request_items(id UUID PRIMARY KEY, request_id UUID NOT NULL, product_id UUID NOT NULL, quantity INT NOT NULL);
CREATE TABLE IF NOT EXISTS purchase_approvals(id UUID PRIMARY KEY, request_id UUID NOT NULL, approved_by UUID, approved_at TIMESTAMP);
CREATE TABLE IF NOT EXISTS purchase_orders(id UUID PRIMARY KEY, supplier_id UUID NOT NULL, status TEXT NOT NULL, created_at TIMESTAMP NOT NULL);
CREATE TABLE IF NOT EXISTS purchase_order_items(id UUID PRIMARY KEY, order_id UUID NOT NULL, product_id UUID NOT NULL, quantity INT, unit_price NUMERIC(12,2));
CREATE TABLE IF NOT EXISTS purchase_receipts(id UUID PRIMARY KEY, order_id UUID NOT NULL, received_at TIMESTAMP, receipt_url TEXT);
CREATE TABLE IF NOT EXISTS purchase_quotes(id UUID PRIMARY KEY, request_id UUID NOT NULL, supplier_id UUID NOT NULL, total NUMERIC(12,2));
CREATE TABLE IF NOT EXISTS purchase_quote_items(id UUID PRIMARY KEY, quote_id UUID NOT NULL, product_id UUID NOT NULL, unit_price NUMERIC(12,2));
CREATE TABLE IF NOT EXISTS purchase_replenishment_suggestions(id UUID PRIMARY KEY, branch_id UUID NOT NULL, product_id UUID NOT NULL, suggested_quantity INT);
