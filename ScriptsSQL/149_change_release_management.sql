CREATE TABLE IF NOT EXISTS change_requests (id BIGSERIAL PRIMARY KEY, title VARCHAR(180), risk_level VARCHAR(20), status VARCHAR(30) DEFAULT 'OPEN');
CREATE TABLE IF NOT EXISTS change_approvals (id BIGSERIAL PRIMARY KEY, change_request_id BIGINT REFERENCES change_requests(id), approver VARCHAR(120), status VARCHAR(20));
CREATE TABLE IF NOT EXISTS release_plans (id BIGSERIAL PRIMARY KEY, change_request_id BIGINT REFERENCES change_requests(id), release_window TIMESTAMP, version VARCHAR(40));
CREATE TABLE IF NOT EXISTS release_checklists (id BIGSERIAL PRIMARY KEY, release_plan_id BIGINT REFERENCES release_plans(id), item TEXT, done BOOLEAN DEFAULT FALSE);
CREATE TABLE IF NOT EXISTS rollback_plans (id BIGSERIAL PRIMARY KEY, release_plan_id BIGINT REFERENCES release_plans(id), steps TEXT);
CREATE TABLE IF NOT EXISTS release_notes (id BIGSERIAL PRIMARY KEY, release_plan_id BIGINT REFERENCES release_plans(id), notes TEXT);
CREATE TABLE IF NOT EXISTS release_communications (id BIGSERIAL PRIMARY KEY, release_plan_id BIGINT REFERENCES release_plans(id), audience VARCHAR(80), message TEXT);