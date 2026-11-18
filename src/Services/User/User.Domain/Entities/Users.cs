using CrossCutting.Entities;
using User.Domain.Enums;

namespace User.Domain.Entities;

public class Users : BaseEntity
{
    public string Document { get; init; } = string.Empty;
    public string HashPassword { get; init; } = string.Empty;
    public ERole Role { get; init; }
    public EUserStatus Status { get; init; }

    protected Users() { }

    
    public Users(string document, string hashPassword, ERole role)
    {
        Document = document;
        HashPassword = hashPassword;
        Role = role;

        if (role == ERole.SELLER)
            Status = EUserStatus.REVIEW_PENDING;
        else
            Status = EUserStatus.ACTIVE;
    }
    
    #region [Factory]
    public static Users CreateSeller(string document, string hashPassword)
    {
        return new Users(document, hashPassword, ERole.SELLER);
    }
    #endregion

    #region [Navigations]
    public UsersProfile? UsersProfile { get; init; }
    #endregion
}
