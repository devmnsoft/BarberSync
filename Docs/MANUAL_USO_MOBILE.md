# Manual de Uso — MobileApp BarberSync 2.0

## Objetivo
Este manual orienta a equipe comercial e operacional no uso do aplicativo móvel do BarberSync para demonstrações e uso diário.

## Pré-requisitos
- Node.js LTS instalado.
- Expo CLI via `npx`.
- API disponível em `http://localhost:8080`.

## Inicialização
1. Entre na pasta `MobileApp`.
2. Instale dependências: `npm install`.
3. Inicie o app: `npx expo start`.
4. Abra no Expo Go (Android/iOS) ou emulador.

## Fluxo de Demonstração (5 minutos)
1. **Login**: autentique com usuário demo.
2. **Home**: apresente cards de resumo e atalhos.
3. **Serviços**: abra catálogo e detalhe de serviço.
4. **Agendamento**: selecione serviço/profissional/data/hora e confirme.
5. **Meus Agendamentos**: exiba status e histórico.
6. **Cashback e Promoções**: mostre saldo e campanhas.
7. **Perfil**: evidencie edição de dados e preferências.

## Boas práticas UX
- Sempre mostrar loading em chamadas de API.
- Exibir estado vazio amigável quando não houver dados.
- Exibir estado de erro com ação de “tentar novamente”.
- Padronizar feedback com toast/snackbar.

## Troubleshooting
- Se API indisponível, usar fallback demo controlado e informar no topo da tela.
- Se houver erro de build do Expo, limpar cache: `npx expo start -c`.
- Se teste falhar por ambiente, validar localmente com internet ativa.

## Checklist de aceite
- Login e navegação funcionando.
- Lista de serviços carregando.
- Fluxo de agendamento concluindo.
- Histórico do cliente visível.
- Módulos de cashback e promoções acessíveis.
