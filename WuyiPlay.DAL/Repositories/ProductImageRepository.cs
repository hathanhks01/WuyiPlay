using WuyiPlay_DAL.Common.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WuyiPlay_DAL.Models;

namespace WuyiPlay_DAL.Repositories
{
    public class ProductImageRepository : GenericRepository<ProductImage>, IGenericRepository<ProductImage>
    {
        public ProductImageRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
