using CrossCutting.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using User.Domain.Dto.Register.Request;

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
