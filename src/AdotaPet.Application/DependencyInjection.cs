using AdotaPet.Application.AdoptionRequests.UseCases;
using AdotaPet.Application.Pets.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace AdotaPet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISearchPetsUseCase, SearchPetsUseCase>();
        services.AddScoped<IGetPetDetailsUseCase, GetPetDetailsUseCase>();
        services.AddScoped<IListShelterPetsUseCase, ListShelterPetsUseCase>();
        services.AddScoped<IGetShelterPetUseCase, GetShelterPetUseCase>();
        services.AddScoped<ICreatePetUseCase, CreatePetUseCase>();
        services.AddScoped<IUpdatePetUseCase, UpdatePetUseCase>();
        services.AddScoped<IUploadPetPhotoUseCase, UploadPetPhotoUseCase>();
        services.AddScoped<IGetPetPhotoUseCase, GetPetPhotoUseCase>();
        services.AddScoped<ICreateAdoptionRequestUseCase, CreateAdoptionRequestUseCase>();
        services.AddScoped<IListAdopterRequestsUseCase, ListAdopterRequestsUseCase>();
        services.AddScoped<IListShelterRequestsUseCase, ListShelterRequestsUseCase>();
        services.AddScoped<IApproveAdoptionRequestUseCase, ApproveAdoptionRequestUseCase>();
        services.AddScoped<IRejectAdoptionRequestUseCase, RejectAdoptionRequestUseCase>();
        return services;
    }
}
