using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AdotaPet.Api.ErrorHandling;

/// <summary>pt-BR replacements for the English messages ASP.NET Core produces on its own.</summary>
public static class ApiErrorMessages
{
    private const string ValidationTitle = "Um ou mais campos são inválidos.";
    private const string ValidationDetail = "Verifique os campos informados e tente novamente.";

    private static readonly Dictionary<int, string> Titles = new()
    {
        [StatusCodes.Status400BadRequest] = "Requisição inválida",
        [StatusCodes.Status401Unauthorized] = "Não autorizado",
        [StatusCodes.Status403Forbidden] = "Acesso negado",
        [StatusCodes.Status404NotFound] = "Não encontrado",
        [StatusCodes.Status405MethodNotAllowed] = "Método não permitido",
        [StatusCodes.Status413PayloadTooLarge] = "Arquivo muito grande",
        [StatusCodes.Status415UnsupportedMediaType] = "Tipo de mídia não suportado",
        [StatusCodes.Status500InternalServerError] = "Erro interno",
    };

    /// <summary>Replaces the framework's English default titles; titles set explicitly (already pt-BR) are kept.</summary>
    public static void LocalizeTitle(ProblemDetailsContext context)
    {
        var problem = context.ProblemDetails;
        if (problem.Status is { } status && Titles.TryGetValue(status, out var title)
            && (string.IsNullOrEmpty(problem.Title) || IsFrameworkDefault(problem.Title)))
            problem.Title = title;
    }

    private static bool IsFrameworkDefault(string title) =>
        title is "Bad Request" or "Unauthorized" or "Forbidden" or "Not Found" or "Method Not Allowed"
            or "Content Too Large" or "Request Entity Too Large" or "Unsupported Media Type"
            or "An error occurred while processing your request.";

    /// <summary>Automatic [ApiController] 400 for model binding failures, in pt-BR.</summary>
    public static IActionResult InvalidModelState(ActionContext context)
    {
        var errors = new Dictionary<string, List<string>>();
        foreach (var (key, entry) in context.ModelState)
        {
            var field = FieldName(key);
            foreach (var error in entry.Errors)
            {
                if (!errors.TryGetValue(field, out var messages)) errors[field] = messages = [];
                var message = Translate(field, error);
                if (!messages.Contains(message)) messages.Add(message);
            }
        }

        var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problem = factory.CreateValidationProblemDetails(
            context.HttpContext,
            new ModelStateDictionary(),
            StatusCodes.Status400BadRequest,
            title: ValidationTitle,
            detail: ValidationDetail);
        foreach (var (field, messages) in errors)
            problem.Errors[field] = [.. messages];

        return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
    }

    // "$.species", "dto.Species" or "Species" -> "species" / "Species": keep only the last segment.
    private static string FieldName(string key)
    {
        var segment = key.Split('.', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "";
        return segment.Replace("$", "");
    }

    private static string Translate(string field, ModelError error)
    {
        var message = error.ErrorMessage;

        if (message.Contains("non-empty request body", StringComparison.Ordinal))
            return "Envie os dados no corpo da requisição.";

        if (field.Length > 0 && message.EndsWith("field is required.", StringComparison.Ordinal))
            return $"O campo {field} é obrigatório.";

        // Malformed JSON, unknown enum names, wrong types ("could not be converted", "is not valid", ...).
        return field.Length == 0 ? "Valor inválido em um dos campos." : $"Valor inválido para o campo {field}.";
    }
}
