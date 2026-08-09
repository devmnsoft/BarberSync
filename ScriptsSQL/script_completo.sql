BEGIN;
SELECT pg_advisory_xact_lock(4200202401);
DO $$ BEGIN
  IF current_setting('server_version_num')::int < 140000 THEN
    RAISE EXCEPTION 'BarberSync requer PostgreSQL 14 ou superior (servidor atual: %).', current_setting('server_version');
  END IF;
END $$;
CREATE SCHEMA IF NOT EXISTS barber;
CREATE TABLE IF NOT EXISTS barber.schema_versions (
 version varchar(10) PRIMARY KEY, description varchar(200) NOT NULL,
 applied_at timestamptz NOT NULL DEFAULT now(), checksum varchar(64) NOT NULL
);
CREATE TABLE IF NOT EXISTS barber.tenants (
 id uuid PRIMARY KEY, slug varchar(120) NOT NULL, name varchar(180) NOT NULL,
 document varchar(40), status varchar(30) NOT NULL DEFAULT 'Active', is_active boolean NOT NULL DEFAULT true,
 created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz
);
CREATE UNIQUE INDEX IF NOT EXISTS ux_tenants_slug ON barber.tenants(lower(slug));
CREATE TABLE IF NOT EXISTS barber.branches (
 id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), name varchar(180) NOT NULL,
 code varchar(60), timezone varchar(80) NOT NULL DEFAULT 'America/Sao_Paulo', status varchar(30) NOT NULL DEFAULT 'Active',
 is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz,
 deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb
);
CREATE UNIQUE INDEX IF NOT EXISTS ux_branches_tenant_code ON barber.branches(tenant_id, lower(code)) WHERE deleted_at IS NULL;

