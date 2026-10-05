using Microsoft.AspNetCore.Http;

namespace Ciee.Curriculos.Api.Exceptions;

public class BusinessException(string message, string title = "Regra de negócio violada", int statusCode = StatusCodes.Status400BadRequest) 
    : AppException(message, title, statusCode)
{
}
