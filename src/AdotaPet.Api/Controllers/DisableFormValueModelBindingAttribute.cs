using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AdotaPet.Api.Controllers;

/// <summary>
/// Stops MVC from reading the request form while it builds the value providers, so the action can read the
/// form itself. Otherwise the form value providers catch Kestrel's 413 (body too large) and turn it into a
/// generic 400 model error before the action runs.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var factories = context.ValueProviderFactories;
        RemoveFactory<FormValueProviderFactory>(factories);
        RemoveFactory<FormFileValueProviderFactory>(factories);
        RemoveFactory<JQueryFormValueProviderFactory>(factories);
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }

    private static void RemoveFactory<T>(IList<IValueProviderFactory> factories) where T : IValueProviderFactory
    {
        var factory = factories.OfType<T>().FirstOrDefault();
        if (factory is not null)
            factories.Remove(factory);
    }
}
