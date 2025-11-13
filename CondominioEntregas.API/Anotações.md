# para entrar no nosso banco de dados 

# se estiver no host com psql instalado:
psql -h localhost -U postgres -d portsafee

# dentro do container (exemplo de comando):
docker exec -it portsafee-postgres psql -U postgres -d portsafee