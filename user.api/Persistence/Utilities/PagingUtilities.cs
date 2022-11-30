using System.Runtime.Serialization.Formatters.Binary;
using Common.Models.Data;
using Common.Models.DTO;
using OpenSearch.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Persistence.Utilities;

public class PagingUtilities
{
	public static PageToken<TData> ParsePageToken<TData>(string encodedPageToken) where TData : class, IDTO
    {
        PageToken<TData> parsedToken = null;

        byte[] tokenBytes = Convert.FromBase64String(encodedPageToken);

        var binaryFormatter = new BinaryFormatter();

        using (var ms = new MemoryStream(tokenBytes))
        {
            object obj = binaryFormatter.Deserialize(ms);
            parsedToken = obj as PageToken<TData>;
        }

        return parsedToken;
    }
}

