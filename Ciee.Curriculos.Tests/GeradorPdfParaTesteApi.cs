using System;
using System.IO;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace Ciee.Curriculos.Tests;

public static class GeradorPdfParaTesteApi
{
    public static byte[] GerarPdfCurriculoExemplo()
    {
        return GerarCurriculoFicticioBytes();
    }

    public static byte[] GerarCurriculoFicticioBytes()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        var fontBold = builder.AddStandard14Font(Standard14Font.HelveticaBold);

        // Cabecalho e Nome Completo com Apostrofo (caso de borda)
        page.AddText("Curriculum Vitae", 11, new PdfPoint(50, 800), font);
        page.AddText("Carlos Eduardo Peixoto D'Angelo", 18, new PdfPoint(50, 775), fontBold);

        // Contato
        page.AddText("carlos.dangelo@ciee.teste.com", 11, new PdfPoint(50, 750), font);
        page.AddText("(11) 98765-4321", 11, new PdfPoint(250, 750), font);
        page.AddText("Sao Paulo - SP", 11, new PdfPoint(380, 750), font);

        // Cargo Pretendido
        page.AddText("Cargo Pretendido: Desenvolvedor .NET Pleno", 13, new PdfPoint(50, 720), fontBold);

        // Resumo Profissional
        page.AddText("Resumo Profissional:", 12, new PdfPoint(50, 690), fontBold);
        page.AddText("Engenheiro de software com experiencia solida no desenvolvimento de APIs RESTful com C#,", 10, new PdfPoint(50, 670), font);
        page.AddText(".NET 8, Entity Framework Core e SQL Server. Atuacao em frontends modernos com Angular,", 10, new PdfPoint(50, 655), font);
        page.AddText("arquitetura de microsservicos e esteiras automatizadas de integracao continua.", 10, new PdfPoint(50, 640), font);

        // Experiencia Profissional e Intervalos de Datas (2020 a 2024 - nao devem colidir com telefone)
        page.AddText("Experiencia Profissional", 12, new PdfPoint(50, 610), fontBold);
        page.AddText("Tech Solutions CIEE - 2021 a 2024", 10, new PdfPoint(50, 590), fontBold);
        page.AddText("Desenvolvedor de Software Backend", 10, new PdfPoint(50, 575), font);
        page.AddText("Atuacao na construcao de sistemas de recrutamento e selecao de estagiarios.", 10, new PdfPoint(50, 560), font);

        page.AddText("Inovacao Digital Brasil - 2018 - 2021", 10, new PdfPoint(50, 535), fontBold);
        page.AddText("Desenvolvedor Junior .NET e Web", 10, new PdfPoint(50, 520), font);
        page.AddText("Manutencao e desenvolvimento de servicos corporativos em nuvem.", 10, new PdfPoint(50, 505), font);

        // Formacao Academica
        page.AddText("Formacao Academica", 12, new PdfPoint(50, 475), fontBold);
        page.AddText("Bacharelado em Ciencia da Computacao - Graduacao", 10, new PdfPoint(50, 455), font);

        return builder.Build();
    }

    public static void GravarCurriculoFicticioEmDisco(string caminhoArquivo)
    {
        var bytes = GerarCurriculoFicticioBytes();
        var diretorio = Path.GetDirectoryName(caminhoArquivo);
        if (!string.IsNullOrEmpty(diretorio) && !Directory.Exists(diretorio))
        {
            Directory.CreateDirectory(diretorio);
        }
        File.WriteAllBytes(caminhoArquivo, bytes);
    }
}
