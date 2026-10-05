# CIEE - Sistema de Triagem e Cadastro Inteligente de Currículos

Sistema fullstack desenvolvido para otimizar o processo de triagem e cadastro de candidatos a estágios e vagas do CIEE, contando com módulo inteligente de extração de dados a partir de arquivos PDF, validações contratuais com FluentValidation, arquitetura desacoplada e interface moderna e reativa.

---

## 1. Tecnologias e Versões Exatas

A tabela abaixo consolida todas as tecnologias, ferramentas e versões exatas em operação no projeto:

| Camada / Componente | Tecnologia / Ferramenta | Versão Exata | Finalidade no Projeto |
| :--- | :--- | :--- | :--- |
| **Backend Runtime** | .NET SDK | `8.0.408` | Plataforma de desenvolvimento, compilação e execução |
| **Web Framework** | ASP.NET Core Web API | `8.0` | Exposição de endpoints RESTful com controllers organizados |
| **ORM / Banco** | Entity Framework Core (SQL Server) | `8.0.11` | Mapeamento objeto-relacional, consultas assíncronas e Migrations |
| **Extração de PDF** | UglyToad.PdfPig | `0.1.9` | Leitura gerenciada de PDFs, extração de texto e análise espacial |
| **Validação** | FluentValidation.AspNetCore | `11.3.0` | Validações desacopladas de DTOs com regras de negócio |
| **Documentação API** | Swashbuckle (Swagger UI) | `6.6.2` | Catálogo interativo OpenAPI exposto na rota `/swagger` |
| **Testes Backend** | xUnit | `2.5.3` | Framework de testes unitários e de integração com asserções |
| **Test Runner** | Microsoft.NET.Test.Sdk | `17.8.0` | Executor integrado para comando `dotnet test` |
| **Frontend Framework** | Angular | `22.2.0` | Interface SPA baseada em Standalone Components e Signals |
| **Linguagem Frontend** | TypeScript | `6.0.2` | Tipagem estática rigorosa para componentes, serviços e modelos |
| **Estilização** | Tailwind CSS / PostCSS / Autoprefixer | `3.4.17` | Utility-first CSS responsivo estilizado na paleta corporativa |
| **Gerenciador de Pacotes** | npm | `12.0.2` | Resolução de dependências e scripts do frontend |
| **Banco de Dados** | Microsoft SQL Server 2022 | `2022-latest` | Banco relacional oficial (`mcr.microsoft.com/mssql/server:2022-latest`) |
| **Orquestração Local** | Docker & Docker Compose | Compose v2 | Provisionamento isolado do container do SQL Server em rede própria |

---

## 2. Arquitetura da Solução

O repositório está organizado nas seguintes pastas e responsabilidades:

```text
Desafio-Tecnico-CIEE/
├── Ciee.Curriculos.Api/              # Backend ASP.NET Core Web API (.NET 8)
│   ├── Common/                      # Helpers de sanitização, formatação e regexes seguros
│   ├── Controllers/                 # Controllers REST (CandidatosController)
│   ├── Data/                        # AppDbContext, configurações EF e script-inicial-banco.sql
│   ├── DTOs/                        # Contratos de entrada e saída (Records imutáveis)
│   ├── Migrations/                  # Histórico versionado de migrações EF Core
│   ├── Models/                      # Entidades de domínio (Candidato)
│   ├── Services/                    # Motor de extração (PdfExtractionService)
│   └── Validators/                  # Validações fluentes (CriarCandidatoDtoValidator)
├── Ciee.Curriculos.Tests/            # Suíte de testes unitários xUnit (86 testes aprovados)
│   ├── CandidatosControllerTests.cs # Testes de upload, extensões, limites e magic bytes
│   ├── PdfExtractionServiceTests.cs # Testes de extração, diacríticos, telefones e cargos
│   └── GeradorPdfParaTesteApi.cs    # Utilitário para geração de PDFs em memória/disco
├── ciee-curriculos-web/              # Frontend Angular 22 (Standalone Components)
│   ├── src/app/pages/               # Componentes inteligentes (Cadastro, Listagem, Detalhes)
│   ├── src/app/services/            # Serviços de integração HTTP (CandidatoService)
│   └── src/app/models/              # Interfaces tipadas TypeScript
├── curriculo_ficticio.pdf           # Fixture permanente estruturado para demonstração e testes
├── docker-compose.yml               # Configuração do container SQL Server 2022
├── README.md                        # Guia oficial de inicialização e documentação
└── AGENTS.md                        # Diretrizes alinhadas da stack para desenvolvimento
```

