#!/bin/bash
# Backup do Firebird do devcontainer (rodar no HOST, com o container ativo).
# Uso: ./scripts/backup-devcontainer.sh [pasta-destino]
set -euo pipefail

DESTINO="${1:-$HOME/Backups/ProjetoEM}"
MANTER=14

CONTAINER=$(docker ps --format '{{.Names}}' --filter "ancestor=jacobalberty/firebird:v5.0" | head -1)
if [ -z "$CONTAINER" ]; then
    echo "Container do Firebird nao esta rodando." >&2
    exit 1
fi

ARQ="PROJETOEM_$(date +%Y%m%d_%H%M%S).fbk"

docker exec "$CONTAINER" gbak -b -user SYSDBA -password masterkey \
    /firebird/data/PROJETOEM.FB5 "/tmp/$ARQ"

mkdir -p "$DESTINO"
docker cp "$CONTAINER:/tmp/$ARQ" "$DESTINO/$ARQ"
docker exec "$CONTAINER" rm "/tmp/$ARQ"

# Retencao: mantem apenas os N backups mais recentes
ls -t "$DESTINO"/*.fbk 2>/dev/null | tail -n +$((MANTER + 1)) | xargs -r rm

echo "Backup OK: $DESTINO/$ARQ"
