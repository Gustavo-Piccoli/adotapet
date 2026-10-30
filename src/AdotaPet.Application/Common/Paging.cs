namespace AdotaPet.Application.Common;

public static class Paging
{
    public const int DefaultPageSize = 12;
    public const int MaxPageSize = 48;

    public static int NormalizePage(int? page) => page is null or < 1 ? 1 : page.Value;

    public static int NormalizePageSize(int? size) =>
        size is null ? DefaultPageSize : Math.Clamp(size.Value, 1, MaxPageSize);
}
