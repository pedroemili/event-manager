namespace EventHub.Domain.Entities.Users;

public sealed class Role
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public static class Ids
    {
        public static readonly Guid Admin = Guid.Parse("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d");
        public static readonly Guid Organizer = Guid.Parse("b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e");
        public static readonly Guid Staff = Guid.Parse("c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f");
        public static readonly Guid Customer = Guid.Parse("d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a");
    }

    public static class Names
    {
        public const string Admin = "Admin";
        public const string Organizer = "Organizer";
        public const string Staff = "Staff";
        public const string Customer = "Customer";
    }
}