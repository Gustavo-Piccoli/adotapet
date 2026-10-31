using AdotPet.Api.ErrorHandling;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AdotPet.Api.IntegrationTests;

// Pure unit tests: they do not touch the factory, so they run without Docker.
public sealed class ApiErrorMessagesTests
{
    private static readonly string[] Parameters = ["input"];

    [Fact]
    public void Missing_required_property_is_named()
    {
        var state = new ModelStateDictionary();
        state.AddModelError("Name", "The Name field is required.");

        var errors = ApiErrorMessages.TranslateErrors(state, Parameters);

        Assert.Equal("O campo Name é obrigatório.", Assert.Single(errors["Name"]));
    }

    [Fact]
    public void Invalid_enum_value_is_named_and_hides_the_noisy_body_parameter_error()
    {
        var state = new ModelStateDictionary();
        state.AddModelError("input", "The input field is required.");
        state.AddModelError("$.species", "The JSON value could not be converted to X. Path: $.species | LineNumber: 0");

        var errors = ApiErrorMessages.TranslateErrors(state, Parameters);

        var (field, messages) = Assert.Single(errors);
        Assert.Equal("species", field);
        Assert.Equal("Valor inválido para o campo species.", Assert.Single(messages));
    }

    [Fact]
    public void Missing_body_asks_for_the_request_body()
    {
        var state = new ModelStateDictionary();
        state.AddModelError("", "A non-empty request body is required.");
        state.AddModelError("input", "The input field is required.");

        var errors = ApiErrorMessages.TranslateErrors(state, Parameters);

        Assert.Equal("Envie os dados no corpo da requisição.", Assert.Single(errors.Values.SelectMany(m => m)));
    }
}
