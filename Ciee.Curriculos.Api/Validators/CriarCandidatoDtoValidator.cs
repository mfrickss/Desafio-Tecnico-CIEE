using Ciee.Curriculos.Api.DTOs;
using FluentValidation;

namespace Ciee.Curriculos.Api.Validators;

public class CriarCandidatoDtoValidator : AbstractValidator<CriarCandidatoDto>
{
    public CriarCandidatoDtoValidator()
    {
        RuleFor(x => x.NomeCompleto)
            .NotEmpty().WithMessage("O nome completo e obrigatorio.")
            .MinimumLength(3).WithMessage("O nome completo deve conter ao menos 3 caracteres.")
            .MaximumLength(150).WithMessage("O nome completo nao pode ultrapassar 150 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail e obrigatorio.")
            .EmailAddress().WithMessage("Informe um e-mail valido no formato exemplo@dominio.com.")
            .MaximumLength(150).WithMessage("O e-mail nao pode ultrapassar 150 caracteres.");

        RuleFor(x => x.Telefone)
            .MaximumLength(30).WithMessage("O telefone nao pode ultrapassar 30 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Telefone));

        RuleFor(x => x.CargoInteresse)
            .MaximumLength(100).WithMessage("O cargo de interesse nao pode ultrapassar 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.CargoInteresse));

        RuleFor(x => x.ResumoProfissional)
            .MaximumLength(2000).WithMessage("O resumo profissional nao pode ultrapassar 2000 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.ResumoProfissional));
    }
}
