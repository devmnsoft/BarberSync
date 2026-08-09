create table if not exists financial_plans (id uuid primary key, tenant_id uuid not null, branch_id uuid not null, month_ref text not null, forecast_revenue numeric(12,2), forecast_expenses numeric(12,2), break_even_point numeric(12,2));
create table if not exists fixed_costs (id uuid primary key, plan_id uuid references financial_plans(id), name text not null, amount numeric(12,2) not null);
create table if not exists variable_costs (id uuid primary key, plan_id uuid references financial_plans(id), name text not null, amount numeric(12,2) not null);
create table if not exists revenue_forecasts (id uuid primary key, plan_id uuid references financial_plans(id), source text not null, amount numeric(12,2) not null);
create table if not exists expense_forecasts (id uuid primary key, plan_id uuid references financial_plans(id), category text not null, amount numeric(12,2) not null);
create table if not exists profit_analysis (id uuid primary key, plan_id uuid references financial_plans(id), margin_service numeric(6,2), margin_product numeric(6,2), simplified_dre jsonb);
create table if not exists cashflow_forecasts (id uuid primary key, plan_id uuid references financial_plans(id), week_ref text not null, inflow numeric(12,2), outflow numeric(12,2));
