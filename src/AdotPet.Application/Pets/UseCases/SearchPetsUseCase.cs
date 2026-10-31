using AdotPet.Application.Common;
using AdotPet.Application.Pets.Dtos;
using AdotPet.Domain.Common;
using AdotPet.Domain.Pets;

namespace AdotPet.Application.Pets.UseCases;

public interface ISearchPetsUseCase
{
    Task<PagedResult<PetSummaryDto>> ExecuteAsync(SearchPetsDto input, CancellationToken ct);
}

public sealed class SearchPetsUseCase(IPetRepository pets) : ISearchPetsUseCase
{
    public async Task<PagedResult<PetSummaryDto>> ExecuteAsync(SearchPetsDto input, CancellationToken ct)
    {
        var criteria = new PetSearchCriteria(
            input.Species, input.Sex, input.Size, input.AgeGroup,
            Clean(input.State)?.ToUpperInvariant(), Clean(input.City),
            Paging.NormalizePage(input.Page), Paging.NormalizePageSize(input.PageSize));

        var result = await pets.SearchAvailableAsync(criteria, ct);
        return new PagedResult<PetSummaryDto>(
            result.Items.Select(PetMapping.ToSummary).ToList(), result.Page, result.PageSize, result.TotalCount);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
