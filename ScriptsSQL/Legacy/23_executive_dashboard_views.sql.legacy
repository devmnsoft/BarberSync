create or replace view vw_unit_ranking as select unit_id, max(snapshot_date) as snapshot_date, sum(revenue) as revenue from unit_performance_snapshots group by unit_id;
create or replace view vw_professional_ranking as select professional_id, max(snapshot_date) as snapshot_date, sum(revenue) as revenue from professional_performance_metrics group by professional_id;
create or replace view vw_client_lifecycle as select tenant_id, status, count(*) as total_clients from client_lifecycle_status group by tenant_id, status;
create or replace view vw_campaign_results as select c.tenant_id, c.name, r.revenue_generated, r.redemptions, r.roi from marketing_campaigns c left join campaign_results r on r.campaign_id = c.id;
create or replace view vw_financial_forecast as select tenant_id, month_ref, sum(forecast_revenue) as forecast_revenue, sum(forecast_expenses) as forecast_expenses from financial_plans group by tenant_id, month_ref;
create or replace view vw_reputation_summary as select tenant_id, branch_id, avg(score)::numeric(4,2) as avg_score, count(*) filter (where score <= 2) as low_scores from reputation_reviews group by tenant_id, branch_id;
create or replace view vw_business_insights as select tenant_id, priority, count(*) as total from business_insights group by tenant_id, priority;
create or replace view vw_executive_dashboard as select f.tenant_id, f.month_ref, f.forecast_revenue, f.forecast_expenses, (f.forecast_revenue-f.forecast_expenses) as projected_result from vw_financial_forecast f;
