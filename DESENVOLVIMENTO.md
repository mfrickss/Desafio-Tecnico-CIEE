# Registro de Desenvolvimento e Memorial Técnico

Este documento detalha de forma contínua, transparente e técnica todo o processo de concepção, arquitetura, implementação, validação e uso de inteligência artificial no sistema de **Cadastro e Consulta de Currículos**, conforme exigido pelas diretrizes do desafio técnico do CIEE.

---

## 1. Organização e Execução do Trabalho

O projeto foi planejado e executado utilizando a metodologia de **Fatias Verticais (Vertical Slicing)**, priorizando entregas funcionais de ponta a ponta em cada ciclo. Isso assegurou que cada etapa conectasse o banco de dados, a API em ASP.NET Core e a interface em Angular com validações e testes desde o primeiro momento.

O trabalho foi estruturado em 5 fases interdependentes:

### Fase 1: Análise de Requisitos e Planejamento Arquitetural

- Levantamento rigoroso dos requisitos funcionais: suporte a cadastro manual e via importação de PDF compartilhado no mesmo formulário reativo, consulta por listagem com busca e tela de detalhamento.
- Definição da stack tecnológica:
  - **Backend:** ASP.NET Core (.NET 8 LTS) estruturado em camadas claras (Controllers, Services, Data, DTOs, Common, Validators).
  - **Frontend:** Angular 22 com componentes estritamente Standalone, eliminando módulos legados (`NgModule`), com formulários reativos (`ReactiveFormsModule`) e estilização utilitária com Tailwind CSS.
  - **Banco de Dados:** Microsoft SQL Server 2022 em container Docker com versionamento de esquema via Entity Framework Core 8 Migrations.
- Estabelecimento da premissa fundamental do produto: o PDF é um acelerador de digitação opcional; qualquer falha de leitura, arquivo corrompido ou documento sem camada de texto não deve bloquear o cadastro manual.

### Fase 2: Infraestrutura Local e Modelagem de Dados

- Configuração do `docker-compose.yml` para orquestração isolada do SQL Server 2022 (`mcr.microsoft.com/mssql/server:2022-latest`), com variáveis parametrizadas via `.env` e `.env.example`.
- Criação da entidade `Candidato` e do mapeamento fluente via `AppDbContext`, com índices otimizados para busca e controle de tamanho máximo de colunas (`HasMaxLength`).
- Implementação de migração inicial (`CriacaoTabelaCandidatos`) com execução automática no startup da API durante o desenvolvimento e geração do script SQL idempotente de apoio (`script-inicial-banco.sql`).

### Fase 3: Desenvolvimento do Backend (.NET 8 API)

- **Controllers:** `CandidatosController` expondo endpoints RESTful com convenções RFC 7807 (`ProblemDetails` e `ValidationProblemDetails`), roteamento limpo e atributos de documentação OpenAPI/Swagger.
- **Camada de Validação:** Adoção do `FluentValidation` para validação robusta de regras de negócio, limites de tamanho e formato canônico de e-mail antes da persistência.
- **Motor de Extração de PDF:** Criação da interface `IPdfExtractionService` e implementação concreta `PdfExtractionService` utilizando a biblioteca gerenciada `PdfPig`. Desenvolvimento de algoritmos de reconstrução espacial, normalização Unicode, deduplicação de diacríticos e extração contextual de cargos e resumos.
- **Higienização de Dados:** Implementação de utilitários dedicados (`TelefoneHelper`, `CargoParsingHelper` e `Iso8601UtcDateTimeJsonConverter`).

### Fase 4: Desenvolvimento do Frontend (Angular 22 Standalone)

- Estruturação de componentes Standalone (`CadastroComponent`, `ListagemComponent`, `DetalhesComponent`) sob rotas desacopladas com lazy loading (`loadComponent`).
- Construção de um formulário reativo único que atende tanto ao fluxo manual quanto ao fluxo preenchido via upload de PDF.
- Área interativa de upload com drag-and-drop, validação imediata no cliente para arquivos acima de 5 MB ou formatos não-PDF, exibição de estados de carregamento e mensagens contextuais de erro e sucesso.
- Implementação do serviço `CandidatoService` com `HttpClient`, centralizando tratamento de erros da API e mapeamento de dados.
- Aprimoramentos de acessibilidade com suporte a navegação por teclado (`Tab`, `Enter`, `Escape`), atributos ARIA e contraste adequado de cores institucionais.

### Fase 5: Testes Automatizados, Integração e Documentação

