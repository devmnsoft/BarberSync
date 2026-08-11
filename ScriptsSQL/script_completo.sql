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
CREATE UNIQUE INDEX IF NOT EXISTS ux_users_tenant_email ON barber.users(tenant_id,lower(email)) WHERE deleted_at IS NULL;
CREATE UNIQUE INDEX IF NOT EXISTS ux_users_refresh_token_hash ON barber.users(refresh_token_hash) WHERE refresh_token_hash IS NOT NULL;
CREATE TABLE IF NOT EXISTS barber.clients (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid REFERENCES barber.branches(id), person_type char(2) NOT NULL DEFAULT 'PF', name varchar(180), normalized_document varchar(20), phone varchar(30), whatsapp varchar(30), email varchar(254), birth_date date, notes text, preferences jsonb NOT NULL DEFAULT '{}'::jsonb, lgpd_consent_at timestamptz, status varchar(30) NOT NULL DEFAULT 'Active', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.professionals (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid REFERENCES barber.branches(id), name varchar(180), specialty varchar(180), default_commission numeric(5,2) NOT NULL DEFAULT 0, work_schedule jsonb NOT NULL DEFAULT '{}'::jsonb, status varchar(30) NOT NULL DEFAULT 'Active', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.services (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid REFERENCES barber.branches(id), category varchar(120), name varchar(180), duration_minutes integer NOT NULL DEFAULT 30, price numeric(14,2) NOT NULL DEFAULT 0, commission_percent numeric(5,2) NOT NULL DEFAULT 0, available_admin boolean NOT NULL DEFAULT true, available_public boolean NOT NULL DEFAULT false, available_mobile boolean NOT NULL DEFAULT false, available_kiosk boolean NOT NULL DEFAULT false, status varchar(30) NOT NULL DEFAULT 'Active', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.professional_services (professional_id uuid NOT NULL REFERENCES barber.professionals(id), service_id uuid NOT NULL REFERENCES barber.services(id), commission_percent numeric(5,2), PRIMARY KEY(professional_id,service_id));
CREATE TABLE IF NOT EXISTS barber.appointments (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id), client_id uuid REFERENCES barber.clients(id), professional_id uuid REFERENCES barber.professionals(id), service_id uuid REFERENCES barber.services(id), scheduled_start timestamptz, scheduled_end timestamptz, status varchar(30) NOT NULL DEFAULT 'Scheduled', origin varchar(30) NOT NULL DEFAULT 'Admin', notes text, cancellation_reason text, checked_in_at timestamptz, started_at timestamptz, completed_at timestamptz, is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.appointment_history (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id), appointment_id uuid NOT NULL REFERENCES barber.appointments(id), from_status varchar(30) NOT NULL, to_status varchar(30) NOT NULL, old_start timestamptz, new_start timestamptz, reason text, changed_by uuid REFERENCES barber.users(id), changed_at timestamptz NOT NULL DEFAULT now());
CREATE INDEX IF NOT EXISTS ix_appointments_availability ON barber.appointments(tenant_id,branch_id,professional_id,scheduled_start,scheduled_end) WHERE deleted_at IS NULL AND status NOT IN ('Cancelled','NoShow');
CREATE TABLE IF NOT EXISTS barber.service_orders (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id), appointment_id uuid REFERENCES barber.appointments(id), client_id uuid REFERENCES barber.clients(id), number varchar(40), subtotal numeric(14,2) NOT NULL DEFAULT 0, discount numeric(14,2) NOT NULL DEFAULT 0, surcharge numeric(14,2) NOT NULL DEFAULT 0, total numeric(14,2) NOT NULL DEFAULT 0, notes text, status varchar(30) NOT NULL DEFAULT 'Open', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.service_order_items (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid REFERENCES barber.branches(id), service_order_id uuid NOT NULL REFERENCES barber.service_orders(id), item_type varchar(20) NOT NULL, service_id uuid REFERENCES barber.services(id), product_id uuid, professional_id uuid REFERENCES barber.professionals(id), description varchar(180), quantity numeric(14,3) NOT NULL DEFAULT 1, unit_price numeric(14,2) NOT NULL DEFAULT 0, discount numeric(14,2) NOT NULL DEFAULT 0, total numeric(14,2) NOT NULL DEFAULT 0, status varchar(30) NOT NULL DEFAULT 'Active', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.products (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid REFERENCES barber.branches(id), sku varchar(80), name varchar(180), cost_price numeric(14,2) NOT NULL DEFAULT 0, sale_price numeric(14,2) NOT NULL DEFAULT 0, current_stock numeric(14,3) NOT NULL DEFAULT 0, minimum_stock numeric(14,3) NOT NULL DEFAULT 0, allow_negative_stock boolean NOT NULL DEFAULT false, status varchar(30) NOT NULL DEFAULT 'Active', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname='fk_order_item_product' AND conrelid='barber.service_order_items'::regclass) THEN ALTER TABLE barber.service_order_items ADD CONSTRAINT fk_order_item_product FOREIGN KEY(product_id) REFERENCES barber.products(id); END IF; END $$;
CREATE TABLE IF NOT EXISTS barber.cash_registers (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id), opened_by uuid REFERENCES barber.users(id), closed_by uuid REFERENCES barber.users(id), opening_balance numeric(14,2) NOT NULL DEFAULT 0, expected_balance numeric(14,2), actual_balance numeric(14,2), opened_at timestamptz NOT NULL DEFAULT now(), closed_at timestamptz, status varchar(30) NOT NULL DEFAULT 'Open', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE TABLE IF NOT EXISTS barber.payments (id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id), service_order_id uuid REFERENCES barber.service_orders(id), cash_register_id uuid REFERENCES barber.cash_registers(id), idempotency_key varchar(100), method varchar(30), amount numeric(14,2) NOT NULL DEFAULT 0, paid_at timestamptz, status varchar(30) NOT NULL DEFAULT 'Pending', is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz, deleted_at timestamptz, payload jsonb NOT NULL DEFAULT '{}'::jsonb);
CREATE UNIQUE INDEX IF NOT EXISTS ux_payments_tenant_idempotency ON barber.payments(tenant_id,idempotency_key) WHERE idempotency_key IS NOT NULL;
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
CREATE TABLE IF NOT EXISTS barber.branch_onboarding (
 tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id),
 current_step smallint NOT NULL DEFAULT 1 CHECK(current_step BETWEEN 1 AND 10), steps jsonb NOT NULL DEFAULT '{}'::jsonb,
 is_completed boolean NOT NULL DEFAULT false, completed_at timestamptz, updated_by uuid REFERENCES barber.users(id),
 created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NOT NULL DEFAULT now(), PRIMARY KEY(tenant_id,branch_id)
);
CREATE INDEX IF NOT EXISTS ix_branch_onboarding_pending ON barber.branch_onboarding(tenant_id,is_completed,updated_at) WHERE NOT is_completed;

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

-- Estruturas operacionais da fase 2. Todas são relacionais, auditáveis e idempotentes.
CREATE TABLE IF NOT EXISTS barber.professional_schedule_blocks (
 id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id),
 professional_id uuid NOT NULL REFERENCES barber.professionals(id), start_at timestamptz NOT NULL, end_at timestamptz NOT NULL,
 reason varchar(30) NOT NULL, description text, created_by uuid REFERENCES barber.users(id), created_at timestamptz NOT NULL DEFAULT now(),
 CONSTRAINT ck_schedule_block_period CHECK (end_at > start_at)
);
CREATE INDEX IF NOT EXISTS ix_schedule_blocks_overlap ON barber.professional_schedule_blocks(tenant_id,branch_id,professional_id,start_at,end_at);
CREATE TABLE IF NOT EXISTS barber.appointment_status_history (
 id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id),
 appointment_id uuid NOT NULL REFERENCES barber.appointments(id), previous_status varchar(30), new_status varchar(30) NOT NULL,
 reason text, changed_by uuid REFERENCES barber.users(id), changed_at timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_appointment_status_history ON barber.appointment_status_history(appointment_id,changed_at);
