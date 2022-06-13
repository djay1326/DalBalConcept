using Dal;
using Dal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bal.Leaves
{
    public class LeavePageClear : ILeavePageClear
    {
        private readonly HelperlandContextData _DbContext;

        public LeavePageClear(HelperlandContextData DbContext)
        {
            _DbContext = DbContext;
        }

        public List<leave> afterclear(bool isManager, bool isAdmin, bool isEmployee,int userId)
        {
            if (isManager == true)
            {
                var students = ((from s in _DbContext.Users
                                 join u in _DbContext.leave
                                 on s.Id equals u.userid
                                 where s.managerid == userId
                                 select new leave
                                 {
                                     leaveid = u.leaveid,
                                     fromDate = u.fromDate,
                                     toDate = u.toDate,
                                     statusid = u.statusid,
                                     userid = u.userid,
                                     roleid = u.roleid
                                 }).Union(
                                    from s in _DbContext.Users
                                    join u in _DbContext.leave
                                    on s.Id equals u.userid
                                    where s.Id == userId
                                    select new leave
                                    {
                                        leaveid = u.leaveid,
                                        fromDate = u.fromDate,
                                        toDate = u.toDate,
                                        statusid = u.statusid,
                                        userid = u.userid,
                                        roleid = u.roleid
                                    })).ToList();
                return students;
            }
            else if (isAdmin == true)
            {
                List<leave> leavedates = _DbContext.leave.ToList();
                return leavedates;
            }
            var query = (from x in _DbContext.leave
                         where (x.userid == userId)
                         select x
                         ).ToList();
            return  query;
            
        }
    }
}
