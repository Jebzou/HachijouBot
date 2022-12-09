namespace HachijouBot.Models
{
    public abstract class DatabaseModel
    {
        public string Id { get; } = Guid.NewGuid().ToString();
    }
}
