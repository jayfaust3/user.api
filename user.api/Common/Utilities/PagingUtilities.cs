using System.Text;
using System.Text.Json;
using Common.Models.Data;

namespace Common.Utilities;

public class PagingUtilities
{
	public static PageToken ParsePageToken(string encodedPageToken)
    {
        byte[] tokenBytes = Convert.FromBase64String(encodedPageToken);

        var tokenJson = Encoding.UTF8.GetString(tokenBytes);

        return JsonSerializer.Deserialize<PageToken>(tokenJson);
    }

    public static string GetDefaultPageToken(int limit = 25)
    {
        var token = new PageToken
        {
            Limit = limit,
            Cursor = 0,
        };

        var tokenJson = JsonSerializer.Serialize(token);

        var tokenBytes = Encoding.UTF8.GetBytes(tokenJson);

        return Convert.ToBase64String(tokenBytes);
    }
}
