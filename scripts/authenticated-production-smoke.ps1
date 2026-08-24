[CmdletBinding(SupportsShouldProcess)] param([string]$BaseUrl = "http://localhost:5080")
$ErrorActionPreference = "Stop"; Set-StrictMode -Version Latest
$Names = "ADMIN_EMAIL","ADMIN_PASSWORD","CASHIER_EMAIL","CASHIER_PASSWORD","PROFESSIONAL_EMAIL","PROFESSIONAL_PASSWORD","CLIENT_EMAIL","CLIENT_PASSWORD","TENANT_ID","BRANCH_ID","KIOSK_DEVICE_CODE"
foreach ($Name in $Names) { if (-not [Environment]::GetEnvironmentVariable("READINESS_$Name")) { throw "READINESS_$Name is required" } }
if ($WhatIfPreference) { Write-Host "Validated authenticated smoke configuration (no credentials printed)."; return }
$Slug = if ($env:READINESS_TENANT_SLUG) {$env:READINESS_TENANT_SLUG} else {"production-readiness"}
function Request([string]$Method,[string]$Path,[string]$Token="",[object]$Body=$null,[int[]]$Expected=@(200)) {
  $Headers=@{}; if($Token){$Headers.Authorization="Bearer $Token"}; $Params=@{Uri="$BaseUrl$Path";Method=$Method;Headers=$Headers}
  if($null-ne $Body){$Params.ContentType="application/json";$Params.Body=($Body|ConvertTo-Json -Compress)}
  try {$R=Invoke-WebRequest @Params} catch [System.Net.WebException] {$R=$_.Exception.Response; if($null-eq $R){throw}}; if($Expected-notcontains [int]$R.StatusCode){throw "$Method $Path returned $($R.StatusCode)"}; if($R.Content){$R.Content|ConvertFrom-Json}else{$null}
}
function Login($Email,$Password){(Request POST /api/auth/login "" @{email=$Email;password=$Password;tenantSlug=$Slug}).data.accessToken}
$Bad=Request POST /api/auth/login "" @{email="invalid@readiness.local";password="invalid-password";tenantSlug=$Slug} @(400,401); if(-not $Bad.traceId){throw "Invalid login has no traceId"}
$Admin=Login $env:READINESS_ADMIN_EMAIL $env:READINESS_ADMIN_PASSWORD; "EVIDENCE:AUTH_SMOKE_LOGIN:PASS"
$Dashboard=Request GET /api/dashboard/summary $Admin; if(($Dashboard|ConvertTo-Json -Depth 20)-notmatch $env:READINESS_TENANT_ID){throw "Dashboard tenant mismatch"}; "EVIDENCE:AUTH_SMOKE_DASHBOARD:PASS"
$Client=Login $env:READINESS_CLIENT_EMAIL $env:READINESS_CLIENT_PASSWORD; $C=Request GET /api/mobile/summary $Client; $CJ=$C|ConvertTo-Json -Depth 20; if($CJ-notmatch 'Client' -or $CJ-match 'commissions|blocks'){throw "Client ownership contract failed"}; "EVIDENCE:AUTH_SMOKE_MOBILE_CLIENT:PASS"
$Professional=Login $env:READINESS_PROFESSIONAL_EMAIL $env:READINESS_PROFESSIONAL_PASSWORD; $P=Request GET /api/mobile/summary $Professional; $PJ=$P|ConvertTo-Json -Depth 20; if($PJ-notmatch 'Professional' -or $PJ-notmatch 'commissions' -or $PJ-notmatch 'blocks'){throw "Professional ownership contract failed"}; "EVIDENCE:AUTH_SMOKE_MOBILE_PROFESSIONAL:PASS"
$Before=Request GET /api/notifications $Admin; if(($Before|ConvertTo-Json -Depth 20)-notmatch 'Readiness notification'){throw "Seed notification missing"}; Request POST /api/notifications/read-all $Admin $null @(200,204)|Out-Null; $After=Request GET /api/notifications $Admin; if(($After|ConvertTo-Json -Depth 20)-match '"status":\s*"Unread"'){throw "Read-all was not persisted"}; "EVIDENCE:AUTH_SMOKE_NOTIFICATIONS:PASS"
$Stock=Request GET /api/stock $Admin; if(($Stock|ConvertTo-Json -Depth 20)-notmatch 'READINESS-PRODUCT'){throw "Readiness stock missing"}; "EVIDENCE:AUTH_SMOKE_STOCK:PASS"
$Cashier=Login $env:READINESS_CASHIER_EMAIL $env:READINESS_CASHIER_PASSWORD; $Cash=Request GET /api/cash-registers/current $Cashier; if(($Cash|ConvertTo-Json -Depth 20)-notmatch $env:READINESS_BRANCH_ID){throw "Cash branch mismatch"}; "EVIDENCE:AUTH_SMOKE_CASH_REGISTER:PASS"
"EVIDENCE:AUTH_SMOKE_POS:SKIPPED_CONTRACT_NOT_FOUND"
Request GET /api/kiosk/services "" $null @(400)|Out-Null; $Kiosk=Request GET "/api/kiosk/services?deviceCode=$($env:READINESS_KIOSK_DEVICE_CODE)"; $KJ=$Kiosk|ConvertTo-Json -Depth 20; if($KJ-notmatch 'Readiness Haircut' -or $KJ-match 'KIOSK-001'){throw "Kiosk device contract failed"}; "EVIDENCE:AUTH_SMOKE_KIOSK:PASS"
