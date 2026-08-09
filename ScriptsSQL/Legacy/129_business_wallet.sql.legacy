-- 129_business_wallet.sql
CREATE TABLE IF NOT EXISTS business_wallets (
  id BIGSERIAL PRIMARY KEY,
  tenant_id BIGINT NOT NULL,
  branch_id BIGINT NULL,
  status VARCHAR(30) NOT NULL DEFAULT 'ACTIVE',
  currency_code CHAR(3) NOT NULL DEFAULT 'BRL',
  created_at TIMESTAMP NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
  UNIQUE(tenant_id, branch_id)
);
CREATE TABLE IF NOT EXISTS business_wallet_balances (
  id BIGSERIAL PRIMARY KEY,
  wallet_id BIGINT NOT NULL REFERENCES business_wallets(id),
  available_balance NUMERIC(14,2) NOT NULL DEFAULT 0,
  future_balance NUMERIC(14,2) NOT NULL DEFAULT 0,
  blocked_balance NUMERIC(14,2) NOT NULL DEFAULT 0,
  chargeback_reserve NUMERIC(14,2) NOT NULL DEFAULT 0,
  updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);
CREATE TABLE IF NOT EXISTS business_wallet_transactions (
  id BIGSERIAL PRIMARY KEY,
  wallet_id BIGINT NOT NULL REFERENCES business_wallets(id),
  transaction_type VARCHAR(40) NOT NULL,
  direction VARCHAR(10) NOT NULL,
  amount NUMERIC(14,2) NOT NULL,
  status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
  reference_code VARCHAR(120) NULL,
  metadata_json JSONB NULL,
  created_at TIMESTAMP NOT NULL DEFAULT NOW()
);
CREATE TABLE IF NOT EXISTS receivables (
  id BIGSERIAL PRIMARY KEY,
  wallet_id BIGINT NOT NULL REFERENCES business_wallets(id),
  source_type VARCHAR(40) NOT NULL,
  gross_amount NUMERIC(14,2) NOT NULL,
  net_amount NUMERIC(14,2) NOT NULL,
  fee_amount NUMERIC(14,2) NOT NULL DEFAULT 0,
  expected_settlement_date DATE NOT NULL,
  status VARCHAR(20) NOT NULL DEFAULT 'OPEN',
  created_at TIMESTAMP NOT NULL DEFAULT NOW()
);
CREATE TABLE IF NOT EXISTS receivable_schedules (
  id BIGSERIAL PRIMARY KEY,
  receivable_id BIGINT NOT NULL REFERENCES receivables(id),
  installment_number INT NOT NULL,
  due_date DATE NOT NULL,
  amount NUMERIC(14,2) NOT NULL,
  status VARCHAR(20) NOT NULL DEFAULT 'PENDING'
);
CREATE TABLE IF NOT EXISTS payout_accounts (
  id BIGSERIAL PRIMARY KEY,
  tenant_id BIGINT NOT NULL,
  account_name VARCHAR(120) NOT NULL,
  provider_name VARCHAR(60) NOT NULL DEFAULT 'MOCK_PROVIDER',
  account_token VARCHAR(120) NOT NULL,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMP NOT NULL DEFAULT NOW()
);
CREATE TABLE IF NOT EXISTS payout_requests (
  id BIGSERIAL PRIMARY KEY,
  wallet_id BIGINT NOT NULL REFERENCES business_wallets(id),
  payout_account_id BIGINT NOT NULL REFERENCES payout_accounts(id),
  requested_amount NUMERIC(14,2) NOT NULL,
  fee_amount NUMERIC(14,2) NOT NULL DEFAULT 0,
  status VARCHAR(20) NOT NULL DEFAULT 'REQUESTED',
  requested_at TIMESTAMP NOT NULL DEFAULT NOW(),
  approved_at TIMESTAMP NULL
);
CREATE TABLE IF NOT EXISTS payout_batches (
  id BIGSERIAL PRIMARY KEY,
  tenant_id BIGINT NOT NULL,
  batch_date DATE NOT NULL,
  status VARCHAR(20) NOT NULL DEFAULT 'OPEN',
  total_amount NUMERIC(14,2) NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS payout_transactions (
  id BIGSERIAL PRIMARY KEY,
  payout_batch_id BIGINT NULL REFERENCES payout_batches(id),
  payout_request_id BIGINT NOT NULL REFERENCES payout_requests(id),
  provider_transaction_id VARCHAR(120) NULL,
  status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
  processed_at TIMESTAMP NULL
);
CREATE TABLE IF NOT EXISTS platform_fees (
  id BIGSERIAL PRIMARY KEY,
  tenant_id BIGINT NOT NULL,
  fee_type VARCHAR(40) NOT NULL,
  percentage NUMERIC(7,4) NOT NULL,
  fixed_amount NUMERIC(14,2) NOT NULL DEFAULT 0,
  is_active BOOLEAN NOT NULL DEFAULT TRUE
);
CREATE TABLE IF NOT EXISTS split_payment_rules (
  id BIGSERIAL PRIMARY KEY,
  tenant_id BIGINT NOT NULL,
  rule_name VARCHAR(120) NOT NULL,
  destination_type VARCHAR(40) NOT NULL,
  destination_id BIGINT NOT NULL,
  percentage NUMERIC(7,4) NOT NULL,
  is_active BOOLEAN NOT NULL DEFAULT TRUE
);
CREATE TABLE IF NOT EXISTS split_payment_transactions (
  id BIGSERIAL PRIMARY KEY,
  transaction_id BIGINT NOT NULL REFERENCES business_wallet_transactions(id),
  split_rule_id BIGINT NOT NULL REFERENCES split_payment_rules(id),
  split_amount NUMERIC(14,2) NOT NULL,
  status VARCHAR(20) NOT NULL DEFAULT 'PENDING'
);
CREATE TABLE IF NOT EXISTS financial_reconciliation_logs (
  id BIGSERIAL PRIMARY KEY,
  tenant_id BIGINT NOT NULL,
  reconciled_at TIMESTAMP NOT NULL DEFAULT NOW(),
  reference_period VARCHAR(7) NOT NULL,
  discrepancies_found INT NOT NULL DEFAULT 0,
  notes TEXT NULL
);
CREATE OR REPLACE VIEW vw_business_wallet_summary AS
SELECT w.tenant_id,w.branch_id,b.available_balance,b.future_balance,b.blocked_balance,b.chargeback_reserve,
COALESCE((SELECT SUM(r.net_amount) FROM receivables r WHERE r.wallet_id=w.id AND r.status='OPEN'),0) open_receivables
FROM business_wallets w
LEFT JOIN business_wallet_balances b ON b.wallet_id=w.id;
