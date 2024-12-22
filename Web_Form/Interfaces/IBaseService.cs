using Web_Form.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Web_Form.Interfaces
{
    public interface IBaseService<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAll();
        Task<TEntity> FindByIdAsync(int id);
        Task<TEntity> GetInsertedObjByAsync(TEntity entity);
        Task<bool> InsertByAsync(TEntity entity);
        Task<bool> InsertRangeByAsync(IEnumerable<TEntity> entities);
        Task<bool> UpdateByAsync(TEntity entity);
        Task<bool> UpdateRangeByAsync(IEnumerable<TEntity> entities);
        Task<bool> DeleteByAsync(TEntity entity);
        Task<bool> DeleteRangeByAsync(IEnumerable<TEntity> entities);
        Task<TEntity> GetSingleData(Expression<Func<TEntity, bool>> Condition);

    }
}