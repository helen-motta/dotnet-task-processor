# Task Processor
Serviço de processamento assíncrono de tarefas desenvolvido em C# com ASP.NET Core, MongoDB e RabbitMQ.

Para conhecer detalhadamente a arquitetura, as implementações e as regras de negócio, consulte a [documentação técnica](./DOCUMENTACAO_TECNICA.md).

## Executando com Docker
Na raiz do projeto, execute:

```powershell
docker compose up --build -d
```

Esse comando inicia:

- API
- MongoDB
- RabbitMQ e painel de gerenciamento

| Serviço | Endereço |
|---|---|
| Swagger | http://localhost:5000/swagger |
| API | http://localhost:5000 |
| RabbitMQ Management | http://localhost:15672 |
| MongoDB | `mongodb://localhost:27017` |

Credenciais do RabbitMQ Management:

```text
Usuário: taskprocessor
Senha: taskprocessor
```

## Utilizando a API

### Criar uma tarefa

```http
POST /api/tasks
Content-Type: application/json
```

Exemplo para e-mail:

```json
{
  "type": "EnviarEmail",
  "data": "Enviar mensagem de boas-vindas"
}
```

Exemplo para relatório:

```json
{
  "type": "GerarRelatorio",
  "data": "Gerar relatório mensal"
}
```

### Consultar todas as tarefas

```http
GET /api/tasks
```

### Consultar uma tarefa pelo ID

```http
GET /api/tasks/{id}
```

## Testes unitários

```powershell
docker compose --profile test run --rm --build unit-tests
```

## Teste de carga com Locust

```powershell
docker compose --profile load up locust
```

Acesse:

```text
http://localhost:8089
```
