﻿using Common.Models.DTO;

namespace Application.Services.Crud
{
    public interface ICrudService<TDTO> where TDTO : class, IDTO
    {
        Task<TDTO> CreateAsync(TDTO recordToCreate);
        Task<TDTO> UpdateAsync(TDTO recordToUpdate);
        Task<IEnumerable<TDTO>> GetAllAsync(string pageToken);
        Task<TDTO?> GetByIdAsync(Guid id);
        Task DeleteByIdAsync(Guid id);
    }
}