CREATE TABLE IF NOT EXISTS barber.commission_rules (
 id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id),
 professional_id uuid REFERENCES barber.professionals(id), service_id uuid REFERENCES barber.services(id), percentage numeric(5,2) NOT NULL,
 valid_from date NOT NULL, valid_until date, is_active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now(),
 CONSTRAINT ck_commission_rule_percentage CHECK (percentage BETWEEN 0 AND 100),
 CONSTRAINT ck_commission_rule_period CHECK (valid_until IS NULL OR valid_until >= valid_from)
);
CREATE INDEX IF NOT EXISTS ix_commission_rules_resolution ON barber.commission_rules(tenant_id,branch_id,professional_id,service_id,valid_from DESC) WHERE is_active;
CREATE TABLE IF NOT EXISTS barber.payment_refunds (
 id uuid PRIMARY KEY, tenant_id uuid NOT NULL REFERENCES barber.tenants(id), branch_id uuid NOT NULL REFERENCES barber.branches(id),
 payment_id uuid NOT NULL REFERENCES barber.payments(id), amount numeric(14,2) NOT NULL, reason text NOT NULL,
 status varchar(30) NOT NULL DEFAULT 'Confirmed', refunded_by uuid REFERENCES barber.users(id), refunded_at timestamptz NOT NULL DEFAULT now(),
 correlation_id varchar(100), CONSTRAINT ck_payment_refund_amount CHECK (amount > 0)
);
CREATE INDEX IF NOT EXISTS ix_payment_refunds_payment ON barber.payment_refunds(tenant_id,payment_id,refunded_at);

-- Backfill não destrutivo: valores relacionais existentes sempre prevalecem sobre payload legado.
UPDATE barber.appointments SET
 client_id = CASE WHEN client_id IS NULL AND payload->>'clientId' ~* '^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$' THEN (payload->>'clientId')::uuid ELSE client_id END,
 professional_id = CASE WHEN professional_id IS NULL AND payload->>'professionalId' ~* '^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$' THEN (payload->>'professionalId')::uuid ELSE professional_id END,
 service_id = CASE WHEN service_id IS NULL AND payload->>'serviceId' ~* '^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$' THEN (payload->>'serviceId')::uuid ELSE service_id END,
 scheduled_start = COALESCE(scheduled_start, CASE WHEN payload->>'scheduledAt' ~ '^\d{4}-\d{2}-\d{2}' THEN (payload->>'scheduledAt')::timestamptz END),
 scheduled_end = COALESCE(scheduled_end, CASE WHEN payload->>'scheduledEnd' ~ '^\d{4}-\d{2}-\d{2}' THEN (payload->>'scheduledEnd')::timestamptz END);
