using AdotaPet.Domain.Adopters;
using AdotaPet.Domain.Common;

namespace AdotaPet.Domain.Tests.Adopters;

public class AdopterTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_Valid_SetsFields()
    {
        var adopter = Adopter.Create("  Maria Silva ", "maria@email.com", "(51) 98888-7777", "Canoas", "rs", Now);

        Assert.NotEqual(Guid.Empty, adopter.Id);
        Assert.Equal("Maria Silva", adopter.Name);
        Assert.Equal("maria@email.com", adopter.Email);
        Assert.Equal("51988887777", adopter.Phone);
        Assert.Equal("Canoas", adopter.City);
        Assert.Equal("RS", adopter.State);
        Assert.Equal(Now, adopter.CreatedAt);
        Assert.Equal(string.Empty, adopter.UserId);
    }

    [Fact]
    public void Create_WithInvalidPhone_Throws() =>
        Assert.Throws<DomainValidationException>(() =>
            Adopter.Create("Maria Silva", "maria@email.com", "123", "Canoas", "RS", Now));

    [Fact]
    public void AssignUser_SetsUserId()
    {
        var adopter = Adopter.Create("Maria Silva", "maria@email.com", "51988887777", "Canoas", "RS", Now);

        adopter.AssignUser("user-1");

        Assert.Equal("user-1", adopter.UserId);
    }

    [Fact]
    public void AssignUser_Twice_Throws()
    {
        var adopter = Adopter.Create("Maria Silva", "maria@email.com", "51988887777", "Canoas", "RS", Now);
        adopter.AssignUser("user-1");

        Assert.Throws<InvalidOperationException>(() => adopter.AssignUser("user-2"));
    }

    [Fact]
    public void AssignUser_WithBlank_Throws()
    {
        var adopter = Adopter.Create("Maria Silva", "maria@email.com", "51988887777", "Canoas", "RS", Now);

        Assert.Throws<DomainValidationException>(() => adopter.AssignUser(""));
    }
}
