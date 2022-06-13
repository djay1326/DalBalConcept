using Dal.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bal.Leaves
{
    public interface IAddSalary
    {
        List<Users> addnewsalary(bool isManager, bool isAdmin, bool isEmployee, int userId);
    }
}
