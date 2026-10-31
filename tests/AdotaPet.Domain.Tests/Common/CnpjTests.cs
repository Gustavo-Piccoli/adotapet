using AdotaPet.Domain.Common;

namespace AdotaPet.Domain.Tests.Common;

public class CnpjTests
{
    [Fact]
    public void Normalize_RemovesMask() =>
        Assert.Equal("11222333000181", Cnpj.Normalize("11.222.333/0001-81"));

    [Fact]
    public void IsValid_WithCorrectCheckDigits_ReturnsTrue() =>
        Assert.True(Cnpj.IsValid("11222333000181"));

    [Fact]
    public void IsValid_WithWrongCheckDigits_ReturnsFalse() =>
        Assert.False(Cnpj.IsValid("11222333000182"));

    [Fact]
    public void IsValid_WithAllEqualDigits_ReturnsFalse() =>
        Assert.False(Cnpj.IsValid("11111111111111"));

    [Fact]
    public void IsValid_WithWrongLength_ReturnsFalse() =>
        Assert.False(Cnpj.IsValid("123"));
}
