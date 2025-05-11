using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperShop.Bll
{
    public interface IUserInfoProvider
    {
        string UserId { get; }
        DateTime DateOfBirth { get; }
    }
}
