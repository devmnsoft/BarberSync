-- Loyalty and cashback
CREATE TABLE loyalty_accounts (
    id UUID PRIMARY KEY,
    client_id UUID NOT NULL REFERENCES clients(id),
    points_balance INT NOT NULL DEFAULT 0,
    cashback_balance NUMERIC(10,2) NOT NULL DEFAULT 0,
    tier_level INT NOT NULL DEFAULT 1,
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE loyalty_transactions (
    id UUID PRIMARY KEY,
    loyalty_account_id UUID NOT NULL REFERENCES loyalty_accounts(id),
    appointment_id UUID REFERENCES appointments(id),
    points_delta INT NOT NULL,
    cashback_delta NUMERIC(10,2) NOT NULL,
    reason VARCHAR(120) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Advanced AI metrics for video and efficiency
CREATE TABLE ai_service_metrics (
    id UUID PRIMARY KEY,
    appointment_id UUID REFERENCES appointments(id),
    professional_id UUID REFERENCES users(id),
    service_id UUID REFERENCES services(id),
    input_type VARCHAR(20) NOT NULL,
    predicted_service VARCHAR(120) NOT NULL,
    confidence NUMERIC(5,4) NOT NULL,
    processing_time_ms INT NOT NULL,
    service_duration_minutes INT NOT NULL,
    efficiency_alert VARCHAR(120),
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE ai_upsell_recommendations (
    id UUID PRIMARY KEY,
    metric_id UUID NOT NULL REFERENCES ai_service_metrics(id),
    suggested_service_id UUID REFERENCES services(id),
    confidence NUMERIC(5,4) NOT NULL,
    accepted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Alerts and notifications
CREATE TABLE alerts_notifications (
    id UUID PRIMARY KEY,
    audience_type VARCHAR(30) NOT NULL,
    audience_id UUID NOT NULL,
    category VARCHAR(60) NOT NULL,
    channel VARCHAR(30) NOT NULL,
    payload JSONB NOT NULL,
    status VARCHAR(30) NOT NULL DEFAULT 'queued',
    trigger_at TIMESTAMP NOT NULL,
    sent_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_ai_service_metrics_professional ON ai_service_metrics(professional_id, created_at DESC);
CREATE INDEX idx_alerts_notifications_status ON alerts_notifications(status, trigger_at);
CREATE INDEX idx_loyalty_accounts_client ON loyalty_accounts(client_id);
