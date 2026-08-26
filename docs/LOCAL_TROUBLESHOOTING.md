# Troubleshooting da stack local

Execute primeiro `.\Scripts\check-local-stack.ps1`. O verificador não mostra segredos e retorna código diferente de zero se algum requisito falhar.

## `Jwt:SigningKey` deve possuir pelo menos 32 caracteres
- **Sintoma:** API encerra na inicialização.
- **Causa provável:** segredo ausente ou curto.
- **Diagnóstico:** `.\Scripts\check-api-config.ps1`.
- **Correção:** execute `.\Scripts\setup-api-local-dev.ps1 -ConnectionString "Host=..."`; o script gera uma chave criptograficamente aleatória quando ela não é informada.

## `ConnectionStrings:DefaultConnection` não foi configurada
- **Sintoma:** API, apply ou seed não conecta.
- **Causa provável:** user-secret da API ausente.
- **Diagnóstico:** `dotnet user-secrets list --project .\Backend\Presentation\BarberSync.Api\BarberSync.Api.csproj`.
- **Correção:** execute `setup-api-local-dev.ps1` com a conexão real.

## `Kiosk:DeviceCode` não foi configurado
- **Sintoma:** página **Totem não configurado**.
- **Causa provável:** setup Web ainda não executado.
- **Diagnóstico:** `.\Scripts\check-web-config.ps1`.
- **Correção:** `.\Scripts\setup-web-local-dev.ps1 -KioskDeviceCode "KIOSK-LOCAL-001"`.

## `localhost:5080` recusou conexão
- **Sintoma:** gateway informa API indisponível.
- **Causa provável:** API parada ou BaseUrl HTTP enquanto apenas HTTPS está ativo.
- **Diagnóstico:** `Invoke-WebRequest https://localhost:7088/health -SkipCertificateCheck`.
- **Correção:** inicie `run-local-stack.ps1` e configure `-ApiBaseUrl "https://localhost:7088"`.

## API sobe, mas Admin não abre
- **Sintoma:** API saudável e porta 7188 indisponível.
- **Causa provável:** processo Admin encerrou ou porta ocupada.
- **Diagnóstico:** `Get-NetTCPConnection -LocalPort 7188`; consulte `artifacts\local-stack\logs\AdminWeb.error.log`.
- **Correção:** libere a porta e reinicie a stack.

## Admin retorna 401
- **Sintoma:** mensagem de credenciais inválidas ou sessão expirada.
- **Causa provável:** seed ausente, senha incorreta ou cookie expirado.
- **Diagnóstico:** `.\Scripts\seed-local-dev.ps1` e tente `admin@barbersync.local`.
- **Correção:** aplique schema/seed; saia e entre novamente. Não desabilite autenticação.

## PublicWeb não lista serviços
- **Sintoma:** Home abre com área dinâmica indisponível ou vazia.
- **Causa provável:** API offline, BaseUrl divergente ou seed ausente.
- **Diagnóstico:** `Invoke-WebRequest https://localhost:7088/api/public/services -SkipCertificateCheck`.
- **Correção:** rode setup Web, schema e seed, depois reinicie a stack.

## Kiosk mostra tela branca
- **Sintoma:** conteúdo não renderiza.
- **Causa provável:** configuração antiga, JavaScript bloqueado ou API offline.
- **Diagnóstico:** abra DevTools e rode `.\Scripts\check-local-stack.ps1`.
- **Correção:** configure o DeviceCode, confirme `/health` e recarregue; o fluxo normal apresenta uma mensagem controlada.

## `psql` não encontrado
- **Sintoma:** apply/seed para antes de conectar.
- **Causa provável:** cliente PostgreSQL ausente do `PATH`.
- **Diagnóstico:** `Get-Command psql`.
- **Correção:** instale PostgreSQL Command Line Tools e reabra o PowerShell.

## dotnet SDK ausente
- **Sintoma:** scripts de secrets/build não iniciam.
- **Causa provável:** SDK não instalado ou fora do `PATH`.
- **Diagnóstico:** `dotnet --info`.
- **Correção:** instale o SDK requerido pelo projeto e reabra o terminal.

## Certificado HTTPS local inválido
- **Sintoma:** navegador ou `Invoke-WebRequest` rejeita localhost.
- **Causa provável:** dev certificate ausente/não confiável.
- **Diagnóstico:** `dotnet dev-certs https --check --trust`.
- **Correção:** `dotnet dev-certs https --clean; dotnet dev-certs https --trust`.

## Porta já em uso
- **Sintoma:** Kestrel informa falha de bind.
- **Causa provável:** execução anterior ainda ativa.
- **Diagnóstico:** `Get-NetTCPConnection -LocalPort 7088,7188,7288,7388`.
- **Correção:** encerre somente o processo local conflitante e execute novamente `run-local-stack.ps1`.

## Runner encerra antes de abrir o navegador
- **Sintoma:** `run-local-stack.ps1` informa que um endpoint não ficou pronto em 45 segundos.
- **Causa provável:** erro de startup, resposta HTTP 4xx/5xx, banco indisponível ou configuração inválida.
- **Diagnóstico:** use o PID exibido pelo runner e consulte os arquivos de stdout/stderr em `artifacts\local-stack\logs`; depois execute `check-local-stack.ps1` para obter uma ação sugerida por componente.
- **Correção:** corrija a primeira exceção do componente. O runner deliberadamente não abre o navegador nem apresenta sucesso quando o health básico falha.
