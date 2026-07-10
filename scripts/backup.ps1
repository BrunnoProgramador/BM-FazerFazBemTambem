# =============================================================
# Backup do banco ProjetoEM via gbak (dump logico consistente).
# Nunca copie o arquivo .FB5 direto com o servidor rodando.
#
# Agendamento diario (executar uma vez num PowerShell como admin):
#   schtasks /Create /SC DAILY /ST 19:00 /TN "Backup ProjetoEM" `
#     /TR "powershell -NoProfile -ExecutionPolicy Bypass -File \"C:\Users\Escolar Manager\Desktop\ProjetoAtual\scripts\backup.ps1\""
# =============================================================
param(
    [string]$Servidor = "localhost/3055",
    [string]$Banco    = "C:\Users\Escolar Manager\Desktop\ProjetoAtual\ProjetoAtual\EM.Repository\Banco\PROJETOEM.FB5",
    [string]$Usuario  = "SYSDBA",
    [string]$Senha    = "masterkey",
    [string]$Destino  = "$env:USERPROFILE\Backups\ProjetoEM",
    [int]$Manter      = 14
)

$ErrorActionPreference = "Stop"

# Localiza o gbak (PATH ou instalacao padrao do Firebird)
$gbak = (Get-Command gbak -ErrorAction SilentlyContinue).Source
if (-not $gbak) {
    $candidatos = Get-ChildItem `
        "C:\Program Files\Firebird\*\gbak.exe", `
        "C:\Program Files (x86)\Firebird\*\gbak.exe" -ErrorAction SilentlyContinue
    if ($candidatos) { $gbak = $candidatos[0].FullName }
    else { throw "gbak.exe nao encontrado. Ajuste o PATH ou informe o caminho no script." }
}

New-Item -ItemType Directory -Force -Path $Destino | Out-Null
$arquivo = Join-Path $Destino ("PROJETOEM_{0:yyyyMMdd_HHmmss}.fbk" -f (Get-Date))

& $gbak -b -v "$Servidor`:$Banco" $arquivo -user $Usuario -pas $Senha
if ($LASTEXITCODE -ne 0) { throw "gbak falhou com codigo $LASTEXITCODE" }

# Retencao: mantem apenas os N backups mais recentes
Get-ChildItem $Destino -Filter *.fbk |
    Sort-Object LastWriteTime -Descending |
    Select-Object -Skip $Manter |
    Remove-Item

Write-Host "Backup OK: $arquivo"
# Dica: copie periodicamente a pasta $Destino para um segundo local
# (OneDrive/Drive/HD externo). Backup no mesmo disco do banco nao
# protege contra falha do disco. E teste a restauracao de vez em quando:
#   gbak -c arquivo.fbk C:\temp\restaurado.fb5
