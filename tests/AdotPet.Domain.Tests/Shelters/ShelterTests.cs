using AdotPet.Domain.Common;
using AdotPet.Domain.Shelters;

namespace AdotPet.Domain.Tests.Shelters;

public class ShelterTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);

    private static Shelter Create(string name = "ONG Patas", string cnpj = "11.222.333/0001-81",
        string state = "rs", string phone = "(51) 99999-0000") =>
        Shelter.Create(name, cnpj, "Porto Alegre", state, phone, "ong@patas.org", "Resgate de animais", Now);

    [Fact]
    public void Create_WithMaskedCnpjAndPhone_StoresDigitsOnly()
    {
        var shelter = Create();

        Assert.Equal("11222333000181", shelter.Cnpj);
        Assert.Equal("51999990000", shelter.Phone);
        Assert.Equal("RS", shelter.State);
        Assert.NotEqual(Guid.Empty, shelter.Id);
        Assert.Equal(Now, shelter.CreatedAt);
        Assert.Equal(string.Empty, shelter.UserId);
    }

    [Fact]
    public void Create_WithInvalidCnpj_Throws() =>
        Assert.Throws<DomainValidationException>(() => Create(cnpj: "11.222.333/0001-82"));

    [Fact]
    public void Create_WithInvalidState_Throws() =>
        Assert.Throws<DomainValidationException>(() => Create(state: "XX"));

    [Fact]
    public void Create_WithShortName_Throws() =>
        Assert.Throws<DomainValidationException>(() => Create(name: "A"));

    [Fact]
    public void AssignUser_SetsUserId()
    {
        var shelter = Create();

        shelter.AssignUser("user-1");

        Assert.Equal("user-1", shelter.UserId);
    }

    [Fact]
    public void AssignUser_Twice_Throws()
    {
        var shelter = Create();
        shelter.AssignUser("user-1");

        Assert.Throws<InvalidOperationException>(() => shelter.AssignUser("user-2"));
    }

    [Fact]
    public void AssignUser_WithBlank_Throws() =>
        Assert.Throws<DomainValidationException>(() => Create().AssignUser(" "));
}
