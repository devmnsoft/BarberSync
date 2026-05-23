CREATE TABLE IF NOT EXISTS expansion_projects(id UUID PRIMARY KEY, tenant_id UUID NOT NULL, name TEXT NOT NULL, city TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS expansion_cost_estimates(id UUID PRIMARY KEY, project_id UUID NOT NULL, investment NUMERIC(14,2), fixed_cost NUMERIC(14,2));
CREATE TABLE IF NOT EXISTS expansion_revenue_forecasts(id UUID PRIMARY KEY, project_id UUID NOT NULL, monthly_revenue NUMERIC(14,2));
CREATE TABLE IF NOT EXISTS expansion_feasibility_analysis(id UUID PRIMARY KEY, project_id UUID NOT NULL, payback_months NUMERIC(6,2), viability TEXT);
CREATE TABLE IF NOT EXISTS expansion_checklists(id UUID PRIMARY KEY, project_id UUID NOT NULL, item TEXT NOT NULL, done BOOLEAN DEFAULT FALSE);
CREATE TABLE IF NOT EXISTS expansion_action_plans(id UUID PRIMARY KEY, project_id UUID NOT NULL, action TEXT NOT NULL, owner TEXT);
