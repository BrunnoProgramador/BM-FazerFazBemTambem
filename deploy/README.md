# Deploy na VM (Oracle Cloud Always Free)

Guia consolidado com as correções discutidas: aplicação inteira
(ASP.NET MVC + Firebird) na mesma VM via Docker Compose, HTTPS pelo
Caddy, banco nunca exposto e backup com `gbak` criptografado.

> **Pré-condição inegociável:** o sistema tem login, mas trata dados
> pessoais de crianças. Só abra as portas 80/443 ao mundo com o HTTPS do
> Caddy funcionando. Sem domínio ainda? Não publique 80/443 na Security
> List e acesse também por túnel SSH até resolver.

## 1. Provisionar a VM

| Shape | RAM/CPU (jul/2026) | Disponibilidade |
|---|---|---|
| VM.Standard.A1.Flex (ARM) | até 12 GB / 2 OCPU | instável ("out of host capacity" é comum) |
| VM.Standard.E2.1.Micro (AMD64) | 1 GB / 1/8 OCPU | sempre disponível |

Para este projeto o **E2.1.Micro basta** e evita dias tentando A1.
Ubuntu 22.04+, chave SSH própria.

**No E2.1.Micro (1 GB), crie swap antes de subir os containers:**

```bash
sudo fallocate -l 2G /swapfile && sudo chmod 600 /swapfile
sudo mkswap /swapfile && sudo swapon /swapfile
echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
```

## 2. Firewall

**Security List/NSG da Oracle:** liberar somente 22 (SSH), 80 e 443.
A porta do Firebird **nunca** aparece aqui.

**ufw na VM** (defesa em profundidade):

```bash
sudo ufw default deny incoming
sudo ufw allow 22/tcp && sudo ufw allow 80/tcp && sudo ufw allow 443/tcp
sudo ufw enable
```

> Atenção: o Docker escreve direto no iptables e **bypassa o ufw** para
> portas publicadas. As proteções reais do banco são o bind em
> `127.0.0.1` no compose e a Security List — o ufw é redundância.

## 3. Instalar e subir

```bash
sudo apt update && sudo apt install -y docker.io docker-compose-plugin git
sudo usermod -aG docker $USER   # relogar depois

git clone <url-do-repo> ~/projetoem && cd ~/projetoem/deploy
cp .env.exemplo .env && nano .env   # senha forte + domínio
docker compose -f docker-compose.producao.yml up -d --build
```

O `MigradorBanco` cria/atualiza o schema sozinho na subida. Primeiro
acesso ao site abre a tela de criação do usuário administrador.

O healthcheck do compose segura o `web` até o Firebird aceitar conexão —
sem crash-loop na subida.

## 4. Migrar os dados que já existem (uma vez)

```powershell
# na máquina atual (trabalho)
gbak -b -user SYSDBA -password *** localhost/3055:C:\...\PROJETOEM.FB5 dados.fbk
scp -i chave.pem dados.fbk ubuntu@IP-DA-VM:~/projetoem/deploy/backups/
```

```bash
# na VM — restaura por cima do banco vazio (e valida o restore no dia um)
cd ~/projetoem/deploy && docker compose -f docker-compose.producao.yml stop web
docker compose -f docker-compose.producao.yml exec firebird \
  gbak -rep -user SYSDBA -password "$FB_PASSWORD" \
  /firebird/backups/dados.fbk /firebird/data/projetoem.fdb
docker compose -f docker-compose.producao.yml start web
```

## 5. Acessar o banco do seu PC (só via túnel SSH)

```bash
ssh -N -L 3055:localhost:3055 -i chave.pem ubuntu@IP-DA-VM
# com o túnel aberto: DBeaver/IBExpert em localhost:3055
```

## 6. Backup diário

`backup.sh` desta pasta: `gbak` → gpg (AES-256) → retenção de 14.
Agendamento e passphrase estão no cabeçalho do script. Envio para fora
da VM: Oracle Object Storage (20 GB no Always Free) via `rclone` —
linha pronta comentada no fim do script.

**Teste a restauração de tempos em tempos** — backup nunca testado é
sorte, não estratégia:

```bash
gpg --decrypt backup_X.fbk.gpg > teste.fbk
gbak -c teste.fbk /tmp/teste_restore.fdb -user SYSDBA -password ***
```

## 7. Padronização (pendência consciente)

Hoje existem três combinações de porta (3050 container, 3055 trabalho,
3056 devcontainer de casa) e dois nomes de arquivo de banco. Em produção
este pacote fixa `projetoem.fdb` interno na 3050, loopback 3055 na VM.
Quando houver folga, alinhe os outros ambientes a esse padrão.
