namespace Common.Utilities;

public class TimeUtilities
{
    public static int GetUnixEpoch()
    {
        return (int)
        (
            DateTime.Now.ToUniversalTime() -
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        ).TotalMilliseconds;
    }
}

