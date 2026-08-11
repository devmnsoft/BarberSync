[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidatePattern('^[^@\s]+@[^@\s]+\.[^@\s]+$')][string]$Email,
    [Parameter(Mandatory)][SecureString]$Password,
    [Parameter(Mandatory)][ValidateLength(3, 180)][string]$FullName,
    [Parameter(Mandatory)][ValidatePattern('^[a-z0-9][a-z0-9-]{2,59}$')][string]$TenantSlug,
    [Parameter(Mandatory)][ValidatePattern('^[A-Za-z0-9-]{2,30}$')][string]$BranchCode,
    [string]$ApiUrl = 'http://localhost:5080'
)

$plainPassword = [System.Net.NetworkCredential]::new('', $Password).Password
try {
    if ($plainPassword.Length -lt 12 -or
        $plainPassword -cnotmatch '[A-Z]' -or
        $plainPassword -cnotmatch '[a-z]' -or
        $plainPassword -notmatch '[0-9]' -or
        $plainPassword -notmatch '[^A-Za-z0-9]') {
        throw 'A senha deve ter ao menos 12 caracteres e incluir maiúscula, minúscula, número e caractere especial.'
    }

    $body = @{
        email = $Email
        password = $plainPassword
        fullName = $FullName
        tenantSlug = $TenantSlug
        branchCode = $BranchCode
    } | ConvertTo-Json

    Invoke-RestMethod -Method Post -Uri "$($ApiUrl.TrimEnd('/'))/api/setup/first-admin" -ContentType 'application/json' -Body $body | Out-Null
    Write-Host 'Primeiro administrador criado. A senha não foi exibida nem armazenada.'
}
finally {
    $plainPassword = $null
    $body = $null
}

