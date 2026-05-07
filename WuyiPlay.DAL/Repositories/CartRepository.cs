using WuyiPlay_DAL.Common.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WuyiPlay_DAL.Models;

namespace WuyiPlay_DAL.Repositories
{
    public class CartRepository : GenericRepository<Cart>, IGenericRepository<Cart>
    {
        public CartRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
