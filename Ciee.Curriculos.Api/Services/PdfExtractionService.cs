using System.Text;
using System.Text.RegularExpressions;
using Ciee.Curriculos.Api.Common;
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
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    private static readonly Regex EmailRegex = new(
        @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        RegexTimeout);

    private static readonly Regex TelefoneRegex = new(
        @"(?:(?:\+|00)?(55)\s*)?(?:\(?([1-9][0-9])\)?\s*)?(?:((?:9\d|[2-9])\d{3})\s*[-.]?\s*(\d{4}))",
        RegexOptions.Compiled,
        RegexTimeout);

    private static readonly Regex InicioResumoRegex = new(
        @"^(?:resumo(?:\s+profissional)?|perfil(?:\s+profissional)?|sobre\s+mim|sobre|apresenta[çc][ãa]o|s[íi]ntese(?:\s+profissional)?|sum[áa]rio(?:\s+de\s+qualifica[çc][õo]es)?|mini\s+bio)\b(?:\s*[:\-])?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        RegexTimeout);

    private static readonly HashSet<string> SecoesFinais = new(StringComparer.OrdinalIgnoreCase)
    {
        "competencias", "competências", "habilidades", "habilidades tecnicas", "habilidades técnicas", "skills", "principais competencias", "principais competências", "tecnologias",
        "experiencia", "experiência", "experiencias", "experiências", "experiencia profissional", "experiência profissional", "historico profissional", "histórico profissional", "atuacao profissional", "atuação profissional",
        "formacao", "formação", "formacao academica", "formação acadêmica", "educacao", "educação", "escolaridade", "graduacao", "graduação",
        "projetos", "projetos relevantes", "principais projetos", "cursos", "certificacoes", "certificações", "certificados", "licencas", "licenças", "idiomas", "linguas", "línguas",
        "contato", "contatos", "informacoes de contato", "informações de contato", "informacoes adicionais", "informações adicionais", "atividades complementares"
    };

    private static readonly HashSet<string> SecoesFinaisSemAcento = new(StringComparer.OrdinalIgnoreCase)
    {
        "competencias", "habilidades", "habilidades tecnicas", "skills", "principais competencias", "tecnologias",
        "experiencia", "experiencias", "experiencia profissional", "historico profissional", "atuacao profissional",
        "formacao", "formacao academica", "educacao", "escolaridade", "graduacao",
        "projetos", "projetos relevantes", "principais projetos", "cursos", "certificacoes", "certificados", "licencas", "idiomas", "linguas",
        "contato", "contatos", "informacoes de contato", "informacoes adicionais", "atividades complementares"
    };

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

            for (var i = 0; i < linhasExtraidas.Count; i++)
            {
                linhasExtraidas[i] = NormalizarTextoPdf(linhasExtraidas[i]);
            }
            var textoCompleto = NormalizarTextoPdf(string.Join(Environment.NewLine, linhasExtraidas));

            var emailEncontrado = ExtrairEmail(textoCompleto);
            var telefoneEncontrado = ExtrairTelefone(textoCompleto);
            var nomeEncontrado = ExtrairNome(linhasExtraidas);
            var resumoSugerido = ExtrairResumo(linhasExtraidas, textoCompleto);
            var cargoSugerido = ExtrairCargo(textoCompleto, linhasExtraidas, nomeEncontrado, resumoSugerido);

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

    public static string NormalizarTextoPdf(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return string.Empty;

        texto = texto.Replace("\0", string.Empty);
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

        // Normaliza cedilhas
        texto = texto.Replace("\u00B8c", "ç").Replace("\u00B8C", "Ç")
                     .Replace("c\u00B8", "ç").Replace("C\u00B8", "Ç");

        // Casos citados no PRD onde o acento foi posicionado incorretamente na sequência de glifos
        // "Jun´ior" ou "Ju´nior" -> "Júnior" (exigindo explicitamente o glifo de acento agudo ou apóstrofo)
        texto = Regex.Replace(texto, @"\bJu[´'\u02CA\u0301]nior\b", "Júnior", RegexOptions.IgnoreCase, RegexTimeout);
        texto = Regex.Replace(texto, @"\bJun[´'\u02CA\u0301]ior\b", "Júnior", RegexOptions.IgnoreCase, RegexTimeout);

        // "Estagiar´io" -> "Estagiário"
        texto = Regex.Replace(texto, @"\bEstagiar[´'\u02CA\u0301]+io\b", "Estagiário", RegexOptions.IgnoreCase, RegexTimeout);
        texto = Regex.Replace(texto, @"\bestagiar[´'\u02CA\u0301]+io\b", "estagiário", RegexOptions.IgnoreCase, RegexTimeout);

        // "jurıd´icos" / "jurid´icos" -> "jurídicos"
        texto = Regex.Replace(texto, @"\bjur[\u0131i]d[´'\u02CA\u0301]+icos\b", "jurídicos", RegexOptions.IgnoreCase, RegexTimeout);

        // "elegıv´eis" / "elegiv´eis" -> "elegíveis"
        texto = Regex.Replace(texto, @"\beleg[\u0131i]v[´'\u02CA\u0301]+eis\b", "elegíveis", RegexOptions.IgnoreCase, RegexTimeout);

        // Mapeamento específico para a letra "i" e dotless i ("\u0131") com acento agudo ou apóstrofo
        texto = Regex.Replace(texto, @"(?:\u0131|i)\s*[\u00B4\u02CA\u0301']", "í", RegexOptions.None, RegexTimeout);
        texto = Regex.Replace(texto, @"[\u00B4\u02CA\u0301']\s*(?:\u0131|i)", "í", RegexOptions.None, RegexTimeout);
        texto = Regex.Replace(texto, @"(?:\u0130|I)\s*[\u00B4\u02CA\u0301']", "Í", RegexOptions.None, RegexTimeout);
        texto = Regex.Replace(texto, @"[\u00B4\u02CA\u0301']\s*(?:\u0130|I)", "Í", RegexOptions.None, RegexTimeout);

        // Converte dotless i remanescente sem acento para i
        texto = texto.Replace("\u0131", "i");
        texto = texto.Replace("\u0130", "I");

        // Diacríticos agudos soltos / decompostos antes ou depois da vogal
        texto = Regex.Replace(texto, @"[\u00B4\u02CA\u0301']\s*([aeouyAEOUY])", m => SubstituirAgudo(m.Groups[1].Value), RegexOptions.None, RegexTimeout);
        texto = Regex.Replace(texto, @"([aeouyAEOUY])\s*[\u00B4\u02CA\u0301']", m => SubstituirAgudo(m.Groups[1].Value), RegexOptions.None, RegexTimeout);

        // Mapeamento de til, circunflexo e crase
        texto = Regex.Replace(texto, @"[\u02DC~\u0303]\s*([aoAO])", m => SubstituirTil(m.Groups[1].Value), RegexOptions.None, RegexTimeout);
        texto = Regex.Replace(texto, @"([aoAO])\s*[\u02DC~\u0303]", m => SubstituirTil(m.Groups[1].Value), RegexOptions.None, RegexTimeout);

        texto = Regex.Replace(texto, @"[\u02C6\^\u0302]\s*([aeoAEO])", m => SubstituirCircunflexo(m.Groups[1].Value), RegexOptions.None, RegexTimeout);
        texto = Regex.Replace(texto, @"([aeoAEO])\s*[\u02C6\^\u0302]", m => SubstituirCircunflexo(m.Groups[1].Value), RegexOptions.None, RegexTimeout);

        texto = Regex.Replace(texto, @"[\u0060\u0300]\s*([aA])", m => m.Groups[1].Value == "a" ? "à" : "À", RegexOptions.None, RegexTimeout);
        texto = Regex.Replace(texto, @"([aA])\s*[\u0060\u0300]", m => m.Groups[1].Value == "a" ? "à" : "À", RegexOptions.None, RegexTimeout);

        var normalizado = texto.Normalize(NormalizationForm.FormC);
        normalizado = Regex.Replace(normalizado, @"[\u0300-\u036F]", string.Empty, RegexOptions.None, RegexTimeout);
        normalizado = Regex.Replace(normalizado, @"[ \t]+", " ", RegexOptions.None, RegexTimeout);

        return normalizado.Trim();
    }

    private static string SubstituirAgudo(string v) => v switch
    {
        "a" => "á", "e" => "é", "i" => "í", "o" => "ó", "u" => "ú",
        "A" => "Á", "E" => "É", "I" => "Í", "O" => "Ó", "U" => "Ú",
        _ => v
    };

    private static string SubstituirTil(string v) => v switch
    {
        "a" => "ã", "o" => "õ", "A" => "Ã", "O" => "Õ",
        _ => v
    };

    private static string SubstituirCircunflexo(string v) => v switch
    {
        "a" => "â", "e" => "ê", "o" => "ô",
        "A" => "Â", "E" => "Ê", "O" => "Ô",
        _ => v
    };

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
        return TelefoneHelper.FormatarTelefoneBrasil(raw);
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

    private static string? ExtrairCargo(
        string textoCompleto,
        IReadOnlyList<string> linhas,
        string? nomeCompleto,
        string? resumoProfissional)
    {
        var cargoExplicito = ExtrairCargoExplicito(textoCompleto);
        if (!string.IsNullOrWhiteSpace(cargoExplicito))
        {
            return cargoExplicito;
        }

        var cargoSubtitulo = ExtrairCargoDeSubtitulo(linhas, nomeCompleto);
        if (!string.IsNullOrWhiteSpace(cargoSubtitulo))
        {
            return cargoSubtitulo;
        }

        return InferirCargoDoResumo(resumoProfissional);
    }

    private static string? ExtrairCargoExplicito(string texto)
    {
        var regexCargo = new Regex(@"(?:cargo(?:\s+pretendido|\s+desejado)?|objetivo(?:\s+profissional)?|interesse|posi[çc][ãa]o(?:\s+desejada)?|vaga(?:\s+desejada|\s+pretendida)?|fun[çc][ãa]o(?:\s+pretendida)?|área\s+de\s+interesse)[\s:]+([^\r\n]+)",
            RegexOptions.IgnoreCase,
            RegexTimeout);

        var match = regexCargo.Match(texto);
        if (match.Success && match.Groups[1].Value.Length > 2)
        {
            var cargo = CargoParsingHelper.LimparEDelimitarCargo(match.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(cargo))
            {
                return cargo;
            }
        }

        return null;
    }

    private static string? ExtrairCargoDeSubtitulo(IReadOnlyList<string> linhas, string? nomeCompleto)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto)) return null;

        var indiceNome = -1;
        for (var i = 0; i < linhas.Count; i++)
        {
            if (linhas[i].Trim().Equals(nomeCompleto.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                indiceNome = i;
                break;
            }
        }

        if (indiceNome < 0) return null;

        for (var i = indiceNome + 1; i < Math.Min(linhas.Count, indiceNome + 4); i++)
        {
            var linha = linhas[i].Trim();
            if (string.IsNullOrWhiteSpace(linha)) continue;
            if (EmailRegex.IsMatch(linha) || TelefoneRegex.IsMatch(linha)) continue;
            if (linha.Contains('@') || linha.StartsWith("http", StringComparison.OrdinalIgnoreCase) || linha.StartsWith("www.", StringComparison.OrdinalIgnoreCase)) continue;
            if (InicioResumoRegex.IsMatch(linha)) break;

            if (linha.Length <= 100 && CargoParsingHelper.NucleoProfissionalRegex.IsMatch(linha))
            {
                linha = RemoverUrlsDaLinha(linha);

                var cargo = CargoParsingHelper.LimparEDelimitarCargo(linha);
                if (!string.IsNullOrWhiteSpace(cargo) && cargo.Length >= 3)
                {
                    return cargo;
                }
            }
        }

        return null;
    }

    private static string RemoverUrlsDaLinha(string linha)
    {
        return Regex.Replace(linha, @"https?:\/\/\S+|www\.\S+", "", RegexOptions.IgnoreCase, RegexTimeout).Trim();
    }

    private static string? InferirCargoDoResumo(string? resumo)
    {
        if (string.IsNullOrWhiteSpace(resumo)) return null;

        var primeiraSentenca = resumo.Split(new[] { '.', '\n', '\r', ';', '|', '!' }, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()?
            .Trim();

        if (string.IsNullOrWhiteSpace(primeiraSentenca)) return null;

        var matchNucleo = CargoParsingHelper.NucleoProfissionalRegex.Match(primeiraSentenca);
        if (matchNucleo.Success)
        {
            var trecho = primeiraSentenca.Substring(matchNucleo.Index).Trim();
            var cargo = CargoParsingHelper.LimparEDelimitarCargo(trecho);
            if (!string.IsNullOrWhiteSpace(cargo) && cargo.Length >= 3)
            {
                return cargo;
            }
        }

        return null;
    }

    public static string LimparEDelimitarCargo(string texto) => CargoParsingHelper.LimparEDelimitarCargo(texto);

    private static string? ExtrairResumo(IReadOnlyList<string> linhas, string textoCompleto)
    {
        var partesResumo = new List<string>();
        var capturando = false;

        foreach (var linha in linhas)
        {
            var linhaTrim = linha.Trim();
            if (string.IsNullOrWhiteSpace(linhaTrim)) continue;

            if (!capturando)
            {
                var match = InicioResumoRegex.Match(linhaTrim);
                if (match.Success)
                {
                    capturando = true;
                    var restoLinha = linhaTrim.Substring(match.Length).Trim(' ', ':', '-');
                    if (!string.IsNullOrWhiteSpace(restoLinha))
                    {
                        partesResumo.Add(restoLinha);
                    }
                }
            }
            else
            {
                if (EhCabecalhoDeSecaoFinal(linhaTrim))
                {
                    break;
                }

                partesResumo.Add(linhaTrim);
            }
        }

        if (partesResumo.Count > 0)
        {
            var texto = string.Join(" ", partesResumo).Trim();
            texto = Regex.Replace(texto, @"\s+", " ", RegexOptions.None, RegexTimeout);
            return texto.Length > 2000 ? texto.Substring(0, 2000).Trim() : texto;
        }

        var regexResumo = new Regex(@"(?:resumo(?:\s+profissional)?|perfil(?:\s+profissional)?|sobre\s+mim|apresenta[çc][ãa]o|s[íi]ntese)[\s:]+([^\r\n]{30,2000})",
            RegexOptions.IgnoreCase,
            RegexTimeout);

        var matchFallback = regexResumo.Match(textoCompleto);
        if (matchFallback.Success)
        {
            var fallback = matchFallback.Groups[1].Value.Trim();
            fallback = Regex.Replace(fallback, @"\s+", " ", RegexOptions.None, RegexTimeout);
            return fallback.Length > 2000 ? fallback.Substring(0, 2000).Trim() : fallback;
        }

        return null;
    }

    private static bool EhCabecalhoDeSecaoFinal(string linha)
    {
        var linhaLimpa = Regex.Replace(linha.ToLowerInvariant(), @"[:\-_|•*]", "", RegexOptions.None, RegexTimeout).Trim();
        if (string.IsNullOrWhiteSpace(linhaLimpa)) return false;

        if (SecoesFinais.Contains(linhaLimpa)) return true;

        var linhaSemAcento = CargoParsingHelper.RemoverAcentos(linhaLimpa);
        if (SecoesFinaisSemAcento.Contains(linhaSemAcento)) return true;

        if (linhaLimpa.Length <= 50)
        {
            foreach (var secao in SecoesFinaisSemAcento)
            {
                if (linhaSemAcento.StartsWith(secao, StringComparison.OrdinalIgnoreCase) &&
                    (linhaSemAcento.Length == secao.Length || char.IsWhiteSpace(linhaSemAcento[secao.Length]) || linhaSemAcento[secao.Length] == '('))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
