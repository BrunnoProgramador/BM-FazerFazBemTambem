# =============================================================
# Demo por tunel - expoe o ProjetoEM para UMA pessoa testar pelo
# navegador, sem ela instalar nada.
#
# O que faz:
#   1. Sobe o app numa janela propria usando um banco de DEMO
#      separado (o banco real nunca entra no tunel)
#   2. Abre um tunel Cloudflare e mostra o link https://... para enviar
#
# Pre-requisitos nesta maquina: .NET SDK e internet. Para o banco:
#   - Firebird nativo na porta 3055 (maquina do trabalho), OU
#   - Docker (o script sobe um Firebird descartavel sozinho)
# O cloudflared e instalado sozinho se faltar.
#
# Uso:  .\demo-tunel.ps1        (Ctrl+C encerra o tunel)
# =============================================================

$ErrorActionPreference = "Stop"
$raiz  = Split-Path -Parent $PSScriptRoot
$porta = 5080

# ---- banco de demo: container ja rodando, Firebird nativo (3055) ou container novo ----
$temDocker = [bool](Get-Command docker -ErrorAction SilentlyContinue)
$containerRodando = $false
if ($temDocker) {
    $containerRodando = [bool](docker ps -q --filter "name=^firebird-demo$")
}

$firebirdNativo = $false
if (-not $containerRodando) {
    $firebirdNativo = Test-NetConnection localhost -Port 3055 -InformationLevel Quiet -WarningAction SilentlyContinue
}

if ($containerRodando) {
    Write-Host "Container firebird-demo ja esta rodando - reutilizando." -ForegroundColor Green
    $bancoDemo = "/firebird/data/demo.fdb"
}
elseif ($firebirdNativo) {
    Write-Host "Firebird local encontrado na porta 3055 - usando banco de demo em C:\Temp." -ForegroundColor Green
    $pastaDemo = "C:\Temp\ProjetoEM-Demo"
    New-Item -ItemType Directory -Force -Path $pastaDemo | Out-Null
    $bancoDemo = Join-Path $pastaDemo "DEMO_PROJETOEM.FB5"
}
elseif ($temDocker) {
    Write-Host "Sem Firebird na 3055 - subindo um Firebird descartavel no Docker..." -ForegroundColor Yellow

    # remove um container antigo apenas se ele existir (sem tocar no stderr,
    # que com ErrorActionPreference=Stop viraria erro fatal)
    $containerAntigo = docker ps -aq --filter "name=^firebird-demo$"
    if ($containerAntigo) { docker rm -f firebird-demo | Out-Null }

    docker run -d --name firebird-demo -p 127.0.0.1:3055:3050 -e ISC_PASSWORD=masterkey -e FIREBIRD_DATABASE=demo.fdb -v projetoem_demo_dados:/firebird/data jacobalberty/firebird:v5 | Out-Null

    # caminho visto pelo SERVIDOR (dentro do container)
    $bancoDemo = "/firebird/data/demo.fdb"

    Write-Host "Aguardando o Firebird do container aceitar conexao..."
    do { Start-Sleep -Seconds 2 }
    until (Test-NetConnection localhost -Port 3055 -InformationLevel Quiet -WarningAction SilentlyContinue)
    Write-Host "Firebird de demo no ar (dados persistem no volume 'projetoem_demo_dados')." -ForegroundColor Green
}
else {
    Write-Host "Nem Firebird (porta 3055) nem Docker encontrados nesta maquina." -ForegroundColor Red
    Write-Host "Inicie o servico do Firebird ou instale/abra o Docker Desktop e rode de novo."
    exit 1
}

# ---- cloudflared: instala via winget se nao existir ----
if (-not (Get-Command cloudflared -ErrorAction SilentlyContinue)) {
    Write-Host "cloudflared nao encontrado - instalando via winget..." -ForegroundColor Yellow
    winget install --id Cloudflare.cloudflared --accept-source-agreements --accept-package-agreements
    $env:Path = [Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [Environment]::GetEnvironmentVariable("Path", "User")
    if (-not (Get-Command cloudflared -ErrorAction SilentlyContinue)) {
        Write-Host "Feche e reabra o PowerShell e rode o script de novo (PATH atualizado)." -ForegroundColor Red
        exit 1
    }
}

# ---- sobe o app com o banco de DEMO (criado sozinho pelo MigradorBanco) ----
$cs = "DataSource=localhost;Port=3055;Database=$bancoDemo;User=SYSDBA;Password=masterkey;Charset=UTF8"
$comandoApp = "`$env:ConnectionStrings__Firebird='$cs'; dotnet run --project '$raiz\ProjetoAtual\EM.Web' --urls http://localhost:$porta"
Start-Process powershell -ArgumentList "-NoExit", "-Command", $comandoApp

Write-Host ""
Write-Host "Aguardando o app subir em http://localhost:$porta ..."
do {
    Start-Sleep -Seconds 2
} until (Test-NetConnection localhost -Port $porta -InformationLevel Quiet -WarningAction SilentlyContinue)

Write-Host ""
Write-Host "App no ar. Abrindo o tunel..." -ForegroundColor Green
Write-Host ""
Write-Host "PASSO A PASSO:" -ForegroundColor Cyan
Write-Host "  1. Copie o link https://....trycloudflare.com que vai aparecer abaixo"
Write-Host "  2. Abra o link VOCE PRIMEIRO e crie o usuario administrador"
Write-Host "     (o primeiro acesso e a tela de configuracao inicial)"
Write-Host "  3. Envie o link para a pessoa e o usuario/senha por outro canal"
Write-Host "     (ex: link por e-mail, senha por WhatsApp)"
Write-Host ""
Write-Host "O link so funciona enquanto esta janela e a janela do app"
Write-Host "estiverem abertas. Ctrl+C encerra o tunel; feche tambem a"
Write-Host "janela do app ao terminar os testes." -ForegroundColor Yellow
if (-not $firebirdNativo) {
    Write-Host "Para desligar o banco de demo: docker rm -f firebird-demo"
    Write-Host "(os dados ficam no volume 'projetoem_demo_dados' para a proxima demo)"
}
Write-Host ""

cloudflared tunnel --url "http://localhost:$porta"
