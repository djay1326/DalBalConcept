using Dal.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bal.Leaves
{
    public interface ILeavePageHome
    {
        List<leave> leavepagehomedata(bool isManager,bool isAdmin,bool isEmployee,int userId);
    }
}