- Escrita de suíte completa de testes de unidade no backend com `xUnit`, cobrindo controladores, serviços de extração, validadores e helpers com gerador de PDF sintético em memória.
- Implementação de testes no frontend para serviços e componentes utilizando o runner nativo do Node.js (`node --test`).
- Homologação de ponta a ponta dos fluxos de cadastro manual, cadastro com PDF e busca.
- Elaboração do `README.md` com instruções completas de configuração e do currículo fictício em PDF (`curriculo_ficticio.pdf`).

---

## 2. Principais Decisões Técnicas e Motivações

### 2.1 Backend: ASP.NET Core (.NET 8) com Controllers Organizados

- **Motivação:** A arquitetura baseada em Controllers explícitos (`[ApiController]`) oferece clara separação de responsabilidades e facilita a documentação e manutenção corporativa.
- **Injeção de Dependências:** Uso estrito do container nativo de DI do .NET (`IServiceCollection`), aplicando ciclos de vida adequados:
  - `AppDbContext`: ciclo `Scoped`.
  - `IPdfExtractionService`: ciclo `Transient`.
  - Validadores do `FluentValidation`: ciclo `Scoped`.
- **Consultas Assíncronas e Otimização:** Todas as operações de banco utilizam chamadas assíncronas (`ToListAsync`, `SaveChangesAsync`) e o modificador `.AsNoTracking()` em consultas de leitura, evitando sobrecarga no Change Tracker do EF Core.

### 2.2 Parser de PDF Gerenciado com PdfPig

- **Decisão:** Escolha da biblioteca `PdfPig` (versão 0.1.9) em vez de utilitários de terceiros ou wrappers de binários nativos em C/C++ (como Poppler ou `pdftotext`).
- **Motivação:** O `PdfPig` é 100% C# gerenciado. Isso elimina a necessidade de instalar pacotes de sistema operacional no host ou de configurar dependências de SO no Dockerfile, garantindo total portabilidade multiplataforma.
- **Algoritmo de Agrupamento Espacial:**
  Muitos documentos PDF não contêm caracteres explícitos de quebra de linha (`\n`), organizando caracteres em fluxos gráficos contínuos. A solução implementada agrupa palavras espacialmente pelo eixo vertical inferior (`BoundingBox.Bottom`), ordenando-as horizontalmente pela esquerda (`BoundingBox.Left`). Isso reconstrói as linhas reais do documento com fidelidade.

### 2.3 Resiliência e Saneamento de Caracteres no PDF

- **Problema Encontrado:** PDFs gerados por diferentes plataformas frequentemente desmembram caracteres acentuados quando não possuem uma tabela `/ToUnicode` completa. Isso gera glifos soltos e acentos deslocados (ex.: `Ju´nior`, `c¸`, `´agil`). Além disso, documentos malformados podem introduzir bytes nulos (`\0`).
- **Solução:**
  - Criação do método `NormalizarTextoPdf`: reconstrói cedilhas, diacríticos agudos, circunflexos, tis e crases antes da extração.
  - Aplicação de `string.Normalize(NormalizationForm.FormC)` para fusão canônica de caracteres Unicode.
  - Remoção de bytes nulos (`\0`) que corromperiam a persistência no banco de dados.
  - Proteção de apóstrofos em nomes próprios (ex.: _Sant'Anna_, _D'Angelo_) para evitar que sejam interpretados erroneamente como acentos agudos.

### 2.4 Delimitação Dinâmica do Resumo Profissional

- **Problema:** Evitar que a leitura do resumo capture tópicos subsequentes como formação acadêmica ou histórico profissional.
- **Solução:** O `PdfExtractionService` inicia a captura a partir de marcadores reconhecidos (_"Resumo"_, _"Perfil Profissional"_, _"Sobre Mim"_) e encerra imediatamente ao encontrar cabeçalhos de seções seguintes (_"Experiência"_, _"Formação"_, _"Habilidades"_, _"Idiomas"_), consultando um conjunto de parada otimizado (`FrozenSet<string>`) e limitando o texto a 2000 caracteres.

### 2.5 Extração Universal de Cargos (`CargoParsingHelper`)

- **Decisão:** Criação de um helper de análise sintática e semântica agnóstico de área profissional.
- **Motivação:** A solução não se restringe a termos de Tecnologia da Informação; ela reconhece posições em Direito, Saúde, Administração, Finanças, Engenharia e áreas operacionais.
- **Regras Sintáticas:**
  - Corte de qualificadores de experiência (_"com sólida vivência"_, _"atuando na área"_) e de formação (_"graduado em"_, _"cursando"_).
  - Preservação de cargos compostos unidos por conjunções (_"Analista de Marketing e Conteúdo"_).
  - Preservação de abreviações profissionais (_"Eng."_, _"Dev."_, _"Jr."_) utilizando expressões regulares com lookbehind negativo, impedindo que o ponto final corte o cargo precocemente.

