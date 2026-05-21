-- BarberSync 2.0 - Futuristic platform expansion
-- Focus: loyalty, gamification, AI telemetry, omnichannel notifications and BI exports

CREATE TABLE IF NOT EXISTS loyalty_wallets (
    id UUID PRIMARY KEY,
    client_id UUID NOT NULL REFERENCES clients(id),
    points_balance INT NOT NULL DEFAULT 0,
    cashback_balance NUMERIC(10,2) NOT NULL DEFAULT 0,
    tier VARCHAR(30) NOT NULL DEFAULT 'bronze',
    tier_score INT NOT NULL DEFAULT 0,
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE(client_id)
);

CREATE TABLE IF NOT EXISTS loyalty_transactions (
    id UUID PRIMARY KEY,
    loyalty_wallet_id UUID NOT NULL REFERENCES loyalty_wallets(id),
    source_type VARCHAR(40) NOT NULL,
    source_reference UUID,
    points_delta INT NOT NULL,
    cashback_delta NUMERIC(10,2) NOT NULL,
    notes TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS gamification_badges (
    id UUID PRIMARY KEY,
    badge_key VARCHAR(80) NOT NULL UNIQUE,
    title VARCHAR(120) NOT NULL,
    description TEXT,
    points_reward INT NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS client_badges (
    id UUID PRIMARY KEY,
    client_id UUID NOT NULL REFERENCES clients(id),
    badge_id UUID NOT NULL REFERENCES gamification_badges(id),
    awarded_at TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE(client_id, badge_id)
);

CREATE TABLE IF NOT EXISTS ai_performance_metrics (
    id UUID PRIMARY KEY,
    professional_id UUID REFERENCES users(id),
    appointment_id UUID REFERENCES appointments(id),
    service_id UUID REFERENCES services(id),
    posture_score NUMERIC(5,4) NOT NULL,
    instrument_score NUMERIC(5,4) NOT NULL,
    predicted_duration_minutes NUMERIC(7,2) NOT NULL,
    real_duration_minutes NUMERIC(7,2),
    efficiency_score NUMERIC(5,4),
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS notification_preferences (
    id UUID PRIMARY KEY,
    user_id UUID REFERENCES users(id),
    client_id UUID REFERENCES clients(id),
    profile_type VARCHAR(20) NOT NULL,
    channel_push BOOLEAN NOT NULL DEFAULT TRUE,
    channel_email BOOLEAN NOT NULL DEFAULT TRUE,
    channel_whatsapp BOOLEAN NOT NULL DEFAULT FALSE,
    channel_telegram BOOLEAN NOT NULL DEFAULT FALSE,
    quiet_hours_start TIME,
    quiet_hours_end TIME,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CHECK ((user_id IS NOT NULL) OR (client_id IS NOT NULL))
);

CREATE TABLE IF NOT EXISTS omni_notifications (
    id UUID PRIMARY KEY,
    target_type VARCHAR(20) NOT NULL,
    target_id UUID NOT NULL,
    trigger_key VARCHAR(100) NOT NULL,
    channel VARCHAR(20) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'queued',
    payload JSONB NOT NULL,
    provider_response JSONB,
    scheduled_to TIMESTAMP,
    sent_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS bi_export_jobs (
    id UUID PRIMARY KEY,
    provider VARCHAR(40) NOT NULL,
    dataset_key VARCHAR(80) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'queued',
    requested_by UUID REFERENCES users(id),
    request_payload JSONB,
    result_location TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    completed_at TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_loyalty_transactions_wallet_created ON loyalty_transactions(loyalty_wallet_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_ai_performance_professional_created ON ai_performance_metrics(professional_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_omni_notifications_status_channel ON omni_notifications(status, channel);
CREATE INDEX IF NOT EXISTS idx_bi_export_jobs_status_created ON bi_export_jobs(status, created_at DESC);

CREATE VIEW IF NOT EXISTS vw_client_vip_ranking AS
SELECT c.id AS client_id,
       c.full_name,
       lw.points_balance,
       lw.cashback_balance,
       lw.tier,
       COUNT(a.id) AS appointments_count
FROM clients c
JOIN loyalty_wallets lw ON lw.client_id = c.id
LEFT JOIN appointments a ON a.client_id = c.id
GROUP BY c.id, c.full_name, lw.points_balance, lw.cashback_balance, lw.tier
ORDER BY lw.points_balance DESC, appointments_count DESC;

CREATE VIEW IF NOT EXISTS vw_professional_productivity_ai AS
SELECT u.id AS professional_id,
       u.full_name,
       COUNT(pm.id) AS measurements,
       AVG(pm.efficiency_score) AS avg_efficiency_score,
       AVG(pm.predicted_duration_minutes) AS avg_predicted_duration,
       AVG(pm.real_duration_minutes) AS avg_real_duration
FROM users u
LEFT JOIN ai_performance_metrics pm ON pm.professional_id = u.id
GROUP BY u.id, u.full_name
ORDER BY avg_efficiency_score DESC NULLS LAST;
