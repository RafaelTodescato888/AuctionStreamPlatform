using CrossCutting.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using User.Domain.Constants.Configuration;
using User.Domain.Dto.Register.Request;
using User.Domain.Enums;

namespace User.Domain.Entities
{
    public class UsersProfile : BaseEntity
    {
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string? Bio { get; init; }
        public DateOnly BirthDate { get; init; }
        public Guid UserId { get; init; }

        protected UsersProfile()
        {
            
        }

        public UsersProfile(AdminConfig adminConfig, string hashedPassword)
        {
            Name = adminConfig.Name;
            Email = adminConfig.Email;
            BirthDate = default;
            User = new Users(adminConfig.Document, hashedPassword, ERole.ADMIN);
        }

        public UsersProfile(RequestRegisterUserDTO content)
        {
            Name = content.Name;
            Email = content.Email;
            Bio = content.Bio;
            BirthDate = content.BirthDate;
            User = new Users(content.Document, content.HashedPassword);
        }

        #region [Foreign Keys]
        [ForeignKey(nameof(UserId))]
        public Users User { get; init; } = null!;
        #endregion

        #region [Factory]
        public static UsersProfile Create(RequestRegisterUserDTO request)
        {
            return new UsersProfile(request);
        }
        #endregion
    }
}
