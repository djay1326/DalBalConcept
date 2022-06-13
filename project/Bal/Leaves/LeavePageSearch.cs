using Dal;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Dal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Bal.Leaves
{
    public class LeavePageSearch : ILeavePageSearch
    {
        private readonly HelperlandContextData _DbContext;
        private readonly UserManager<Users> _userManager;

        public LeavePageSearch(HelperlandContextData DbContext, UserManager<Users> userManager)
        {
            _DbContext = DbContext;
            _userManager = userManager;
        }

        //public ClaimsPrincipal User { get; private set; }

        public List<leave> leavepage(DateTime startdate, DateTime enddate, int? statusids,int userId, bool isManager, bool isAdmin, bool isEmployee)
        {
            try
            {
            //int userId = int.Parse(userinfo);
            DateTime xy = DateTime.Parse("01-01-0001 12:00:00 AM");
            DateTime uy = DateTime.Parse("01-01-9999 12:00:00 AM");
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
                if (statusids == null)
                {
                    var result1 = students.Where(x => (x.fromDate >= startdate) && (x.toDate <= enddate)).ToList();
                    //ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
                    //ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
                    return result1;
                }
                else if (startdate == xy && enddate == xy)
                {
                    var result2 = students.Where(x => (x.statusid == statusids)).ToList();
                    //ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
                    //ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
                    return result2;
                }
                var result = students.Where(x => (x.fromDate >= startdate) && (x.toDate <= enddate) && (x.statusid == statusids)).ToList();
                //ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
                //ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
                return result;
            }
            else if (isAdmin == true)
            {
                List<leave> leavedates = _DbContext.leave.ToList();
                if (statusids == null)
                {
                    var result1 = leavedates.Where(x => (x.fromDate >= startdate) && (x.toDate <= enddate)).ToList();
                    //ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
                    //ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
                    return result1;
                }
                else if (startdate == xy && enddate == uy)
                {
                    var result2 = leavedates.Where(x => (x.statusid == statusids)).ToList();
                    if (statusids == 1)
                    {
                        //ViewBag.status = "Pending";
                    }
                    else if (statusids == 2)
                    {
                        //ViewBag.status = "Accepted";
                    }
                    else if (statusids == 3)
                    {
                        //ViewBag.status = "Rejected";
                    }
                    return result2;
                }
                var result = leavedates.Where(x => (x.fromDate >= startdate) && (x.toDate <= enddate) && (x.statusid == statusids)).ToList();
                //ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
                //ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
                return result;
            }
            else if(isEmployee == true)
            {
            List<leave> leavedate = _DbContext.leave.Where(x => x.userid == userId).ToList();
            if (statusids == null)
            {
                //var query1 = (from x in _DbContext.leave
                //              where (x.userid == userId)
                //              select x
                //         ).ToList();
                var query1 = leavedate.Where(x => (x.fromDate >= startdate) && (x.toDate <= enddate)).ToList();
                //var query1 = (from x in _DbContext.leave
                //              where ((x.fromDate >= startdate) && (x.toDate <= enddate) && (x.userid == userId))
                //              select x
                //         ).ToList();
                return query1;
            }
            else
            {
                var query = (from x in _DbContext.leave
                             where ((x.fromDate >= startdate) && (x.toDate <= enddate) && (x.userid == userId) && (x.statusid == statusids))
                             //((x.statusid == searchstatus) && (x.userid == userId)) ||
                             //((x.fromDate >= startdate) && (x.toDate <= enddate) && (x.userid == userId))
                             select x
                             ).ToList();
                return query;
            }
            }
                return null;
            }

            catch
            {
                return null;
            }
        }
    }
}
