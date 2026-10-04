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

    [Theory]
    [InlineData("2019 - 2023")]
    [InlineData("2018 a 2022")]
    [InlineData("2020-2024")]
    [InlineData("2015 – 2019")]
    [InlineData("2016 até 2021")]
    public void FormatarTelefoneBrasil_IntervalosDeAnos_DeveRetornarNulo(string intervaloAnos)
    {
        var resultado = TelefoneHelper.FormatarTelefoneBrasil(intervaloAnos);
        Assert.Null(resultado);
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
