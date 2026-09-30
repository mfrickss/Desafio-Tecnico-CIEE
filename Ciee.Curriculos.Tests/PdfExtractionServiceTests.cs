using System.IO;
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
    public void ExtrairDados_StreamVazioOuInvalido_DeveRetornarFalhaAmigavel()
    {
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });

        var resultado = _service.ExtrairDados(stream);

        Assert.False(resultado.Sucesso);
        Assert.NotNull(resultado.Mensagem);
    }
}
