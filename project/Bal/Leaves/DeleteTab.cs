using Dal;
using Dal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bal.Leaves
{
   public class DeleteTab : IDeleteTab // Always remember to add "public" here
    {
        private readonly HelperlandContextData _DbContext;

        public DeleteTab(HelperlandContextData DbContext)
        {
            _DbContext = DbContext;
        }
        public void deleteTab(int i)
        {
            leave x = _DbContext.leave.Where(z => z.leaveid == i).FirstOrDefault();
            _DbContext.leave.Remove(x);
            _DbContext.SaveChanges();
            
            
        }
    }
}
