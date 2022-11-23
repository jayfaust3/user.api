using Common.Models.Configuration;
using Common.Models.DTO;
using OpenSearch.Client;

namespace Persistence.Repositories;

public class UserRepository : BaseRepository<UserDTO>
{
	public UserRepository(IOpenSearchSettings settings) : base(settings) {}

    protected override ISearchRequest GenerateSearchRequest(UserDTO entityLike)
    {
        return new SearchRequest();
    }
}

