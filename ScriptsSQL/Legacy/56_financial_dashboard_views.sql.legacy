CREATE OR REPLACE VIEW vw_financial_dashboard AS SELECT tenant_id, SUM(CASE WHEN type='IN' THEN amount ELSE -amount END) AS projected_cash FROM cashflow_entries GROUP BY tenant_id;
