CREATE TABLE IF NOT EXISTS ai_agents (id BIGSERIAL PRIMARY KEY, name VARCHAR(80), domain VARCHAR(80), active BOOLEAN DEFAULT TRUE);
CREATE TABLE IF NOT EXISTS ai_agent_runs (id BIGSERIAL PRIMARY KEY, agent_id BIGINT REFERENCES ai_agents(id), context JSONB, created_at TIMESTAMP DEFAULT NOW());
CREATE TABLE IF NOT EXISTS ai_agent_suggestions (id BIGSERIAL PRIMARY KEY, run_id BIGINT REFERENCES ai_agent_runs(id), priority VARCHAR(20), impact VARCHAR(20), suggestion TEXT);
CREATE TABLE IF NOT EXISTS ai_agent_actions (id BIGSERIAL PRIMARY KEY, run_id BIGINT REFERENCES ai_agent_runs(id), action_type VARCHAR(80), requires_approval BOOLEAN DEFAULT TRUE, status VARCHAR(30) DEFAULT 'PENDING');
CREATE TABLE IF NOT EXISTS ai_agent_action_approvals (id BIGSERIAL PRIMARY KEY, action_id BIGINT REFERENCES ai_agent_actions(id), approver VARCHAR(120), status VARCHAR(20), approved_at TIMESTAMP);
CREATE TABLE IF NOT EXISTS ai_agent_feedback (id BIGSERIAL PRIMARY KEY, run_id BIGINT REFERENCES ai_agent_runs(id), feedback_score INT, comments TEXT);
CREATE TABLE IF NOT EXISTS ai_agent_guardrails (id BIGSERIAL PRIMARY KEY, action_type VARCHAR(80), rule_text TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS ai_agent_execution_logs (id BIGSERIAL PRIMARY KEY, run_id BIGINT REFERENCES ai_agent_runs(id), log_level VARCHAR(20), message TEXT, created_at TIMESTAMP DEFAULT NOW());