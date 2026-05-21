-- BarberSync 2.0 - Ultra Advanced Final Expansion

CREATE TABLE IF NOT EXISTS loyalty_transactions (
    id UUID PRIMARY KEY,
    client_id UUID NOT NULL,
    appointment_id UUID,
    transaction_type VARCHAR(40) NOT NULL,
    points_delta INT NOT NULL,
    cashback_delta NUMERIC(12,2) NOT NULL DEFAULT 0,
    metadata JSONB,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS ai_professional_metrics (
    id UUID PRIMARY KEY,
    professional_id UUID NOT NULL,
    appointment_id UUID,
    posture_score NUMERIC(5,4) NOT NULL,
    instrument_score NUMERIC(5,4) NOT NULL,
    technique_score NUMERIC(5,4) NOT NULL,
    efficiency_score NUMERIC(5,4) NOT NULL,
    processed_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS demand_predictions (
    id UUID PRIMARY KEY,
    service_id UUID,
    predicted_hour TIMESTAMP NOT NULL,
    expected_volume INT NOT NULL,
    confidence NUMERIC(5,4) NOT NULL,
    generated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS audit_log_entries (
    id UUID PRIMARY KEY,
    actor_user_id UUID,
    module_name VARCHAR(80) NOT NULL,
    action_name VARCHAR(120) NOT NULL,
    severity VARCHAR(20) NOT NULL,
    payload JSONB,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_loyalty_transactions_client_created ON loyalty_transactions(client_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_ai_metrics_professional_processed ON ai_professional_metrics(professional_id, processed_at DESC);
CREATE INDEX IF NOT EXISTS idx_demand_predictions_hour ON demand_predictions(predicted_hour);
CREATE INDEX IF NOT EXISTS idx_audit_log_module_created ON audit_log_entries(module_name, created_at DESC);

-- Predictive analytics query base for BI tools
CREATE OR REPLACE VIEW vw_predictive_kpis AS
SELECT
    dp.predicted_hour,
    s.name AS service_name,
    dp.expected_volume,
    dp.confidence,
    AVG(apm.efficiency_score) AS avg_efficiency,
    AVG(apm.technique_score) AS avg_technique
FROM demand_predictions dp
LEFT JOIN services s ON s.id = dp.service_id
LEFT JOIN ai_professional_metrics apm ON apm.appointment_id IS NOT NULL
GROUP BY dp.predicted_hour, s.name, dp.expected_volume, dp.confidence;