UPDATE barber.products SET current_stock=CASE WHEN payload->>'currentStock' ~ '^-?[0-9]+(\.[0-9]+)?$' THEN (payload->>'currentStock')::numeric ELSE current_stock END, minimum_stock=CASE WHEN payload->>'minimumStock' ~ '^[0-9]+(\.[0-9]+)?$' THEN (payload->>'minimumStock')::numeric ELSE minimum_stock END WHERE payload ? 'currentStock' OR payload ? 'minimumStock';
UPDATE barber.services SET price=CASE WHEN payload->>'price' ~ '^[0-9]+(\.[0-9]+)?$' THEN (payload->>'price')::numeric ELSE price END, duration_minutes=CASE WHEN payload->>'durationMinutes' ~ '^[0-9]+$' THEN (payload->>'durationMinutes')::integer ELSE duration_minutes END WHERE payload ? 'price' OR payload ? 'durationMinutes';
UPDATE barber.payments SET amount=CASE WHEN payload->>'amount' ~ '^[0-9]+(\.[0-9]+)?$' THEN (payload->>'amount')::numeric ELSE amount END WHERE amount=0 AND payload ? 'amount';
UPDATE barber.service_orders SET subtotal=CASE WHEN payload->>'subtotal' ~ '^[0-9]+(\.[0-9]+)?$' THEN (payload->>'subtotal')::numeric ELSE subtotal END, discount=CASE WHEN payload->>'discount' ~ '^[0-9]+(\.[0-9]+)?$' THEN (payload->>'discount')::numeric ELSE discount END, total=CASE WHEN payload->>'total' ~ '^[0-9]+(\.[0-9]+)?$' THEN (payload->>'total')::numeric ELSE total END WHERE payload ? 'total';
UPDATE barber.professionals SET default_commission=CASE WHEN payload->>'defaultCommission' ~ '^[0-9]+(\.[0-9]+)?$' THEN (payload->>'defaultCommission')::numeric ELSE default_commission END WHERE default_commission=0 AND payload ? 'defaultCommission';

INSERT INTO barber.permissions(id,code,description) VALUES
('10000000-0000-4000-8000-000000000001','Appointment.View','Visualizar agenda'),('10000000-0000-4000-8000-000000000002','Appointment.Create','Criar agendamento'),
('10000000-0000-4000-8000-000000000003','Appointment.Reschedule','Reagendar'),('10000000-0000-4000-8000-000000000004','Appointment.Cancel','Cancelar agendamento'),
('10000000-0000-4000-8000-000000000005','Appointment.OverrideConflict','Sobrescrever conflito'),('10000000-0000-4000-8000-000000000006','Attendance.Start','Iniciar atendimento'),
('10000000-0000-4000-8000-000000000007','Attendance.Finish','Finalizar atendimento'),('10000000-0000-4000-8000-000000000008','ServiceOrder.View','Visualizar comandas'),
('10000000-0000-4000-8000-000000000009','ServiceOrder.Create','Criar comanda'),('10000000-0000-4000-8000-000000000010','ServiceOrder.Discount','Aplicar desconto'),
('10000000-0000-4000-8000-000000000011','ServiceOrder.HighDiscount','Autorizar desconto elevado'),('10000000-0000-4000-8000-000000000012','Payment.Create','Registrar pagamento'),
('10000000-0000-4000-8000-000000000013','Payment.Refund','Estornar pagamento'),('10000000-0000-4000-8000-000000000014','Cash.View','Visualizar caixa'),
('10000000-0000-4000-8000-000000000015','Cash.Open','Abrir caixa'),('10000000-0000-4000-8000-000000000016','Cash.Supply','Registrar suprimento'),
('10000000-0000-4000-8000-000000000017','Cash.Withdraw','Registrar sangria'),('10000000-0000-4000-8000-000000000018','Cash.Close','Fechar caixa'),
('10000000-0000-4000-8000-000000000019','Stock.View','Visualizar estoque'),('10000000-0000-4000-8000-000000000020','Stock.Entry','Registrar entrada'),
('10000000-0000-4000-8000-000000000021','Stock.Adjust','Ajustar estoque'),('10000000-0000-4000-8000-000000000022','Commission.ViewOwn','Visualizar comissão própria'),
('10000000-0000-4000-8000-000000000023','Commission.ViewAll','Visualizar todas as comissões'),('10000000-0000-4000-8000-000000000024','User.Manage','Gerenciar usuários'),
('10000000-0000-4000-8000-000000000025','Branch.Manage','Gerenciar unidades') ON CONFLICT(id) DO UPDATE SET code=excluded.code,description=excluded.description;
INSERT INTO barber.roles(id,tenant_id,name,code,is_system) VALUES
('20000000-0000-4000-8000-000000000001',NULL,'Owner','Owner',true),('20000000-0000-4000-8000-000000000002',NULL,'Manager','Manager',true),
('20000000-0000-4000-8000-000000000003',NULL,'Reception','Reception',true),('20000000-0000-4000-8000-000000000004',NULL,'Professional','Professional',true),
('20000000-0000-4000-8000-000000000005',NULL,'Cashier','Cashier',true),('20000000-0000-4000-8000-000000000006',NULL,'Stock','Stock',true)
ON CONFLICT(id) DO UPDATE SET name=excluded.name,code=excluded.code,is_system=true;

CREATE OR REPLACE FUNCTION barber.set_updated_at() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN NEW.updated_at=now(); RETURN NEW; END $$;
DO $$ DECLARE t text; BEGIN FOREACH t IN ARRAY ARRAY['tenants','branches','users','clients','professionals','services','appointments','service_orders','products','payments'] LOOP
 IF NOT EXISTS(SELECT 1 FROM pg_trigger WHERE tgname='trg_'||t||'_updated_at' AND tgrelid=('barber.'||t)::regclass) THEN EXECUTE format('CREATE TRIGGER %I BEFORE UPDATE ON barber.%I FOR EACH ROW EXECUTE FUNCTION barber.set_updated_at()','trg_'||t||'_updated_at',t); END IF;
