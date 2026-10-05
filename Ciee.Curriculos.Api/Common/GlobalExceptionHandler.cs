using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ciee.Curriculos.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ciee.Curriculos.Api.Common;

public class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail, errors) = exception switch
        {
            ValidationException valEx => (
                valEx.StatusCode,
                valEx.Title,
                valEx.Message,
                valEx.Errors
            ),
            AppException appEx => (
                appEx.StatusCode,
                appEx.Title,
                appEx.Message,
                (IReadOnlyDictionary<string, string[]>?)null
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno no servidor",
                environment.IsDevelopment() ? exception.Message : "Ocorreu um erro interno inesperado durante o processamento da requisição.",
                (IReadOnlyDictionary<string, string[]>?)null
            )
        };

        if (statusCode >= 500)
        {
            logger.LogError(exception, "Exceção não tratada capturada pelo GlobalExceptionHandler: {Message}", exception.Message);
        }
        else
        {
            logger.LogWarning("Falha de aplicação capturada ({StatusCode} - {Title}): {Detail}", statusCode, title, detail);
        }

        httpContext.Response.StatusCode = statusCode;

        ProblemDetails problemDetails;
        if (errors is not null && errors.Count > 0)
        {
            var validationProblem = new HttpValidationProblemDetails(new Dictionary<string, string[]>(errors))
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };
            problemDetails = validationProblem;
        }
        else
        {
            problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }
}
