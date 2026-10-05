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
    [InlineData("candidato@ciee.org.br")]
    [InlineData("usuario.sobrenome@dominio.com")]
    [InlineData("dev_test+tag@sub.empresa.io")]
    public void Validar_EmailCorporativoValidoComTLD_DevePassar(string emailValido)
    {
        var dto = new CriarCandidatoDto(
            NomeCompleto: "João Silva",
            Email: emailValido,
            Telefone: null,
            CargoInteresse: null,
            ResumoProfissional: null
        );

        var result = _validator.Validate(dto);

        Assert.True(result.IsValid);
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
    [InlineData("usuario@dominio")]
    [InlineData("usuario@dominio.c")]
    [InlineData("usuario@dominio..com")]
    public void Validar_EmailInvalidoOuSemTLD_DeveFalharComErroEspecifico(string emailInvalido)
    {
        var dto = new CriarCandidatoDto(
            NomeCompleto: "João Souza",
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
    public void Validar_MensagensDeValidacao_DevemEstarAcentuadasCorretamente()
    {
        var dto = new CriarCandidatoDto(
            NomeCompleto: "",
            Email: "",
            Telefone: null,
            CargoInteresse: null,
            ResumoProfissional: null
        );

        var result = _validator.Validate(dto);

        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("é obrigatório"));
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