### 2.6 Formatação e Filtro de Telefones (`TelefoneHelper`)

- **Decisão:** Higienização de números para o padrão nacional com DDD (10 dígitos para fixo, 11 dígitos para celular com nono dígito móvel).
- **Filtro de Falsos Positivos:** Implementação de filtro específico para ignorar intervalos de anos (ex.: `2019 - 2023` ou `2018-2022`) comuns em seções de formação e histórico de empresas, que frequentemente eram capturados por expressões regulares ingênuas de telefone.

### 2.7 Segurança e Prevenção contra ReDoS

- **Decisão:** Todas as instâncias de `Regex` estáticas possuem `RegexOptions.Compiled` e timeout explícito de 1 segundo (`RegexTimeout = TimeSpan.FromSeconds(1)`).
- **Motivação:** Protege o servidor contra ataques de negação de serviço por expressões regulares (ReDoS) ao processar PDFs com cadeias textuais patológicas.

### 2.8 Frontend com Angular 22 Standalone e Validações Reativas

- **Decisão:** Eliminação completa de `NgModule`, adotando componentes Standalone puros, injeção moderna via `inject()` e formulários estritamente reativos (`FormGroup`, `FormControl`, `Validators`).
- **UX e Resiliência:**
  - O formulário é compartilhado: quando o usuário anexa um PDF, a API é acionada via multipart/form-data. Se a extração for bem-sucedida, os campos são preenchidos e uma sinalização de sucesso é exibida.
  - O usuário tem total liberdade para alterar qualquer campo pré-preenchido antes de submeter.
  - Em caso de falha de leitura (ex.: arquivo escaneado ou PDF protegido), uma mensagem clara informa a situação e mantém o formulário pronto para preenchimento manual imediato.

### 2.9 Escopo Deliberado: Ausência de Autenticação

- **Decisão:** Não implementar autenticação com JWT ou controle de perfis de acesso nesta versão.
- **Justificativa:** O desafio solicita uma ferramenta operacional de triagem para recrutadores internos. Implementar um sistema proprietário de login traria complexidade acidental (over-engineering), desviando o foco da qualidade do core business (leitura de PDF, validações, persistência e interface). Em ambiente corporativo real, o acesso seria integrado via Single Sign-On (SSO / Azure AD).

---

## 3. Uso de Inteligência Artificial

- **Ferramenta Utilizada:** Codex Desktop / CLI.
- **Modelo de Linguagem:** GPT-6 Astra.
- **Papel da IA no Projeto:** A ferramenta atuou estritamente como copiloto técnico de engenharia (pair programmer avançado). A IA foi utilizada para acelerar o desenvolvimento de código estrutural, formular algoritmos matemáticos e sintáticos de extração de texto em PDF, sugerir testes automatizados de casos de borda e acelerar a construção de componentes visuais em Angular 22.

---

## 4. Etapas em que a IA Ajudou (com Exemplos Reais)

### 4.1 Resolução da Extração Espacial de PDF (PdfPig)

- **Cenário:** O `PdfPig` retornava sentenças truncadas ou concatenava blocos de texto fora de ordem porque o PDF do currículo não utilizava caracteres `\n`.
- **Pedido Real:**
  > _"Como agrupar as palavras lidas pelo PdfPig considerando suas coordenadas cartesianas de forma que o texto de cada linha seja preservado na ordem de leitura humana?"_
- **Aproveitamento:** A IA sugeriu agrupar os objetos `Word` pelo arredondamento do eixo Y inferior (`BoundingBox.Bottom`), ordenando as linhas de cima para baixo (`OrderByDescending`) e as palavras da esquerda para a direita (`BoundingBox.Left`). Essa abordagem resolveu a reconstituição do texto estruturado.

### 4.2 Recomposição de Diacríticos e Acentos Desmembrados

- **Cenário:** PDFs gerados no Canva ou ferramentas web apresentavam glifos separados (como `Ju´nior`, `c¸`, `´agil`).
- **Pedido Real:**
  > _"Elabore uma rotina em C# para recompor acentos agudos e cedilhas que foram extraídos como caracteres isolados antes ou depois da vogal, sem corromper apóstrofos de nomes como Sant'Anna e normalizando em FormC."_
