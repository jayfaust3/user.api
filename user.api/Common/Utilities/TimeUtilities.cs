namespace Common.Utilities;

public class TimeUtilities
{
    public static long GetUnixEpoch() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}
