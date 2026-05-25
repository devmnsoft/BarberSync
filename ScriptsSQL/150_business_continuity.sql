CREATE TABLE IF NOT EXISTS business_continuity_plans (id BIGSERIAL PRIMARY KEY, name VARCHAR(160), rto_minutes INT, rpo_minutes INT);
CREATE TABLE IF NOT EXISTS disaster_recovery_plans (id BIGSERIAL PRIMARY KEY, continuity_plan_id BIGINT REFERENCES business_continuity_plans(id), dr_region VARCHAR(80), status VARCHAR(30));
CREATE TABLE IF NOT EXISTS recovery_tests (id BIGSERIAL PRIMARY KEY, dr_plan_id BIGINT REFERENCES disaster_recovery_plans(id), tested_at TIMESTAMP, result VARCHAR(30));
CREATE TABLE IF NOT EXISTS backup_status_logs (id BIGSERIAL PRIMARY KEY, component VARCHAR(120), backup_status VARCHAR(30), checked_at TIMESTAMP DEFAULT NOW());
CREATE TABLE IF NOT EXISTS critical_dependencies (id BIGSERIAL PRIMARY KEY, dependency_name VARCHAR(120), owner VARCHAR(120));
CREATE TABLE IF NOT EXISTS emergency_contacts (id BIGSERIAL PRIMARY KEY, name VARCHAR(120), role_name VARCHAR(80), phone VARCHAR(40));
CREATE TABLE IF NOT EXISTS continuity_evidences (id BIGSERIAL PRIMARY KEY, continuity_plan_id BIGINT REFERENCES business_continuity_plans(id), evidence TEXT);