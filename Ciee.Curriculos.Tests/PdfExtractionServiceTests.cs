using System;
using System.IO;
using System.Text;
using Ciee.Curriculos.Api.Common;
using Ciee.Curriculos.Api.Services;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;
using Xunit;

namespace Ciee.Curriculos.Tests;

public class PdfExtractionServiceTests
{
    private readonly PdfExtractionService _service = new();

    [Fact]
    public void ExtrairDados_PdfValidoComTexto_DeveIdentificarCamposPrincipais()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Lucas Oliveira", 16, new PdfPoint(50, 750), font);
        page.AddText("lucas.oliveira@empresa.com", 12, new PdfPoint(50, 720), font);
        page.AddText("(11) 98765-4321", 12, new PdfPoint(50, 700), font);
        page.AddText("Cargo: Engenheiro de Software .NET", 12, new PdfPoint(50, 680), font);
        page.AddText("Resumo: Especialista em C#, Entity Framework e microsservicos.", 12, new PdfPoint(50, 660), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Lucas Oliveira", resultado.NomeCompleto);
        Assert.Equal("lucas.oliveira@empresa.com", resultado.Email);
        Assert.Equal("(11) 98765-4321", resultado.Telefone);
        Assert.NotNull(resultado.CargoInteresse);
        Assert.Contains("Engenheiro de Software", resultado.CargoInteresse);
    }

    [Fact]
    public void ExtrairDados_TelefoneSemMascaraNoPdf_DeveFormatarCanonico()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Ana Paula", 16, new PdfPoint(50, 750), font);
        page.AddText("ana.paula@email.com", 12, new PdfPoint(50, 720), font);
        page.AddText("Telefone: 11987654321", 12, new PdfPoint(50, 700), font);
        page.AddText("Cargo: Desenvolvedora .NET", 12, new PdfPoint(50, 680), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal("(11) 98765-4321", resultado.Telefone);
    }

    [Fact]
    public void ExtrairDados_IgnorarIntervaloDeAnosNaIdentificacaoDeTelefone()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Marcos Pontes", 16, new PdfPoint(50, 750), font);
        page.AddText("marcos.pontes@empresa.com", 12, new PdfPoint(50, 720), font);
        page.AddText("Experiência: 2019 - 2023 na empresa Tech", 12, new PdfPoint(50, 700), font);
        page.AddText("Telefone: (11) 99887-6655", 12, new PdfPoint(50, 680), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal("(11) 99887-6655", resultado.Telefone);
    }

    [Fact]
    public void ExtrairDados_PeriodoDeDatasComConjuncaoA_NaoDeveConfundirComTelefone()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Juliana Ribeiro", 16, new PdfPoint(50, 750), font);
        page.AddText("juliana.ribeiro@empresa.com", 12, new PdfPoint(50, 720), font);
        page.AddText("Periodo: 2020 a 2024 na Consultoria CIEE", 12, new PdfPoint(50, 700), font);
        page.AddText("Contato: (21) 98111-2233", 12, new PdfPoint(50, 680), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal("(21) 98111-2233", resultado.Telefone);
    }

    [Fact]
    public void ExtrairDados_CabecalhosCompostos_DevemSerIgnoradosNaDetecaoDeNome()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Curriculum Vitae", 18, new PdfPoint(50, 800), font);
        page.AddText("Dados Pessoais", 14, new PdfPoint(50, 775), font);
        page.AddText("Julio Sant'Anna", 16, new PdfPoint(50, 745), font);
        page.AddText("julio.santanna@empresa.com", 12, new PdfPoint(50, 720), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Julio Sant'Anna", resultado.NomeCompleto);
    }

    [Theory]
    [InlineData("Marco D'Angelo")]
    [InlineData("Julio Sant'Anna")]
    [InlineData("Patrick O'Connor")]
    public void ExtrairDados_NomesComApostrofo_DevemPreservarApostrofoIntegro(string nomeComApostrofo)
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Curriculo Profissional", 14, new PdfPoint(50, 780), font);
        page.AddText(nomeComApostrofo, 16, new PdfPoint(50, 750), font);
        page.AddText("candidato@teste.com", 12, new PdfPoint(50, 720), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal(nomeComApostrofo, resultado.NomeCompleto);
    }

    [Fact]
    public void NormalizarTextoPdf_PreservarApostrofoEmSobrenomesLiterais()
    {
        var entrada = "Julio Sant'Anna e Marco D'Angelo";
        var resultado = PdfExtractionService.NormalizarTextoPdf(entrada);

        Assert.Equal("Julio Sant'Anna e Marco D'Angelo", resultado);
    }

