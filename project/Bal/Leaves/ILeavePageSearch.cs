using Dal.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bal.Leaves
{
    public interface ILeavePageSearch
    {
        List<leave> leavepage(DateTime startdate, DateTime enddate, int? statusids, int userId, bool isManager, bool isAdmin, bool isEmployee);
    }
}
