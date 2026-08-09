CREATE TABLE users (id UUID PRIMARY KEY, name VARCHAR(120), email VARCHAR(120) UNIQUE, password_hash VARCHAR(255), created_at TIMESTAMP);
CREATE TABLE clients (id UUID PRIMARY KEY, name VARCHAR(120), phone VARCHAR(30), created_at TIMESTAMP);
CREATE TABLE services (id UUID PRIMARY KEY, name VARCHAR(120), price NUMERIC(10,2), duration_minutes INT);
CREATE TABLE appointments (id UUID PRIMARY KEY, client_id UUID REFERENCES clients(id), service_id UUID REFERENCES services(id), appointment_at TIMESTAMP, status VARCHAR(30));
CREATE TABLE payments (id UUID PRIMARY KEY, appointment_id UUID REFERENCES appointments(id), amount NUMERIC(10,2), method VARCHAR(30), status VARCHAR(30));
CREATE TABLE stock (id UUID PRIMARY KEY, name VARCHAR(120), quantity INT, min_quantity INT);
CREATE TABLE service_recognition (id UUID PRIMARY KEY, image_url TEXT, predicted_service VARCHAR(120), confidence NUMERIC(5,4));