- **Aproveitamento:** A resposta forneceu a lógica base com proteção de apóstrofo via token temporário e substituição seletiva dos diacríticos agudos, posteriormente integrada ao `NormalizarTextoPdf`.

### 4.3 Criação do Helper Universal de Cargos (`CargoParsingHelper`)

- **Cenário:** Necessidade de extrair cargos em currículos de diversas profissões sem que frases longas como _"atuando como advogado júnior com sólida vivência em direito civil"_ fossem integralmente inseridas no campo de cargo.
- **Pedido Real:**
  > _"Escreva regras em C# para identificar títulos profissionais multidisciplinares e truncar qualificadores de experiência ou formação acadêmica mantendo cargos compostos."_
- **Aproveitamento:** A IA estruturou a separação entre núcleos profissionais, pontuações de corte e conectivos válidos, gerando a base que refinamos no `CargoParsingHelper`.

### 4.4 Geração de Testes Automatizados com Casos de Borda

- **Cenário:** Criação de testes unitários xUnit com cenários de documentos malformados e PDFs sintéticos gerados em memória.
- **Pedido Real:**
  > _"Gere testes xUnit testando streams corrompidos, arquivos vazios, PDFs sem páginas de texto e validação de magic bytes %PDF- no controller."_
- **Aproveitamento:** O código dos testes foi incorporado no projeto `Ciee.Curriculos.Tests`, reduzindo o tempo de criação de mocks e streams binários.

---

## 5. O que Precisou ser Corrigido, Adaptado ou Descartado da IA

1. **Descarte de Sugestão de Bibliotecas Pesadas de NLP:**
   - A IA inicialmente sugeriu incorporar bibliotecas de NLP baseadas em Python ou pacotes pesados de machine learning para identificação de entidades nomeadas (NER).
   - **Ação:** A sugestão foi descartada por violar o princípio da simplicidade e introduzir complexidade operacional excessiva. Optou-se por algoritmos sintáticos e heurísticos determinísticos em C#, que rodam em milissegundos sem dependências externas.
2. **Correção de Falsos Positivos de Telefone:**
   - As expressões regulares propostas pela IA capturavam anos de experiência profissional (ex.: `2018-2022`) como números de telefone.
   - **Ação:** Foi desenvolvida a regra de exclusão explícita no `TelefoneHelper` (`IntervaloAnosRegex`) para descartar intervalos de quatro dígitos que representam períodos de anos.
3. **Adequação ao Ecossistema Angular 22:**
   - Em alguns scaffolds, a IA gerou estruturas com padrões antigos do Angular (como módulos `CommonModule` redundantes ou declarações `NgModule`).
   - **Ação:** O código foi corrigido para adotar estritamente Standalone Components puros, controle com Angular Signals e sintaxe moderna do Angular 22.
4. **Tratamento de Timeout em Expressões Regulares:**
   - A IA sugeriu expressões regulares sem especificar timeouts de execução.
   - **Ação:** Todos os métodos foram adaptados para exigir `RegexTimeout = TimeSpan.FromSeconds(1)`, assegurando proteção ativa contra travamento por ReDoS.

---

## 6. Como a Solução Foi Verificada

A conformidade da solução foi comprovada por meio de verificação em múltiplas camadas:

### 6.1 Testes Automatizados no Backend (xUnit)

- **Suíte de Testes:** 56 testes automatizados xUnit cobrindo:
  - Validação de entrada com `FluentValidation` (campos obrigatórios, limites de tamanho e validação de formato de e-mail).
  - Reconstrução e normalização de texto em PDFs com acentuação corrompida.
  - Extração de cargos para múltiplas áreas (Direito, Saúde, TI, Engenharia, Administração).
  - Descarte de intervalos de anos na captura de telefones e formatação com máscara brasileira.
  - Validação de arquivos na API: rejeição de extensões não-PDF, arquivos acima de 5 MB, streams vazios e verificação de assinatura binária de magic bytes (`%PDF-`).
  - Serialização canônica de datas em UTC com o conversor customizado `Iso8601UtcDateTimeJsonConverter`.

### 6.2 Testes Automatizados no Frontend (19 testes com Node test runner)

- Testes unitários para o serviço `CandidatoService` cobrindo sucesso e tratamento de erros de API.
- Testes de componentes para `CadastroComponent` e `ListagemComponent` utilizando o runner nativo do Node.js (`node --test`).

### 6.3 Testes Manuais de Integração e Ponta a Ponta

