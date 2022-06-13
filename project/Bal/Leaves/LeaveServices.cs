using Dal.Data;
using Dal.Models;
using System;
//using Dal.Models;
using Dal.Models;
using Dal;
//using project.Data;
//using project.Data;

namespace Bal
{
    public class LeaveServices : ILeaveServices
    {
        private readonly HelperlandContextData _DbContext;
        //private readonly ILeaveServices _leaveservices;

        public LeaveServices(HelperlandContextData DbContext)
        {
            _DbContext = DbContext;
            //_leaveservices = leaveservices;
        }
        public void leavesave(leave saveleavedata)
        {
            _DbContext.leave.Add(saveleavedata);
            _DbContext.SaveChanges();
        }
    }
}
