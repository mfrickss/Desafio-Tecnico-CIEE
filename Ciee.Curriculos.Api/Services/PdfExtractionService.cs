using System.Text;
using System.Text.RegularExpressions;
using Ciee.Curriculos.Api.DTOs;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Ciee.Curriculos.Api.Services;

public interface IPdfExtractionService
{
    ExtracaoPdfResponseDto ExtrairDados(Stream pdfStream);
}

public class PdfExtractionService : IPdfExtractionService
{
    private static readonly Regex EmailRegex = new(
        @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex TelefoneRegex = new(
        @"(?:(?:\+|00)?(55)\s*)?(?:\(?([1-9][0-9])\)?\s*)?(?:((?:9\d|[2-9])\d{3})\s*[-.]?\s*(\d{4}))",
        RegexOptions.Compiled);

    public ExtracaoPdfResponseDto ExtrairDados(Stream pdfStream)
    {
        try
        {
            var linhasExtraidas = new List<string>();

            using (var document = PdfDocument.Open(pdfStream))
            {
                if (document.NumberOfPages == 0)
                {
                    return new ExtracaoPdfResponseDto(
                        null, null, null, null, null, string.Empty, false,
                        "O arquivo PDF esta vazio ou nao possui paginas legiveis.");
                }

                foreach (var page in document.GetPages())
                {
                    // Agrupa palavras pela coordenada Y vertical para formar linhas reais
                    var palavras = page.GetWords().ToList();
                    if (palavras.Count > 0)
                    {
                        var linhasDaPagina = palavras
                            .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 0))
                            .OrderByDescending(g => g.Key)
                            .Select(g => string.Join(" ", g.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text.Trim())))
                            .Where(l => !string.IsNullOrWhiteSpace(l));

                        linhasExtraidas.AddRange(linhasDaPagina);
                    }
                    else if (!string.IsNullOrWhiteSpace(page.Text))
                    {
                        linhasExtraidas.AddRange(page.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                    }
                }
            }

            if (linhasExtraidas.Count == 0)
            {
                return new ExtracaoPdfResponseDto(
                    null, null, null, null, null, string.Empty, false,
                    "Nao foi possivel extrair texto do PDF. O documento pode ser uma imagem escaneada.");
            }

            var textoCompleto = string.Join(Environment.NewLine, linhasExtraidas);

            var emailEncontrado = ExtrairEmail(textoCompleto);
            var telefoneEncontrado = ExtrairTelefone(textoCompleto);
            var nomeEncontrado = ExtrairNome(linhasExtraidas);
            var cargoSugerido = ExtrairCargo(textoCompleto);
            var resumoSugerido = ExtrairResumo(textoCompleto);

            return new ExtracaoPdfResponseDto(
                NomeCompleto: nomeEncontrado,
                Email: emailEncontrado,
                Telefone: telefoneEncontrado,
                CargoInteresse: cargoSugerido,
                ResumoProfissional: resumoSugerido,
                TextoBruto: textoCompleto,
                Sucesso: true,
                Mensagem: "Dados extraidos do curriculo com sucesso.");
        }
        catch (Exception ex)
        {
            return new ExtracaoPdfResponseDto(
                null, null, null, null, null, string.Empty, false,
                $"Falha ao processar o arquivo PDF: {ex.Message}");
        }
    }

    private static string? ExtrairEmail(string texto)
    {
        var match = EmailRegex.Match(texto);
        return match.Success ? match.Value.Trim() : null;
    }

    private static string? ExtrairTelefone(string texto)
    {
        var match = TelefoneRegex.Match(texto);
        if (!match.Success) return null;

        var raw = match.Value.Trim();
        var apenasDigitos = Regex.Replace(raw, @"\D", "");

        if (apenasDigitos.Length >= 10 && apenasDigitos.Length <= 11)
        {
            var ddd = apenasDigitos.Substring(0, 2);
            var numero = apenasDigitos.Substring(2);
            if (numero.Length == 9)
            {
                return $"({ddd}) {numero.Substring(0, 5)}-{numero.Substring(5)}";
            }
            return $"({ddd}) {numero.Substring(0, 4)}-{numero.Substring(4)}";
        }

        return raw;
    }

    private static string? ExtrairNome(IEnumerable<string> linhas)
    {
        var palavrasIgnoradas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "curriculo", "curriculum", "vitae", "resume", "cv", "dados", "contato", "telefone",
            "email", "endereço", "endereco", "perfil", "sobre", "objetivo", "formação", "formacao"
        };

        foreach (var linha in linhas.Take(8))
        {
            var linhaLimpa = linha.Trim();
            if (linhaLimpa.Length < 3) continue;
            if (palavrasIgnoradas.Contains(linhaLimpa)) continue;
            if (EmailRegex.IsMatch(linhaLimpa)) continue;
            if (TelefoneRegex.IsMatch(linhaLimpa)) continue;

            if (linhaLimpa.Contains("http", StringComparison.OrdinalIgnoreCase) ||
                linhaLimpa.Contains("www.", StringComparison.OrdinalIgnoreCase) ||
                linhaLimpa.Contains("linkedin", StringComparison.OrdinalIgnoreCase) ||
                linhaLimpa.Contains("github", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var palavras = linhaLimpa.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (palavras.Length >= 2 && palavras.Length <= 5)
            {
                return linhaLimpa;
            }
        }

        return null;
    }

    private static string? ExtrairCargo(string texto)
    {
        var regexCargo = new Regex(@"(?:cargo|objetivo|interesse|posicao|vaga|funcao)[\s:]+([^\r\n]+)",
            RegexOptions.IgnoreCase);

        var match = regexCargo.Match(texto);
        if (match.Success && match.Groups[1].Value.Length > 2)
        {
            var cargo = match.Groups[1].Value.Trim();
            return cargo.Length > 100 ? cargo.Substring(0, 100) : cargo;
        }

        return null;
    }

    private static string? ExtrairResumo(string texto)
    {
        var regexResumo = new Regex(@"(?:resumo|perfil profissional|sobre mim|apresentacao|sintese)[\s:]+([^\r\n]{20,500})",
            RegexOptions.IgnoreCase);

        var match = regexResumo.Match(texto);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        return null;
    }
}
