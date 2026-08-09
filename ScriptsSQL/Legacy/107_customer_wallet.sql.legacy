CREATE TABLE IF NOT EXISTS customer_wallets (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    client_id UUID NOT NULL,
    cashback_balance NUMERIC(12,2) NOT NULL DEFAULT 0,
    credit_balance NUMERIC(12,2) NOT NULL DEFAULT 0,
    is_blocked BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);
CREATE TABLE IF NOT EXISTS wallet_transaction_types (
    id UUID PRIMARY KEY,
    code VARCHAR(40) UNIQUE NOT NULL,
    description VARCHAR(120) NOT NULL
);
CREATE TABLE IF NOT EXISTS wallet_transactions (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    wallet_id UUID NOT NULL REFERENCES customer_wallets(id),
    transaction_type_id UUID NOT NULL REFERENCES wallet_transaction_types(id),
    amount NUMERIC(12,2) NOT NULL,
    currency VARCHAR(5) NOT NULL DEFAULT 'BRL',
    notes VARCHAR(255),
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);
