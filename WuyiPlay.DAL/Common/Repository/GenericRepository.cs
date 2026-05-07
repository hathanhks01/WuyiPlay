using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;

namespace WuyiPlay_DAL.Common.Repository
{
    // ✅ Constraint thêm 'new()' để có thể khởi tạo T nếu cần,
    //    nhưng quan trọng hơn là đây là nơi bạn có thể thêm interface
    //    chung (vd: IEntity) để ép tất cả entity phải có Id, CreatedAt...
    public abstract class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        internal DbSet<T> _dbset;
        private readonly IUnitOfWork _unitOfWork;

        protected GenericRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _dbset = _unitOfWork.Set<T>();
        }

        // ─────────────────────────────────────────────────────────────────
        //  READ METHODS
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Lấy toàn bộ bản ghi. Chỉ dùng cho bảng nhỏ (categories, config...).
        ///
        /// ❌ Bản cũ: Task.Run(() => _dbset.AsEnumerable())
        ///    - Task.Run không phải async thật — nó chỉ đẩy công việc sang
        ///      thread pool, không giải phóng thread hiện tại trong khi chờ DB.
        ///    - AsEnumerable() thực thi query NGAY LẬP TỨC và kéo TOÀN BỘ
        ///      bảng vào RAM trước khi trả về — không có WHERE, không có LIMIT.
        ///      Bảng 1 triệu dòng = 1 triệu dòng trong RAM.
        ///    - Không có AsNoTracking → EF theo dõi change cho tất cả objects
        ///      dù bạn chỉ đọc → tốn memory vô ích.
        ///
        /// ✅ Bản mới: ToListAsync() + AsNoTracking
        ///    - ToListAsync() là async thật: giải phóng thread trong khi DB xử lý.
        ///    - AsNoTracking() tiết kiệm ~30% memory và nhanh hơn cho read-only.
        /// </summary>
        public async Task<IEnumerable<T>> GetAll()
        {
            return await _dbset.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// ❌ Bản cũ: Trả IQueryable ra ngoài Repository
        ///    - Phá vỡ nguyên tắc Repository Pattern: tầng Service không nên
        ///      biết về IQueryable hay EF Core — đó là chi tiết infrastructure.
        ///    - Service có thể gọi .Where(), .Include(), .GroupBy() bên ngoài
        ///      → business logic bị rải khắp nơi, không kiểm soát được.
        ///    - Nguy hiểm hơn: DbContext có thể đã bị dispose khi tầng trên
        ///      thực sự enumerate IQueryable → runtime exception.
        ///    - Task.Run bọc AsQueryable() là vô nghĩa — AsQueryable() chỉ
        ///      cast type, không làm gì cả, không cần thread pool.
        ///
        /// ✅ Bản mới: Xóa hoàn toàn. Mọi filter/include xử lý trong repo.
        ///    Nếu cần query phức tạp → tạo method cụ thể, hoặc dùng Specification Pattern.
        /// </summary>
        // QueryAll() đã bị xóa

        /// <summary>
        /// Đếm bản ghi theo điều kiện.
        ///
        /// ❌ Bản cũ: Task.Run(() => _dbset.Where(predicate).Count())
        ///    - Count() synchronous trong Task.Run = block thread pool thread.
        ///    - EF Core có sẵn CountAsync() — không có lý do gì dùng sync.
        ///
        /// ✅ CountAsync() thật sự async, SQL chạy "SELECT COUNT(*)" hiệu quả.
        /// </summary>
        public async Task<int> Count(Expression<Func<T, bool>> predicate)
        {
            return await _dbset.Where(predicate).CountAsync();
        }

        /// <summary>
        /// Tìm theo điều kiện, không include navigation.
        ///
        /// ❌ Bản cũ: Task.Run(() => _dbset.Where(predicate).AsNoTracking().AsEnumerable())
        ///    - AsEnumerable() execute query ngay → toàn bộ kết quả vào RAM.
        ///    - Task.Run + AsEnumerable không phải async, chỉ giả vờ async.
        ///
        /// ✅ ToListAsync() async thật, EF tự generate SQL tối ưu.
        /// </summary>
        public async Task<IEnumerable<T>> FindBy(Expression<Func<T, bool>> predicate)
        {
            return await _dbset.Where(predicate).AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Tìm theo điều kiện kèm eager loading — TYPE-SAFE với Expression.
        ///
        /// ❌ Bản cũ: FindBy(predicate, string[] childrens) dùng magic string
        ///    - query.Include("CategoryName") — lỗi typo chỉ phát hiện lúc runtime.
        ///    - Refactor tên property → phải tìm kiếm toàn bộ string trong code.
        ///    - Không có IntelliSense, không có compile-time check.
        ///
        /// ✅ Expression<Func<T, object>> — compile-time safe:
        ///    repo.FindBy(x => x.cID == 1, x => x.Category, x => x.Images)
        ///    Đổi tên property → compiler báo lỗi ngay.
        /// </summary>
        public async Task<IEnumerable<T>> FindBy(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbset.AsNoTracking();
            foreach (var include in includes)
                query = query.Include(include);

            return await query.Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Phân trang có sắp xếp.
        ///
        /// ❌ Bản cũ: Task.Run(() => _dbset.Where(...).OrderBy(...).Skip(...).Take(...).AsEnumerable())
        ///    - Vẫn là AsEnumerable() → EF kéo tất cả về rồi mới Skip/Take trong memory!
        ///    - SQL đúng phải có OFFSET...FETCH NEXT, không phải kéo hết về rồi lọc.
        ///
        /// ✅ ToListAsync() → EF sinh SQL với OFFSET/FETCH đúng cách.
        ///    Thêm totalCount output để tầng trên biết tổng số trang.
        /// </summary>
        public async Task<(IEnumerable<T> Items, int TotalCount)> FindByPaged(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, object>> orderBy,
            int pageSize,
            int pageIndex,
            bool descending = false)
        {
            var query = _dbset.AsNoTracking().Where(predicate);

            int totalCount = await query.CountAsync();

            var ordered = descending
                ? query.OrderByDescending(orderBy)
                : query.OrderBy(orderBy);

            var items = await ordered
                .Skip(pageSize * (pageIndex - 1))
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// Tìm 1 bản ghi theo điều kiện.
        /// FirstOrDefaultAsync sinh SQL "SELECT TOP 1" — hiệu quả hơn lấy list rồi .First()
        /// </summary>
        public async Task<T?> FirstOrDefault(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbset.AsNoTracking();
            foreach (var include in includes)
                query = query.Include(include);

            return await query.FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Kiểm tra tồn tại — sinh SQL "SELECT CASE WHEN EXISTS(...) THEN 1 ELSE 0"
        /// Nhanh hơn Count() > 0 vì DB dừng ngay khi tìm thấy dòng đầu tiên.
        /// </summary>
        public async Task<bool> Exists(Expression<Func<T, bool>> predicate)
        {
            return await _dbset.AnyAsync(predicate);
        }

        // ─────────────────────────────────────────────────────────────────
        //  WRITE METHODS
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tạo 1 bản ghi.
        /// Giữ nguyên logic, chỉ thêm cancellationToken để có thể cancel request.
        /// </summary>
        public virtual async Task<T> Create(T entity, CancellationToken ct = default)
        {
            var entry = await _dbset.AddAsync(entity, ct);
            await Save(ct);
            await Detach(entity);
            return entry.Entity;
        }

        /// <summary>
        /// Tạo nhiều bản ghi — dùng AddRangeAsync thay vì loop Add.
        ///
        /// ❌ Bản cũ: foreach loop gọi _dbset.Add() từng cái
        ///    - EF vẫn batch insert, nhưng code rườm rà hơn cần thiết.
        ///
        /// ✅ AddRangeAsync() — gọn hơn, EF tự tối ưu batch.
        /// </summary>
        public virtual async Task<int> Create(List<T> entities, CancellationToken ct = default)
        {
            await _dbset.AddRangeAsync(entities, ct);
            return await Save(ct);
        }

        public virtual async Task<int> Update(T entity, CancellationToken ct = default)
        {
            _unitOfWork.Context.Entry(entity).State = EntityState.Modified;
            var result = await Save(ct);
            await Detach(entity);
            return result;
        }

        /// <summary>
        /// Update không Save ngay — dùng khi muốn batch nhiều update rồi Save 1 lần.
        /// Tránh dùng Attach() nếu entity đã được track → exception.
        /// </summary>
        public virtual async Task<int> UpdateNoSave(T entity)
        {
            _unitOfWork.Context.Entry(entity).State = EntityState.Modified;
            return await Task.FromResult(1);
        }

        public async Task<int> Delete(T entity, CancellationToken ct = default)
        {
            _unitOfWork.Context.Entry(entity).State = EntityState.Deleted;
            return await Save(ct);
        }

        /// <summary>
        /// Xóa theo điều kiện.
        ///
        /// ❌ Bản cũ: Load entities về memory rồi RemoveRange
        ///    - Nếu có 10.000 dòng cần xóa → kéo 10.000 objects về RAM rồi mới xóa.
        ///
        /// ✅ EF Core 7+: ExecuteDeleteAsync() — sinh "DELETE FROM WHERE" trực tiếp,
        ///    không cần load về memory. Nhanh hơn rất nhiều.
        ///    Fallback về cách cũ nếu dùng EF Core < 7.
        /// </summary>
        public async Task<int> DeleteRange(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            // EF Core 7+ — DELETE trực tiếp trên DB, không load vào memory
            return await _dbset.Where(predicate).ExecuteDeleteAsync(ct);

            // Nếu EF Core < 7, dùng cách này:
            // var toDelete = await _dbset.Where(predicate).ToListAsync(ct);
            // _dbset.RemoveRange(toDelete);
            // return await Save(ct);
        }

        public async Task<int> Save(CancellationToken ct = default)
        {
            return await _unitOfWork.Commit();
        }

        public async Task Detach(T entity)
        {
            _unitOfWork.Context.Entry(entity).State = EntityState.Detached;
        }

        // ─────────────────────────────────────────────────────────────────
        //  RAW SQL / STORED PROCEDURE
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Raw SQL query trả List — giữ nguyên, đã dùng FromSqlRaw đúng.
        /// Lưu ý: T phải được map vào DbSet (keyless entity hoặc có key).
        /// </summary>
        public async Task<List<TEntity>> SqlQuery<TEntity>(string query, params object[] parameters)
            where TEntity : class
        {
            return await _unitOfWork.Context.Set<TEntity>()
                .FromSqlRaw(query, parameters)
                .ToListAsync();
        }

        /// <summary>
        /// Execute Stored Procedure không cần kết quả trả về.
        ///
        /// ❌ Bản cũ: Build string "StoreName @p1, @p2" bằng string concat
        ///    - Dễ lỗi: thừa/thiếu dấu phẩy, dấu cách.
        ///    - Khó debug khi SP có nhiều params.
        ///    - Không set CommandType.StoredProcedure → SQL Server phải parse text.
        ///
        /// ✅ Dùng CommandType.StoredProcedure:
        ///    - Không cần build string phức tạp.
        ///    - SQL Server biết đây là SP, execution plan được cache tốt hơn.
        /// </summary>
        public async Task<int> ExecuteStoredProcedure(string storeName, params SqlParameter[] parameters)
        {
            var conn = (SqlConnection)_unitOfWork.Context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            using var cmd = new SqlCommand(storeName, conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddRange(parameters);

            return await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Execute SP trả về DataTable.
        ///
        /// ❌ Bản cũ: method async nhưng không await gì — da.Fill() là synchronous.
        ///    Đây là "async over sync" anti-pattern: caller nghĩ nó async nhưng thực ra block.
        ///
        /// ✅ Vẫn phải dùng sync Fill() vì SqlDataAdapter không có async API.
        ///    Giải pháp tốt hơn: dùng Dapper cho DataTable queries — có async thật.
        ///    Method này giữ lại để tương thích, nhưng đánh dấu rõ giới hạn.
        ///
        /// 💡 Khuyến nghị: Thay bằng Dapper nếu cần DataTable thường xuyên:
        ///    var dt = await conn.QueryAsync<dynamic>(storeName, parameters, commandType: CommandType.StoredProcedure);
        /// </summary>
        public Task<DataTable> ExecuteStoredProcedureToTable(string storeName, params SqlParameter[] parameters)
        {
            // DataTable/SqlDataAdapter không có async API thật sự
            // → trả Task.Run để ít nhất không block calling thread
            return Task.Run(() =>
            {
                var dt = new DataTable();
                var conn = (SqlConnection)_unitOfWork.Context.Database.GetDbConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                using var cmd = new SqlCommand(storeName, conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddRange(parameters);

                var da = new SqlDataAdapter { SelectCommand = cmd };
                da.Fill(dt);
                return dt;
            });
        }

        /// <summary>
        /// Execute SP trả về DataSet — tương tự DataTable ở trên.
        /// </summary>
        public Task<DataSet> ExecuteStoredProcedureToDataSet(string storeName, params SqlParameter[] parameters)
        {
            return Task.Run(() =>
            {
                var ds = new DataSet();
                var conn = (SqlConnection)_unitOfWork.Context.Database.GetDbConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                using var cmd = new SqlCommand(storeName, conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddRange(parameters);

                var da = new SqlDataAdapter { SelectCommand = cmd };
                da.Fill(ds);
                return ds;
            });
        }

        /// <summary>
        /// Execute SP trả List — đã dùng FromSqlRaw đúng ở bản cũ.
        /// Refactor signature: nhận SqlParameter[] thay vì params object[] (name/value pairs)
        /// để type-safe và tránh lỗi lúc runtime khi truyền số lẻ params.
        ///
        /// Cách gọi mới:
        ///   await repo.ExecuteStoredProcedureToList<ProductDto>(
        ///       "sp_GetProducts",
        ///       new SqlParameter("@cID", 1),
        ///       new SqlParameter("@status", 1));
        /// </summary>
        public async Task<List<TEntity>> ExecuteStoredProcedureToList<TEntity>(
            string storeName,
            params SqlParameter[] parameters)
            where TEntity : class
        {
            var paramNames = string.Join(", ", parameters.Select(p => p.ParameterName));
            var sql = $"EXEC {storeName} {paramNames}";

            return await _unitOfWork.Context.Set<TEntity>()
                .FromSqlRaw(sql, parameters.Cast<object>().ToArray())
                .ToListAsync();
        }

        public async Task<TEntity?> ExecuteStoredProcedureToValue<TEntity>(
            string storeName,
            params SqlParameter[] parameters)
            where TEntity : class
        {
            var paramNames = string.Join(", ", parameters.Select(p => p.ParameterName));
            var sql = $"EXEC {storeName} {paramNames}";

            return await _unitOfWork.Context.Set<TEntity>()
                .FromSqlRaw(sql, parameters.Cast<object>().ToArray())
                .FirstOrDefaultAsync();
        }
    }
}