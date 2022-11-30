﻿using Common.Models.Configuration;
using Common.Models.Data;
using Common.Models.DTO;
using OpenSearch.Client;

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

    protected override ISearchRequest GenerateFindAllSearchRequest(PageToken<UserDTO> pageToken)
    {
        return new SearchRequest<UserDTO>()
        {
            From = pageToken.Cursor,
            Size = pageToken.Limit,
            //Query = new MatchQuery
            //{
            //    Field = Infer.Field<UserDTO>(f => f.Id),
            //    Query = entityLike.Id?.ToString()
            //}
        };
    }

    
}

