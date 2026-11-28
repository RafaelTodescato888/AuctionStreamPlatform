using System.Text.Json;
using User.Domain.Dto.Register.Request;

namespace User.UnitTests.Domain.DTO.Register.Request;

public class RequestRegisterUserDTOTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("Data")]
    public void RequestRegisterUserDTO_Should_Have_Valid_Properties(string? bio)
    {
        string name = "name";
        string email = "email";
        DateOnly birthDate = DateOnly.MinValue;
        string document = "document";
        string password = "password";
        var dto = new RequestRegisterUserDTO(name, email, bio, birthDate, document, password);
        Assert.Equal(name, dto.Name);
        Assert.Equal(email, dto.Email);
        Assert.Equal(bio, dto.Bio);
        Assert.Equal(birthDate, dto.BirthDate);
        Assert.Equal(document, dto.Document);
        Assert.Equal(password, dto.Password);
    }

    [Fact]
    public void RequestRegisterUserDTO_Should_Set_HashedPassword()
    {
        string name = "name";
        string email = "email";
        string? bio = "bio";
        DateOnly birthDate = DateOnly.MinValue;
        string document = "document";
        string password = "password";
        var dto = new RequestRegisterUserDTO(name, email, bio, birthDate, document, password);
        string hashedPassword = "hashed-password";
        dto.SetHashedPassword(hashedPassword);
        Assert.Equal(hashedPassword, dto.HashedPassword);
    }

    [Fact]
    public void Should_Normalize_Document_When_Deserializing()
    {
        const string json = """
        {
            "Name": "Nome",
            "Email": "email@email.com",
            "Bio": null,
            "BirthDate": "1990-01-01",
            "Document": "123.456.789-00",
            "Password": "senha"
        }
        """;

        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
    
        var dto = JsonSerializer.Deserialize<RequestRegisterUserDTO>(json, options);
    
        Assert.NotNull(dto);
        Assert.Equal("12345678900", dto.Document);
    }
}