END LOOP; END $$;
-- Fase 4: estruturas relacionais de crescimento e retenção.
ALTER TABLE barber.services ADD COLUMN IF NOT EXISTS recommended_return_days integer CHECK (recommended_return_days BETWEEN 1 AND 365);
CREATE TABLE IF NOT EXISTS barber.return_recommendations (id uuid PRIMARY KEY,tenant_id uuid NOT NULL REFERENCES barber.tenants(id),branch_id uuid NOT NULL REFERENCES barber.branches(id),client_id uuid NOT NULL REFERENCES barber.clients(id),service_id uuid NOT NULL REFERENCES barber.services(id),appointment_id uuid REFERENCES barber.appointments(id),recommended_at date NOT NULL,status varchar(20) NOT NULL DEFAULT 'Pending',booked_appointment_id uuid REFERENCES barber.appointments(id),created_at timestamptz NOT NULL DEFAULT now(),updated_at timestamptz,CHECK(status IN ('Pending','Booked','Dismissed','Expired')));
CREATE INDEX IF NOT EXISTS ix_return_recommendations_client ON barber.return_recommendations(tenant_id,client_id,recommended_at) WHERE status='Pending';
CREATE TABLE IF NOT EXISTS barber.client_tags (tenant_id uuid NOT NULL REFERENCES barber.tenants(id),client_id uuid NOT NULL REFERENCES barber.clients(id),tag varchar(50) NOT NULL,created_at timestamptz NOT NULL DEFAULT now(),PRIMARY KEY(tenant_id,client_id,tag));
CREATE TABLE IF NOT EXISTS barber.client_notes (id uuid PRIMARY KEY,tenant_id uuid NOT NULL REFERENCES barber.tenants(id),branch_id uuid NOT NULL REFERENCES barber.branches(id),client_id uuid NOT NULL REFERENCES barber.clients(id),note text NOT NULL,created_by uuid REFERENCES barber.users(id),created_at timestamptz NOT NULL DEFAULT now());
CREATE TABLE IF NOT EXISTS barber.contact_tasks (id uuid PRIMARY KEY,tenant_id uuid NOT NULL REFERENCES barber.tenants(id),branch_id uuid NOT NULL REFERENCES barber.branches(id),client_id uuid NOT NULL REFERENCES barber.clients(id),title text NOT NULL,due_at timestamptz NOT NULL,channel varchar(20),status varchar(20) NOT NULL DEFAULT 'Pending',assigned_to uuid REFERENCES barber.users(id),created_at timestamptz NOT NULL DEFAULT now());
CREATE TABLE IF NOT EXISTS barber.campaign_audiences (campaign_id uuid NOT NULL REFERENCES barber.campaigns(id),client_id uuid NOT NULL REFERENCES barber.clients(id),status varchar(20) NOT NULL DEFAULT 'Eligible',reached_at timestamptz,converted_order_id uuid REFERENCES barber.service_orders(id),revenue numeric(14,2) NOT NULL DEFAULT 0,created_at timestamptz NOT NULL DEFAULT now(),PRIMARY KEY(campaign_id,client_id));
CREATE TABLE IF NOT EXISTS barber.coupon_redemptions (id uuid PRIMARY KEY,tenant_id uuid NOT NULL REFERENCES barber.tenants(id),branch_id uuid NOT NULL REFERENCES barber.branches(id),coupon_id uuid NOT NULL REFERENCES barber.coupons(id),client_id uuid NOT NULL REFERENCES barber.clients(id),service_order_id uuid NOT NULL REFERENCES barber.service_orders(id),discount numeric(14,2) NOT NULL,redeemed_at timestamptz NOT NULL DEFAULT now(),reversed_at timestamptz,UNIQUE(coupon_id,service_order_id));
CREATE TABLE IF NOT EXISTS barber.loyalty_settings (branch_id uuid PRIMARY KEY REFERENCES barber.branches(id),tenant_id uuid NOT NULL REFERENCES barber.tenants(id),points_per_real numeric(10,4) NOT NULL DEFAULT 1,cashback_percentage numeric(5,2) NOT NULL DEFAULT 0,points_valid_days integer NOT NULL DEFAULT 365,minimum_redemption numeric(14,2) NOT NULL DEFAULT 0,partial_redemption boolean NOT NULL DEFAULT true,maximum_redemption_per_order numeric(14,2),updated_at timestamptz NOT NULL DEFAULT now());

