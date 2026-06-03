namespace User.Domain.Constants.Configuration
{
    public class AppConfig
    {
        public int MaxBytes { get; set; }
        public int LanesNumber { get; set; }
        public int MemorySize { get; set; }
        public int Iterations { get; set; }
        public string Hash { get; set; } = string.Empty;
    }
}
