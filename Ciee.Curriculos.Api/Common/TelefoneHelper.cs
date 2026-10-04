using System;
using System.Text.RegularExpressions;

namespace Ciee.Curriculos.Api.Common;

public static class TelefoneHelper
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);
    private static readonly Regex ApenasDigitosRegex = new(@"\D", RegexOptions.Compiled, RegexTimeout);
    
    // Identifica e descarta sequências temporais de anos (ex.: 2019 - 2023, 2018 a 2022, 2020-2024)
    public static readonly Regex IntervaloAnosRegex = new(
        @"\b(?:19|20)\d{2}\s*(?:-|–|—|a|ate|até)\s*(?:19|20)\d{2}\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        RegexTimeout);

    public static string? FormatarTelefoneBrasil(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone)) return null;

        var textoLimpo = telefone.Trim();

        // Se contiver padrão de intervalo de anos, não deve ser considerado telefone
        if (IntervaloAnosRegex.IsMatch(textoLimpo))
        {
            return null;
        }

        var digitos = ApenasDigitosRegex.Replace(textoLimpo, "");

        // Remove DDI 55 se fornecido
        if (digitos.StartsWith("55") && (digitos.Length == 12 || digitos.Length == 13))
        {
            digitos = digitos.Substring(2);
        }

        // Celular padrão: 11 dígitos -> (XX) 9XXXX-XXXX
        if (digitos.Length == 11)
        {
            var ddd = digitos.Substring(0, 2);
            var numero = digitos.Substring(2);
            return $"({ddd}) {numero.Substring(0, 5)}-{numero.Substring(5)}";
        }

        // Fixo ou formato de 10 dígitos -> (XX) XXXX-XXXX
        if (digitos.Length == 10)
        {
            var ddd = digitos.Substring(0, 2);
            var numero = digitos.Substring(2);
            return $"({ddd}) {numero.Substring(0, 4)}-{numero.Substring(4)}";
        }

        return textoLimpo;
    }
}
