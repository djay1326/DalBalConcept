using Dal;
using Dal.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bal.Leaves
{
    public class UpdateLeave : IUpdateLeave
    {
        private readonly HelperlandContextData _DbContext;

        public UpdateLeave(HelperlandContextData DbContext)
        {
            _DbContext = DbContext;
        }

        public void updateleave([FromBody] leave change)
        {
            leave z = _DbContext.leave.Where(x => x.leaveid == change.leaveid).FirstOrDefault();
            z.fromDate = change.fromDate;
            z.toDate = change.toDate;
            z.reason = change.reason;
            _DbContext.leave.Update(z);
            _DbContext.SaveChanges();
        }
    }
}
