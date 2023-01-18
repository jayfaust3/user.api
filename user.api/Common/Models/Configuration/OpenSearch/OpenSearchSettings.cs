namespace Common.Models.Configuration;

public class OpenSearchSettings : IOpenSearchSettings
{
    public OpenSearchSettings
    (
        IEnumerable<string> nodeURIs,
        string indexName
    )
    {
        NodeURIs = nodeURIs;
        IndexName = indexName;
    }

    public IEnumerable<string> NodeURIs { get; }
    public string IndexName { get; }
}

