# Roteiro de demonstração para cliente — BarberSync Demo 10.0

## 1. Abertura

Apresente o BarberSync como plataforma SaaS omnichannel para barbearias, salões, estética e franquias: Admin, PublicWeb, Totem, Mobile, API, PostgreSQL e observabilidade com Seq.

## 2. AdminWeb

1. Abrir `http://localhost:8081/Admin`.
2. Mostrar dashboard executivo com indicadores de agenda, receita, estoque, campanhas, reputação e totem.
3. Navegar por Clientes, Profissionais, Serviços, Agenda, Comandas, Estoque, Campanhas, Cupons, Fidelidade e Copilot.
4. Executar uma ação demo: criar cliente, serviço ou agendamento.
5. Destacar persistência local via DemoStore/localStorage e eventos via EventBus.

## 3. PublicWeb

1. Abrir `http://localhost:8082/`.
2. Mostrar catálogo de serviços e profissionais carregados por `/PublicApi`.
3. Preencher o formulário de agendamento.
4. Mostrar protocolo demo e CTA para Admin/Lead-to-Cash.

## 4. KioskWeb

1. Abrir `http://localhost:8083/Kiosk/Services`.
2. Escolher serviço.
3. Informar cliente.
4. Escolher profissional.
5. Confirmar atendimento.
6. Simular pagamento.
7. Ver tela de sucesso e avaliação.
8. Mostrar `sessionStorage` com `selectedService`, `selectedClient`, `selectedProfessional` e `selectedPayment`.

## 5. Mobile

Apresente o MobileApp como identidade visual consistente para serviços, agendamentos, cashback, cupons, notificações, avaliações e perfil.

## 6. Fechamento comercial

Conectar benefícios a dor operacional: menos faltas, giro de agenda, controle de estoque, comandas, caixa, canais digitais e fidelização.
