using Npgsql;

namespace BarberSync.Api.Services.Enterprise;

public sealed class BarberSchemaInitializer(
    IConfiguration configuration,
    IWebHostEnvironment environment,
    ILogger<BarberSchemaInitializer> logger) : IBarberSchemaInitializer
{
    private const string SchemaName = "barber";
    private const string ConnectionStringName = "DefaultConnection";
    private static readonly SemaphoreSlim InitializationLock = new(1, 1);
    private static volatile bool _initialized;
    private static volatile bool _attempted;
    private readonly string _connectionString = configuration.GetConnectionString(ConnectionStringName)
        ?? "Host=localhost;Port=5432;Database=barber;Username=barbersync;Password=barbersync_demo_10";

    public DatabaseHealthResult LastResult { get; private set; } = new(
        Success: false,
        DatabaseConnected: false,
        SchemaReady: false,
        Message: "Schema BarberSync ainda não inicializado.",
        Database: "barber",
        Schema: SchemaName,
        Environment: "Unknown");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized) return;

        await InitializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized) return;

            _attempted = true;
            await using var connection = new NpgsqlConnection(_connectionString);
            var info = BuildConnectionInfo(connection);
            await connection.OpenAsync(cancellationToken);

            foreach (var step in BuildSchemaSteps())
            {
                await ExecuteStepAsync(connection, step, info, cancellationToken);
            }

            _initialized = true;
            LastResult = new DatabaseHealthResult(
                Success: true,
                DatabaseConnected: true,
                SchemaReady: true,
                Message: "Schema BarberSync preparado com sucesso.",
                Database: info.Database,
                Schema: SchemaName,
                Environment: environment.EnvironmentName,
                Step: "Completed");

            logger.LogInformation(
                "Schema BarberSync preparado com sucesso. Database={Database}, Schema={Schema}, Environment={Environment}, ConnectionString={ConnectionString}",
                info.Database,
                SchemaName,
                environment.EnvironmentName,
                info.MaskedConnectionString);
        }
        catch (Exception ex)
        {
            var info = BuildConnectionInfo(new NpgsqlConnection(_connectionString));
            LastResult = new DatabaseHealthResult(
                Success: false,
                DatabaseConnected: false,
                SchemaReady: false,
                Message: $"Falha ao preparar schema BarberSync: {ex.Message}",
                Database: info.Database,
                Schema: SchemaName,
                Environment: environment.EnvironmentName,
                Step: LastResult.Step ?? "OpenConnection",
                Error: ex.ToString());

            logger.LogError(ex,
                "Falha ao preparar schema BarberSync. Message={Message}, InnerException={InnerException}, StackTrace={StackTrace}, Database={Database}, Schema={Schema}, Step={Step}, Environment={Environment}, ConnectionString={ConnectionString}",
                ex.Message,
                ex.InnerException?.ToString(),
                ex.StackTrace,
                info.Database,
                SchemaName,
                LastResult.Step,
                environment.EnvironmentName,
                info.MaskedConnectionString);
        }
        finally
        {
            InitializationLock.Release();
        }
    }

    public async Task<DatabaseHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        if (!_attempted || !_initialized)
        {
            await InitializeAsync(cancellationToken);
        }

        var info = BuildConnectionInfo(new NpgsqlConnection(_connectionString));
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand("select count(*) = 20 from information_schema.tables where table_schema = @schema and table_name in ('clients','professionals','services','products','appointments','service_orders','service_order_items','payments','stock_movements','campaigns','coupons','loyalty_accounts','loyalty_transactions','reviews','public_leads','kiosk_devices','kiosk_sessions','audit_logs','notifications','branches')", connection);
            command.Parameters.AddWithValue("schema", SchemaName);
            var schemaReady = Convert.ToBoolean(await command.ExecuteScalarAsync(cancellationToken));

            LastResult = LastResult with
            {
                Success = schemaReady,
                DatabaseConnected = true,
                SchemaReady = schemaReady,
                Message = schemaReady ? "Banco conectado e schema BarberSync pronto." : "Banco conectado, mas schema BarberSync incompleto.",
                Database = info.Database,
                Schema = SchemaName,
                Environment = environment.EnvironmentName,
                Step = schemaReady ? "HealthCheck" : LastResult.Step,
                Error = schemaReady ? null : LastResult.Error
            };
        }
        catch (Exception ex)
        {
            LastResult = new DatabaseHealthResult(
                Success: false,
                DatabaseConnected: false,
                SchemaReady: false,
                Message: $"Falha controlada ao validar banco BarberSync: {ex.Message}",
                Database: info.Database,
                Schema: SchemaName,
                Environment: environment.EnvironmentName,
                Step: "HealthCheck",
                Error: ex.ToString());

            logger.LogError(ex,
                "Falha ao validar health database BarberSync. Message={Message}, InnerException={InnerException}, StackTrace={StackTrace}, Database={Database}, Schema={Schema}, Step={Step}, Environment={Environment}, ConnectionString={ConnectionString}",
                ex.Message,
                ex.InnerException?.ToString(),
                ex.StackTrace,
                info.Database,
                SchemaName,
                "HealthCheck",
                environment.EnvironmentName,
                info.MaskedConnectionString);
        }

        return LastResult;
    }

    private async Task ExecuteStepAsync(NpgsqlConnection connection, SchemaStep step, ConnectionInfo info, CancellationToken cancellationToken)
    {
        LastResult = LastResult with
        {
            DatabaseConnected = true,
            Database = info.Database,
            Schema = SchemaName,
            Environment = environment.EnvironmentName,
            Step = step.Name,
            Message = $"Executando etapa de schema: {step.Name}."
        };

        try
        {
            await using var command = new NpgsqlCommand(step.Sql, connection) { CommandTimeout = 120 };
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Falha ao preparar schema BarberSync. Message={Message}, InnerException={InnerException}, StackTrace={StackTrace}, Database={Database}, Schema={Schema}, Step={Step}, Sql={Sql}, Environment={Environment}, ConnectionString={ConnectionString}",
                ex.Message,
                ex.InnerException?.ToString(),
                ex.StackTrace,
                info.Database,
                SchemaName,
                step.Name,
                step.Sql,
                environment.EnvironmentName,
                info.MaskedConnectionString);
            throw;
        }
    }

    private static IReadOnlyList<SchemaStep> BuildSchemaSteps() =>
    [
        new("Extensions", "CREATE EXTENSION IF NOT EXISTS pgcrypto;"),
        new("Schema", "CREATE SCHEMA IF NOT EXISTS barber;"),
        new("Core tables", CoreTablesSql),
        new("Resource tables and indexes", ResourceTablesSql),
        new("Audit and notification tables", AuditTablesSql),
        new("Business indexes", BusinessIndexesSql),
        new("Seed demo data", SeedSql)
    ];

    private ConnectionInfo BuildConnectionInfo(NpgsqlConnection connection)
    {
        var builder = new NpgsqlConnectionStringBuilder(_connectionString);
        var database = string.IsNullOrWhiteSpace(builder.Database) ? connection.Database : builder.Database;
        builder.Password = string.IsNullOrWhiteSpace(builder.Password) ? string.Empty : "***";
        return new ConnectionInfo(database, builder.ToString());
    }

    private sealed record SchemaStep(string Name, string Sql);
    private sealed record ConnectionInfo(string Database, string MaskedConnectionString);

    private const string CoreTablesSql = """
CREATE TABLE IF NOT EXISTS barber.tenants (
  id uuid primary key default gen_random_uuid(),
  slug varchar(120) unique not null,
  name varchar(180) not null,
  document varchar(40),
  status varchar(30) not null default 'Active',
  is_active boolean not null default true,
  created_at timestamp not null default now(),
  updated_at timestamp null,
  deleted_at timestamp null,
  created_by uuid null,
  updated_by uuid null
);

CREATE TABLE IF NOT EXISTS barber.branches (
  id uuid primary key default gen_random_uuid(),
  tenant_id uuid not null references barber.tenants(id),
  name varchar(180) not null,
  code varchar(60),
  status varchar(30) not null default 'Active',
  is_active boolean not null default true,
  created_at timestamp not null default now(),
  updated_at timestamp null,
  deleted_at timestamp null,
  created_by uuid null,
  updated_by uuid null,
  payload jsonb not null default '{}'::jsonb
);
""";

    private const string ResourceTablesSql = """
DO $$
DECLARE t text;
BEGIN
  FOREACH t IN ARRAY ARRAY[
    'users','profiles','permissions','clients','professionals','professional_services','services','products',
    'appointments','attendances','service_orders','service_order_items','payments','cash_registers',
    'stock_movements','campaigns','coupons','loyalty_accounts','loyalty_transactions','reviews',
    'public_leads','kiosk_devices','kiosk_sessions','app_settings'
  ] LOOP
    EXECUTE format($fmt$
      CREATE TABLE IF NOT EXISTS barber.%I (
        id uuid primary key default gen_random_uuid(),
        tenant_id uuid not null references barber.tenants(id),
        branch_id uuid null references barber.branches(id),
        status varchar(30) not null default 'Active',
        is_active boolean not null default true,
        created_at timestamp not null default now(),
        updated_at timestamp null,
        deleted_at timestamp null,
        created_by uuid null,
        updated_by uuid null,
        payload jsonb not null default '{}'::jsonb
      );
    $fmt$, t);
    EXECUTE format('CREATE INDEX IF NOT EXISTS %I ON barber.%I (tenant_id);', 'ix_' || t || '_tenant_id', t);
    EXECUTE format('CREATE INDEX IF NOT EXISTS %I ON barber.%I (branch_id);', 'ix_' || t || '_branch_id', t);
    EXECUTE format('CREATE INDEX IF NOT EXISTS %I ON barber.%I (status);', 'ix_' || t || '_status', t);
    EXECUTE format('CREATE INDEX IF NOT EXISTS %I ON barber.%I (created_at);', 'ix_' || t || '_created_at', t);
    EXECUTE format('CREATE INDEX IF NOT EXISTS %I ON barber.%I (deleted_at);', 'ix_' || t || '_deleted_at', t);
  END LOOP;
END $$;
""";

    private const string AuditTablesSql = """
CREATE TABLE IF NOT EXISTS barber.audit_logs (
  id uuid primary key default gen_random_uuid(),
  tenant_id uuid null references barber.tenants(id),
  branch_id uuid null references barber.branches(id),
  user_id uuid null,
  module varchar(80) not null,
  action varchar(80) not null,
  entity_name varchar(120) not null,
  entity_id uuid null,
  description text not null,
  metadata jsonb not null default '{}'::jsonb,
  payload jsonb not null default '{}'::jsonb,
  status varchar(30) not null default 'Active',
  created_at timestamp not null default now(),
  updated_at timestamp null,
  deleted_at timestamp null,
  is_active boolean not null default true,
  created_by uuid null,
  updated_by uuid null
);

ALTER TABLE barber.audit_logs ADD COLUMN IF NOT EXISTS status varchar(30) not null default 'Active';
ALTER TABLE barber.audit_logs ADD COLUMN IF NOT EXISTS payload jsonb not null default '{}'::jsonb;

CREATE TABLE IF NOT EXISTS barber.notifications (
  id uuid primary key default gen_random_uuid(),
  tenant_id uuid null references barber.tenants(id),
  branch_id uuid null references barber.branches(id),
  title varchar(160) not null,
  message text not null,
  entity_name varchar(120) null,
  entity_id uuid null,
  payload jsonb not null default '{}'::jsonb,
  status varchar(30) not null default 'Unread',
  is_active boolean not null default true,
  created_at timestamp not null default now(),
  updated_at timestamp null,
  deleted_at timestamp null,
  created_by uuid null,
  updated_by uuid null
);
""";

    private const string BusinessIndexesSql = """
CREATE INDEX IF NOT EXISTS ix_audit_logs_tenant_id ON barber.audit_logs (tenant_id);
CREATE INDEX IF NOT EXISTS ix_audit_logs_branch_id ON barber.audit_logs (branch_id);
CREATE INDEX IF NOT EXISTS ix_audit_logs_created_at ON barber.audit_logs (created_at);
CREATE INDEX IF NOT EXISTS ix_notifications_tenant_id ON barber.notifications (tenant_id);
CREATE INDEX IF NOT EXISTS ix_notifications_branch_id ON barber.notifications (branch_id);
CREATE INDEX IF NOT EXISTS ix_notifications_status ON barber.notifications (status);
CREATE INDEX IF NOT EXISTS ix_notifications_created_at ON barber.notifications (created_at);
CREATE UNIQUE INDEX IF NOT EXISTS ux_clients_document_tenant_active ON barber.clients (tenant_id, (payload->>'document')) WHERE deleted_at IS NULL AND coalesce(payload->>'document','') <> '';
CREATE INDEX IF NOT EXISTS ix_appointments_professional_scheduled ON barber.appointments (tenant_id, (payload->>'professionalId'), (payload->>'scheduledAt')) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_products_current_min_stock ON barber.products (((payload->>'currentStock')::numeric), ((payload->>'minStock')::numeric)) WHERE deleted_at IS NULL;
""";

    private const string SeedSql = """
INSERT INTO barber.tenants (id, slug, name, document, status, is_active)
VALUES ('11111111-1111-1111-1111-111111111111', 'barbersync-demo', 'BarberSync Demo', '00.000.000/0001-00', 'Active', true)
ON CONFLICT (id) DO UPDATE SET name = excluded.name, status = excluded.status, is_active = excluded.is_active;

INSERT INTO barber.branches (id, tenant_id, name, code, status, is_active, payload)
VALUES ('22222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111', 'Unidade Centro', 'CENTRO', 'Active', true, '{"city":"São Paulo","state":"SP"}'::jsonb)
ON CONFLICT (id) DO UPDATE SET name = excluded.name, payload = excluded.payload;

INSERT INTO barber.users (id, tenant_id, branch_id, payload, status)
VALUES ('33333333-3333-3333-3333-333333333333', '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', '{"name":"Administrador Demo","email":"admin@barbersync.demo","role":"Admin"}'::jsonb, 'Active')
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.clients (id, tenant_id, branch_id, payload, status) VALUES
('00000000-0000-0000-0000-000000000101','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Marcos Vinícius","phone":"(11) 99901-0101","document":"123.456.789-01","email":"marcos@demo.local","segment":"VIP"}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000102','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Thiago Almeida","phone":"(11) 99902-0102","document":"123.456.789-02","email":"thiago@demo.local","segment":"Regular"}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000103','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Fernanda Costa","phone":"(11) 99903-0103","document":"123.456.789-03","email":"fernanda@demo.local","segment":"VIP"}'::jsonb,'Active')
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.professionals (id, tenant_id, branch_id, payload, status) VALUES
('00000000-0000-0000-0000-000000000201','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Rafael Barber","specialty":"Cortes masculinos","commissionPercent":45,"visibleOnPublicWeb":true}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000202','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Camila Beauty","specialty":"Estética e sobrancelhas","commissionPercent":40,"visibleOnPublicWeb":true}'::jsonb,'Active')
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.services (id, tenant_id, branch_id, payload, status) VALUES
('00000000-0000-0000-0000-000000000301','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Corte Masculino","category":"Cabelo","price":70,"durationMinutes":45,"commissionPercent":45,"visibleOnPublicWeb":true,"visibleOnKiosk":true}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000302','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Barba Tradicional","category":"Barba","price":45,"durationMinutes":30,"commissionPercent":40,"visibleOnPublicWeb":true,"visibleOnKiosk":true}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000303','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Corte + Barba","category":"Combo","price":110,"durationMinutes":75,"commissionPercent":45,"visibleOnPublicWeb":true,"visibleOnKiosk":true}'::jsonb,'Active')
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.products (id, tenant_id, branch_id, payload, status) VALUES
('00000000-0000-0000-0000-000000000401','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Pomada Matte","category":"Revenda","sku":"BS-0001","costPrice":12,"salePrice":39,"currentStock":25,"minStock":10,"unit":"un"}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000402','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Shampoo Profissional","category":"Revenda","sku":"BS-0002","costPrice":18,"salePrice":59,"currentStock":9,"minStock":10,"unit":"un"}'::jsonb,'Active')
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.appointments (id, tenant_id, branch_id, payload, status)
SELECT ('00000000-0000-0000-0000-' || lpad((500 + i)::text, 12, '0'))::uuid, '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', jsonb_build_object('clientName', (ARRAY['Marcos Vinícius','Thiago Almeida','Fernanda Costa'])[1 + ((i-1)%3)], 'serviceName', (ARRAY['Corte Masculino','Barba Tradicional','Corte + Barba'])[1 + ((i-1)%3)], 'professionalName', (ARRAY['Rafael Barber','Camila Beauty','Rafael Barber'])[1 + ((i-1)%3)], 'scheduledAt', to_char(now() + (i || ' hours')::interval, 'YYYY-MM-DD"T"HH24:MI:SS'), 'origin', 'Admin', 'notes', 'Seed desenvolvimento'), 'Scheduled'
FROM generate_series(1,5) i
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.payments (id, tenant_id, branch_id, payload, status)
SELECT ('00000000-0000-0000-0000-' || lpad((700 + i)::text, 12, '0'))::uuid, '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', jsonb_build_object('method', (ARRAY['PIX','Cartão','Dinheiro'])[1 + ((i-1)%3)], 'amount', 75 + i, 'receiptNumber', 'REC-2026-' || (1000+i)), 'Paid'
FROM generate_series(1,3) i
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.campaigns (id, tenant_id, branch_id, payload, status)
SELECT ('00000000-0000-0000-0000-' || lpad((800 + i)::text, 12, '0'))::uuid, '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', jsonb_build_object('name', (ARRAY['Volte e Ganhe','Combo do Mês','Aniversariantes'])[i], 'audience', 'Base ativa', 'channel', (ARRAY['WhatsApp','Instagram','SMS'])[i], 'message', 'Campanha seed'), 'Active'
FROM generate_series(1,3) i
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.coupons (id, tenant_id, branch_id, payload, status)
SELECT ('00000000-0000-0000-0000-' || lpad((900 + i)::text, 12, '0'))::uuid, '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', jsonb_build_object('code', (ARRAY['VIP10','VOLTE20','COMBO15'])[i], 'type', 'Percentual', 'percent', (ARRAY[10,20,15])[i], 'usageLimit', 100), 'Active'
FROM generate_series(1,3) i
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.reviews (id, tenant_id, branch_id, payload, status)
SELECT ('00000000-0000-0000-0000-' || lpad((1000 + i)::text, 12, '0'))::uuid, '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', jsonb_build_object('clientName', (ARRAY['Marcos','Thiago','Fernanda'])[1 + ((i-1)%3)], 'rating', (ARRAY[5,5,4])[1 + ((i-1)%3)], 'comment', 'Avaliação seed BarberSync'), 'Published'
FROM generate_series(1,3) i
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.kiosk_devices (id, tenant_id, branch_id, payload, status)
VALUES ('00000000-0000-0000-0000-000000001101','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"deviceCode":"KIOSK-DEMO-001","name":"Totem Demo Centro","lastSeenAt":"2026-06-07T00:00:00Z"}'::jsonb,'Online')
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.audit_logs (tenant_id, branch_id, module, action, entity_name, description, metadata)
SELECT '11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','Seed','DatabaseSeeded','barber_full_database_postgresql','Seeds mínimos de desenvolvimento aplicados.','{"source":"BarberSchemaInitializer"}'::jsonb
WHERE NOT EXISTS (select 1 from barber.audit_logs where module = 'Seed' and action = 'DatabaseSeeded' and entity_name = 'barber_full_database_postgresql');
""";
}
