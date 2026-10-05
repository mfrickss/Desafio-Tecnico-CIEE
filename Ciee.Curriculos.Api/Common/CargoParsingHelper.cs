using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Ciee.Curriculos.Api.Common;

public static class CargoParsingHelper
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    public static readonly Regex SenioridadeFinalRegex = new(
        @"\b(j[úu]nior|jr\.?|pleno|pl\.?|s[êe]nior|sr\.?|trainee|estagi[áa]ri[oa]|estagiari[oa]|est[áa]gio|" +
        @"especialista|master|lead)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        RegexTimeout);

    public static readonly Regex NucleoProfissionalRegex = new(
        @"\b(desenvolvedor(?:a)?|programador(?:a)?|engenheir[oa]|analista|arquiteto(?:a)?|" +
        @"advogad[oa]|m[ée]dic[oa]|enfermeir[oa]|assistente|auxiliar|estagi[áa]ri[oa]|estagiari[oa]|trainee|" +
        @"coordenador(?:a)?|gerente|diretor(?:a)?|supervisor(?:a)?|especialista|consultor(?:a)?|" +
        @"designer|cientista|t[ée]cnic[oa]|tecnic[oa]|dev\.?|eng\.?)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        RegexTimeout);

    // Pontuações que marcam fim de oração ou separação explicativa, ignorando abreviações comuns (Jr., Sr., Pl., Eng., Dev.)
    private static readonly Regex FimOracaoRegex = new(
        @"[;,]|(?<!\b(?:jr|sr|pl|eng|dev))\.(?:\s|$)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        RegexTimeout);

    private static readonly string[] MarcadoresDeCorteSemAcento = new[]
    {
        // Formação acadêmica
        " graduado em", " graduada em", " formado em", " formada em", " cursando",
        " estudante de", " bacharel em", " tecnologo em", " pos-graduado", " pos-graduada",
        " com mba", " com pos", " pos graduado", " pos graduada", " especializacao em",
        // Vivência e experiência prática
        " com vivencia", " com vivência", " com experiencia", " com experiência",
        " com solida", " com sólida", " com atuacao", " com atuação", " com forte",
        " com conhecimento", " com foco", " focado em", " focado no", " focado na",
        " focada em", " focada no", " focada na", " atuando em", " atuante em",
        " atuando com", " com passagem", " no ciclo", " com historico",
        // Intenções e perfil
        " buscando oportunidade", " em busca de", " apaixonado por", " apaixonada por",
        " dedicado a", " dedicada a", " com objetivo de", " almejando"
    };

    public static string LimparEDelimitarCargo(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

        var cargo = texto.Trim();

        // 1. Truncamento na primeira pontuação frasal (ignorando abreviações como "Eng. de Software" ou "Dev. Jr.")
        var matchPontuacao = FimOracaoRegex.Match(cargo);
        if (matchPontuacao.Success)
        {
            cargo = cargo.Substring(0, matchPontuacao.Index).Trim();
        }

        // 2. Truncamento em travessões e hífens explicativos isolados (" - ", " — ", " – ")
        var separadoresHifen = new[] { " - ", " — ", " – " };
        foreach (var sep in separadoresHifen)
        {
            var idxSep = cargo.IndexOf(sep, StringComparison.Ordinal);
            if (idxSep > 0)
            {
                cargo = cargo.Substring(0, idxSep).Trim();
            }
        }

        // 3. Truncamento por marcadores universais de corte com verificação resiliente a acentos
        var cargoSemAcento = RemoverAcentos(cargo);
        var menorCorte = cargo.Length;

        foreach (var marcador in MarcadoresDeCorteSemAcento)
        {
            var idx = cargoSemAcento.IndexOf(marcador, StringComparison.OrdinalIgnoreCase);
            if (idx > 0 && idx < menorCorte)
            {
                menorCorte = idx;
            }
        }

        cargo = cargo.Substring(0, menorCorte).Trim();

        // 4. Limitação após senioridade caso haja sobras que não sejam conectivos compostos ('/', 'e', '&')
        var matches = SenioridadeFinalRegex.Matches(cargo);
        if (matches.Count > 0)
        {
            var ultimaSenioridade = matches[matches.Count - 1];
            var fimSenioridade = ultimaSenioridade.Index + ultimaSenioridade.Length;
            
            // Se logo após a senioridade houver um ponto de abreviação (ex.: "Jr."), inclui o ponto
            if (fimSenioridade < cargo.Length && cargo[fimSenioridade] == '.')
            {
                fimSenioridade++;
            }

            if (fimSenioridade < cargo.Length)
            {
                var sufixo = cargo.Substring(fimSenioridade).Trim();
                if (!sufixo.StartsWith("/") && !sufixo.StartsWith("e ", StringComparison.OrdinalIgnoreCase) && !sufixo.StartsWith("&"))
                {
                    cargo = cargo.Substring(0, fimSenioridade).Trim();
                }
            }
        }

        // 5. Garantia de limite de armazenamento do banco (100 caracteres)
        return cargo.Length > 100 ? cargo.Substring(0, 100).Trim() : cargo;
    }

    public static string RemoverAcentos(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return string.Empty;

        var normalized = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
