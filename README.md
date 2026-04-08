# AgroTech IoT API

API desenvolvida em ASP.NET Core (.NET 8) com Clean Architecture para ingestão, processamento, armazenamento e consulta de dados de sensores IoT enviados via Node-RED.

---

## Sumário

- [Visão Geral do Projeto](#visão-geral-do-projeto)
- [Objetivo do Projeto](#objetivo-do-projeto)
- [Escopo](#escopo)
- [Requisitos Funcionais](#requisitos-funcionais)
- [Requisitos Não Funcionais](#requisitos-não-funcionais)
- [Arquitetura da Solução](#arquitetura-da-solução)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Entidade de Negócio](#entidade-de-negócio)
- [Fluxo IoT com Node-RED](#fluxo-iot-com-node-red)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Endpoints da API](#endpoints-da-api)
- [HATEOAS](#hateoas)
- [Health Checks](#health-checks)
- [Logging Estruturado](#logging-estruturado)
- [Observabilidade com OpenTelemetry](#observabilidade-com-opentelemetry)
- [Testes Automatizados](#testes-automatizados)
- [Como Executar o Projeto](#como-executar-o-projeto)
- [Como Executar as Migrations](#como-executar-as-migrations)
- [Como Executar os Testes](#como-executar-os-testes)
- [Exemplos de Requisição](#exemplos-de-requisição)
- [Status Atual do Projeto](#status-atual-do-projeto)
- [Autor](#autor)

## Visão Geral do Projeto

O AgroTech IoT API é uma aplicação backend criada para receber leituras de sensores agrícolas e ambientais, processar esses dados e disponibilizá-los para consulta por meio de uma API REST.

O foco do projeto é representar telemetria de sensores, e não cadastro fixo de dispositivos. Cada registro salvo no banco representa uma leitura recebida de um sensor em determinado momento.

O sistema foi construído seguindo os princípios de arquitetura limpa, separando responsabilidades entre as camadas de Domínio, Aplicação, Infraestrutura e Web.

## Objetivo do Projeto

#### Uma API para:

- receber dados de sensores IoT enviados via HTTP pelo Node-RED;
- validar e processar essas leituras;
- armazenar os dados em banco Oracle;
- disponibilizar consultas com filtros, ordenação e paginação;
- expor informações para uso futuro em dashboards e monitoramento;
- implementar observabilidade, health checks, logs e testes automatizados.

## Escopo

- API REST para CRUD de leituras de sensores;
- endpoint de busca com paginação, filtros e ordenação;
- health checks da aplicação e do banco;
- logging estruturado com Serilog;
- tracing e métricas com OpenTelemetry;
- testes unitários e de integração;
- documentação de uso do sistema.


## Requisitos Funcionais

- Receber leituras de sensores em lote via HTTP.
- Validar lista de sensores recebida.
- Validar nome do sensor.
- Validar tipo do sensor.
- Converter o campo Type recebido como string numérica para inteiro.
- Armazenar leituras no banco de dados.
- Permitir consulta de todas as leituras.
- Permitir consulta por identificador.
- Permitir atualização de leitura.
- Permitir remoção de leitura.
- Permitir busca com:
  - filtro por nome;
  - filtro por tipo;
  - filtro por valor mínimo;
  - filtro por valor máximo;
  - filtro por intervalo de datas;
  - paginação;
  - ordenação ascendente e descendente.
- Expor links HATEOAS nos retornos da API.
- Expor endpoints de health check.

## Requisitos Não Funcionais

- Aplicação desenvolvida em .NET 8.
- Arquitetura baseada em Clean Architecture.
- Persistência com Entity Framework Core e Oracle.
- Documentação da API com Swagger.
- Logging estruturado com Serilog.
- Observabilidade com OpenTelemetry.
- Testes automatizados com xUnit, Moq, FluentAssertions e WebApplicationFactory.
- Organização em projetos separados para testes unitários e integração.
- Nomenclatura padronizada de testes no formato:
  - MetodoTestado_Cenario_ResultadoEsperado

---

## Arquitetura da Solução

A aplicação foi estruturada com Clean Architecture, separando responsabilidades em camadas.

### 1. Domínio

Responsável pelas entidades e contratos centrais do negócio.

Contém:
- Sensor
- BaseEntity
- IRepository<T>
- ISensorRepository
- SensorType

### 2. Aplicação

Responsável pelos casos de uso, regras de negócio, DTOs e serviços.

Contém:
- SensorService
- ISensorService
- SensorDTO
- SensorSearchDTO
- PagedResultDTO<T>
- LinkDTO
- DomainException

### 3. Infraestrutura

Responsável pela persistência e acesso a dados.

Contém:
- AgroTechDbContext
- Repository<T>
- SensorRepository
- Migrations do EF Core

### 4. Web

Responsável por expor a API e recursos HTTP.

Contém:
- SensorsController
- middleware de tratamento de exceções
- configuração de Swagger
- health checks
- Serilog
- OpenTelemetry
- rotas MVC e API

---

## Estrutura do Projeto

```text
AgroTech/
├── AgroTech/
│   ├── Program.cs
│   ├── Program.Public.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Migrations/
│   ├── logs/
│   ├── src/
│   │   ├── Application/
│   │   │   ├── DTOs/
│   │   │   ├── Exceptions/
│   │   │   ├── Interfaces/
│   │   │   └── Services/
│   │   ├── Domain/
│   │   │   ├── Common/
│   │   │   ├── Entities/
│   │   │   ├── Enums/
│   │   │   └── Interfaces/
│   │   ├── Infrastructure/
│   │   │   ├── Data/
│   │   │   └── Repositories/
│   │   └── Web/
│   │       ├── Controllers/
│   │       ├── Middleware/
│   │       └── Views/
│   └── README.md
│
├── AgroTech.UnitTests/
│   └── Services/
│       └── SensorServiceTests.cs
│
└── AgroTech.IntegrationTests/
    ├── CustomWebApplicationFactory.cs
    └── Api/
        ├── HealthChecksIntegrationTests.cs
        └── SensorsIntegrationTests.cs
```

## Entidade de Negócio
### Sensor

A entidade Sensor representa uma leitura de telemetria.

#### Campos principais:

- Id
- Name
- Type
- Value
- Timestamp
- CreatedAt
- UpdatedAt

### Importante: 
- O sistema não trabalha com cadastro fixo de sensor.

- Cada linha salva no banco representa uma nova leitura recebida.

### Fluxo IoT com Node-RED

-> O Node-RED envia dados para a API por HTTP no formato JSON.

#### Exemplo de payload
```
[
  {
    "name": "Temperatura",
    "type": "1",
    "value": 25.4,
    "timestamp": "2026-04-07T10:00:00Z"
  },
  {
    "name": "Umidade",
    "type": "2",
    "value": 60,
    "timestamp": "2026-04-07T10:05:00Z"
  }
]
```

### Fluxo
1. Node-RED envia uma lista de leituras para POST /api/sensors

2. A API valida os dados recebidos

3. O serviço converte DTO para entidade

4. Os registros são persistidos no Oracle

5. Os dados ficam disponíveis para consulta via endpoints REST

#### Tecnologias Utilizadas
- .NET 8
- ASP.NET Core Web API / MVC
- Entity Framework Core
- Oracle Entity Framework Core Provider
- Swagger / Swashbuckle
- Serilog
- OpenTelemetry
- xUnit
- Moq
- FluentAssertions
- Microsoft.AspNetCore.Mvc.Testing
- EntityFrameworkCore.InMemory (somente nos testes de integração)

## Endpoints da API

### Base URL local
```
http://localhost:5081
```
### Swagger
```
http://localhost:5081/swagger
```
### 1. Listar todos os sensores

```
GET /api/sensors
```

Retorna todas as leituras cadastradas.

2. Buscar sensor por ID

```
GET /api/sensors/{id}
```

#### Exemplo:
```
GET /api/sensors/11111111-1111-1111-1111-111111111111
```
3. Criar sensores em lote
```
POST /api/sensors
```

#### Exemplo de body
```
[
  {
    "name": "Temperatura",
    "type": "1",
    "value": 25.4,
    "timestamp": "2026-04-07T10:00:00Z"
  },
  {
    "name": "Umidade",
    "type": "2",
    "value": 60,
    "timestamp": "2026-04-07T10:05:00Z"
  }
]
```
4. Atualizar sensor
```
PUT /api/sensors/{id}
```

#### Exemplo de body

```
{
  "id": "11111111-1111-1111-1111-111111111111",
  "name": "Temperatura Atualizada",
  "type": "1",
  "value": 28.9,
  "timestamp": "2026-04-07T11:00:00Z"
}
```

5. Remover sensor
```
DELETE /api/sensors/{id}
```

#### Exemplo:
```
DELETE /api/sensors/11111111-1111-1111-1111-111111111111
```
6. Buscar sensores com filtros, paginação e ordenação
```
GET /api/sensors/search
```
- Parâmetros suportados
- name
- type
- minValue
- maxValue
- startTimestamp
- endTimestamp
- orderBy
- direction
- page
- pageSize

#### Exemplo

```
GET /api/sensors/search?name=temp&type=1&page=1&pageSize=10&orderBy=timestamp&direction=desc
```
### HATEOAS

Os DTOs expostos pela API incluem links HATEOAS com:

- self
- update
- delete
- search

#### Exemplo
```
{
  "id": "11111111-1111-1111-1111-111111111111",
  "name": "Temperatura",
  "type": "1",
  "value": 25.4,
  "timestamp": "2026-04-07T10:00:00Z",
  "links": [
    {
      "rel": "self",
      "href": "/api/sensors/11111111-1111-1111-1111-111111111111",
      "method": "GET"
    },
    {
      "rel": "update",
      "href": "/api/sensors/11111111-1111-1111-1111-111111111111",
      "method": "PUT"
    },
    {
      "rel": "delete",
      "href": "/api/sensors/11111111-1111-1111-1111-111111111111",
      "method": "DELETE"
    },
    {
      "rel": "search",
      "href": "/api/sensors/search",
      "method": "GET"
    }
  ]
}
```
## Health Checks

A aplicação possui endpoints de health check para monitoramento.

### Endpoints disponíveis

- GET /health
- GET /health/live
- GET /health/ready

### O que é verificado

- saúde da própria API (self)
- conectividade com o banco (oracle)
- disponibilidade de serviço externo configurado (external_service)

### Como monitorar a aplicação

- /health: visão completa da saúde da aplicação
- /health/live: verifica se a API está viva
- /health/ready: verifica se a API está pronta para uso, incluindo dependências

Status possíveis:

- Healthy: funcionamento normal
- Degraded: funcionamento parcial ou configuração ausente
- Unhealthy: falha de dependência ou indisponibilidade

### Exemplo de resposta

```json
{
  "status": "Healthy",
  "totalDurationMs": 120,
  "checks": [
    {
      "name": "self",
      "status": "Healthy",
      "description": "API está saudável",
      "durationMs": 0.8
    },
    {
      "name": "oracle",
      "status": "Healthy",
      "description": "Healthy",
      "durationMs": 21.4
    },
    {
      "name": "external_service",
      "status": "Healthy",
      "description": "Serviço externo disponível. StatusCode: 200",
      "durationMs": 12.5
    }
  ]
}
```


### Logging Estruturado

O projeto utiliza Serilog com saída para:

- Console
- Arquivo

#### Níveis utilizados

- Information
- Warning
- Error

#### Onde há logs

- controllers
- middleware de tratamento de exceções
- inicialização da aplicação
- logs automáticos de requisição HTTP

#### Correlação de requisições

A API utiliza o header X-Correlation-ID para correlacionar requisições e logs.

Se o cliente enviar X-Correlation-ID, esse valor será reutilizado.
Caso não envie, a aplicação gera um identificador automaticamente.

O CorrelationId é incluído:

- nos logs do Serilog
- no header da resposta
- nas respostas de erro tratadas pelo middleware

#### Diretório de logs

AgroTech/logs/

### Observabilidade com OpenTelemetry

A aplicação utiliza OpenTelemetry para tracing e métricas.

#### Tracing configurado para
- ASP.NET Core
- HttpClient
- Entity Framework Core
#### Métricas configuradas para
- runtime
- ASP.NET Core
- HttpClient
#### Export atual
- Console exporter

### Testes Automatizados

O projeto possui testes organizados em dois projetos separados.

1. Testes Unitários

#### Projeto:
```
AgroTech.UnitTests
```

Cobrem principalmente:

- camada de Domínio
- camada da Aplicação
- validações do SensorService
- cenários felizes e cenários de erro

2. Testes de Integração

#### Projeto:
```
AgroTech.IntegrationTests
```

#### Cobrem:

- health checks
- endpoints REST da API
- busca
- criação
- atualização
- remoção
- fluxo HTTP completo com WebApplicationFactory

#### Os testes de integração utilizam:
- WebApplicationFactory
- CustomWebApplicationFactory
- Collection Fixture
- EntityFrameworkCore.InMemory

#### Resultado atual
- 26 testes unitários
- 13 testes de integração

Total:

- 39 testes automatizados aprovados

### Como executar somente os testes

#### Rodar todos os testes
```bash
dotnet test
```
#### Roder somente testes unitários
```bash
dotnet test ./AgroTech.UnitTests
```
#### Rodar somente integração
```bash
dotnet test ./AgroTech.IntegrationTests
```

#### Como Executar o Projeto
1. Clonar o repositório

```bash
git clone <URL_DO_REPOSITORIO>
cd AgroTech
```
2. Restaurar dependências
```bash
dotnet restore
```
3. Configurar a connection string

No arquivo `AgroTech/appsettings.json` ou `AgroTech/appsettings.Development.json`, configure a connection string Oracle:

```
{
  "ConnectionStrings": {
    "AgroTechOracle": "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=SEU_ORACLE"
  }
}
```
4. Executar a aplicação
```bash
dotnet run --project ./AgroTech
```
5. Acessar no navegador
- API: http://localhost:5081
- Swagger: http://localhost:5081/swagger
- Health: http://localhost:5081/health

### Como Executar as Migrations
#### Criar migration
```bash
dotnet ef migrations add NomeDaMigration --project ./AgroTech
```
#### Aplicar migration
```bash
dotnet ef database update --project ./AgroTech
```
### Migration atual

O projeto já possui migration inicial para a tabela de sensores.

### Exemplos de requisição

#### Criar sensores
```bash
curl -X POST "http://localhost:5081/api/sensors" \
  -H "Content-Type: application/json" \
  -d '[
    {
      "name": "Temperatura",
      "type": "1",
      "value": 25.4,
      "timestamp": "2026-04-07T10:00:00Z"
    },
    {
      "name": "Umidade",
      "type": "2",
      "value": 60,
      "timestamp": "2026-04-07T10:05:00Z"
    }
  ]'
  ```

#### Buscar todos
```bash
curl "http://localhost:5081/api/sensors"
```
#### Buscar por ID
```bash
curl "http://localhost:5081/api/sensors/11111111-1111-1111-1111-111111111111"
```
#### Buscar com filtro
```bash
curl "http://localhost:5081/api/sensors/search?name=temp&page=1&pageSize=10"
```
#### Health
```bash
curl "http://localhost:5081/health"
curl "http://localhost:5081/health/live"
curl "http://localhost:5081/health/ready"
```

## Observação Final
Essa API é parte componente do projeto Agrotech. 
Idealizado como uma solução para pequenos e médios agricultores, o Agrotech visa facilitar o manejo e plantio em zonas rurais, permitindo que usuários cadastrem suas propriedades e implementem sensores IoT diretamente em campo, realizando assim a coleta e análise de informações para tomada de decisões estratégicas na produção. 

Componentes do projeto:
API .net realiza a integração de sensores IoT via Node-Red por protocolo MQTT ao Oracle.


Fluxo 1: 
```
-> Sensores -> Broker MQTT -> Node-Red (conversão para HTTP) -> API .net -> Banco de dados Oracle -> Oracle 23ai -> LLM Retorna sugestões de manejo ao agricultor (predição) -> Dashboard Exibido no frontend React ao user
```  
Fluxo 2:
```
-> Frontend permite CRUD de informações (Cadastro de Usuário, Propriedade, Campo, etc) -> API Spring Boot Java recebe os dados -> Envia ao Banco Oracle 
```





