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
        return services;
    }
}
