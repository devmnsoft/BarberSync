# Manual de uso do PublicWeb BarberSync

## Landing comercial

A página inicial apresenta hero premium, problemas, solução, serviços, profissionais, Totem, Mobile, Dashboard, Copilot, planos, depoimentos, FAQ e footer.

## Agendamento público

O formulário coleta nome, telefone, serviço, profissional, data, hora e observação. A submissão usa `/PublicApi/appointments`; se a API falhar, o fallback mantém a experiência e exibe toast de sucesso demo.

## Proxies

Serviços e profissionais são carregados por `/PublicApi/services` e `/PublicApi/professionals`. O navegador nunca usa a URL interna Docker da API.