CREATE OR REPLACE FUNCTION barber.client_360(p_tenant uuid,p_client uuid) RETURNS jsonb LANGUAGE sql STABLE AS $$ SELECT jsonb_strip_nulls(jsonb_build_object('id',c.id,'name',c.payload->>'name','phone',c.payload->>'phone','whatsapp',c.payload->>'whatsapp','email',c.payload->>'email','status',c.status,'origin',c.payload->>'origin','registeredAt',c.created_at,'tags',coalesce((select jsonb_agg(tag) from barber.client_tags t where t.tenant_id=c.tenant_id and t.client_id=c.id),'[]'::jsonb),'totalVisits',(select count(*) from barber.appointments a where a.client_id=c.id and a.status='Finished'),'totalSpent',coalesce((select sum(p.amount) from barber.service_orders so join barber.payments p on p.service_order_id=so.id and p.status='Paid' where so.client_id=c.id),0),'averageTicket',coalesce((select avg(p.amount) from barber.service_orders so join barber.payments p on p.service_order_id=so.id and p.status='Paid' where so.client_id=c.id),0),'lastVisit',(select max(a.scheduled_start) from barber.appointments a where a.client_id=c.id and a.status='Finished'),'nextAppointment',(select min(a.scheduled_start) from barber.appointments a where a.client_id=c.id and a.status in ('Scheduled','Confirmed') and a.scheduled_start>now()),'daysWithoutVisit',coalesce(extract(day from now()-(select max(a.scheduled_start) from barber.appointments a where a.client_id=c.id and a.status='Finished'))::int,0),'loyaltyBalance',coalesce((select sum(points) from barber.loyalty_accounts l where l.client_id=c.id and l.deleted_at is null),0),'noShows',(select count(*) from barber.appointments a where a.client_id=c.id and a.status='NoShow'),'averageRating',(select round(avg(r.rating),2) from barber.reviews r where r.client_id=c.id),'recommendedReturnAt',(select min(rr.recommended_at) from barber.return_recommendations rr where rr.client_id=c.id and rr.status='Pending'))) FROM barber.clients c WHERE c.tenant_id=p_tenant AND c.id=p_client AND c.deleted_at IS NULL $$;
CREATE OR REPLACE VIEW barber.vw_clients_to_reactivate AS SELECT c.tenant_id,c.branch_id,c.id client_id,c.payload->>'name' client_name,c.payload->>'phone' phone,max(a.scheduled_start) last_visit,coalesce(extract(day from now()-max(a.scheduled_start))::int,9999) days_without_visit,coalesce(sum(distinct so.total) filter(where so.status='Paid'),0) historical_spend,coalesce(avg(distinct so.total) filter(where so.status='Paid'),0) average_ticket,CASE WHEN max(a.scheduled_start) IS NULL THEN 'Convidar para a primeira experiência' WHEN extract(day from now()-max(a.scheduled_start))>=90 THEN 'Oferecer condição especial de reativação' ELSE 'Lembrar o retorno recomendado' END suggested_approach FROM barber.clients c LEFT JOIN barber.appointments a ON a.client_id=c.id AND a.status='Finished' LEFT JOIN barber.service_orders so ON so.client_id=c.id WHERE c.deleted_at IS NULL GROUP BY c.tenant_id,c.branch_id,c.id;

-- BarberSync 2.0: operação assistida, privacidade, integrações e comercialização SaaS.
CREATE TABLE IF NOT EXISTS barber.privacy_settings (branch_id uuid PRIMARY KEY REFERENCES barber.branches(id),tenant_id uuid NOT NULL REFERENCES barber.tenants(id),basic_data_consent_required boolean NOT NULL DEFAULT true,camera_enabled boolean NOT NULL DEFAULT false,camera_consent_required boolean NOT NULL DEFAULT true,evidence_retention_days integer NOT NULL DEFAULT 30 CHECK(evidence_retention_days BETWEEN 1 AND 365),public_privacy_policy_url text,updated_by uuid REFERENCES barber.users(id),updated_at timestamptz NOT NULL DEFAULT now());
CREATE TABLE IF NOT EXISTS barber.privacy_requests (id uuid PRIMARY KEY,tenant_id uuid NOT NULL REFERENCES barber.tenants(id),branch_id uuid REFERENCES barber.branches(id),client_id uuid NOT NULL REFERENCES barber.clients(id),request_type varchar(30) NOT NULL,status varchar(20) NOT NULL DEFAULT 'Pending',requested_by uuid REFERENCES barber.users(id),requested_at timestamptz NOT NULL DEFAULT now(),completed_at timestamptz,result_uri text,CHECK(request_type IN ('Export','Anonymize','Deactivate')));
CREATE TABLE IF NOT EXISTS barber.camera_devices (id uuid PRIMARY KEY,tenant_id uuid NOT NULL REFERENCES barber.tenants(id),branch_id uuid NOT NULL REFERENCES barber.branches(id),name varchar(120) NOT NULL,provider_key varchar(80),is_active boolean NOT NULL DEFAULT false,created_at timestamptz NOT NULL DEFAULT now());
CREATE TABLE IF NOT EXISTS barber.camera_zones (id uuid PRIMARY KEY,tenant_id uuid NOT NULL REFERENCES barber.tenants(id),branch_id uuid NOT NULL REFERENCES barber.branches(id),camera_device_id uuid NOT NULL REFERENCES barber.camera_devices(id),name varchar(120) NOT NULL,zone_type varchar(40) NOT NULL,created_at timestamptz NOT NULL DEFAULT now());
CREATE TABLE IF NOT EXISTS barber.service_recognition_events (id uuid PRIMARY KEY,tenant_id uuid NOT NULL REFERENCES barber.tenants(id),branch_id uuid NOT NULL REFERENCES barber.branches(id),camera_device_id uuid REFERENCES barber.camera_devices(id),camera_zone_id uuid REFERENCES barber.camera_zones(id),appointment_id uuid REFERENCES barber.appointments(id),occurred_at timestamptz NOT NULL,event_signals jsonb NOT NULL DEFAULT '[]',status varchar(20) NOT NULL DEFAULT 'Pending',created_at timestamptz NOT NULL DEFAULT now());
CREATE TABLE IF NOT EXISTS barber.service_recognition_evidences (id uuid PRIMARY KEY,event_id uuid NOT NULL REFERENCES barber.service_recognition_events(id),evidence_type varchar(30) NOT NULL,storage_uri text,signal_metadata jsonb NOT NULL DEFAULT '{}',expires_at timestamptz,created_at timestamptz NOT NULL DEFAULT now(),CHECK(storage_uri IS NULL OR expires_at IS NOT NULL));
CREATE TABLE IF NOT EXISTS barber.service_recognition_suggestions (id uuid PRIMARY KEY,event_id uuid NOT NULL REFERENCES barber.service_recognition_events(id),suggested_service_id uuid REFERENCES barber.services(id),provider varchar(80) NOT NULL,confidence numeric(5,4) NOT NULL CHECK(confidence BETWEEN 0 AND 1),reason text NOT NULL,status varchar(20) NOT NULL DEFAULT 'Pending',created_at timestamptz NOT NULL DEFAULT now());
CREATE TABLE IF NOT EXISTS barber.service_recognition_confirmations (id uuid PRIMARY KEY,suggestion_id uuid NOT NULL REFERENCES barber.service_recognition_suggestions(id),decision varchar(20) NOT NULL,corrected_service_id uuid REFERENCES barber.services(id),service_order_id uuid REFERENCES barber.service_orders(id),confirmed_by uuid NOT NULL REFERENCES barber.users(id),confirmed_at timestamptz NOT NULL DEFAULT now(),notes text,CHECK(decision IN ('Confirmed','Corrected','Discarded')),CHECK(decision='Discarded' OR corrected_service_id IS NOT NULL));
CREATE UNIQUE INDEX IF NOT EXISTS ux_recognition_human_decision ON barber.service_recognition_confirmations(suggestion_id);
CREATE INDEX IF NOT EXISTS ix_recognition_pending ON barber.service_recognition_events(tenant_id,branch_id,occurred_at DESC) WHERE status='Pending';

