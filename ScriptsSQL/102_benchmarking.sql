CREATE TABLE IF NOT EXISTS benchmark_groups(id UUID PRIMARY KEY, tenant_id UUID NOT NULL, name TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS benchmark_metrics(id UUID PRIMARY KEY, group_id UUID NOT NULL, metric TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS benchmark_results(id UUID PRIMARY KEY, branch_id UUID NOT NULL, metric_id UUID NOT NULL, value NUMERIC(14,2), period DATE);
CREATE TABLE IF NOT EXISTS benchmark_rankings(id UUID PRIMARY KEY, metric_id UUID NOT NULL, branch_id UUID NOT NULL, rank INT);
CREATE TABLE IF NOT EXISTS benchmark_insights(id UUID PRIMARY KEY, group_id UUID NOT NULL, insight TEXT NOT NULL, priority TEXT);
