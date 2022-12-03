using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Common.Models.Data;
using Common.Models.DTO;
using OpenSearch.Net;

namespace Persistence.Utilities;

public class PagingUtilities
{
	public static PageToken ParsePageToken(string encodedPageToken)
    {
        byte[] tokenBytes = Convert.FromBase64String(encodedPageToken);

        var binaryFormatter = new BinaryFormatter();

        using (var stream = new MemoryStream(tokenBytes))
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
            {
                var readObject = reader.Read();

                return readObject as PageToken;
            }
        }
    }
}

