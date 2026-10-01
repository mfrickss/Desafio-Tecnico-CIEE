using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace Ciee.Curriculos.Tests;

public static class GeradorPdfParaTesteApi
{
    public static byte[] GerarPdfCurriculoExemplo()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Carlos Eduardo Peixoto", 16, new PdfPoint(50, 750), font);
        page.AddText("carlos.peixoto@ciee.teste.com", 12, new PdfPoint(50, 720), font);
        page.AddText("(21) 99876-5432", 12, new PdfPoint(50, 700), font);
        page.AddText("Cargo: Analista de Sistemas .NET", 12, new PdfPoint(50, 680), font);
        page.AddText("Resumo: Experiencia com desenvolvimento de APIs, SQL Server e Angular.", 12, new PdfPoint(50, 660), font);

        return builder.Build();
    }
}
