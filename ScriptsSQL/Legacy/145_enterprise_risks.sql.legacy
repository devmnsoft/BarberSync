CREATE TABLE IF NOT EXISTS risk_categories (id BIGSERIAL PRIMARY KEY, name VARCHAR(80) NOT NULL UNIQUE);
CREATE TABLE IF NOT EXISTS enterprise_risks (id BIGSERIAL PRIMARY KEY, category_id BIGINT REFERENCES risk_categories(id), title VARCHAR(180) NOT NULL, probability INT NOT NULL, impact INT NOT NULL, status VARCHAR(30) NOT NULL DEFAULT 'OPEN');
CREATE TABLE IF NOT EXISTS risk_assessments (id BIGSERIAL PRIMARY KEY, risk_id BIGINT REFERENCES enterprise_risks(id), score INT NOT NULL, assessed_at TIMESTAMP NOT NULL DEFAULT NOW());
CREATE TABLE IF NOT EXISTS risk_mitigations (id BIGSERIAL PRIMARY KEY, risk_id BIGINT REFERENCES enterprise_risks(id), plan TEXT NOT NULL, owner VARCHAR(120) NOT NULL, due_date DATE);
CREATE TABLE IF NOT EXISTS risk_evidences (id BIGSERIAL PRIMARY KEY, risk_id BIGINT REFERENCES enterprise_risks(id), evidence TEXT NOT NULL, created_at TIMESTAMP DEFAULT NOW());
CREATE TABLE IF NOT EXISTS risk_reviews (id BIGSERIAL PRIMARY KEY, risk_id BIGINT REFERENCES enterprise_risks(id), review_notes TEXT NOT NULL, reviewed_at TIMESTAMP DEFAULT NOW());
CREATE TABLE IF NOT EXISTS risk_heatmap_snapshots (id BIGSERIAL PRIMARY KEY, snapshot_date DATE NOT NULL, payload JSONB NOT NULL);