CREATE TABLE IF NOT EXISTS barber.notification_outbox (id uuid PRIMARY KEY,tenant_id uuid NOT NULL REFERENCES barber.tenants(id),branch_id uuid REFERENCES barber.branches(id),channel varchar(20) NOT NULL,recipient varchar(254) NOT NULL,template varchar(100) NOT NULL,payload jsonb NOT NULL DEFAULT '{}',status varchar(20) NOT NULL DEFAULT 'Pending',attempts integer NOT NULL DEFAULT 0,last_error text,scheduled_at timestamptz NOT NULL DEFAULT now(),sent_at timestamptz,created_at timestamptz NOT NULL DEFAULT now(),CHECK(status IN ('Pending','Processing','Sent','Failed','Unconfigured')),CHECK(sent_at IS NULL OR status='Sent'));
CREATE INDEX IF NOT EXISTS ix_notification_outbox_dispatch ON barber.notification_outbox(status,scheduled_at) WHERE status IN ('Pending','Failed');
CREATE TABLE IF NOT EXISTS barber.automations (id uuid PRIMARY KEY,tenant_id uuid NOT NULL REFERENCES barber.tenants(id),branch_id uuid REFERENCES barber.branches(id),name varchar(180) NOT NULL,trigger_type varchar(80) NOT NULL,conditions jsonb NOT NULL DEFAULT '{}',channel varchar(20),template varchar(100),status varchar(20) NOT NULL DEFAULT 'Inactive',last_run_at timestamptz,next_run_at timestamptz,created_at timestamptz NOT NULL DEFAULT now());

CREATE TABLE IF NOT EXISTS barber.saas_plans (id uuid PRIMARY KEY,code varchar(30) UNIQUE NOT NULL,name varchar(80) NOT NULL,limits jsonb NOT NULL,features jsonb NOT NULL DEFAULT '{}',is_active boolean NOT NULL DEFAULT true,created_at timestamptz NOT NULL DEFAULT now());
CREATE TABLE IF NOT EXISTS barber.tenant_subscriptions (id uuid PRIMARY KEY,tenant_id uuid UNIQUE NOT NULL REFERENCES barber.tenants(id),plan_id uuid NOT NULL REFERENCES barber.saas_plans(id),status varchar(20) NOT NULL DEFAULT 'Trial',period_start date NOT NULL,period_end date NOT NULL,pending_plan_id uuid REFERENCES barber.saas_plans(id),created_at timestamptz NOT NULL DEFAULT now(),updated_at timestamptz,CHECK(period_end>=period_start));
CREATE TABLE IF NOT EXISTS barber.usage_counters (tenant_id uuid NOT NULL REFERENCES barber.tenants(id),metric varchar(50) NOT NULL,period_start date NOT NULL,used bigint NOT NULL DEFAULT 0 CHECK(used>=0),updated_at timestamptz NOT NULL DEFAULT now(),PRIMARY KEY(tenant_id,metric,period_start));
CREATE TABLE IF NOT EXISTS barber.billing_accounts (id uuid PRIMARY KEY,tenant_id uuid UNIQUE NOT NULL REFERENCES barber.tenants(id),legal_name varchar(180),document varchar(30),billing_email varchar(254),address jsonb NOT NULL DEFAULT '{}',gateway_customer_id text,created_at timestamptz NOT NULL DEFAULT now(),updated_at timestamptz);
CREATE TABLE IF NOT EXISTS barber.billing_invoices (id uuid PRIMARY KEY,tenant_id uuid NOT NULL REFERENCES barber.tenants(id),subscription_id uuid REFERENCES barber.tenant_subscriptions(id),number varchar(50) NOT NULL,status varchar(20) NOT NULL DEFAULT 'Draft',currency char(3) NOT NULL DEFAULT 'BRL',total numeric(14,2) NOT NULL DEFAULT 0,due_at timestamptz,paid_at timestamptz,created_at timestamptz NOT NULL DEFAULT now(),UNIQUE(tenant_id,number),CHECK(paid_at IS NULL OR status='Paid'));
CREATE TABLE IF NOT EXISTS barber.billing_invoice_items (id uuid PRIMARY KEY,invoice_id uuid NOT NULL REFERENCES barber.billing_invoices(id),description varchar(180) NOT NULL,quantity numeric(12,3) NOT NULL DEFAULT 1,unit_price numeric(14,2) NOT NULL,total numeric(14,2) NOT NULL);
CREATE TABLE IF NOT EXISTS barber.billing_payment_attempts (id uuid PRIMARY KEY,invoice_id uuid NOT NULL REFERENCES barber.billing_invoices(id),status varchar(20) NOT NULL DEFAULT 'Unconfigured',provider varchar(50),provider_reference text,error text,attempted_at timestamptz NOT NULL DEFAULT now(),CHECK(status<>'Approved' OR provider_reference IS NOT NULL));

