CREATE TABLE IF NOT EXISTS competitors(id UUID PRIMARY KEY, tenant_id UUID NOT NULL, name TEXT NOT NULL, positioning TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS competitor_services(id UUID PRIMARY KEY, competitor_id UUID NOT NULL, service_name TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS competitor_prices(id UUID PRIMARY KEY, competitor_service_id UUID NOT NULL, price NUMERIC(12,2) NOT NULL, captured_at TIMESTAMP NOT NULL);
CREATE TABLE IF NOT EXISTS competitor_reviews(id UUID PRIMARY KEY, competitor_id UUID NOT NULL, rating NUMERIC(3,2) NOT NULL, source TEXT);
CREATE TABLE IF NOT EXISTS competitor_market_positions(id UUID PRIMARY KEY, competitor_id UUID NOT NULL, segment TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS competitive_analysis(id UUID PRIMARY KEY, branch_id UUID NOT NULL, summary TEXT NOT NULL, created_at TIMESTAMP NOT NULL);
CREATE TABLE IF NOT EXISTS competitive_opportunities(id UUID PRIMARY KEY, branch_id UUID NOT NULL, title TEXT NOT NULL, impact_score NUMERIC(5,2) NOT NULL);