    [Theory]
    [InlineData("usuario-invalido@")]
    [InlineData("@dominio.com.br")]
    [InlineData("sem-arroba-apenas-texto")]
    public void ExtrairDados_EmailsInvalidosNoTexto_NaoDevemSerCapturadosComoEmailValido(string emailInvalido)
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Renata Vasconcelos", 16, new PdfPoint(50, 750), font);
        page.AddText($"Contato: {emailInvalido}", 12, new PdfPoint(50, 720), font);
        page.AddText("Telefone: (11) 98765-4321", 12, new PdfPoint(50, 700), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Renata Vasconcelos", resultado.NomeCompleto);
        Assert.Null(resultado.Email);
    }

    [Fact]
    public void ExtrairDados_PdfCorrompidoOuInvalido_DeveRetornarMensagemAmigavelEmPortugues()
    {
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });

        var resultado = _service.ExtrairDados(stream);

        Assert.False(resultado.Sucesso);
        Assert.Contains("Não foi possível processar o arquivo PDF", resultado.Mensagem);
    }

    [Fact]
    public void ExtrairDados_ResumoProfissionalMultilinhas_DeveCapturarTodosOsParagrafosComoTextoContinuo()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Mariana Santos", 16, new PdfPoint(50, 750), font);
        page.AddText("mariana.santos@email.com", 12, new PdfPoint(50, 720), font);
        page.AddText("Resumo Profissional:", 14, new PdfPoint(50, 690), font);
        page.AddText("Desenvolvedora com solida experiencia no ecossistema .NET e Angular.", 12, new PdfPoint(50, 670), font);
        page.AddText("Atuacao em microsservicos, mensageria com RabbitMQ e bancos relacionais SQL Server.", 12, new PdfPoint(50, 650), font);
        page.AddText("Foco continuo em entregas orientadas a testes unitarios e CI/CD.", 12, new PdfPoint(50, 630), font);
        page.AddText("Experiencia Profissional", 14, new PdfPoint(50, 600), font);
        page.AddText("Tech Corp - 2021 a 2024", 12, new PdfPoint(50, 580), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Mariana Santos", resultado.NomeCompleto);
        Assert.NotNull(resultado.ResumoProfissional);
        Assert.Contains("solida experiencia no ecossistema .NET", resultado.ResumoProfissional);
        Assert.Contains("RabbitMQ", resultado.ResumoProfissional);
        Assert.Contains("orientadas a testes unitarios", resultado.ResumoProfissional);
        Assert.DoesNotContain("Tech Corp", resultado.ResumoProfissional);
        Assert.DoesNotContain("\n", resultado.ResumoProfissional);
    }

    [Fact]
    public void ExtrairDados_CargoComoSubtituloAbaixoDoNome_DeveInferirCorretamente()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Guilherme Silva", 16, new PdfPoint(50, 750), font);
        page.AddText("Desenvolvedor Full Stack Junior", 13, new PdfPoint(50, 730), font);
        page.AddText("guilherme.silva@email.com", 12, new PdfPoint(50, 700), font);
        page.AddText("Resumo: Apaixonado por desenvolvimento de software e tecnologias web modernas.", 12, new PdfPoint(50, 670), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Guilherme Silva", resultado.NomeCompleto);
        Assert.Equal("Desenvolvedor Full Stack Junior", resultado.CargoInteresse);
    }

    [Fact]
    public void ExtrairDados_CargoNaPrimeiraSentencaDoResumo_DeveRecortarTermosDeTransicao()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Rafael Mendes", 16, new PdfPoint(50, 750), font);
        page.AddText("rafael.mendes@email.com", 12, new PdfPoint(50, 720), font);
        page.AddText("Perfil:", 14, new PdfPoint(50, 690), font);
        page.AddText("Desenvolvedor Junior focado em backend C# e arquitetura limpa com ASP.NET Core.", 12, new PdfPoint(50, 670), font);
        page.AddText("Habilidades", 14, new PdfPoint(50, 640), font);
        page.AddText("C#, .NET 8, EF Core, SQL Server", 12, new PdfPoint(50, 620), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Rafael Mendes", resultado.NomeCompleto);
        Assert.Equal("Desenvolvedor Junior", resultado.CargoInteresse);
    }

    [Fact]
    public void NormalizarTextoPdf_ComAcentosEspacadoresEGlifosQuebrados_DeveRecuperarTextoFormatadoCorretamente()
    {
        var entradaBugada = "Desenvolvedor Full Stack Ju´nior focado em desenvolvimento ´agil assistido por ferramentas de IA e constru¸c˜ao de aplica¸c˜oes";
        var resultado = PdfExtractionService.NormalizarTextoPdf(entradaBugada);

        Assert.Equal("Desenvolvedor Full Stack Júnior focado em desenvolvimento ágil assistido por ferramentas de IA e construção de aplicações", resultado);
    }

    [Theory]
    [InlineData("curri´culo", "currículo")]
    [InlineData("Jun´ior", "Júnior")]
    [InlineData("Estagiar´io", "Estagiário")]
    [InlineData("jur\u0131d´icos", "jurídicos")]
    [InlineData("eleg\u0131v´eis", "elegíveis")]
    [InlineData("experi^encia e sa´ıda", "experiência e saída")]
    [InlineData("I´nicio", "Ínicio")]
    public void NormalizarTextoPdf_CasosDeVogalIComAcentosEDotlessI_DeveNormalizarCorretamente(string entrada, string esperado)
    {
        var resultado = PdfExtractionService.NormalizarTextoPdf(entrada);
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void NormalizarTextoPdf_VariacoesDeAcentosQuebrados_DeveCorrigirCorretamente()
    {
        var entrada = "Voc^e est´a na experi^encia de gest~ao e atua¸c~ao `a tarde";
        var resultado = PdfExtractionService.NormalizarTextoPdf(entrada);

        Assert.Equal("Você está na experiência de gestão e atuação à tarde", resultado);
    }

    [Fact]
    public void ExtrairDados_OrdenacaoEspacialPorCoordenadasX_DevePreservarSequenciaDasLinhas()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Carlos Silva", 16, new PdfPoint(50, 750), font);
        page.AddText("carlos@email.com", 12, new PdfPoint(50, 720), font);
        page.AddText("Resumo:", 14, new PdfPoint(50, 690), font);
        page.AddText("Angular", 12, new PdfPoint(240, 660), font);
        page.AddText("e", 12, new PdfPoint(215, 660), font);
        page.AddText("C#", 12, new PdfPoint(180, 660), font);
        page.AddText("com", 12, new PdfPoint(140, 660), font);
        page.AddText("Especialista", 12, new PdfPoint(50, 660), font);
        page.AddText("Formacao", 14, new PdfPoint(50, 630), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.ResumoProfissional);
        Assert.Equal("Especialista com C# e Angular", resultado.ResumoProfissional);
    }

    [Fact]
    public void ExtrairDados_ResumoSemMarcadorDeFim_DeveExtrairAteFinalSemErro()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Aline Souza", 16, new PdfPoint(50, 750), font);
        page.AddText("aline@email.com", 12, new PdfPoint(50, 720), font);
        page.AddText("Mini Bio:", 14, new PdfPoint(50, 690), font);
        page.AddText("Profissional dedicada a engenharia de software com ampla vivencia em times ageis.", 12, new PdfPoint(50, 670), font);
        page.AddText("Entusiasta de arquitetura limpa e testes automatizados.", 12, new PdfPoint(50, 650), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.ResumoProfissional);
        Assert.Contains("Profissional dedicada a engenharia de software", resultado.ResumoProfissional);
        Assert.Contains("Entusiasta de arquitetura limpa", resultado.ResumoProfissional);
        Assert.DoesNotContain("\n", resultado.ResumoProfissional);
    }

    [Fact]
    public void ExtrairDados_SpamDeCabecalhos_NaoDeveCausarReDoS()
    {
        var linhasSpam = new StringBuilder();
        linhasSpam.AppendLine("Nome: Teste Spam");
        linhasSpam.AppendLine("Email: spam@teste.com");
        for (int i = 0; i < 500; i++)
        {
            linhasSpam.AppendLine($"Resumo Profissional: Linha de conteudo repetido {i}");
        }

        var inicio = DateTime.UtcNow;
        var resultadoNormalizado = PdfExtractionService.NormalizarTextoPdf(linhasSpam.ToString());
        var duracao = DateTime.UtcNow - inicio;

        Assert.True(duracao.TotalSeconds < 2, "A normalização e execução de Regex deve concluir em menos de 2 segundos.");
        Assert.NotNull(resultadoNormalizado);
    }

    [Fact]
    public void NormalizarTextoPdf_ComCaracteresNulosEDiacriticosOrfaos_DeveHigienizarSeguramente()
    {
        var entradaHostil = "Nome\0 com byte nulo e acentos combinantes órfãos\u0300\u0301\u0302 soltos.";
        var resultado = PdfExtractionService.NormalizarTextoPdf(entradaHostil);

        Assert.False(resultado.Contains('\0'), "O resultado não deve conter caractere nulo.");
        Assert.DoesNotContain("\u0300", resultado);
        Assert.DoesNotContain("\u0301", resultado);
        Assert.DoesNotContain("\u0302", resultado);
        Assert.Contains("Nome com byte nulo", resultado);
    }

    [Fact]
    public void ExtrairDados_CargosCompostosComVivenciaPratica_DeveCortarExplicacaoEReterApenasCargos()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Marcio Souza", 16, new PdfPoint(50, 750), font);
        page.AddText("marcio.souza@email.com", 12, new PdfPoint(50, 720), font);
        page.AddText("Objetivo: Desenvolvedor Junior e Assistente de Desenvolvimento com vivencia pratica no ciclo completo de suste", 12, new PdfPoint(50, 690), font);
        page.AddText("Resumo: Experiencia em desenvolvimento de software e sustentacao de sistemas.", 12, new PdfPoint(50, 660), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Marcio Souza", resultado.NomeCompleto);
        Assert.Equal("Desenvolvedor Junior e Assistente de Desenvolvimento", resultado.CargoInteresse);
    }

    [Fact]
    public void ExtrairDados_CargoCompostoComBarrasEFormacaoAcademica_DeveCortarFormacao()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Beatriz Lima", 16, new PdfPoint(50, 750), font);
        page.AddText("Cargo Pretendido: Desenvolvedor Back-End / Web graduado em ADS", 12, new PdfPoint(50, 720), font);
        page.AddText("beatriz.lima@email.com", 12, new PdfPoint(50, 690), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Beatriz Lima", resultado.NomeCompleto);
        Assert.Equal("Desenvolvedor Back-End / Web", resultado.CargoInteresse);
    }

    [Fact]
    public void ExtrairDados_ProfissaoNaoTI_AdvogadaComSubtituloEPosGraduacao_DeveDelimitarCargoComPrecisao()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Fernanda Carvalho", 16, new PdfPoint(50, 750), font);
        page.AddText("Advogada Trabalhista Plena pos-graduada em Direito Corporativo", 13, new PdfPoint(50, 730), font);
        page.AddText("fernanda.carvalho@oab.org.br", 12, new PdfPoint(50, 700), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Fernanda Carvalho", resultado.NomeCompleto);
        Assert.Equal("Advogada Trabalhista Plena", resultado.CargoInteresse);
    }

    [Fact]
    public void ExtrairDados_ProfissaoGestaoMarketingEVendasComExperiencia_DevePreservarEspecialidadeComE()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Roberto Nogueira", 16, new PdfPoint(50, 750), font);
        page.AddText("roberto@empresa.com", 12, new PdfPoint(50, 720), font);
        page.AddText("Posicao Desejada: Coordenador de Marketing e Vendas com solida experiencia em prospeccao", 12, new PdfPoint(50, 690), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Roberto Nogueira", resultado.NomeCompleto);
        Assert.Equal("Coordenador de Marketing e Vendas", resultado.CargoInteresse);
    }

    [Fact]
    public void ExtrairDados_SemCargoExplicitoOuResumoComCargo_DeveRetornarNulo()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Juliana Martins", 16, new PdfPoint(50, 750), font);
        page.AddText("juliana.martins@email.com", 12, new PdfPoint(50, 720), font);
        page.AddText("Resumo: Profissional dinamica e proativa em busca de novos desafios no mercado.", 12, new PdfPoint(50, 690), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Juliana Martins", resultado.NomeCompleto);
        Assert.Null(resultado.CargoInteresse);
    }

    [Theory]
    [InlineData("Desenvolvedor Júnior e Assistente de Desenvolvimento com vivência prática no ciclo completo de suste", "Desenvolvedor Júnior e Assistente de Desenvolvimento")]
    [InlineData("Desenvolvedor Back-End / Web graduado em ADS", "Desenvolvedor Back-End / Web")]
    [InlineData("Advogado Trabalhista Pleno pós-graduado em Direito Corporativo e atuação contenciosa", "Advogado Trabalhista Pleno")]
    [InlineData("Assistente Administrativo Pleno com sólida experiência no setor financeiro", "Assistente Administrativo Pleno")]
    [InlineData("Coordenador de Marketing e Vendas - Atuação B2B", "Coordenador de Marketing e Vendas")]
    public void LimparEDelimitarCargo_CasosReaisCompostos_DeveDelimitarComExatidao(string entrada, string esperado)
    {
        var resultado = CargoParsingHelper.LimparEDelimitarCargo(entrada);
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void LimparEDelimitarCargo_AbreviacoesComPonto_NaoDeveCortarPrematuramente()
    {
        var entrada = "Eng. de Software Pleno com vivência prática em microsserviços";
        var resultado = CargoParsingHelper.LimparEDelimitarCargo(entrada);
        Assert.Equal("Eng. de Software Pleno", resultado);

        var entradaDev = "Dev. Jr. com experiência em C#";
        var resultadoDev = CargoParsingHelper.LimparEDelimitarCargo(entradaDev);
        Assert.Equal("Dev. Jr.", resultadoDev);
    }

    [Fact]
    public void ExtrairDados_SubtituloComUrlColada_DeveDescartarUrlEReterApenasCargo()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Carlos Eduardo", 16, new PdfPoint(50, 750), font);
        page.AddText("Arquiteto de Solucoes https://linkedin.com/in/perfil", 12, new PdfPoint(50, 730), font);
        page.AddText("carlos@email.com", 12, new PdfPoint(50, 700), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Carlos Eduardo", resultado.NomeCompleto);
        Assert.Equal("Arquiteto de Solucoes", resultado.CargoInteresse);
    }

    [Fact]
    public void LimparEDelimitarCargo_TextoLongoAcimaDe100Caracteres_DeveLimitarEm100()
    {
        var textoLongo = "Arquiteto Corporativo e Lider Tecnico Responsavel por Governanca de Plataformas de Alta Disponibilidade e Projetos Globais de TI";
        var resultado = CargoParsingHelper.LimparEDelimitarCargo(textoLongo);

        Assert.NotNull(resultado);
        Assert.True(resultado.Length <= 100);
        Assert.Equal(100, resultado.Length);
    }

    [Theory]
    [InlineData("Curriculum Vitae")]
    [InlineData("Curriculo Profissional")]
    [InlineData("Dados Pessoais")]
    [InlineData("Informacoes Pessoais")]
    [InlineData("Historico Profissional")]
    public void ExtrairDados_CabecalhoComoPrimeiraLinha_NaoDeveSerCapturadoComoNome(string cabecalho)
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText(cabecalho, 16, new PdfPoint(50, 750), font);
        page.AddText("Mariana Silva", 14, new PdfPoint(50, 720), font);
        page.AddText("mariana@email.com", 12, new PdfPoint(50, 700), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Mariana Silva", resultado.NomeCompleto);
    }

    [Fact]
    public void ExtrairDados_TextoComOracaoInteresseEmAprender_NaoDeveIdentificarComoCargo()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Gabriel Souza", 16, new PdfPoint(50, 750), font);
        page.AddText("gabriel@email.com", 12, new PdfPoint(50, 720), font);
        page.AddText("Resumo: Tenho grande interesse em aprender novas tecnologias e frameworks modernos.", 12, new PdfPoint(50, 700), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Null(resultado.CargoInteresse);
    }

    [Fact]
    public void ExtrairDados_CargoComContextoAreaDeInteresse_DeveCapturarCargoCorretamente()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Luciana Dias", 16, new PdfPoint(50, 750), font);
        page.AddText("luciana@email.com", 12, new PdfPoint(50, 720), font);
        page.AddText("Area de Interesse: Desenvolvimento de Software", 12, new PdfPoint(50, 700), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Desenvolvimento de Software", resultado.CargoInteresse);
    }

    [Fact]
    public void ExtrairDados_ExcecaoInternaDeStreamOuMotorPdf_DeveRetornarMensagemAmigavelEmPortugues()
    {
        // Simula stream invalido que causa excecao no PdfDocument.Open
        using var streamInvalido = new MemoryStream(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x00, 0xFF, 0xFE });
        var resultado = _service.ExtrairDados(streamInvalido);

        Assert.False(resultado.Sucesso);
        Assert.Equal("Não foi possível processar o arquivo PDF. Verifique se o documento não está corrompido ou protegido por senha.", resultado.Mensagem);
    }

    [Theory]
    [InlineData("Curriculum Vitae:")]
    [InlineData("Curriculum Vitae -")]
    [InlineData("Dados Pessoais:")]
    [InlineData("Dados Pessoais |")]
    [InlineData("Historico Profissional -")]
    [InlineData("Informacoes de Contato:")]
    public void ExtrairDados_CabecalhosComPontuacaoDeBorda_DevemSerDescartadosNaDetecaoDeNome(string cabecalhoComPontuacao)
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText(cabecalhoComPontuacao, 16, new PdfPoint(50, 750), font);
        page.AddText("Rodrigo Alcantara", 14, new PdfPoint(50, 720), font);
        page.AddText("rodrigo@email.com", 12, new PdfPoint(50, 700), font);

        var bytes = builder.Build();
        using var stream = new MemoryStream(bytes);

        var resultado = _service.ExtrairDados(stream);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Rodrigo Alcantara", resultado.NomeCompleto);
    }
}
