namespace Common.Models.Configuration;

public interface IOpenSearchSettings
{
    IEnumerable<string> NodeURIs { get; }
    string IndexName { get; }
}

