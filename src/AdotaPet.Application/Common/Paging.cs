namespace AdotaPet.Application.Common;

public static class Paging
{
    public const int DefaultPageSize = 12;
    public const int MaxPageSize = 48;

    // Keeps (page - 1) * pageSize inside int range when the repository computes Skip.
    public const int MaxPage = 100_000;

    public static int NormalizePage(int? page) => page is null or < 1 ? 1 : Math.Min(page.Value, MaxPage);

    public static int NormalizePageSize(int? size) =>
        size is null ? DefaultPageSize : Math.Clamp(size.Value, 1, MaxPageSize);
}
