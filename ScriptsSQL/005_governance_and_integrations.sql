-- Governance, integrations and automation
CREATE TABLE IF NOT EXISTS access_policies (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id),
    module_key VARCHAR(80) NOT NULL,
    can_read BOOLEAN NOT NULL DEFAULT TRUE,
    can_write BOOLEAN NOT NULL DEFAULT FALSE,
    can_approve BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS audit_logs (
    id UUID PRIMARY KEY,
    actor_user_id UUID REFERENCES users(id),
    module_key VARCHAR(80) NOT NULL,
    action_key VARCHAR(80) NOT NULL,
    entity_type VARCHAR(80) NOT NULL,
    entity_id UUID,
    payload JSONB,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS external_calendar_links (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id),
    provider VARCHAR(40) NOT NULL,
    external_calendar_id VARCHAR(255) NOT NULL,
    access_token_encrypted TEXT NOT NULL,
    refresh_token_encrypted TEXT,
    synced_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS payment_gateway_events (
    id UUID PRIMARY KEY,
    payment_id UUID REFERENCES payments(id),
    gateway_name VARCHAR(80) NOT NULL,
    event_type VARCHAR(80) NOT NULL,
    status VARCHAR(40) NOT NULL,
    payload JSONB,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_access_policies_user_module ON access_policies(user_id, module_key);
CREATE INDEX IF NOT EXISTS idx_audit_logs_module_created ON audit_logs(module_key, created_at DESC);

CREATE TABLE IF NOT EXISTS smart_notification_triggers (
    id UUID PRIMARY KEY,
    event_key VARCHAR(80) NOT NULL,
    channel VARCHAR(40) NOT NULL,
    target_type VARCHAR(40) NOT NULL,
    template TEXT NOT NULL,
    is_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS ai_video_metrics (
    id UUID PRIMARY KEY,
    appointment_id UUID REFERENCES appointments(id),
    professional_id UUID REFERENCES users(id),
    predicted_service VARCHAR(120) NOT NULL,
    confidence NUMERIC(5,4) NOT NULL,
    posture_score NUMERIC(5,4) NOT NULL,
    instrument_detected VARCHAR(120),
    duration_seconds INT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE VIEW IF NOT EXISTS vw_predictive_peak_hours AS
SELECT EXTRACT(HOUR FROM start_time) AS hour_of_day,
       COUNT(*) AS total_appointments,
       AVG(EXTRACT(EPOCH FROM (end_time - start_time))/60.0) AS avg_minutes
FROM appointments
GROUP BY EXTRACT(HOUR FROM start_time)
ORDER BY total_appointments DESC;

CREATE VIEW IF NOT EXISTS vw_professional_efficiency AS
SELECT sr.professional_id,
       COUNT(*) AS total_recognitions,
       AVG(sr.confidence) AS avg_confidence,
       AVG(sr.duration_seconds)/60.0 AS avg_service_minutes
FROM service_recognition sr
GROUP BY sr.professional_id;
