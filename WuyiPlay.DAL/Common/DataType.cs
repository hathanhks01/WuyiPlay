using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WuyiPlay_DAL.Common
{
    public class DataType
    {
        public enum role
        {
            Admin = 0,
            Collaborator = 1,
            Customer = 2
        }
        public enum UserStatus
        {
            Inactive = 0,
            Active = 1,
            Suspended = 2
        }

        /// <summary>
        /// Trạng thái sản phẩm (tài khoản game)
        /// 0 = Đã bán, 1 = Còn hàng
        /// </summary>
        public enum ProductStatus
        {
            Sold = 0,
            Available = 1
        }

    }
}
