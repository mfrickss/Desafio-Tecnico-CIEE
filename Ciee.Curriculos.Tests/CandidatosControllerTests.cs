using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Ciee.Curriculos.Api.Controllers;
using Ciee.Curriculos.Api.DTOs;
using Ciee.Curriculos.Api.Exceptions;
using Ciee.Curriculos.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Ciee.Curriculos.Tests;

public class CandidatosControllerTests
{
    private readonly PdfExtractionService _pdfExtractionService = new();

    private CandidatosController CriarController()
    {
        return new CandidatosController(
            context: null!,
            pdfExtractionService: _pdfExtractionService,
            validator: null!,
            logger: NullLogger<CandidatosController>.Instance);
    }

    [Fact]
    public void ExtrairPdf_ArquivoNulo_DeveLancarBusinessException()
    {
        var controller = CriarController();

        var ex = Assert.Throws<BusinessException>(() => controller.ExtrairPdf(null));

        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
        Assert.Contains("Nenhum arquivo enviado", ex.Title);
    }

    [Fact]
    public void ExtrairPdf_ArquivoVazioComZeroBytes_DeveLancarBusinessException()
    {
        var controller = CriarController();
        var emptyFile = new FormFile(Stream.Null, 0, 0, "arquivo", "curriculo_vazio.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        var ex = Assert.Throws<BusinessException>(() => controller.ExtrairPdf(emptyFile));

        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
        Assert.Contains("Nenhum arquivo enviado", ex.Title);
    }

    [Fact]
    public void ExtrairPdf_ArquivoMaiorQue5MB_DeveLancarBusinessException()
    {
        var controller = CriarController();
        const long tamanhoAcimaDoLimite = (5 * 1024 * 1024) + 1; // 5 MB + 1 byte
        var fakeFile = new FormFile(Stream.Null, 0, tamanhoAcimaDoLimite, "arquivo", "curriculo_pesado.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        var ex = Assert.Throws<BusinessException>(() => controller.ExtrairPdf(fakeFile));

        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
        Assert.Contains("5 MB", ex.Title);
    }

    [Theory]
    [InlineData("curriculo.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("documento.txt", "text/plain")]
    [InlineData("imagem.png", "image/png")]
    [InlineData("tabela.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public void ExtrairPdf_ExtensaoDivergenteDePdf_DeveLancarBusinessException(string nomeArquivo, string contentType)
    {
        var controller = CriarController();
        var bytes = Encoding.UTF8.GetBytes("Conteúdo qualquer de documento inválido");
        using var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "arquivo", nomeArquivo)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };

        var ex = Assert.Throws<BusinessException>(() => controller.ExtrairPdf(file));

        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
        Assert.Contains("Formato de arquivo inválido", ex.Title);
    }

    [Fact]
    public void ExtrairPdf_ConteudoCorrompidoSemMagicBytesPdf_DeveLancarBusinessException()
    {
        var controller = CriarController();
        var bytesFalsos = Encoding.UTF8.GetBytes("ESTE_CONTEUDO_NAO_E_UM_PDF_VALIDO_BINARIAMENTE");
        using var stream = new MemoryStream(bytesFalsos);
        var file = new FormFile(stream, 0, bytesFalsos.Length, "arquivo", "corrompido.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        var ex = Assert.Throws<BusinessException>(() => controller.ExtrairPdf(file));

        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
        Assert.Contains("PDF válido ou está corrompido", ex.Title);
    }

    [Fact]
    public void ExtrairPdf_PdfValidoEstruturado_DeveRetornarOkComDadosExtraidos()
    {
        var controller = CriarController();
        var pdfBytes = GeradorPdfParaTesteApi.GerarCurriculoFicticioBytes();
        using var stream = new MemoryStream(pdfBytes);
        var file = new FormFile(stream, 0, pdfBytes.Length, "arquivo", "curriculo_ficticio.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        var resultado = controller.ExtrairPdf(file);

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var dto = Assert.IsType<ExtracaoPdfResponseDto>(okResult.Value);
        Assert.True(dto.Sucesso);
        Assert.Contains("Carlos Eduardo", dto.NomeCompleto);
        Assert.Equal("carlos.dangelo@ciee.teste.com", dto.Email);
        Assert.Equal("(11) 98765-4321", dto.Telefone);
        Assert.Equal("Desenvolvedor .NET Pleno", dto.CargoInteresse);
    }

    [Fact]
    public void GeradorFixture_SalvarCurriculoFicticioEmDisco_DeveCriarArquivoPdfValido()
    {
        var caminhoRaiz = Path.Combine(Directory.GetCurrentDirectory(), "..", "curriculo_ficticio.pdf");
        GeradorPdfParaTesteApi.GravarCurriculoFicticioEmDisco(caminhoRaiz);

        Assert.True(File.Exists(caminhoRaiz));
        var bytes = File.ReadAllBytes(caminhoRaiz);
        Assert.True(bytes.Length > 0);

        var magicBytes = Encoding.ASCII.GetBytes("%PDF-");
        var magicBuffer = new byte[magicBytes.Length];
        Array.Copy(bytes, 0, magicBuffer, 0, magicBytes.Length);
        Assert.Equal(magicBytes, magicBuffer);
    }
}
