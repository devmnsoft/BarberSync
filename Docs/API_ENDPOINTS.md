# API_ENDPOINTS

Endpoints mínimos auditados para o fluxo principal BarberSync:

## Auth e dashboard
- `POST /api/auth/login`
- `GET /api/auth/me`
- `GET /api/dashboard/summary`

## Cadastros e agenda
- `GET /api/clients`
- `POST /api/clients`
- `PUT /api/clients/{id}`
- `DELETE /api/clients/{id}`
- `GET /api/professionals`
- `POST /api/professionals`
- `GET /api/services`
- `POST /api/services`
- `GET /api/appointments`
- `POST /api/appointments`
- `POST /api/appointments/{id}/confirm`
- `POST /api/appointments/{id}/check-in`
- `POST /api/appointments/{id}/cancel`

## Ordens, estoque e produtos
- `GET /api/service-orders`
- `POST /api/service-orders/open`
- `POST /api/service-orders/{id}/add-service`
- `POST /api/service-orders/{id}/pay`
- `POST /api/service-orders/{id}/close`
- `GET /api/products`
- `POST /api/products`
- `GET /api/stock/critical`

## Kiosk, configuração e IA
- `GET /api/admin/settings/kiosk`
- `POST /api/admin/settings/kiosk`
- `GET /api/kiosk/config/{deviceCode}`
- `GET /api/kiosk/services`
- `POST /api/kiosk/payment/mock`
- `POST /api/copilot/ask`
