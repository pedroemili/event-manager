namespace EventHub.Domain.Entities.Users;

public sealed class Role
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public static class Names
    {
        public const string Admin = "Admin";
        public const string Organizer = "Organizer";
        public const string Staff = "Staff";
        public const string Customer = "Customer";
    }
}