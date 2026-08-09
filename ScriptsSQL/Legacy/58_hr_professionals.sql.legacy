-- HR Professionals base schema for BarberSync 2.0
CREATE TABLE IF NOT EXISTS professional_profiles (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    branch_id UUID NOT NULL,
    full_name VARCHAR(180) NOT NULL,
    employment_type VARCHAR(30) NOT NULL,
    admission_date DATE NOT NULL,
    termination_date DATE NULL,
    technical_level VARCHAR(60) NOT NULL,
    professional_registry VARCHAR(80) NULL,
    default_commission_percent NUMERIC(8,2) NOT NULL DEFAULT 0,
    monthly_goal NUMERIC(12,2) NOT NULL DEFAULT 0,
    weekly_workload_hours INT NOT NULL DEFAULT 44,
    operational_status VARCHAR(30) NOT NULL DEFAULT 'Ativo',
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS professional_specialties (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    professional_id UUID NOT NULL REFERENCES professional_profiles(id),
    specialty_name VARCHAR(120) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS professional_service_authorizations (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    professional_id UUID NOT NULL REFERENCES professional_profiles(id),
    service_id UUID NOT NULL,
    certification_required BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);
