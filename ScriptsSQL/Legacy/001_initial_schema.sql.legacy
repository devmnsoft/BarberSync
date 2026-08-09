CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE Users(
    UserId UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    Name VARCHAR(150) NOT NULL,
    Email VARCHAR(150) UNIQUE NOT NULL,
    PasswordHash VARCHAR(250) NOT NULL,
    Role VARCHAR(50) NOT NULL,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    Status CHAR(1) DEFAULT 'A'
);

CREATE TABLE Clients(
    ClientId UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    Name VARCHAR(150) NOT NULL,
    Document VARCHAR(50),
    Phone VARCHAR(20),
    Email VARCHAR(150),
    CreatedAt TIMESTAMP DEFAULT NOW(),
    Status CHAR(1) DEFAULT 'A'
);

CREATE TABLE Services(
    ServiceId UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    Name VARCHAR(150) NOT NULL,
    DurationMinutes INT NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    Category VARCHAR(100),
    CreatedAt TIMESTAMP DEFAULT NOW(),
    Status CHAR(1) DEFAULT 'A'
);

CREATE TABLE Appointments(
    AppointmentId UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    ClientId UUID REFERENCES Clients(ClientId),
    ServiceId UUID REFERENCES Services(ServiceId),
    ProfessionalId UUID REFERENCES Users(UserId),
    ScheduledAt TIMESTAMP NOT NULL,
    Status VARCHAR(50) NOT NULL,
    CreatedAt TIMESTAMP DEFAULT NOW()
);

CREATE TABLE Payments(
    PaymentId UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    AppointmentId UUID REFERENCES Appointments(AppointmentId),
    Amount DECIMAL(10,2) NOT NULL,
    PaymentMethod VARCHAR(50) NOT NULL,
    PaidAt TIMESTAMP DEFAULT NOW()
);

CREATE TABLE Stock(
    ProductId UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    Name VARCHAR(150) NOT NULL,
    Quantity INT NOT NULL,
    MinQuantity INT NOT NULL,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    Status CHAR(1) DEFAULT 'A'
);

CREATE TABLE ServiceRecognition(
    RecognitionId UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    AppointmentId UUID REFERENCES Appointments(AppointmentId),
    ServiceDetected VARCHAR(150),
    Confidence DECIMAL(5,2),
    ImageCaptured BYTEA,
    CreatedAt TIMESTAMP DEFAULT NOW()
);
