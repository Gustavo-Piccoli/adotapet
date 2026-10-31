using AdotaPet.Application.Accounts;
using AdotaPet.Application.Common;
using AdotaPet.Application.Pets.Photos;
using AdotaPet.Domain.Adopters;
using AdotaPet.Domain.AdoptionRequests;
using AdotaPet.Domain.Common;
using AdotaPet.Domain.Pets;
using AdotaPet.Domain.Shelters;
using AdotaPet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AdotaPet.Infrastructure.Seed;

/// <summary>Populates a fresh Development database with demo shelters, adopters, pets and requests.</summary>
public static class DevelopmentSeeder
{
    // Fictitious CNPJs with valid check digits.
    private const string FirstCnpj = "11.222.333/0001-81";
    private const string SecondCnpj = "11.444.777/0001-61";
    private const string Password = "Adota123";

    private sealed record PetSeed(string Name, Species Species, Sex Sex, Size Size, AgeGroup AgeGroup,
        bool IsNeutered, bool IsVaccinated, string Description);

    private static readonly PetSeed[] ShelterOnePets =
    [
        new("Thor", Species.Dog, Sex.Male, Size.Large, AgeGroup.Adult, true, true,
            "Thor é um gigante gentil que adora passear e brincar de bolinha. Convive bem com crianças e outros cães. Está pronto para ser o melhor amigo da sua família."),
        new("Mia", Species.Cat, Sex.Female, Size.Small, AgeGroup.Puppy, false, true,
            "Mia é uma gatinha curiosa e cheia de energia. Adora perseguir brinquedos e dormir no colo. Procura um lar com muito carinho e janelas protegidas."),
        new("Bidu", Species.Dog, Sex.Male, Size.Medium, AgeGroup.Adult, true, true,
            "Bidu é um vira-lata simpático e muito leal. Aprende comandos rapidinho e adora companhia. Seu coração já encontrou uma família."),
        new("Luna", Species.Cat, Sex.Female, Size.Small, AgeGroup.Adult, true, true,
            "Luna é uma gata tranquila e elegante, que gosta de ronronar ao lado de quem ama. Prefere ambientes calmos. Vai encher sua casa de serenidade."),
        new("Pipoca", Species.Dog, Sex.Female, Size.Small, AgeGroup.Puppy, false, false,
            "Pipoca é uma filhote saltitante que ainda está descobrindo o mundo. Precisa de paciência, passeios e muito amor. Tem tudo para crescer feliz ao seu lado."),
        new("Nina", Species.Cat, Sex.Female, Size.Medium, AgeGroup.Senior, true, true,
            "Nina é uma senhorinha sábia que adora um cantinho ensolarado. Já viveu muito e agora só quer colo e sossego. Merece uma aposentadoria cheia de afeto."),
        new("Rex", Species.Dog, Sex.Male, Size.Large, AgeGroup.Senior, true, true,
            "Rex é um cão veterano, calmo e muito carinhoso. Gosta de passeios curtos e de ficar perto das pessoas. Sua gratidão por um novo lar é imensa."),
    ];

