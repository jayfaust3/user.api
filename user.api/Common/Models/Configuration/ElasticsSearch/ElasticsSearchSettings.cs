namespace Common.Models.Configuration;

public class ElasticsSearchSettings : IElasticsSearchSettings
{
    public ElasticsSearchSettings
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

