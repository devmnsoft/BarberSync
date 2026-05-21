-- BarberSync 2.0 - Futuristic AI Automation Expansion

CREATE TABLE IF NOT EXISTS AiModelRegistry (
    Id UUID PRIMARY KEY,
    ModelName VARCHAR(150) NOT NULL,
    ModelType VARCHAR(80) NOT NULL,
    VersionTag VARCHAR(40) NOT NULL,
    AccuracyScore NUMERIC(5,2) NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    TrainedAtUtc TIMESTAMP NOT NULL,
    CreatedAtUtc TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS DemandForecast (
    Id UUID PRIMARY KEY,
    ForecastDate DATE NOT NULL,
    ForecastHour SMALLINT NOT NULL,
    ServiceId UUID NULL,
    ExpectedAppointments INTEGER NOT NULL,
    OccupancyRate NUMERIC(5,2) NOT NULL,
    ExpectedStockUsage INTEGER NOT NULL,
    Confidence NUMERIC(5,2) NOT NULL,
    GeneratedAtUtc TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS ProfessionalAiMetrics (
    Id UUID PRIMARY KEY,
    ProfessionalId UUID NOT NULL,
    ServiceId UUID NULL,
    PrecisionScore NUMERIC(5,2) NOT NULL,
    EfficiencyScore NUMERIC(5,2) NOT NULL,
    SatisfactionScore NUMERIC(5,2) NOT NULL,
    UpsellConversionRate NUMERIC(5,2) NOT NULL,
    EstimatedDurationMinutes INTEGER NOT NULL,
    CapturedAtUtc TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS SmartNotificationLog (
    Id UUID PRIMARY KEY,
    TriggerType VARCHAR(80) NOT NULL,
    Channel VARCHAR(40) NOT NULL,
    AudienceType VARCHAR(50) NOT NULL,
    Payload JSONB NOT NULL,
    DeliveryStatus VARCHAR(30) NOT NULL,
    SentAtUtc TIMESTAMP NULL,
    CreatedAtUtc TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS IX_DemandForecast_DateHour ON DemandForecast(ForecastDate, ForecastHour);
CREATE INDEX IF NOT EXISTS IX_ProfessionalAiMetrics_Professional ON ProfessionalAiMetrics(ProfessionalId, CapturedAtUtc DESC);
CREATE INDEX IF NOT EXISTS IX_SmartNotificationLog_Trigger ON SmartNotificationLog(TriggerType, CreatedAtUtc DESC);
