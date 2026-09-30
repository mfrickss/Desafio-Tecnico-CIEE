# Desafio Técnico CIEE — Cadastro e Triagem de Currículos

Aplicação fullstack desenvolvida para a equipe de recrutamento cadastrar e consultar candidatos, oferecendo suporte a **cadastro manual** e **cadastro inteligente com extração de currículos em PDF**.

---

## 🛠️ Tecnologias Utilizadas

- **Backend:** ASP.NET Core (.NET 8 LTS) com Controllers organizados
- **Banco de Dados:** Microsoft SQL Server 2022 (Linux Container via Docker)
- **ORM & Migrations:** Entity Framework Core 8.0.11
- **Parser de PDF:** PdfPig 0.1.9 (100% C# gerenciado, sem dependências nativas)
- **Validação:** FluentValidation 11.3.0
- **Documentação de API:** Swagger / OpenAPI
- **Frontend:** Angular 18/22 com Standalone Components (sem NgModules)
- **Estilização:** Tailwind CSS 3.4
- **Testes Automatizados:** xUnit com 11 testes unitários

---

## 📋 Funcionalidades

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

## 🚀 Como Executar o Projeto

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+ ou 24+](https://nodejs.org/)
- [Docker e Docker Compose](https://www.docker.com/)

---

### Passo 1: Subir o Banco de Dados (SQL Server)

Na raiz do repositório, execute:

`ash
docker compose up -d
`

O container ciee-sqlserver subirá na porta padrão 1433.

---

### Passo 2: Executar o Backend (.NET 8 API)

Navegue até a pasta da API e execute:

`ash
cd Ciee.Curriculos.Api
dotnet run --urls "http://localhost:5004"
`

> **Nota:** Na primeira inicialização, a API executa automaticamente as migrations do Entity Framework Core e cria o banco CieeCurriculosDb e suas tabelas no SQL Server.

- **URL da API / Swagger:** [http://localhost:5004](http://localhost:5004)

Caso deseje rodar a suíte de testes automatizados do backend:
`ash
dotnet test Ciee.Curriculos.Tests/Ciee.Curriculos.Tests.csproj
`

---

### Passo 3: Executar o Frontend (Angular)

Em outro terminal, acesse a pasta do frontend e inicie o servidor de desenvolvimento:

`ash
cd ciee-curriculos-web
npm install
npm start
`

- **Acesse a aplicação no navegador:** [http://localhost:4200](http://localhost:4200)

---

## 📁 Estrutura do Repositório

`	ext
├── Ciee.Curriculos.Api/              # Backend ASP.NET Core (.NET 8)
│   ├── Controllers/                  # CandidatosController (rotas RESTful)
│   ├── Data/                         # AppDbContext e migrations do EF Core
│   │   └── script-inicial-banco.sql  # Script SQL de criação do banco
│   ├── DTOs/                         # DTOs de entrada e resposta
│   ├── Models/                       # Entidade Candidato
│   ├── Services/                     # PdfExtractionService (PdfPig + Regex)
│   ├── Validators/                   # Regras de negócio com FluentValidation
│   └── Program.cs                    # Configurações de DI, CORS e Swagger
│
├── Ciee.Curriculos.Tests/            # Testes unitários com xUnit
│   ├── CriarCandidatoDtoValidatorTests.cs
│   └── PdfExtractionServiceTests.cs
│
├── ciee-curriculos-web/              # Frontend Angular Standalone
│   ├── src/app/
│   │   ├── models/                   # Interfaces TypeScript
│   │   ├── services/                 # CandidatoService (HttpClient)
│   │   └── pages/
│   │       ├── cadastro/             # Formulário unificado + upload PDF
│   │       ├── listagem/             # Tabela com busca e status
│   │       └── detalhes/             # Visualização de perfil
│
├── docker-compose.yml                # Configuração do SQL Server 2022
├── DESENVOLVIMENTO.md                # Registro completo do uso de IA e decisões
└── README.md                         # Documentação e instruções de execução
`

---

## 📄 Registro do Uso de Inteligência Artificial

Para detalhes aprofundados sobre como a Inteligência Artificial participou do desenvolvimento (perguntas feitas, código aproveitado, o que foi corrigido e descartado, tempos e limitações), consulte o arquivo [DESENVOLVIMENTO.md](DESENVOLVIMENTO.md).
