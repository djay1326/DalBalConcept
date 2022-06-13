using Dal;
using Dal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bal.Leaves
{
    public class LeavePageHome : ILeavePageHome
    {
        private readonly HelperlandContextData _DbContext;

        public LeavePageHome(HelperlandContextData DbContext)
        {
            _DbContext = DbContext;
        }

        public List<leave> leavepagehomedata(bool isManager, bool isAdmin, bool isEmployee, int userId)
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
                //List<UserAddress> u = _DbContext.UserAddress.Where(x => x.UserId == ty).ToList();
                List<leave> l = _DbContext.leave.ToList();
                return l;
            }
            else
            {
                var students = _DbContext.leave.Where(x => x.userid == userId).ToList();
                //var students = (from s in _DbContext.leave
                //                where s.userid == userId
                //                select s);
                return students;
            }
        }
    }
}
