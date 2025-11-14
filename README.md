# Sistema de Gestão de Notas Fiscais - Desenvolvido por Ícaro C. Pavan

O objetivo desse projeto é realizar a construção de um serviço de gestão de notas fiscais simples para o programa de estágio da Korp ERP.

## 📋 Sobre o Projeto

Este sistema permite o gerenciamento completo de produtos em estoque e emissão de notas fiscais, com controle automático de disponibilidade e baixa de estoque.

### Tecnologias Utilizadas

**Backend:**
- .NET 9.0 (C#)
- ASP.NET Core Minimal APIs
- Entity Framework Core
- PostgreSQL 16
- YARP (Yet Another Reverse Proxy)
- Polly (Resiliência e Circuit Breaker)

**Frontend:**
- Angular 19
- Angular Material
- RxJS
- TypeScript

**Infraestrutura:**
- Docker & Docker Compose
- Nginx

## 🚀 Como Executar o Projeto

### Pré-requisitos

Certifique-se de ter instalado em sua máquina:

- [Docker](https://docs.docker.com/get-docker/) (versão 20.10 ou superior)
- [Docker Compose](https://docs.docker.com/compose/install/) (versão 2.0 ou superior)

### Passo a Passo

#### 1. Clone o repositório

```bash
git clone https://github.com/icaropvn/Korp_Teste_IcaroPavan.git
cd Korp_Teste_IcaroPavan
```

#### 2. Estrutura de pastas esperada

Certifique-se de que seu projeto está organizado da seguinte forma:

```
projeto/
├── docker/
│   ├── docker-compose.yml
│   └── .env
├── estoque-api/
│   ├── Dockerfile
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Production.json
│   └── ...
├── faturamento-api/
│   ├── Dockerfile
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Production.json
│   └── ...
├── gateway/
│   ├── Dockerfile
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Production.json
│   └── ...
└── frontend/
    ├── Dockerfile
    ├── nginx.conf
    ├── angular.json
    └── ...
```

#### 3. Configure as variáveis de ambiente

Utilize o padrão do arquivo `.env.example` e crie um `.env` dentro da pasta `/infra` com os seguintes valores:

```env
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_VERSION=16

ESTOQUE_DB=estoque_db
FATURAMENTO_DB=faturamento_db
```

#### 4. Navegue até a pasta docker

```bash
cd docker
```

#### 5. Construa e inicie os containers

```bash
docker-compose up --build
```

> **Nota:** A primeira execução pode levar alguns minutos, pois o Docker precisará baixar as imagens base e construir todos os serviços.

#### 6. Aguarde a inicialização completa

Você saberá que está tudo pronto quando ver mensagens similares a estas nos logs:

```
estoque-api      | Now listening on: http://0.0.0.0:8080
faturamento-api  | Now listening on: http://0.0.0.0:8080
gateway          | Now listening on: http://0.0.0.0:8080
frontend         | /docker-entrypoint.sh: Launching /docker-entrypoint.d/30-tune-worker-processes.sh
```

#### 7. Acesse a aplicação

Abra seu navegador e acesse:

```
http://localhost:4200/produtos
ou
http://localhost:4200/notas
```

## 🔍 Endpoints da API

### API de Estoque (via Gateway: `/api/estoque`)

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/produtos` | Lista todos os produtos |
| GET | `/produtos/{id}` | Obtém um produto específico |
| POST | `/produtos` | Cria um novo produto |
| PUT | `/produtos/{id}` | Atualiza um produto |
| DELETE | `/produtos/{id}` | Remove um produto |
| GET | `/produtos/{id}/disponibilidade` | Verifica disponibilidade de estoque |
| POST | `/produtos/baixas` | Realiza baixa em lote de produtos |

### API de Faturamento (via Gateway: `/api/faturamento`)

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/notas` | Lista todas as notas fiscais |
| GET | `/notas/{id}` | Obtém uma nota específica |
| POST | `/notas` | Cria uma nova nota |
| PUT | `/notas/{id}` | Atualiza uma nota (apenas abertas) |
| DELETE | `/notas/{id}` | Remove uma nota (apenas abertas) |
| POST | `/notas/{id}/impressao` | Imprime nota e baixa estoque |

## 🐳 Portas Utilizadas

| Serviço | Porta Externa | Porta Interna |
|---------|---------------|---------------|
| Frontend | 4200 | 80 |
| Gateway | 5000 | 8080 |
| Estoque API | 5001 | 8080 |
| Faturamento API | 5002 | 8080 |
| Estoque DB | 5433 | 5432 |
| Faturamento DB | 5434 | 5432 |

## 📝 Observações

- As notas fiscais só podem ser editadas ou excluídas enquanto estiverem com status "Aberta"
- Ao imprimir uma nota, o estoque é automaticamente baixado e a nota não pode mais ser alterada
- O sistema possui validação de estoque em tempo real antes de criar ou atualizar notas
- Códigos de produtos são gerados automaticamente no formato `P00001`, `P00002`, etc.

## 📄 Licença

Este projeto foi desenvolvido para fins de avaliação para uma vaga de estágio.

---

**Desenvolvido com ❤️ por Ícaro Costa Pavan**