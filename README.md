# Desafio Técnico CIEE — Cadastro e Triagem de Currículos

Aplicação fullstack desenvolvida para a equipe de recrutamento cadastrar e consultar candidatos, oferecendo suporte a **cadastro manual** e **cadastro inteligente com extração de currículos em PDF**.

---

## Tecnologias Utilizadas

- **Backend:** ASP.NET Core (.NET 8 LTS)
- **Banco de Dados:** Microsoft SQL Server 2022 (Linux Container via Docker)
- **ORM & Migrations:** Entity Framework Core 8.0.11
- **Parser de PDF:** PdfPig 0.1.9
- **Validação:** FluentValidation 11.3.0
- **Documentação de API:** Swagger / OpenAPI
- **Frontend:** Angular 18/22 com Standalone Components
- **Estilização:** Tailwind CSS 3.4
- **Testes Automatizados:** xUnit

---

## Funcionalidades

1. **Cadastro com Leitura de PDF (Opcional):**
   - Upload de arquivo PDF (arrastar e soltar ou clique) com validação de formato e tamanho máximo de 5 MB.
   - O backend extrai o texto do PDF e identifica automaticamente **Nome Completo**, **E-mail**, **Telefone**, **Cargo de Interesse** e **Resumo Profissional**.
   - As informações encontradas preenchem o formulário automaticamente para conferência e edição antes de salvar.
   - Mensagens claras de alerta para arquivos inválidos ou PDFs sem camada legível de texto (o cadastro manual nunca é bloqueado).

2. **Cadastro Manual:**
   - Formulário reativo unificado com validação em tempo real de campos obrigatórios (Nome e E-mail válido).

3. **Listagem de Candidatos:**
   - Tabela responsiva com busca em tempo real por nome, e-mail ou cargo.
   - Identificação visual clara da origem do cadastro (Manual ou com Leitura de PDF).
   - Data e hora formatadas do cadastro.

4. **Tela de Detalhes:**
   - Visualização completa das informações do candidato, incluindo resumo profissional e dados de contato.

---

## Como Executar o Projeto

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+ ou 24+](https://nodejs.org/)
- [Docker e Docker Compose](https://www.docker.com/)

---

### Passo 1: Subir o Banco de Dados (SQL Server)

Na raiz do repositório, execute:

```bash
docker compose up -d
```

O container ciee-sqlserver subirá na porta padrão 1433.

---

### Passo 2: Executar o Backend (.NET 8 API)

Navegue até a pasta da API e execute:

```bash
cd Ciee.Curriculos.Api
dotnet run --urls "http://localhost:5004"
```

> **Nota:** Na primeira inicialização, a API executa automaticamente as migrations do Entity Framework Core e cria o banco CieeCurriculosDb e suas tabelas no SQL Server.

- **URL da API / Swagger:** [http://localhost:5004](http://localhost:5004)

Caso deseje rodar a suíte de testes automatizados do backend:
```bash
dotnet test Ciee.Curriculos.Tests/Ciee.Curriculos.Tests.csproj
```

---

### Passo 3: Executar o Frontend (Angular)

Em outro terminal, acesse a pasta do frontend e inicie o servidor de desenvolvimento:

```bash
cd ciee-curriculos-web
npm install
npm start
```

- **Acesse a aplicação no navegador:** [http://localhost:4200](http://localhost:4200)

---

## Estrutura do Repositório

```text
Desafio-Tecnico-CIEE/
│
├── .gitignore                      # Regras globais de exclusão (.NET, Angular, logs e IDEs)
├── AGENTS.md                       # Diretrizes de arquitetura, padrões e papéis dos agentes
├── DESENVOLVIMENTO.md              # Documentação técnica do ciclo de desenvolvimento
├── README.md                       # Documentação principal e instruções de execução
├── docker-compose.yml              # Orquestração do banco SQL Server 2022 em container
│
├── Ciee.Curriculos.Api/            # Backend ASP.NET Core Web API (.NET 8)
│   ├── Ciee.Curriculos.Api.csproj  # Dependências (EF Core, SqlServer, PdfPig, FluentValidation)
│   ├── Program.cs                  # Configuração de DI, Middlewares, CORS e Swagger
│   ├── appsettings.json            # Connection strings e configurações gerais
│   ├── Controllers/                # Controllers finos REST (ex: CandidatosController.cs)
│   ├── Data/                       # DbContext (EF Core) e scripts SQL de inicialização
│   ├── DTOs/                       # Contratos de entrada e saída (Requests/Responses)
│   ├── Migrations/                 # Histórico de versionamento do schema do banco
│   ├── Models/                     # Entidades de domínio (ex: Candidato.cs)
│   ├── Services/                   # Lógica de negócio e extração de PDF (PdfExtractionService)
│   └── Validators/                 # Regras de validação com FluentValidation
│
├── Ciee.Curriculos.Tests/          # Suíte de Testes Automatizados (.NET 8 / xUnit)
│   ├── Ciee.Curriculos.Tests.csproj
│   ├── CriarCandidatoDtoValidatorTests.cs
│   ├── GeradorPdfParaTesteApi.cs
│   └── PdfExtractionServiceTests.cs
│
└── ciee-curriculos-web/            # Frontend Angular 18 (Standalone Components + Tailwind CSS)
    ├── package.json                # Dependências do ecossistema Node/Angular
    ├── angular.json                # Configurações de build e workspace Angular CLI
    ├── tailwind.config.js          # Configuração do framework de estilização
    ├── tsconfig.json               # Configurações do compilador TypeScript
    └── src/
        ├── index.html              # Ponto de entrada HTML
        ├── main.ts                 # Bootstrap da aplicação Angular
        ├── styles.css              # Estilos globais e diretivas do Tailwind
        └── app/
            ├── app.config.ts       # Provedores de injeção globais (HttpClient, Router)
            ├── app.routes.ts       # Mapeamento de rotas da aplicação
            ├── models/             # Interfaces e tipagens TypeScript
            ├── services/           # Serviços HTTP injetáveis (CandidatoService)
            └── pages/              # Componentes de páginas standalone
                ├── cadastro/       # Formulário com upload de PDF e cadastro manual
                ├── listagem/       # Tabela de triagem, filtros e ações
                └── detalhes/       # Visualização detalhada do currículo
```

---

## Registro do Uso de Inteligência Artificial

Para detalhes aprofundados sobre como a Inteligência Artificial participou do desenvolvimento (perguntas feitas, código aproveitado, o que foi corrigido e descartado, tempos e limitações), consulte o arquivo [DESENVOLVIMENTO.md](DESENVOLVIMENTO.md).
