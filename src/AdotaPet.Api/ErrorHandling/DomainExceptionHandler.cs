using AdotaPet.Application.Accounts;
using AdotaPet.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AdotaPet.Api.ErrorHandling;

public sealed class DomainExceptionHandler(IProblemDetailsService problemDetails, ILogger<DomainExceptionHandler> logger)
    : IExceptionHandler
{
    private const string GenericDetail = "Ocorreu um erro inesperado. Tente novamente mais tarde.";

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (status, title, detail) = exception switch
        {
            DomainValidationException e => (StatusCodes.Status400BadRequest, "Requisição inválida", e.Message),
            NotFoundException e => (StatusCodes.Status404NotFound, "Não encontrado", e.Message),
            ConflictException e => (StatusCodes.Status409Conflict, "Conflito", e.Message),
            InvalidCredentialsException e => (StatusCodes.Status401Unauthorized, "Não autorizado", e.Message),
            BadHttpRequestException e => (e.StatusCode, e.StatusCode == StatusCodes.Status413PayloadTooLarge
                ? "Arquivo muito grande" : "Requisição inválida",
                e.StatusCode == StatusCodes.Status413PayloadTooLarge
                    ? "A foto deve ter no máximo 5 MB." : "Requisição inválida."),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno", GenericDetail),
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");
        else if (exception is BadHttpRequestException)
            logger.LogWarning(exception, "Bad HTTP request ({Status})", status);

        httpContext.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = detail },
        });
    }
}
