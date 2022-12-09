using Common.Models.Configuration;
using Common.Models.Data;
using Common.Models.DTO;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;

namespace Persistence.Repositories;

public class UserRepository : BaseRepository<UserEntity, UserDTO>, IUserRepository
{
	public UserRepository(IOpenSearchSettings settings) : base(settings) {}

    protected override UserEntity MapToEntity(UserDTO dto)
    {
        return new UserEntity
        {
            first_name = dto.FirstName,
            last_name = dto.LastName,
            email_address = dto.EmailAddress
        };
    }

    protected override UserDTO MapToDTO(UserEntity dto)
    {
        return new UserDTO
        {
            Id = dto.id,
            CreatedOn = dto.created_on,
            FirstName = dto.first_name,
            LastName = dto.last_name,
            EmailAddress = dto.email_address
        };
    }
}