-- Tabelas são criadas sem extensões: UUIDs são produzidos pela aplicação.
CREATE TABLE IF NOT EXISTS barber.users (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid REFERENCES barber.branches(id), email varchar(254), password_hash text, full_name varchar(180), refresh_token_hash text, refresh_token_expires_at timestamptz, status varchar(30) NOT NULL DEFAULT 'Active', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.roles (id uuid PRIMARY KEY, tenant_id uuid REFERENCES barber.tenants(id), name varchar(100) NOT NULL, code varchar(80) NOT NULL, is_system boolean NOT NULL DEFAULT false, created_at timestamptz NOT NULL DEFAULT now());
CREATE TABLE IF NOT EXISTS barber.permissions (id uuid PRIMARY KEY, code varchar(120) NOT NULL, description varchar(240) NOT NULL);
CREATE TABLE IF NOT EXISTS barber.user_roles (user_id uuid NOT NULL REFERENCES barber.users(id), role_id uuid NOT NULL REFERENCES barber.roles(id), PRIMARY KEY(user_id,role_id));
CREATE TABLE IF NOT EXISTS barber.role_permissions (role_id uuid NOT NULL REFERENCES barber.roles(id), permission_id uuid NOT NULL REFERENCES barber.permissions(id), PRIMARY KEY(role_id,permission_id));
CREATE TABLE IF NOT EXISTS barber.clients (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid REFERENCES barber.branches(id), person_type char(2) NOT NULL DEFAULT 'PF', name varchar(180), normalized_document varchar(20), phone varchar(30), whatsapp varchar(30), email varchar(254), birth_date date, notes text, preferences jsonb NOT NULL DEFAULT '{}'::jsonb, lgpd_consent_at timestamptz, status varchar(30) NOT NULL DEFAULT 'Active', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.professionals (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid REFERENCES barber.branches(id), name varchar(180), specialty varchar(180), default_commission numeric(5,2) NOT NULL DEFAULT 0, work_schedule jsonb NOT NULL DEFAULT '{}'::jsonb, status varchar(30) NOT NULL DEFAULT 'Active', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.services (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid REFERENCES barber.branches(id), category varchar(120), name varchar(180), duration_minutes integer NOT NULL DEFAULT 30, price numeric(14,2) NOT NULL DEFAULT 0, commission_percent numeric(5,2) NOT NULL DEFAULT 0, available_admin boolean NOT NULL DEFAULT true, available_public boolean NOT NULL DEFAULT false, available_mobile boolean NOT NULL DEFAULT false, available_kiosk boolean NOT NULL DEFAULT false, status varchar(30) NOT NULL DEFAULT 'Active', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.professional_services (professional_id uuid NOT NULL REFERENCES barber.professionals(id), service_id uuid NOT NULL REFERENCES barber.services(id), commission_percent numeric(5,2), PRIMARY KEY(professional_id,service_id));
CREATE TABLE IF NOT EXISTS barber.appointments (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id), client_id uuid REFERENCES barber.clients(id), professional_id uuid REFERENCES barber.professionals(id), service_id uuid REFERENCES barber.services(id), scheduled_start timestamptz, scheduled_end timestamptz, status varchar(30) NOT NULL DEFAULT 'Scheduled', origin varchar(30) NOT NULL DEFAULT 'Admin', notes text, cancellation_reason text, checked_in_at timestamptz, started_at timestamptz, completed_at timestamptz, is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.service_orders (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id), appointment_id uuid REFERENCES barber.appointments(id), client_id uuid REFERENCES barber.clients(id), number varchar(40), subtotal numeric(14,2) NOT NULL DEFAULT 0, discount numeric(14,2) NOT NULL DEFAULT 0, surcharge numeric(14,2) NOT NULL DEFAULT 0, total numeric(14,2) NOT NULL DEFAULT 0, notes text, status varchar(30) NOT NULL DEFAULT 'Open', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.service_order_items (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid REFERENCES barber.branches(id), service_order_id uuid NOT NULL REFERENCES barber.service_orders(id), item_type varchar(20) NOT NULL, service_id uuid REFERENCES barber.services(id), product_id uuid, professional_id uuid REFERENCES barber.professionals(id), description varchar(180), quantity numeric(14,3) NOT NULL DEFAULT 1, unit_price numeric(14,2) NOT NULL DEFAULT 0, discount numeric(14,2) NOT NULL DEFAULT 0, total numeric(14,2) NOT NULL DEFAULT 0, status varchar(30) NOT NULL DEFAULT 'Active', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.products (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid REFERENCES barber.branches(id), sku varchar(80), name varchar(180), cost_price numeric(14,2) NOT NULL DEFAULT 0, sale_price numeric(14,2) NOT NULL DEFAULT 0, current_stock numeric(14,3) NOT NULL DEFAULT 0, minimum_stock numeric(14,3) NOT NULL DEFAULT 0, allow_negative_stock boolean NOT NULL DEFAULT false, status varchar(30) NOT NULL DEFAULT 'Active', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname='fk_order_item_product' AND conrelid='barber.service_order_items'::regclass) THEN ALTER TABLE barber.service_order_items ADD CONSTRAINT fk_order_item_product FOREIGN KEY(product_id) REFERENCES barber.products(id); END IF; END $$;
CREATE TABLE IF NOT EXISTS barber.cash_registers (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id), opened_by uuid REFERENCES barber.users(id), closed_by uuid REFERENCES barber.users(id), opening_balance numeric(14,2) NOT NULL DEFAULT 0, expected_balance numeric(14,2), actual_balance numeric(14,2), opened_at timestamptz NOT NULL DEFAULT now(), closed_at timestamptz, status varchar(30) NOT NULL DEFAULT 'Open', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.payments (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id), service_order_id uuid REFERENCES barber.service_orders(id), cash_register_id uuid REFERENCES barber.cash_registers(id), idempotency_key varchar(100), method varchar(30), amount numeric(14,2) NOT NULL DEFAULT 0, paid_at timestamptz, status varchar(30) NOT NULL DEFAULT 'Pending', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.payment_splits (id uuid PRIMARY KEY, payment_id uuid NOT NULL REFERENCES barber.payments(id), method varchar(30) NOT NULL, amount numeric(14,2) NOT NULL);
CREATE TABLE IF NOT EXISTS barber.cash_transactions (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id), cash_register_id uuid NOT NULL REFERENCES barber.cash_registers(id), payment_id uuid REFERENCES barber.payments(id), type varchar(30) NOT NULL, amount numeric(14,2) NOT NULL, description text, created_at timestamptz NOT NULL DEFAULT now());
CREATE TABLE IF NOT EXISTS barber.stock_movements (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id), product_id uuid NOT NULL REFERENCES barber.products(id), service_order_id uuid REFERENCES barber.service_orders(id), type varchar(30) NOT NULL, quantity numeric(14,3) NOT NULL, balance_after numeric(14,3), reason text, created_at timestamptz NOT NULL DEFAULT now(), created_by uuid REFERENCES barber.users(id), status varchar(30) NOT NULL DEFAULT 'Posted', is_active boolean NOT NULL DEFAULT true, updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.commissions (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id), professional_id uuid NOT NULL REFERENCES barber.professionals(id), payment_id uuid NOT NULL REFERENCES barber.payments(id), service_order_item_id uuid REFERENCES barber.service_order_items(id), base_amount numeric(14,2) NOT NULL, percentage numeric(5,2) NOT NULL, amount numeric(14,2) NOT NULL, status varchar(30) NOT NULL DEFAULT 'Pending', created_at timestamptz NOT NULL DEFAULT now());
CREATE TABLE IF NOT EXISTS barber.loyalty_accounts (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid REFERENCES barber.branches(id), client_id uuid NOT NULL REFERENCES barber.clients(id), points numeric(14,2) NOT NULL DEFAULT 0, status varchar(30) NOT NULL DEFAULT 'Active', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.loyalty_transactions (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid REFERENCES barber.branches(id), loyalty_account_id uuid NOT NULL REFERENCES barber.loyalty_accounts(id), payment_id uuid REFERENCES barber.payments(id), points numeric(14,2) NOT NULL, type varchar(30) NOT NULL, created_at timestamptz NOT NULL DEFAULT now(), status varchar(30) NOT NULL DEFAULT 'Posted', is_active boolean NOT NULL DEFAULT true, updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.coupons (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid REFERENCES barber.branches(id), code varchar(60), discount_percent numeric(5,2), valid_from timestamptz, valid_until timestamptz, status varchar(30) NOT NULL DEFAULT 'Active', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.campaigns (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid REFERENCES barber.branches(id), name varchar(180), channel varchar(40), starts_at timestamptz, ends_at timestamptz, status varchar(30) NOT NULL DEFAULT 'Draft', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.reviews (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid REFERENCES barber.branches(id), client_id uuid REFERENCES barber.clients(id), appointment_id uuid REFERENCES barber.appointments(id), professional_id uuid REFERENCES barber.professionals(id), rating smallint, comment text, status varchar(30) NOT NULL DEFAULT 'Pending', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.notifications (id uuid PRIMARY KEY, tenant_id uuid REFERENCES barber.tenants(id), branch_id uuid REFERENCES barber.branches(id), user_id uuid REFERENCES barber.users(id), title varchar(160) NOT NULL, message text NOT NULL, read_at timestamptz, status varchar(30) NOT NULL DEFAULT 'Unread', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.kiosk_devices (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id), code varchar(80), name varchar(160), last_seen_at timestamptz, status varchar(30) NOT NULL DEFAULT 'Offline', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.kiosk_sessions (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id), device_id uuid NOT NULL REFERENCES barber.kiosk_devices(id), client_id uuid REFERENCES barber.clients(id), started_at timestamptz NOT NULL DEFAULT now(), finished_at timestamptz, status varchar(30) NOT NULL DEFAULT 'Started', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.service_recognitions (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id), professional_id uuid REFERENCES barber.professionals(id), appointment_id uuid REFERENCES barber.appointments(id), started_at timestamptz NOT NULL DEFAULT now(), finished_at timestamptz, predicted_service_id uuid REFERENCES barber.services(id), confidence numeric(5,4), confirmed_service_id uuid REFERENCES barber.services(id), confirmed_by uuid REFERENCES barber.users(id), confirmed_at timestamptz, status varchar(30) NOT NULL DEFAULT 'Pending', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.recognition_evidences (id uuid PRIMARY KEY, recognition_id uuid NOT NULL REFERENCES barber.service_recognitions(id), storage_uri text NOT NULL, media_type varchar(80), metadata jsonb NOT NULL DEFAULT '{}'::jsonb, created_at timestamptz NOT NULL DEFAULT now());
CREATE TABLE IF NOT EXISTS barber.audit_logs (id uuid PRIMARY KEY, tenant_id uuid REFERENCES barber.tenants(id), branch_id uuid REFERENCES barber.branches(id), user_id uuid REFERENCES barber.users(id), operation varchar(80) NOT NULL DEFAULT 'Unknown', entity_name varchar(120) NOT NULL, entity_id uuid, before_data jsonb, after_data jsonb, correlation_id varchar(100), created_at timestamptz NOT NULL DEFAULT now(), metadata jsonb NOT NULL DEFAULT '{}'::jsonb, module varchar(80), action varchar(80), description text, status varchar(30) NOT NULL DEFAULT 'Active', is_active boolean NOT NULL DEFAULT true, updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);

-- Evolução não destrutiva das tabelas genéricas das versões anteriores.
ALTER TABLE barber.clients ADD COLUMN IF NOT EXISTS email varchar(254);
ALTER TABLE barber.clients ADD COLUMN IF NOT EXISTS person_type char(2) NOT NULL DEFAULT 'PF';
ALTER TABLE barber.professionals ADD COLUMN IF NOT EXISTS default_commission numeric(5,2) NOT NULL DEFAULT 0;
ALTER TABLE barber.services ADD COLUMN IF NOT EXISTS price numeric(14,2) NOT NULL DEFAULT 0;
ALTER TABLE barber.services ADD COLUMN IF NOT EXISTS duration_minutes integer NOT NULL DEFAULT 30;
ALTER TABLE barber.services ADD COLUMN IF NOT EXISTS commission_percent numeric(5,2) NOT NULL DEFAULT 0;
ALTER TABLE barber.service_orders ADD COLUMN IF NOT EXISTS subtotal numeric(14,2) NOT NULL DEFAULT 0;
ALTER TABLE barber.service_orders ADD COLUMN IF NOT EXISTS discount numeric(14,2) NOT NULL DEFAULT 0;
ALTER TABLE barber.service_orders ADD COLUMN IF NOT EXISTS surcharge numeric(14,2) NOT NULL DEFAULT 0;
ALTER TABLE barber.service_orders ADD COLUMN IF NOT EXISTS total numeric(14,2) NOT NULL DEFAULT 0;
ALTER TABLE barber.service_order_items ADD COLUMN IF NOT EXISTS quantity numeric(14,3) NOT NULL DEFAULT 1;
ALTER TABLE barber.service_order_items ADD COLUMN IF NOT EXISTS unit_price numeric(14,2) NOT NULL DEFAULT 0;
ALTER TABLE barber.service_order_items ADD COLUMN IF NOT EXISTS discount numeric(14,2) NOT NULL DEFAULT 0;
ALTER TABLE barber.service_order_items ADD COLUMN IF NOT EXISTS total numeric(14,2) NOT NULL DEFAULT 0;
ALTER TABLE barber.products ADD COLUMN IF NOT EXISTS cost_price numeric(14,2) NOT NULL DEFAULT 0;
ALTER TABLE barber.products ADD COLUMN IF NOT EXISTS sale_price numeric(14,2) NOT NULL DEFAULT 0;
ALTER TABLE barber.products ADD COLUMN IF NOT EXISTS allow_negative_stock boolean NOT NULL DEFAULT false;
ALTER TABLE barber.clients ADD COLUMN IF NOT EXISTS normalized_document varchar(20);
ALTER TABLE barber.clients ADD COLUMN IF NOT EXISTS name varchar(180);
ALTER TABLE barber.appointments ADD COLUMN IF NOT EXISTS scheduled_start timestamptz;
ALTER TABLE barber.appointments ADD COLUMN IF NOT EXISTS scheduled_end timestamptz;
ALTER TABLE barber.appointments ADD COLUMN IF NOT EXISTS professional_id uuid;
ALTER TABLE barber.appointments ADD COLUMN IF NOT EXISTS client_id uuid;
ALTER TABLE barber.appointments ADD COLUMN IF NOT EXISTS service_id uuid;
ALTER TABLE barber.payments ADD COLUMN IF NOT EXISTS amount numeric(14,2) NOT NULL DEFAULT 0;
ALTER TABLE barber.products ADD COLUMN IF NOT EXISTS current_stock numeric(14,3) NOT NULL DEFAULT 0;
ALTER TABLE barber.products ADD COLUMN IF NOT EXISTS minimum_stock numeric(14,3) NOT NULL DEFAULT 0;
ALTER TABLE barber.services ALTER COLUMN price TYPE numeric(14,2) USING price::numeric(14,2);
ALTER TABLE barber.services ALTER COLUMN price SET DEFAULT 0;
UPDATE barber.services SET price=0 WHERE price IS NULL;
ALTER TABLE barber.services ALTER COLUMN price SET NOT NULL;

-- Saneamento anterior às constraints: preserva registros, corrigindo somente valores impossíveis.
UPDATE barber.services SET duration_minutes=1 WHERE duration_minutes < 1;
UPDATE barber.services SET commission_percent=greatest(0,least(100,commission_percent));
UPDATE barber.professionals SET default_commission=greatest(0,least(100,default_commission));
UPDATE barber.products SET current_stock=0 WHERE current_stock < 0 AND NOT allow_negative_stock;
UPDATE barber.clients SET email=lower(trim(email)), normalized_document=regexp_replace(normalized_document,'[^0-9]','','g');
DO $$ DECLARE item record; BEGIN
 FOR item IN SELECT * FROM (VALUES
 ('ck_clients_person_type','clients','person_type IN (''PF'',''PJ'')'),
 ('ck_services_price','services','price >= 0'),
 ('ck_services_duration','services','duration_minutes > 0'),
 ('ck_services_commission','services','commission_percent BETWEEN 0 AND 100'),
 ('ck_professionals_commission','professionals','default_commission BETWEEN 0 AND 100'),
 ('ck_order_totals','service_orders','subtotal >= 0 AND discount >= 0 AND surcharge >= 0 AND total >= 0'),
 ('ck_order_item_values','service_order_items','quantity > 0 AND unit_price >= 0 AND discount >= 0 AND total >= 0'),
 ('ck_payment_amount','payments','amount > 0'),
 ('ck_split_amount','payment_splits','amount > 0'),
 ('ck_product_values','products','cost_price >= 0 AND sale_price >= 0 AND minimum_stock >= 0'),
 ('ck_commission_values','commissions','base_amount >= 0 AND percentage BETWEEN 0 AND 100 AND amount >= 0'),
 ('ck_review_rating','reviews','rating BETWEEN 1 AND 5'),
 ('ck_recognition_confidence','service_recognitions','confidence BETWEEN 0 AND 1')
 ) AS x(conname,tbl,expression)
 LOOP IF NOT EXISTS(SELECT 1 FROM pg_constraint WHERE conname=item.conname AND conrelid=('barber.'||item.tbl)::regclass) THEN EXECUTE format('ALTER TABLE barber.%I ADD CONSTRAINT %I CHECK (%s) NOT VALID',item.tbl,item.conname,item.expression); END IF; END LOOP;
END $$;
CREATE UNIQUE INDEX IF NOT EXISTS ux_users_tenant_email ON barber.users(tenant_id,lower(email)) WHERE deleted_at IS NULL AND email IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS ux_clients_tenant_document ON barber.clients(tenant_id,normalized_document) WHERE deleted_at IS NULL AND normalized_document IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS ux_products_tenant_branch_sku ON barber.products(tenant_id,branch_id,lower(sku)) WHERE deleted_at IS NULL AND sku IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS ux_payments_idempotency ON barber.payments(tenant_id,idempotency_key) WHERE idempotency_key IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_appointments_schedule ON barber.appointments(tenant_id,branch_id,professional_id,scheduled_start,scheduled_end) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_orders_status ON barber.service_orders(tenant_id,branch_id,status,created_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_payments_order ON barber.payments(tenant_id,service_order_id,status);
CREATE INDEX IF NOT EXISTS ix_stock_product ON barber.stock_movements(tenant_id,branch_id,product_id,created_at DESC);
CREATE INDEX IF NOT EXISTS ix_products_low_stock ON barber.products(tenant_id,branch_id,current_stock,minimum_stock) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_audit_entity ON barber.audit_logs(tenant_id,entity_name,entity_id,created_at DESC);
CREATE INDEX IF NOT EXISTS ix_soft_delete_clients ON barber.clients(tenant_id,deleted_at);

CREATE OR REPLACE FUNCTION barber.set_updated_at() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN NEW.updated_at=now(); RETURN NEW; END $$;
DO $$ DECLARE t text; BEGIN FOREACH t IN ARRAY ARRAY['tenants','branches','users','clients','professionals','services','appointments','service_orders','products','payments'] LOOP
 IF NOT EXISTS(SELECT 1 FROM pg_trigger WHERE tgname='trg_'||t||'_updated_at' AND tgrelid=('barber.'||t)::regclass) THEN EXECUTE format('CREATE TRIGGER %I BEFORE UPDATE ON barber.%I FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at()','trg_'||t||'_updated_at',t); END IF;
END LOOP; END $$;
INSERT INTO barber.schema_versions(version,description,checksum) VALUES
('001','Core organizacional','core-20260809'),('002','Agenda relacional','agenda-20260809'),('003','Financeiro e caixa','finance-20260809'),('004','Estoque e comissões','stock-20260809'),('005','Atendimento e relacionamento','attendance-20260809'),('006','Segurança e governança','security-20260809'),('007','Reconhecimento de serviços','recognition-20260809')
ON CONFLICT(version) DO UPDATE SET description=excluded.description,checksum=excluded.checksum;
COMMIT;
