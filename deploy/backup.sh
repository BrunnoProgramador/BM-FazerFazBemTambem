#!/bin/bash
# =============================================================
# Backup do Firebird na VM de produção — versão corrigida:
# - carrega o .env explicitamente (cron não herda variáveis do compose)
# - gbak escreve direto no volume ./backups (sem docker cp)
# - criptografa com gpg antes de qualquer envio externo
# - retenção dos últimos 14
#
# Agendar (crontab -e), todo dia às 3h:
#   0 3 * * * /home/ubuntu/projetoem/deploy/backup.sh >> /home/ubuntu/projetoem/deploy/backup.log 2>&1
#
# Pré-requisito: criar a passphrase uma única vez:
#   openssl rand -base64 32 > ~/.backup_passphrase && chmod 600 ~/.backup_passphrase
#   (guarde uma cópia da passphrase em local seguro — sem ela o backup não abre)
# =============================================================
set -euo pipefail
cd "$(dirname "$0")"

# cron não carrega o .env do compose — carregamos aqui
set -a
source ./.env
set +a

DATA=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR=./backups
ARQUIVO="backup_${DATA}.fbk"
mkdir -p "$BACKUP_DIR"

# Dump lógico consistente (nunca copie o .fdb bruto com o servidor rodando)
docker compose -f docker-compose.producao.yml exec -T firebird \
  gbak -b -user SYSDBA -password "$FB_PASSWORD" \
  /firebird/data/projetoem.fdb "/firebird/backups/$ARQUIVO"

# O volume ./backups é compartilhado — o arquivo já está no host.
# Criptografa (o .fbk contém dados pessoais de crianças) e remove o original
gpg --batch --yes --passphrase-file "$HOME/.backup_passphrase" \
  --symmetric --cipher-algo AES256 "$BACKUP_DIR/$ARQUIVO"
rm "$BACKUP_DIR/$ARQUIVO"

# Retenção: mantém só os 14 mais recentes
ls -t "$BACKUP_DIR"/*.gpg | tail -n +15 | xargs -r rm

echo "[$(date '+%F %T')] backup ok: $ARQUIVO.gpg"

# Envio externo (opcional, recomendado) — Oracle Object Storage via rclone:
# rclone copy "$BACKUP_DIR" remote-oracle:projetoem-backups --include "*.gpg"
