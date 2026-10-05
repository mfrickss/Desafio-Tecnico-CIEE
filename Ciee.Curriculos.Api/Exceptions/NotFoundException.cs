using Microsoft.AspNetCore.Http;

namespace Ciee.Curriculos.Api.Exceptions;

public class NotFoundException(string message) 
    : AppException(message, "Recurso não encontrado", StatusCodes.Status404NotFound)
{
}
