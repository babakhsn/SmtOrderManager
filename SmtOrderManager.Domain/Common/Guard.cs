namespace SmtOrderManager.Domain.Common;

public static class Guard
{
    public static string NotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{paramName} must not be empty.");
        return value.Trim();
    }

    public static int Positive(int value, string paramName)
    {
        if (value <= 0)
            throw new DomainException($"{paramName} must be > 0.");
        return value;
    }

    public static double Positive(double value, string paramName)
    {
        if (value <= 0)
            throw new DomainException($"{paramName} must be > 0.");
        return value;
    }
}