    private static readonly PetSeed[] ShelterTwoPets =
    [
        new("Mel", Species.Dog, Sex.Female, Size.Medium, AgeGroup.Adult, true, true,
            "Mel é doce, obediente e adora aprender truques. Se dá bem com outros animais e com crianças. É uma companheira para todas as horas."),
        new("Amora", Species.Other, Sex.Female, Size.Small, AgeGroup.Adult, false, false,
            "Amora é uma coelhinha dócil e curiosa que adora cenouras e carinho nas orelhas. Precisa de espaço para saltitar. Já foi adotada por uma família apaixonada."),
        new("Fred", Species.Dog, Sex.Male, Size.Small, AgeGroup.Puppy, false, true,
            "Fred é um filhote brincalhão que transforma qualquer momento em diversão. Está aprendendo a andar na coleira. Ideal para quem quer alegria em casa."),
        new("Tigrinho", Species.Cat, Sex.Male, Size.Medium, AgeGroup.Puppy, false, true,
            "Tigrinho é um gatinho aventureiro, cheio de manhas e muito sociável. Adora subir em tudo e brincar com bolinhas. Vai deixar seus dias bem mais divertidos."),
        new("Dona Maria", Species.Cat, Sex.Female, Size.Medium, AgeGroup.Senior, true, true,
            "Dona Maria é uma gata de idade avançada, serena e muito afetuosa. Passa horas ronronando no colo. Procura um lar sossegado para os anos que vêm."),
    ];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct)
    {
        var db = services.GetRequiredService<AppDbContext>();
        if (await db.Shelters.AnyAsync(ct))
            return;

        var shelters = services.GetRequiredService<IShelterRepository>();
        var adopters = services.GetRequiredService<IAdopterRepository>();
        var pets = services.GetRequiredService<IPetRepository>();
        var requests = services.GetRequiredService<IAdoptionRequestRepository>();
        var identity = services.GetRequiredService<IIdentityService>();
        var storage = services.GetRequiredService<IPhotoStorage>();
        var unitOfWork = services.GetRequiredService<IUnitOfWork>();
        var now = services.GetRequiredService<IClock>().UtcNow;

        await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var shelterOne = Shelter.Create("Abrigo Patas Felizes", FirstCnpj, "Porto Alegre", "RS",
                "(51) 3333-4444", "abrigo@adotapet.dev",
                "Abrigo comunitário que resgata e cuida de cães e gatos até encontrarem um lar definitivo.", now);
            shelterOne.AssignUser(await identity.CreateUserAsync("abrigo@adotapet.dev", Password, Roles.Shelter, token));
            shelters.Add(shelterOne);

            var shelterTwo = Shelter.Create("ONG Focinhos do Bem", SecondCnpj, "Curitiba", "PR",
                "(41) 99999-8888", "ong@adotapet.dev",
                "ONG voluntária dedicada ao resgate, à reabilitação e à adoção responsável de animais abandonados.", now);
            shelterTwo.AssignUser(await identity.CreateUserAsync("ong@adotapet.dev", Password, Roles.Shelter, token));
            shelters.Add(shelterTwo);

            var ana = Adopter.Create("Ana Souza", "ana@adotapet.dev", "(51) 98888-7777", "Porto Alegre", "RS", now);
            ana.AssignUser(await identity.CreateUserAsync("ana@adotapet.dev", Password, Roles.Adopter, token));
            adopters.Add(ana);

            var bruno = Adopter.Create("Bruno Lima", "bruno@adotapet.dev", "(41) 97777-6666", "Curitiba", "PR", now);
            bruno.AssignUser(await identity.CreateUserAsync("bruno@adotapet.dev", Password, Roles.Adopter, token));
            adopters.Add(bruno);

            // Stagger creation times (the first pet is the newest) so the showcase order is deterministic.
            var all = ShelterOnePets.Select(s => (Shelter: shelterOne, Seed: s))
                .Concat(ShelterTwoPets.Select(s => (Shelter: shelterTwo, Seed: s)))
                .Select((x, i) => CreatePet(x.Shelter, x.Seed, now.AddHours(-i)))
                .ToList();

            for (var i = 0; i < all.Count; i++)
            {
                await using var photo = File.OpenRead(
                    Path.Combine(AppContext.BaseDirectory, "Seed", "photos", $"pet-{i + 1:00}.jpg"));
                all[i].SetPhoto(await storage.SaveAsync(photo, ".jpg", token), now);
                pets.Add(all[i]);
            }

            AdoptionRequest AddRequest(Pet pet, Adopter adopter, string message)
            {
                var request = AdoptionRequest.Create(pet, adopter.Id, message, now);
                requests.Add(request);
                return request;
            }

            // Pending: three for shelter 1's panel, one for shelter 2.
            AddRequest(all[0], ana, "Olá! Moro em casa com quintal e adoraria receber o Thor na nossa família.");
            AddRequest(all[1], bruno, "Sempre quis ter uma gatinha como a Mia. Posso oferecer muito carinho e cuidado.");
            AddRequest(all[3], bruno, "A Luna parece perfeita para o meu apartamento tranquilo. Gostaria de conhecê-la.");
            AddRequest(all[9], ana, "O Fred me conquistou! Tenho experiência com filhotes e tempo para dedicar a ele.");

            // Rejected: the pet stays available.
            AddRequest(all[4], bruno, "Gostaria muito de adotar a Pipoca para fazer companhia aos meus filhos.").Reject(now);

            // Adopted via an approved request; competing pending requests are rejected.
            var biduApproved = AddRequest(all[2], ana, "O Bidu é exatamente o companheiro que procuro para caminhadas diárias.");
            var biduLoser = AddRequest(all[2], bruno, "Adoraria dar um lar ao Bidu e muito amor para ele.");
            biduApproved.Approve(now);
            biduLoser.Reject(now);
            all[2].MarkAdopted(now);

            AddRequest(all[8], bruno, "Tenho espaço e experiência com coelhos, e a Amora seria muito bem cuidada.").Approve(now);
            all[8].MarkAdopted(now);

            await unitOfWork.SaveChangesAsync(token);
            return true;
        }, ct);
    }

    private static Pet CreatePet(Shelter shelter, PetSeed s, DateTimeOffset now) =>
        Pet.Create(shelter.Id, s.Name, s.Species, s.Sex, s.Size, s.AgeGroup, s.IsNeutered, s.IsVaccinated, s.Description, now);
}
