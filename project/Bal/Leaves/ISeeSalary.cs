using Dal;
using Dal.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bal.Leaves
{
    public interface ISeeSalary
    {
        List<salary> viewsalary(bool isManager,int userId,DateTime monthyear, HelperlandContextData _DbContext);
    }
}
