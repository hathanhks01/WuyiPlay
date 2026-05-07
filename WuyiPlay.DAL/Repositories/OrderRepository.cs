using WuyiPlay_DAL.Common.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WuyiPlay_DAL.Models;

namespace WuyiPlay_DAL.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IGenericRepository<Order>
    {
        public OrderRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
