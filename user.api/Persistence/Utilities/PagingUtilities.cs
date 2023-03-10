using System.Text;
using System.Text.Json;
using Common.Models.Data;
using Common.Models.DTO;

namespace Persistence.Utilities;

public class PagingUtilities
{
	public static PageToken ParsePageToken(string encodedPageToken)
    {
        byte[] tokenBytes = Convert.FromBase64String(encodedPageToken);

        var tokenJson = Encoding.UTF8.GetString(tokenBytes);

        return JsonSerializer.Deserialize<PageToken>(tokenJson);
    }
}