---

## 3. Instruções de Inicialização

### 3.1. Pré-requisitos
- .NET 8 SDK instalado (`dotnet --version` >= 8.0.x)
- Node.js (versão 20.x ou superior) e npm
- Docker e Docker Compose instalados e em execução

---

### 3.2. Passo 1: Inicialização do Banco de Dados com Docker
O banco de dados oficial é o **Microsoft SQL Server 2022**, provisionado via Docker Compose:

```powershell
docker compose up -d
```

O container inicializa com as seguintes configurações padrão:
- **Porta:** `1433`
- **Usuário:** `sa`
- **Senha:** `CieeDesafio@2026!` (ou configurável via variável de ambiente `MSSQL_SA_PASSWORD`)
- **Healthcheck:** Verificação ativa a cada 10 segundos utilizando `/opt/mssql-tools18/bin/sqlcmd`

Para verificar o status do container:
```powershell
docker compose ps
```

---

### 3.3. Passo 2: Execução do Backend (.NET 8 Web API)
Ao iniciar, o backend aplica automaticamente as migrações pendentes no SQL Server através de `context.Database.Migrate()` em seu startup:

```powershell
cd Ciee.Curriculos.Api
dotnet run
```

- A API estará disponível em: `http://localhost:5000` ou `https://localhost:5001`
- A interface interativa do Swagger UI estará acessível em: `http://localhost:5000/swagger`

---

### 3.4. Passo 3: Execução do Frontend (Angular 22)
Em um novo terminal, instale as dependências e inicie o servidor de desenvolvimento:

```powershell
cd ciee-curriculos-web
npm install
npm start
```

- A aplicação web estará acessível no navegador em: `http://localhost:4200`

---

## 4. Estrutura e Estratégia do Script SQL

O projeto disponibiliza duas estratégias complementares de gestão de esquema de banco de dados:

1. **Aplicação Automática via EF Core (Padrão de Desenvolvimento):**
   - Configurada no `Program.cs` através de `db.Database.Migrate()`.
   - Cria o banco `CieeCurriculosDb`, tabela de histórico `__EFMigrationsHistory`, a tabela `Candidatos` e o índice de busca por e-mail `IX_Candidatos_Email` de forma transparente na primeira execução.

2. **Script SQL Idempotente Manual (`script-inicial-banco.sql`):**
   - Localizado em `Ciee.Curriculos.Api/Data/script-inicial-banco.sql`.
   - Gera todas as instruções DDL protegidas por verificações condicionais `IF NOT EXISTS` sobre a tabela `__EFMigrationsHistory`.
   - Pode ser executado repetidas vezes em qualquer ambiente de banco (SQL Server corporativo, Azure SQL ou Docker) sem risco de falha por objetos duplicados.

### Estrutura da Tabela `Candidatos`:
```sql
CREATE TABLE [Candidatos] (
    [Id] uniqueidentifier NOT NULL,
    [NomeCompleto] nvarchar(150) NOT NULL,
    [Email] nvarchar(150) NOT NULL,
    [Telefone] nvarchar(30) NULL,
    [CargoInteresse] nvarchar(100) NULL,
    [ResumoProfissional] nvarchar(2000) NULL,
    [DataCadastro] datetime2 NOT NULL,
    [TeveOrigemPdf] bit NOT NULL,
    CONSTRAINT [PK_Candidatos] PRIMARY KEY ([Id])
);

CREATE INDEX [IX_Candidatos_Email] ON [Candidatos] ([Email]);
```

---

## 5. Comandos de Execução e Testes

### 5.1. Testes Automatizados do Backend (xUnit)
O projeto conta com suíte de testes com **86 testes automatizados**, cobrindo validações de DTOs, serialização UTC, helpers de telefone e todos os casos de borda do motor de PDF:

```powershell
# Execução direta com cache de restore local
dotnet test .\Ciee.Curriculos.Tests\Ciee.Curriculos.Tests.csproj --no-restore

# Execução padrão completa
dotnet test .\Ciee.Curriculos.Tests\Ciee.Curriculos.Tests.csproj
```

