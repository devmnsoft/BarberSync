CREATE TABLE IF NOT EXISTS incident_categories (id BIGSERIAL PRIMARY KEY, name VARCHAR(120) NOT NULL);
CREATE TABLE IF NOT EXISTS incidents (id BIGSERIAL PRIMARY KEY, category_id BIGINT REFERENCES incident_categories(id), title VARCHAR(180) NOT NULL, severity VARCHAR(20) NOT NULL, status VARCHAR(30) NOT NULL DEFAULT 'OPEN', region VARCHAR(60), opened_at TIMESTAMP DEFAULT NOW());
CREATE TABLE IF NOT EXISTS incident_updates (id BIGSERIAL PRIMARY KEY, incident_id BIGINT REFERENCES incidents(id), message TEXT NOT NULL, created_at TIMESTAMP DEFAULT NOW());
CREATE TABLE IF NOT EXISTS incident_impacted_tenants (id BIGSERIAL PRIMARY KEY, incident_id BIGINT REFERENCES incidents(id), tenant_id BIGINT NOT NULL);
CREATE TABLE IF NOT EXISTS incident_sla_tracking (id BIGSERIAL PRIMARY KEY, incident_id BIGINT REFERENCES incidents(id), response_minutes INT, resolution_minutes INT, breached BOOLEAN DEFAULT FALSE);
CREATE TABLE IF NOT EXISTS incident_postmortems (id BIGSERIAL PRIMARY KEY, incident_id BIGINT REFERENCES incidents(id), root_cause TEXT, action_items TEXT);
CREATE TABLE IF NOT EXISTS incident_prevention_actions (id BIGSERIAL PRIMARY KEY, incident_id BIGINT REFERENCES incidents(id), action_name VARCHAR(160), owner VARCHAR(120), due_date DATE);
CREATE TABLE IF NOT EXISTS service_status_components (id BIGSERIAL PRIMARY KEY, component_name VARCHAR(120), status VARCHAR(20), updated_at TIMESTAMP DEFAULT NOW());
CREATE TABLE IF NOT EXISTS service_status_history (id BIGSERIAL PRIMARY KEY, component_id BIGINT REFERENCES service_status_components(id), status VARCHAR(20), changed_at TIMESTAMP DEFAULT NOW());
