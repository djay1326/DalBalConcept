using Dal;
using Dal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bal.Leaves
{
    public class SeeSalary :ISeeSalary
    {
        private readonly HelperlandContextData _DbContext;

        public SeeSalary(HelperlandContextData DbContext)
        {
            _DbContext = DbContext;
        }

        public List<salary> viewsalary(bool isManager, int userId, DateTime monthyear, HelperlandContextData _DbContext)
        {
            if (isManager == true)
            {
                var data = (from u in _DbContext.Users
                            join s in _DbContext.salary
                            on u.Id equals s.userid
                            where u.managerid == userId
                            select s).ToList();
                var result1 = data.Where(x => x.createddate == monthyear).ToList();
                //ViewBag.msg1 = monthyear.ToString("yyyy-MM");
                return result1;
            }
            var query = _DbContext.salary.Where(x =>x.createddate == monthyear).ToList();
            //var query = (from s in _DbContext.salary
            //             where s.createddate == monthyear
            //             select s).ToList();
            //ViewBag.msg1 = monthyear.ToString("yyyy-MM");
            return query;
        }
    }
}
