using System;
using System.IO;
using System.Text;
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

    [Fact]
    public void NormalizarTextoPdf_VariacoesDeAcentosQuebrados_DeveCorrigirCorretamente()
    {
        var entrada = "Voc^e est´a na experi^encia de gest~ao e atua¸c~ao `a tarde";
        var resultado = PdfExtractionService.NormalizarTextoPdf(entrada);

        Assert.Equal("Você está na experiência de gestão e atuação à tarde", resultado);
    }

    [Fact]
    public void CT04_ExtrairDados_OrdenacaoEspacialPorCoordenadasX_DevePreservarSequenciaDasLinhas()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Carlos Silva", 16, new PdfPoint(50, 750), font);
        page.AddText("carlos@email.com", 12, new PdfPoint(50, 720), font);
        page.AddText("Resumo:", 14, new PdfPoint(50, 690), font);
        // Adiciona palavras na mesma linha Y (660), porém inseridas em ordem inversa de X
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
    public void CT05_ExtrairDados_ResumoSemMarcadorDeFim_DeveExtrairAteFinalSemErro()
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
    public void CT06_ExtrairDados_SpamDeCabecalhos_NaoDeveCausarReDoS()
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
    public void CT07_NormalizarTextoPdf_ComCaracteresNulosEDiacriticosOrfaos_DeveHigienizarSeguramente()
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
    public void ExtrairDados_StreamVazioOuInvalido_DeveRetornarFalhaAmigavel()
    {
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });

        var resultado = _service.ExtrairDados(stream);

        Assert.False(resultado.Sucesso);
        Assert.NotNull(resultado.Mensagem);
    }
}