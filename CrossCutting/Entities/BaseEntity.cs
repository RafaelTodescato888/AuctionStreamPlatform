namespace CrossCutting.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; init; } = Guid.CreateVersion7();
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
