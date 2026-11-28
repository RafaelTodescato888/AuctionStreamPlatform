using User.Domain.Constants.Configuration;
using User.Domain.Dto.Register.Request;
using User.Domain.Dto.Update.Request;
using User.Domain.Entities;
using User.Domain.Enums;

namespace User.UnitTests.Domain.Entities;

public class UsersProfileTests
{
    [Fact]
    public void UsersProfile_Should_Have_Valid_Properties_When_Created()
    {
        var request = new RequestRegisterUserDTO(
            "John Doe",
            "john@email.com",
            "Software Developer",
            new DateOnly(2000, 1, 1),
            "12345678900",
            "password"
        );

        request.SetHashedPassword("hashed-password");

        var profile = new UsersProfile(request, ERole.BIDDER);

        Assert.Equal(request.Name, profile.Name);
        Assert.Equal(request.Email, profile.Email);
        Assert.Equal(request.Bio, profile.Bio);
        Assert.Equal(request.BirthDate, profile.BirthDate);

        Assert.NotNull(profile.User);
        Assert.Equal(request.Document, profile.User.Document);
        Assert.Equal(request.HashedPassword, profile.User.HashPassword);
        Assert.Equal(ERole.BIDDER, profile.User.Role);
        Assert.Equal(EUserStatus.ACTIVATION_PENDING, profile.User.Status);
    }

    [Fact]
    public void UsersProfile_Should_Create_Admin_Profile_When_Created_With_AdminConfig()
    {
        var adminConfig = new AdminCredentialsConfig
        {
            Name = "Admin",
            Email = "admin@email.com",
            Document = "12345678900",
            Password = "password"
        };

        var hashedPassword = "hashed-password";

        var profile = new UsersProfile(adminConfig, hashedPassword);

        Assert.Equal(adminConfig.Name, profile.Name);
        Assert.Equal(adminConfig.Email, profile.Email);
        Assert.Equal(default, profile.BirthDate);

        Assert.NotNull(profile.User);
        Assert.Equal(adminConfig.Document, profile.User.Document);
        Assert.Equal(hashedPassword, profile.User.HashPassword);
        Assert.Equal(ERole.ADMIN, profile.User.Role);
        Assert.Equal(EUserStatus.ACTIVATION_PENDING, profile.User.Status);
    }

    [Fact]
    public void UsersProfile_Should_Create_Profile_When_Create()
    {
        var request = new RequestRegisterUserDTO(
            "John Doe",
            "john@email.com",
            "Software Developer",
            new DateOnly(2000, 1, 1),
            "12345678900",
            "password"
        );

        request.SetHashedPassword("hashed-password");

        var profile = UsersProfile.Create(request, ERole.SELLER);

        Assert.Equal(request.Name, profile.Name);
        Assert.Equal(request.Email, profile.Email);
        Assert.Equal(request.Bio, profile.Bio);
        Assert.Equal(request.BirthDate, profile.BirthDate);
        Assert.Equal(ERole.SELLER, profile.User.Role);
        Assert.Equal(EUserStatus.REVIEW_PENDING, profile.User.Status);
    }

    [Fact]
    public void UsersProfile_Should_Update_Name_And_Bio_When_Update()
    {
        var request = new RequestRegisterUserDTO(
            "John Doe",
            "john@email.com",
            "Old bio",
            new DateOnly(2000, 1, 1),
            "12345678900",
            "password"
        );

        request.SetHashedPassword("hashed-password");

        var profile = new UsersProfile(request, ERole.BIDDER);

        var updateRequest = new RequestUpdateProfileUserDTO(
            "Updated Name",
            "Updated bio"
        );

        profile.Update(updateRequest);

        Assert.Equal(updateRequest.Name, profile.Name);
        Assert.Equal(updateRequest.Bio, profile.Bio);
        Assert.Equal(request.Email, profile.Email);
    }

    [Fact]
    public void UsersProfile_Should_Update_Bio_To_Null_When_Update()
    {
        var request = new RequestRegisterUserDTO(
            "John Doe",
            "john@email.com",
            "Old bio",
            new DateOnly(2000, 1, 1),
            "12345678900",
            "password"
        );

        var profile = new UsersProfile(request, ERole.BIDDER);

        var updateRequest = new RequestUpdateProfileUserDTO(
            "Updated Name",
            null
        );

        profile.Update(updateRequest);

        Assert.Equal("Updated Name", profile.Name);
        Assert.Null(profile.Bio);
    }
}