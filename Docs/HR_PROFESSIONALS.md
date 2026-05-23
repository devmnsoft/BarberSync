# HR Professionals

Este módulo implementa a base operacional de RH para profissionais.

## Endpoints

- `GET /api/hr/professionals?tenantId={tenantId}`
- `POST /api/hr/professionals`
- `GET /api/hr/professionals/{id}/profile?tenantId={tenantId}`

## Campos suportados

- Tipo de vínculo (CLT, MEI, Autônomo, Comissionado, Parceiro, Freelancer)
- Data de admissão e desligamento
- Nível técnico
- Comissão padrão
- Meta mensal
- Carga horária
- Especialidades
- Serviços autorizados
- Documentos obrigatórios

## Observações

- Serviço implementado em memória para ambiente de desenvolvimento.
- Estrutura SQL inicial adicionada em `ScriptsSQL/58_hr_professionals.sql`.
