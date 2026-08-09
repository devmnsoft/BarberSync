-- BarberSync 2.0 Operational bootstrap for predictive analytics and automation readiness.

CREATE TABLE IF NOT EXISTS ai_professional_metrics (
    id UUID PRIMARY KEY,
    professional_id UUID NOT NULL,
    service_id UUID NOT NULL,
    work_date DATE NOT NULL,
    avg_duration_minutes NUMERIC(10,2) NOT NULL,
    efficiency_score NUMERIC(5,2) NOT NULL,
    posture_score NUMERIC(5,2),
    upsell_conversion_rate NUMERIC(5,2),
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS demand_predictions (
    id UUID PRIMARY KEY,
    prediction_date DATE NOT NULL,
    hour_slot SMALLINT NOT NULL,
    service_type VARCHAR(120) NOT NULL,
    expected_volume NUMERIC(10,2) NOT NULL,
    confidence_score NUMERIC(5,2) NOT NULL,
    model_version VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS notification_automation_log (
    id UUID PRIMARY KEY,
    channel VARCHAR(40) NOT NULL,
    target_profile VARCHAR(40) NOT NULL,
    trigger_event VARCHAR(120) NOT NULL,
    payload JSONB NOT NULL,
    delivered BOOLEAN NOT NULL DEFAULT FALSE,
    delivered_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_ai_metrics_professional_date
    ON ai_professional_metrics(professional_id, work_date);

CREATE INDEX IF NOT EXISTS idx_demand_predictions_slot
    ON demand_predictions(prediction_date, hour_slot, service_type);

CREATE INDEX IF NOT EXISTS idx_notification_automation_channel
    ON notification_automation_log(channel, created_at);

-- Analytics view for BI tools.
CREATE OR REPLACE VIEW vw_operational_intelligence AS
SELECT
    m.professional_id,
    m.service_id,
    m.work_date,
    m.avg_duration_minutes,
    m.efficiency_score,
    d.hour_slot,
    d.service_type,
    d.expected_volume,
    d.confidence_score
FROM ai_professional_metrics m
LEFT JOIN demand_predictions d ON d.prediction_date = m.work_date;