ALTER TABLE barber.reviews ADD COLUMN IF NOT EXISTS nps smallint CHECK(nps BETWEEN 0 AND 10);
ALTER TABLE barber.reviews ADD COLUMN IF NOT EXISTS service_id uuid REFERENCES barber.services(id);
ALTER TABLE barber.reviews ADD COLUMN IF NOT EXISTS service_order_id uuid REFERENCES barber.service_orders(id);
ALTER TABLE barber.reviews ADD COLUMN IF NOT EXISTS channel varchar(30);
ALTER TABLE barber.reviews ADD COLUMN IF NOT EXISTS response text;
ALTER TABLE barber.reviews ADD COLUMN IF NOT EXISTS highlighted boolean NOT NULL DEFAULT false;
CREATE TABLE IF NOT EXISTS barber.internal_tasks (id uuid PRIMARY KEY,tenant_id uuid NOT NULL REFERENCES barber.tenants(id),branch_id uuid REFERENCES barber.branches(id),title varchar(180) NOT NULL,description text,assigned_to uuid REFERENCES barber.users(id),client_id uuid REFERENCES barber.clients(id),origin_module varchar(80),priority varchar(20) NOT NULL DEFAULT 'Medium',due_at timestamptz,status varchar(20) NOT NULL DEFAULT 'Pending',created_by uuid REFERENCES barber.users(id),created_at timestamptz NOT NULL DEFAULT now(),updated_at timestamptz,CHECK(status IN ('Pending','InProgress','Completed','Cancelled')));
CREATE INDEX IF NOT EXISTS ix_internal_tasks_assignee ON barber.internal_tasks(tenant_id,assigned_to,status,due_at) WHERE status IN ('Pending','InProgress');

