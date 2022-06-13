using Dal;
using Dal.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace Bal.Leaves
{
    public class AdminaddsEmp : IAdminaddsEmp
    {
        private readonly HelperlandContextData _DbContext;

        public AdminaddsEmp(HelperlandContextData DbContext)
        {
            _DbContext = DbContext;
        }

        public List<mixed> addempadmin()
        {
            var query = (from t in _DbContext.Users
                         join r in _DbContext.UserRoles
                         on t.Id equals r.UserId
                         where r.RoleId == 2
                         select new mixed
                         {
                             username = t.UserName,
                             userid = t.Id
                         }
                 ).ToList();
            return query;
        }
    }
}
