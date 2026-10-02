using System.Text.RegularExpressions;

namespace Ciee.Curriculos.Api.Common;

public static class TelefoneHelper
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);
    private static readonly Regex ApenasDigitosRegex = new(@"\D", RegexOptions.Compiled, RegexTimeout);

    public static string? FormatarTelefoneBrasil(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone)) return null;

        var digitos = ApenasDigitosRegex.Replace(telefone, "");

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

        return telefone.Trim();
    }
}
