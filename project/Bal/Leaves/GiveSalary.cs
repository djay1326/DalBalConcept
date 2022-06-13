using Dal;
using Dal.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bal.Leaves
{
    public class GiveSalary : IGiveSalary
    {
        private readonly HelperlandContextData _DbContext;       

        public GiveSalary(HelperlandContextData DbContext)
        {
            _DbContext = DbContext;            
        }

        public void paysalary(salary s)
        {
            _DbContext.salary.Add(s);
            _DbContext.SaveChanges();
        }
    }
}
