CREATE TABLE IF NOT EXISTS customer_preferences (id UUID PRIMARY KEY, tenant_id UUID NOT NULL, client_id UUID NOT NULL, preferred_branch_id UUID, preferred_time_window VARCHAR(50), notes TEXT);
CREATE TABLE IF NOT EXISTS customer_favorites (id UUID PRIMARY KEY, tenant_id UUID NOT NULL, client_id UUID NOT NULL, favorite_type VARCHAR(20) NOT NULL, reference_id UUID NOT NULL);
CREATE TABLE IF NOT EXISTS customer_wishlist (id UUID PRIMARY KEY, tenant_id UUID NOT NULL, client_id UUID NOT NULL, product_id UUID, service_id UUID, created_at TIMESTAMP DEFAULT NOW());
CREATE TABLE IF NOT EXISTS customer_restrictions (id UUID PRIMARY KEY, tenant_id UUID NOT NULL, client_id UUID NOT NULL, restriction_type VARCHAR(40), description VARCHAR(255));
CREATE TABLE IF NOT EXISTS customer_communication_preferences (id UUID PRIMARY KEY, tenant_id UUID NOT NULL, client_id UUID NOT NULL, whatsapp_opt_in BOOLEAN DEFAULT TRUE, email_opt_in BOOLEAN DEFAULT TRUE, promo_opt_in BOOLEAN DEFAULT TRUE);
