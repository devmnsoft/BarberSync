function Convert-ToLocalPostgresUri([string]$Value) {
  if ($Value -match '^postgres(ql)?://') { return $Value }
  $parts = @{}; $Value -split ';' | ForEach-Object { if ($_ -match '^\s*([^=]+)=(.*)$') { $parts[$matches[1].Trim().ToLowerInvariant()] = $matches[2].Trim() } }
  $hostName = $parts['host']; $port = if ($parts['port']) { $parts['port'] } else { '5432' }; $database = $parts['database']
  $user = if ($parts['username']) { $parts['username'] } else { $parts['user id'] }; $password = $parts['password']
  if (-not $hostName -or -not $database -or -not $user) { throw 'ConnectionString inválida: Host, Database e Username são obrigatórios.' }
  "postgresql://$([Uri]::EscapeDataString($user)):$([Uri]::EscapeDataString($password))@$hostName`:$port/$([Uri]::EscapeDataString($database))"
}
