using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace WuyiPlay_DAL.Common.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();

        Task<int> Count(Expression<Func<T, bool>> predicate);

        Task<bool> Exists(Expression<Func<T, bool>> predicate);

        Task<T?> FirstOrDefault(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes);

        Task<IEnumerable<T>> FindBy(
            Expression<Func<T, bool>> predicate);

        Task<IEnumerable<T>> FindBy(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes);   // type-safe includes

        Task<(IEnumerable<T> Items, int TotalCount)> FindByPaged(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, object>> orderBy,
            int pageSize,
            int pageIndex,
            bool descending = false);

        // ── Write ─────────────────────────────────────────────────────────
        Task<T> Create(T entity, CancellationToken ct = default);
        Task<int> Create(List<T> entities, CancellationToken ct = default);
        Task<int> Update(T entity, CancellationToken ct = default);
        Task<int> UpdateNoSave(T entity);
        Task<int> Delete(T entity, CancellationToken ct = default);
        Task<int> DeleteRange(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<int> Save(CancellationToken ct = default);

        // ── Raw SQL / SP ──────────────────────────────────────────────────
        Task<List<TEntity>> SqlQuery<TEntity>(string query, params object[] parameters)
            where TEntity : class;

        Task<int> ExecuteStoredProcedure(string storeName, params SqlParameter[] parameters);

        Task<DataTable> ExecuteStoredProcedureToTable(string storeName, params SqlParameter[] parameters);

        Task<DataSet> ExecuteStoredProcedureToDataSet(string storeName, params SqlParameter[] parameters);

        Task<List<TEntity>> ExecuteStoredProcedureToList<TEntity>(string storeName, params SqlParameter[] parameters)
            where TEntity : class;

        Task<TEntity?> ExecuteStoredProcedureToValue<TEntity>(string storeName, params SqlParameter[] parameters)
            where TEntity : class;
    }
}
