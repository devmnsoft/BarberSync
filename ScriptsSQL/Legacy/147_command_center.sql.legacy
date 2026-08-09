CREATE TABLE IF NOT EXISTS command_center_snapshots (id BIGSERIAL PRIMARY KEY, captured_at TIMESTAMP DEFAULT NOW(), payload JSONB NOT NULL);
CREATE TABLE IF NOT EXISTS regional_operations (id BIGSERIAL PRIMARY KEY, region VARCHAR(80) NOT NULL, active_tenants INT NOT NULL DEFAULT 0);
CREATE TABLE IF NOT EXISTS regional_platform_metrics (id BIGSERIAL PRIMARY KEY, region VARCHAR(80), metric_name VARCHAR(80), metric_value NUMERIC(18,2), captured_at TIMESTAMP DEFAULT NOW());
CREATE TABLE IF NOT EXISTS national_alerts (id BIGSERIAL PRIMARY KEY, severity VARCHAR(20), message TEXT, created_at TIMESTAMP DEFAULT NOW());
CREATE TABLE IF NOT EXISTS platform_health_events (id BIGSERIAL PRIMARY KEY, component VARCHAR(80), status VARCHAR(20), event_time TIMESTAMP DEFAULT NOW());