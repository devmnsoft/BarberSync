DO $$
DECLARE
    missing text;
BEGIN
    SELECT string_agg(required_table, ', ' ORDER BY required_table)
      INTO missing
      FROM unnest(ARRAY[
        'cash_movements', 'products', 'stock_movements',
        'financial_entries', 'commissions', 'notifications', 'audit_logs',
        'service_orders', 'service_order_items', 'purchase_receipts'
      ]) AS required_table
     WHERE to_regclass('barber.' || required_table) IS NULL;

    IF missing IS NOT NULL THEN
        RAISE EXCEPTION 'Missing critical tables: %', missing;
    END IF;
END $$;
