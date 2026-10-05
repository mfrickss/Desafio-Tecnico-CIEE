using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ciee.Curriculos.Api.Common;
using Ciee.Curriculos.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Ciee.Curriculos.Tests;

public class GlobalExceptionHandlerTests
{
    private class FakeHostEnvironment(string environmentName = "Development") : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Ciee.Curriculos.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    private class FakeProblemDetailsService : IProblemDetailsService
    {
        public ProblemDetailsContext? LastContext { get; private set; }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            LastContext = context;
            context.HttpContext.Response.StatusCode = context.ProblemDetails.Status ?? StatusCodes.Status500InternalServerError;
            return ValueTask.FromResult(true);
        }

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            LastContext = context;
            context.HttpContext.Response.StatusCode = context.ProblemDetails.Status ?? StatusCodes.Status500InternalServerError;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_DeveRetornarValidationProblemDetailsComStatus400()
    {
        var fakeProblemDetails = new FakeProblemDetailsService();
        var fakeEnv = new FakeHostEnvironment();
        var handler = new GlobalExceptionHandler(fakeProblemDetails, fakeEnv, NullLogger<GlobalExceptionHandler>.Instance);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/candidatos";

        var erros = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Informe um e-mail válido." } }
        };
        var exception = new ValidationException(erros, "Erros de validação encontrados.");

        var tratado = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(tratado);
        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
        var validationProblem = Assert.IsType<HttpValidationProblemDetails>(fakeProblemDetails.LastContext?.ProblemDetails);
        Assert.Equal(StatusCodes.Status400BadRequest, validationProblem.Status);
        Assert.True(validationProblem.Errors.ContainsKey("Email"));
        Assert.Equal("/api/candidatos", validationProblem.Instance);
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundException_DeveRetornarProblemDetailsComStatus404()
    {
        var fakeProblemDetails = new FakeProblemDetailsService();
        var fakeEnv = new FakeHostEnvironment();
        var handler = new GlobalExceptionHandler(fakeProblemDetails, fakeEnv, NullLogger<GlobalExceptionHandler>.Instance);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/candidatos/guid-inexistente";
        var exception = new NotFoundException("Candidato não encontrado.");

        var tratado = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(tratado);
        Assert.Equal(StatusCodes.Status404NotFound, httpContext.Response.StatusCode);
        var problem = fakeProblemDetails.LastContext?.ProblemDetails;
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("Recurso não encontrado", problem.Title);
        Assert.Equal("Candidato não encontrado.", problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_ExcecaoGenerica_DeveRetornarStatus500()
    {
        var fakeProblemDetails = new FakeProblemDetailsService();
        var fakeEnv = new FakeHostEnvironment(Environments.Production);
        var handler = new GlobalExceptionHandler(fakeProblemDetails, fakeEnv, NullLogger<GlobalExceptionHandler>.Instance);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/teste";
        var exception = new InvalidOperationException("Falha inesperada no banco");

        var tratado = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(tratado);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
        var problem = fakeProblemDetails.LastContext?.ProblemDetails;
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        Assert.Equal("Erro interno no servidor", problem.Title);
    }
}
