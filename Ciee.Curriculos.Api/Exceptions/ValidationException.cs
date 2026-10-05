using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Ciee.Curriculos.Api.Exceptions;

public class ValidationException(
    IReadOnlyDictionary<string, string[]> errors,
    string message = "Um ou mais campos contêm erros de validação.",
    string title = "Dados inválidos para cadastro do candidato.") 
    : AppException(message, title, StatusCodes.Status400BadRequest)
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;

    public ValidationException(string campo, string erro) 
        : this(new Dictionary<string, string[]> { { campo, new[] { erro } } })
    {
    }
}
