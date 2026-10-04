using System;
using System.Text.RegularExpressions;
using Ciee.Curriculos.Api.Common;
using Ciee.Curriculos.Api.DTOs;
using FluentValidation;

namespace Ciee.Curriculos.Api.Validators;

public class CriarCandidatoDtoValidator : AbstractValidator<CriarCandidatoDto>
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);
    private static readonly Regex ApenasDigitosRegex = new(@"\D", RegexOptions.Compiled, RegexTimeout);
    
    // Exige usuário válido, @, labels de domínio separados por ponto único (sem pontos consecutivos) e TLD com mínimo de 2 caracteres
    private static readonly Regex EmailEstritoRegex = new(
        @"^[a-zA-Z0-9._%+-]+@(?:[a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        RegexTimeout);

    public CriarCandidatoDtoValidator()
    {
        RuleFor(x => x.NomeCompleto)
            .NotEmpty().WithMessage("O nome completo é obrigatório.")
            .MinimumLength(3).WithMessage("O nome completo deve conter ao menos 3 caracteres.")
            .MaximumLength(150).WithMessage("O nome completo não pode ultrapassar 150 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .Matches(EmailEstritoRegex).WithMessage("Informe um e-mail válido no formato exemplo@dominio.com.")
            .MaximumLength(150).WithMessage("O e-mail não pode ultrapassar 150 caracteres.");

        RuleFor(x => x.Telefone)
            .MaximumLength(30).WithMessage("O telefone não pode ultrapassar 30 caracteres.")
            .Must(ValidarFormatoOuDigitosTelefone)
            .WithMessage("O telefone informado deve conter um número válido com DDD (10 ou 11 dígitos).")
            .When(x => !string.IsNullOrWhiteSpace(x.Telefone));

        RuleFor(x => x.CargoInteresse)
            .MaximumLength(100).WithMessage("O cargo de interesse não pode ultrapassar 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.CargoInteresse));

        RuleFor(x => x.ResumoProfissional)
            .MaximumLength(2000).WithMessage("O resumo profissional não pode ultrapassar 2000 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.ResumoProfissional));
    }

    private static bool ValidarFormatoOuDigitosTelefone(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone)) return true;

        var digitos = ApenasDigitosRegex.Replace(telefone, "");
        if (digitos.StartsWith("55") && (digitos.Length == 12 || digitos.Length == 13))
        {
            digitos = digitos.Substring(2);
        }

        return digitos.Length == 10 || digitos.Length == 11;
    }
}
