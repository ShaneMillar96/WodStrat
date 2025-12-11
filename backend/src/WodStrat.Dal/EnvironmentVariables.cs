namespace WodStrat.Dal;

public static class EnvironmentVariables
{
    public static string DbConnectionString => GetRequired("DB_CONNECTION_STRING");

    private static string GetRequired(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Required environment variable '{name}' is not set.");
        }
        return value;
    }

    private static string GetOptional(string name, string defaultValue = "")
    {
        return Environment.GetEnvironmentVariable(name) ?? defaultValue;
    }
}
