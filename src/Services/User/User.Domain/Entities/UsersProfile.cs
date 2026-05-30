using CrossCutting.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace User.Domain.Entities
{
    public class UsersProfile : BaseEntity
    {
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string? Bio { get; init; }
        public DateOnly BirthDate { get; init; }
        public Guid UserId { get; init; }

        #region [Foreign Keys]
        [ForeignKey(nameof(UserId))]
        public Users User { get; init; } = null!;
        #endregion
    }
}
