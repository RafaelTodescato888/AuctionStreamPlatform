using CrossCutting.Entities;
using User.Domain.Enums;

namespace User.Domain.Entities;

public class Users : BaseEntity
{
    public string Document { get; init; } = string.Empty;
    public string HashPassword { get; init; } = string.Empty;
    public ERole Role { get; init; }
    public EUserStatus Status { get; private set; }
    public sbyte FailedLoginAttempts { get; private set; }

    protected Users() { }

    
    public Users(string document, string hashPassword, ERole role)
    {
        Document = document;
        HashPassword = hashPassword;
        Role = role;
        FailedLoginAttempts = 0;

        if (role == ERole.SELLER)
            Status = EUserStatus.REVIEW_PENDING;
        else
            Status = EUserStatus.ACTIVATION_PENDING;
    }
    
    #region [Factory]
    public static Users CreateSeller(string document, string hashPassword)
    {
        return new Users(document, hashPassword, ERole.SELLER);
    }

    public void IncrementFailedLoginAttempts()
    {
        FailedLoginAttempts++;
    }

    public void Block()
    {
        Status = EUserStatus.TEMPORARILY_BLOCKED;
    }

    public void ResetLoginAttemps()
    {
        FailedLoginAttempts = 0;
    }
    #endregion

    #region [Navigations]
    public UsersProfile? UsersProfile { get; init; }
    #endregion
}
