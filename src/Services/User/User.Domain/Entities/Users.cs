using CrossCutting.Entities;

namespace User.Domain.Entities
{
    public class Users : BaseEntity
    {
        public string Document { get; init; } = string.Empty;
        public string HashPassword {  get; init; } = string.Empty;
    }
}
