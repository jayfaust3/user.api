namespace Common.Models.Configuration;

public interface IElasticsSearchSettings
{
    IEnumerable<string> NodeURIs { get; }
    string IndexName { get; }
}

