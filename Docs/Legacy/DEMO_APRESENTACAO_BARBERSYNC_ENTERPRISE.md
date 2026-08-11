# BarberSync — Roteiro de demonstração comercial enterprise

## Objetivo
Apresentar o BarberSync como uma plataforma SaaS coesa para barbearias, salões, estética e franquias, com AdminWeb, PublicWeb, KioskWeb, MobileApp e API conectados por proxies MVC seguros.

## Fluxo recomendado
1. Abrir `http://localhost:8080/swagger` para mostrar que a API permanece documentada.
2. Abrir `http://localhost:8081/Admin` e destacar o dashboard executivo com KPIs, próximos agendamentos, estoque crítico e sugestões Copilot.
3. Navegar por Clientes, Profissionais, Serviços, Agendamentos, Comandas, Estoque, Campanhas, Cupons, Avaliações, Fidelidade e Copilot.
4. Criar um registro demo em Clientes, Profissionais ou Serviços e mostrar o toast de sucesso.
5. Abrir `http://localhost:8082/` para apresentar a landing premium, serviços, profissionais, planos mock e formulário público.
6. Abrir `http://localhost:8083/Kiosk/Services` para demonstrar o fluxo de totem: serviço, cliente, profissional, confirmação, pagamento mock, sucesso e avaliação.
7. Mostrar o MobileApp como base visual consistente com home, serviços rápidos, cashback e ação de agendamento.

## Mensagem técnica importante
O navegador chama somente `/AdminApi`, `/PublicApi` e `/KioskApi`. A URL interna `http://api:8080` fica restrita ao servidor MVC e ao Docker Compose via `ApiSettings:BaseUrl`.

## Plano de contingência demo
Se a API estiver indisponível durante a apresentação, os proxies retornam 200 com dados demonstrativos. Assim o Admin, o PublicWeb e o Kiosk não ficam vazios.