- **Cadastro Manual:** Submissão de candidato através da interface web sem anexo de arquivo, verificando retorno HTTP 201 Created e atualização imediata na listagem.
- **Cadastro com PDF:** Upload do currículo de teste (`curriculo_ficticio.pdf`), observando o preenchimento automático de Nome, E-mail, Telefone, Cargo e Resumo Profissional, seguido de edição manual e salvamento.
- **Cenários de Erro:** Envio de arquivo não-PDF (rejeição com mensagem clara), envio de arquivo maior que 5 MB e teste com PDF sem camada de texto (exibição de alerta e liberação imediata para preenchimento manual).
- **Compilação de Produção:** Execução com sucesso do build de produção do frontend (`ng build`) sem advertências ou erros de tipagem.

---

## 7. Tempo Dedicado ao Desafio

O tempo total dedicado ao projeto foi de aproximadamente **20 horas**, distribuídas de maneira equilibrada entre as fases de desenvolvimento:

| Etapa                                 | Descrição                                                                            | Tempo Dedicado |
| :------------------------------------ | :----------------------------------------------------------------------------------- | :------------- |
| **Planejamento e Arquitetura**        | Alinhamento do escopo, modelagem do domínio, design de contratos e decisões técnicas | 3 horas        |
| **Infraestrutura e Banco de Dados**   | Docker Compose do SQL Server, EF Core Migrations e script SQL inicial                | 2 horas        |
| **Backend (.NET 8 Web API)**          | Controllers, regras de negócio, motor de extração PdfPig, sanitização e testes xUnit | 6 horas        |
| **Frontend (Angular 22)**             | Standalone Components, formulários reativos, interface Tailwind, upload e validações | 3 horas        |
| **Testes, Integração e Documentação** | Homologação ponta a ponta, testes de frontend, confecção do PDF fictício e memoriais | 4 horas        |
| **Total**                             |                                                                                      | **18 horas**   |

---

## 8. Dificuldades Encontradas, Limitações e Melhorias Futuras

### 8.1 Dificuldades Encontradas

- **Ausência de Padronização Estrutural em Currículos:** Cada candidato elabora o documento com layouts distintos (duas colunas, cabeçalhos estilizados, caixas de texto flutuantes). Conciliar a extração de dados sem quebrar a ordem de leitura exigiu heurísticas espaciais no eixo Y e X.
- **Problemas de Codificação de Caracteres em PDFs:** A decomposição de caracteres acentuados exigiu investigação minuciosa da tabela Unicode e a criação de rotinas de higienização prévia.

### 8.2 Limitações da Solução Atual

- **Limitação Nativa do PdfPig (Ausência de OCR):**
  O `PdfPig` é projetado estritamente para ler fluxos de texto vetorial e fontes embutidas na estrutura interna do PDF. Por concepção, **ele não possui um motor de OCR (Optical Character Recognition)**. Caso o candidato envie um documento gerado a partir de foto ou escaneamento físico (onde a página é uma imagem rasterizada sem camada de texto vetorial), o `PdfPig` não consegue ler nenhum caractere. O sistema foi desenhado para lidar com isso graciosamente: informa ao usuário que o texto não pôde ser lido e permite o cadastro manual completo sem bloqueios.
- **Persistência Exclusiva dos Dados Textuais:** O binário do PDF enviado é utilizado apenas em memória durante a requisição de extração e não é gravado em storage permanente.
- **Listagem com Limite Fixo:** A consulta de candidatos retorna atualmente até 100 registros mais recentes (`Take(100)`), o que atende à operação inicial, mas necessitaria de paginação no futuro.

### 8.3 Melhorias Futuras com Mais Tempo

1. **Pipeline de OCR para Documentos Digitalizados:** Integração de serviço de OCR assíncrono (ex.: Tesseract OCR ou Azure AI Document Intelligence) como alternativa automática quando o `PdfPig` detectar zero palavras em um arquivo.
2. **Armazenamento de Binários em Nuvem:** Gravação do arquivo PDF original em serviço de armazenamento de objetos (como Azure Blob Storage ou AWS S3), permitindo download e visualização do currículo na tela de detalhes.
3. **Paginação e Filtros Avançados:** Implementação de paginação no backend com parâmetros `pageNumber` e `pageSize`, além de filtros por data e área de interesse.
4. **Análise Semântica de Aderência a Vagas (AI Matching):** Comparação semântica entre o perfil profissional extraído do candidato e os requisitos de vagas abertas cadastradas no sistema.
5. **Autenticação Corporativa:** Integração com Microsoft Entra ID (Azure AD) ou Single Sign-On via OpenID Connect para controle de acesso seguro por recrutadores.
