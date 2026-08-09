CREATE INDEX IF NOT EXISTS idx_appointments_date ON appointments(start_at);
CREATE INDEX IF NOT EXISTS idx_payments_status ON payments(status);
CREATE VIEW IF NOT EXISTS vw_dashboard AS
SELECT date(start_at) AS day, count(*) AS appointments FROM appointments GROUP BY date(start_at);
