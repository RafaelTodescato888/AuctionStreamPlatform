using CrossCutting.Entities;
using User.Domain.Dto.Common;
using User.Domain.Enums;

namespace User.Domain.Entities
{
    public class Users : BaseEntity
    {
        public string Document { get; init; } = string.Empty;
        public string HashPassword { get; init; } = string.Empty;
        public string? RefreshToken { get; private set; }
        public DateTime? RefreshTokenExpiration { get; private set; }
        public ERole Role { get; init; }
        public EUserStatus Status { get; init; }

        protected Users() { }

        
        public Users(string document, string hashPassword)
        {
            Document = document;
            HashPassword = hashPassword;
            Role = ERole.BIDDER;
            Status = EUserStatus.ACTIVE;
        }
        
        public Users(string document, string hashPassword, ERole role)
        {
            Document = document;
            HashPassword=hashPassword;
            Role = role;
        } 
        

        #region [Factory]
        public void SetRefreshToken(RefreshTokenDTO content)
        {
            RefreshToken = content.RefreshToken;
            RefreshTokenExpiration = content.ExpiresAt;
        }
        #endregion

        #region [Navigations]
        public UsersProfile? UsersProfile { get; init; }
        #endregion
    }
}