CREATE OR REPLACE FUNCTION barber.assistant_operational_insights(p_tenant uuid,p_branch uuid)
RETURNS TABLE(title text,description text,priority text,reason text,related_module text,action_label text,action_url text) LANGUAGE sql STABLE AS $$
WITH stats AS (
 SELECT
 (SELECT count(*) FROM barber.clients c WHERE c.tenant_id=p_tenant AND c.branch_id=p_branch AND c.deleted_at IS NULL AND NOT EXISTS(SELECT 1 FROM barber.appointments a WHERE a.client_id=c.id AND a.status='Finished' AND a.scheduled_start>now()-interval '45 days')) inactive,
 (SELECT count(*) FROM barber.products p WHERE p.tenant_id=p_tenant AND p.branch_id=p_branch AND p.deleted_at IS NULL AND p.current_stock<=p.minimum_stock) critical_stock,
 (SELECT count(*) FROM barber.products p WHERE p.tenant_id=p_tenant AND p.branch_id=p_branch AND p.deleted_at IS NULL AND p.sale_price>0 AND (p.sale_price-p.cost_price)/p.sale_price<.2) low_margin,
 (SELECT count(*) FROM barber.service_orders o WHERE o.tenant_id=p_tenant AND o.branch_id=p_branch AND o.status='Open' AND o.created_at<now()-interval '2 hours') stalled_orders,
 (SELECT count(*) FROM barber.cash_registers c WHERE c.tenant_id=p_tenant AND c.branch_id=p_branch AND c.status='Open' AND c.opened_at<now()-interval '12 hours') old_cash,
 (SELECT count(*) FROM barber.appointments a WHERE a.tenant_id=p_tenant AND a.branch_id=p_branch AND a.scheduled_start>now()-interval '30 days' AND a.status='NoShow') no_shows,
 (SELECT count(*) FROM barber.appointments a WHERE a.tenant_id=p_tenant AND a.branch_id=p_branch AND a.scheduled_start>now()-interval '30 days') recent_appointments,
 (SELECT count(*) FROM barber.professionals p WHERE p.tenant_id=p_tenant AND p.branch_id=p_branch AND p.deleted_at IS NULL AND (SELECT count(*) FROM barber.appointments a WHERE a.professional_id=p.id AND a.scheduled_start::date=current_date AND a.status NOT IN ('Cancelled','NoShow'))>=8) overloaded,
 (SELECT count(*) FROM barber.clients c JOIN barber.loyalty_accounts l ON l.client_id=c.id WHERE c.tenant_id=p_tenant AND c.branch_id=p_branch AND l.points>=100 AND NOT EXISTS(SELECT 1 FROM barber.appointments a WHERE a.client_id=c.id AND a.status='Finished' AND a.scheduled_start>now()-interval '45 days')) vip_inactive,
 (SELECT count(*) FROM barber.campaigns c WHERE c.tenant_id=p_tenant AND c.branch_id=p_branch AND c.status='Finished' AND CASE WHEN c.payload->>'conversionRate' ~ '^[0-9]+(\.[0-9]+)?$' THEN (c.payload->>'conversionRate')::numeric ELSE 0 END<.02) weak_campaigns,
 (SELECT count(*) FROM barber.services s WHERE s.tenant_id=p_tenant AND s.branch_id=p_branch AND (SELECT count(*) FROM barber.appointments a WHERE a.service_id=s.id AND a.scheduled_start BETWEEN now()-interval '30 days' AND now()) < (SELECT count(*)*.7 FROM barber.appointments a WHERE a.service_id=s.id AND a.scheduled_start BETWEEN now()-interval '60 days' AND now()-interval '30 days')) falling_services,
 (SELECT count(*) FROM barber.professionals p WHERE p.tenant_id=p_tenant AND p.branch_id=p_branch AND NOT EXISTS(SELECT 1 FROM barber.appointments a WHERE a.professional_id=p.id AND a.scheduled_start BETWEEN now() AND now()+interval '4 hours' AND a.status NOT IN ('Cancelled','NoShow'))) free_slots)
 SELECT * FROM (
 SELECT 'Clientes para reativar',inactive||' clientes estão há mais de 45 dias sem retorno.','High','Ausência de atendimento concluído no período.','Clients','Abrir lista de reativação','/Admin/Clients?segment=inactive' FROM stats WHERE inactive>0 UNION ALL
 SELECT 'Horários livres para encaixe',free_slots||' profissionais possuem janela nas próximas 4 horas.','Medium','Não há agendamento ativo no intervalo.','Appointments','Abrir agenda','/Admin/Appointments' FROM stats WHERE free_slots>0 UNION ALL
 SELECT 'Profissional sobrecarregado',overloaded||' profissionais têm 8 ou mais atendimentos hoje.','High','Carga diária acima do limite operacional.','Appointments','Redistribuir agenda','/Admin/Appointments' FROM stats WHERE overloaded>0 UNION ALL
 SELECT 'Estoque crítico',critical_stock||' produtos atingiram o estoque mínimo.','Critical','Saldo atual menor ou igual ao mínimo.','Stock','Repor estoque','/Admin/Stock' FROM stats WHERE critical_stock>0 UNION ALL
 SELECT 'Produto com margem baixa',low_margin||' produtos têm margem inferior a 20%.','Medium','Preço de venda pouco superior ao custo.','Products','Revisar preços','/Admin/Products' FROM stats WHERE low_margin>0 UNION ALL
 SELECT 'Comanda parada',stalled_orders||' comandas estão abertas há mais de 2 horas.','High','Comanda sem encerramento no prazo.','ServiceOrders','Conferir comandas','/Admin/ServiceOrders' FROM stats WHERE stalled_orders>0 UNION ALL
 SELECT 'Caixa aberto há muito tempo',old_cash||' caixas estão abertos há mais de 12 horas.','Critical','Turno de caixa excedeu 12 horas.','Cash','Conferir caixa','/Admin/Cash' FROM stats WHERE old_cash>0 UNION ALL
 SELECT 'Campanha com baixo retorno',weak_campaigns||' campanhas converteram menos de 2%.','Medium','Conversão registrada abaixo da referência.','Campaigns','Analisar campanha','/Admin/Campaigns' FROM stats WHERE weak_campaigns>0 UNION ALL
 SELECT 'Cliente VIP sem retorno',vip_inactive||' clientes VIP estão sem retorno há 45 dias.','High','Saldo de fidelidade alto e ausência recente.','Clients','Criar follow-up','/Admin/Tasks' FROM stats WHERE vip_inactive>0 UNION ALL
 SELECT 'Serviço com queda de demanda',falling_services||' serviços caíram mais de 30% no mês.','Medium','Comparação dos últimos dois períodos de 30 dias.','Reports','Analisar demanda','/Admin/Reports' FROM stats WHERE falling_services>0 UNION ALL
 SELECT 'Alto índice de no-show',no_shows||' faltas em '||recent_appointments||' agendamentos recentes.','High','No-show igual ou superior a 10% no período.','Appointments','Revisar confirmações','/Admin/Automations' FROM stats WHERE recent_appointments>0 AND no_shows::numeric/recent_appointments>=.1) insights;
$$;
CREATE INDEX IF NOT EXISTS ix_clients_server_search ON barber.clients(tenant_id,branch_id,lower(name)) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_reviews_reputation ON barber.reviews(tenant_id,branch_id,created_at DESC) WHERE deleted_at IS NULL;

INSERT INTO barber.schema_versions(version,description,checksum) VALUES
('001','Core organizacional','core-20260809'),('002','Agenda relacional','agenda-20260809'),('003','Financeiro e caixa','finance-20260809'),('004','Estoque e comissões','stock-20260809'),('005','Atendimento e relacionamento','attendance-20260809'),('006','Segurança e governança','security-20260809'),('007','Reconhecimento de serviços','recognition-20260809'),('008','Crescimento e retenção fase 4','growth-20260809'),('009','BarberSync 2.0 SaaS e operação assistida','saas2-20260811')
ON CONFLICT(version) DO UPDATE SET description=excluded.description,checksum=excluded.checksum;
COMMIT;
