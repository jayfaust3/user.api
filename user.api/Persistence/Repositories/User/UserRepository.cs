using Common.Models.Context;
using Common.Models.Data;
using Common.Models.DTO;
using Clients.ElasticSearch;

namespace Persistence.Repositories;

public class UserRepository : BaseRepository<UserEntity, UserDTO>, IUserRepository
{
	public UserRepository
    (
        IElasticSearchClient client,
        IUserContext userContext
    ) : base
    (
        client,
        userContext
    ) {}

    protected override UserEntity MapToEntity(UserDTO dto) => new UserEntity
    {
        first_name = dto.FirstName,
        last_name = dto.LastName,
        email_address = dto.EmailAddress
    };

    protected override UserDTO MapToDTO(UserEntity dto) => new UserDTO
    {
        Id = dto.id,
        CreatedOn = dto.created_on,
        FirstName = dto.first_name,
        LastName = dto.last_name,
        EmailAddress = dto.email_address
    };
}
