using Ciee.Curriculos.Api.DTOs;
using Ciee.Curriculos.Api.Validators;
using Xunit;

namespace Ciee.Curriculos.Tests;

public class CriarCandidatoDtoValidatorTests
{
    private readonly CriarCandidatoDtoValidator _validator = new();

    [Fact]
    public void Validar_CandidatoCompletoValido_DevePassarSemErros()
    {
        var dto = new CriarCandidatoDto(
            NomeCompleto: "Maria Silva",
            Email: "maria.silva@exemplo.com",
            Telefone: "(11) 98765-4321",
            CargoInteresse: "Desenvolvedora .NET",
            ResumoProfissional: "Profissional com 4 anos de experiência em backend C# e SQL Server.",
            TeveOrigemPdf: false
        );

        var result = _validator.Validate(dto);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Ab")]
    public void Validar_NomeInvalido_DeveFalharComErroEspecifico(string nomeInvalido)
    {
        var dto = new CriarCandidatoDto(
            NomeCompleto: nomeInvalido,
            Email: "candidato@exemplo.com",
            Telefone: null,
            CargoInteresse: null,
            ResumoProfissional: null
        );

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CriarCandidatoDto.NomeCompleto));
    }

    [Theory]
    [InlineData("")]
    [InlineData("email-invalido")]
    [InlineData("usuario@")]
    [InlineData("@dominio.com")]
    public void Validar_EmailInvalido_DeveFalharComErroEspecifico(string emailInvalido)
    {
        var dto = new CriarCandidatoDto(
            NomeCompleto: "Joao Souza",
            Email: emailInvalido,
            Telefone: null,
            CargoInteresse: null,
            ResumoProfissional: null
        );

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CriarCandidatoDto.Email));
    }

    [Fact]
    public void Validar_CamposOpcionaisNulos_DevePassarComSucesso()
    {
        var dto = new CriarCandidatoDto(
            NomeCompleto: "Carlos Alberto",
            Email: "carlos@dominio.com.br",
            Telefone: null,
            CargoInteresse: null,
            ResumoProfissional: null
        );

        var result = _validator.Validate(dto);

        Assert.True(result.IsValid);
    }
}
