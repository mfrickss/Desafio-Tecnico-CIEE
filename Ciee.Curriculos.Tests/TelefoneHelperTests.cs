using Ciee.Curriculos.Api.Common;
using Xunit;

namespace Ciee.Curriculos.Tests;

public class TelefoneHelperTests
{
    [Theory]
    [InlineData("11987654321", "(11) 98765-4321")]
    [InlineData("11 98765-4321", "(11) 98765-4321")]
    [InlineData("(11) 987654321", "(11) 98765-4321")]
    [InlineData("+55 11 98765-4321", "(11) 98765-4321")]
    [InlineData("5511987654321", "(11) 98765-4321")]
    [InlineData("1187654321", "(11) 8765-4321")]
    [InlineData("11 3456-7890", "(11) 3456-7890")]
    [InlineData("551134567890", "(11) 3456-7890")]
    public void FormatarTelefoneBrasil_FormatosValidos_DeveFormatarCanonico(string entrada, string esperado)
    {
        var resultado = TelefoneHelper.FormatarTelefoneBrasil(entrada);
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void FormatarTelefoneBrasil_NuloOuVazio_DeveRetornarNulo()
    {
        Assert.Null(TelefoneHelper.FormatarTelefoneBrasil(null));
        Assert.Null(TelefoneHelper.FormatarTelefoneBrasil(""));
        Assert.Null(TelefoneHelper.FormatarTelefoneBrasil("   "));
    }

    [Fact]
    public void FormatarTelefoneBrasil_NumeroIncompletoOuNaoPadrao_DeveRetornarTrimOriginal()
    {
        var entrada = "12345";
        var resultado = TelefoneHelper.FormatarTelefoneBrasil(entrada);
        Assert.Equal("12345", resultado);
    }
}
