using Common.Models.Configuration;
using Common.Models.DTO;
using OpenSearch.Client;

namespace Persistence.Repositories;

public class UserRepository : BaseRepository<UserDTO>, IUserRepository
{
	public UserRepository(IOpenSearchSettings settings) : base(settings) {}


    protected override ISearchRequest GenerateFindAllSearchRequest(UserDTO entityLike)
    {
        return new SearchRequest<UserDTO>()
        //{
        //    From = 0,
        //    Size = 1,
        //    Query = new MatchQuery
        //    {
        //        Field = Infer.Field<UserDTO>(f => f.Id),
        //        Query = entityLike.Id?.ToString()
        //    }
        //}
        ;
    }

    
}

