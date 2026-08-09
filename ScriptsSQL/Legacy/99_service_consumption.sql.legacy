CREATE TABLE IF NOT EXISTS service_consumption_templates(id UUID PRIMARY KEY, service_id UUID NOT NULL, name TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS service_consumption_items(id UUID PRIMARY KEY, template_id UUID NOT NULL, product_id UUID NOT NULL, expected_quantity NUMERIC(12,3));
CREATE TABLE IF NOT EXISTS actual_service_consumption(id UUID PRIMARY KEY, appointment_id UUID NOT NULL, product_id UUID NOT NULL, actual_quantity NUMERIC(12,3));
CREATE TABLE IF NOT EXISTS consumption_variance_logs(id UUID PRIMARY KEY, appointment_id UUID NOT NULL, expected_quantity NUMERIC(12,3), actual_quantity NUMERIC(12,3));