### 5.2. Testes Automatizados do Frontend (Angular)
A suíte de testes unitários do frontend valida formulários reativos, serviços HTTP e componentes:

```powershell
cd ciee-curriculos-web
npm test
```

---

## 6. Documentação Detalhada das Limitações Técnicas da Extração de PDF

O motor de extração de currículos foi desenvolvido sobre a biblioteca `UglyToad.PdfPig` (biblioteca 100% C# gerenciada), combinada com agrupamento espacial no plano cartesiano da página e heurísticas sintáticas com expressões regulares seguras (timeout de 1s para prevenção de ReDoS).

Com base na arquitetura atual do código em `PdfExtractionService.cs`, destacam-se as seguintes características e limitações:

### 6.1. Ausência de OCR em PDFs Digitalizados ou Escaneados
- **Como funciona:** O `PdfPig` analisa o stream vetorial e textual interno do PDF (`page.GetWords()`), capturando caracteres que possuem glifos e mapeamentos de fontes incorporados.
- **Limitação:** Currículos digitalizados via scanner ou salvos puramente como imagens (JPG/PNG inseridos no PDF sem camada de texto OCR associada) não possuem objetos de texto legíveis.
- **Tratamento no Sistema:** Quando `linhasExtraidas.Count == 0`, o backend identifica a ausência de texto vetorial e retorna graciosamente:
  > *"Não foi possível extrair texto do PDF. O documento pode ser uma imagem escaneada."*
- O frontend trata esse retorno como alerta amigável, permitindo que o recrutador ou candidato realize o preenchimento manual completo do formulário sem bloqueio operacional.

### 6.2. Documentos Protegidos por Senha ou Criptografados
- **Como funciona:** O `PdfDocument.Open(stream)` tenta carregar e decodificar a estrutura de objetos do arquivo.
- **Limitação:** PDFs protegidos por senha de abertura (User Password) ou restrição criptográfica proprietária não permitem acesso ao conteúdo sem o fornecimento da chave de descriptografia.
- **Tratamento no Sistema:** O processamento captura exceções de segurança em bloco defensivo e responde:
  > *"Não foi possível processar o arquivo PDF. Verifique se o documento não está corrompido ou protegido por senha."*

### 6.3. Layouts Colunares (Multi-colunas) e Agrupamento Espacial
- **Como funciona no código:** As palavras do documento são agrupadas na mesma linha através da coordenada vertical inferior arredondada (`Math.Round(w.BoundingBox.Bottom, 0)`) e ordenadas da esquerda para a direita pelo eixo horizontal (`w.BoundingBox.Left`).
- **Comportamento em Colunas:**
  - Em currículos com blocos de texto sequenciais ou colunas assimétricas, a ordenação horizontal consegue reconstituir os dados cadastrais (nome, e-mail, telefone, cargo pretendido).
  - No entanto, em layouts com **duas ou três colunas estritas paralelas** (por exemplo: coluna esquerda com lista de competências e coluna direita com descrição de experiências de trabalho na mesma altura física), as palavras que compartilham a mesma linha de base vertical (eixo Y) são concatenadas juntas em uma única linha horizontal.
  - Isso pode mesclar tópicos independentes em uma mesma sentença na captura do Resumo Profissional, sendo uma característica intrínseca de parsers espaciais univariados.
  - **Mitigação Atual:** O sistema isola nomes de seções finais (`Experiência`, `Formação`, `Competências`) e limita o corte do resumo profissional ao primeiro bloco encontrado, além de permitir revisão e edição imediata de todos os campos no formulário do frontend antes de persistir no banco.

---

## 7. Endpoints Principais da API

| Método | Endpoint | Descrição |
| :--- | :--- | :--- |
| `POST` | `/api/candidatos/extrair-pdf` | Recebe arquivo multipart/form-data (máx. 5 MB) e extrai campos cadastrais |
| `GET` | `/api/candidatos` | Lista até 100 candidatos com suporte a filtro textual (`?busca=termo`) |
| `GET` | `/api/candidatos/{id}` | Retorna detalhes completos do candidato por identificador GUID |
| `POST` | `/api/candidatos` | Cadastra novo candidato com validações rigorosas (FluentValidation) |
