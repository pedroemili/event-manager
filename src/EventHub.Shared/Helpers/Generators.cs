namespace EventHub.Shared.Helpers;

public static class SlugHelper
{
    public static string Generate(string text)
    {
        return text.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');
    }
}

public static class OrderNumberGenerator
{
    public static string Generate()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
    }
}

public static class TicketNumberGenerator
{
    public static string Generate(string eventPrefix, int sequence)
    {
        return $"TKT-{eventPrefix}-{sequence:D6}";
    }
}