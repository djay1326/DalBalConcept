using Dal;
using Dal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bal.Leaves
{
    public class AddSalary : IAddSalary
    {
        private readonly HelperlandContextData _DbContext;

        public AddSalary(HelperlandContextData DbContext)
        {
            _DbContext = DbContext;
        }

        public List<Users> addnewsalary(bool isManager, bool isAdmin, bool isEmployee, int userId)
        {
            if (isManager == true)
            {

                //var query = (from x in _DbContext.Users
                //             where x.manager == y
                //             select x).ToList();
                //(s.fromDate.Value.Date.Month == DateTime.Now.Date.Month)
                var query1 = (from x in _DbContext.Users
                              where x.managerid == userId
                              select new Users
                              {
                                  Id = x.Id,
                                  UserName = x.UserName
                              }).Distinct().ToList();
                var query = (from x in _DbContext.Users
                             join s in _DbContext.salary
                             on x.Id equals s.userid
                             where (x.managerid == userId) && (s.createddate.Value.Date.Month == DateTime.Now.Date.Month) && (s.createddate.Value.Date.Year == DateTime.Now.Date.Year)
                             select new Users
                             {
                                 Id = x.Id,
                                 UserName = x.UserName
                             }
                             ).ToList();
                if (query.Count == 0)
                {
                    var query2 = _DbContext.Users.Where(x => x.managerid == userId).ToList();
                    //ViewBag.msg = query2;
                    //return View();
                    return query2;
                }
                else if (query.Count != 0)
                {
                    // var result = query1.Except(query).ToList();
                    //var result = query1.RemoveAll(x => query.Contains(x));
                    foreach (var item in query1.ToList())
                    {
                        if (query.Any(x => x.Id == item.Id))
                        {
                            query1.Remove(item);
                        }
                    }
                    //ViewBag.msg = query1;
                    //return View();
                    return query1;
                }
            }
            else if (isAdmin == true)
            {
                var query11 = (from x in _DbContext.Users
                               where x.managerid == userId
                               select new Users
                               {
                                   Id = x.Id,
                                   UserName = x.UserName
                               }).Distinct().ToList();
                var queryy = (from x in _DbContext.Users
                              join s in _DbContext.salary
                              on x.Id equals s.userid
                              where (x.managerid == userId) && (s.createddate.Value.Date.Month == DateTime.Now.Date.Month) && (s.createddate.Value.Date.Year == DateTime.Now.Date.Year)
                              select new Users
                              {
                                  Id = x.Id,
                                  UserName = x.UserName
                              }
                         ).ToList();
                if (queryy.Count == 0)
                {
                    var query2 = _DbContext.Users.Where(x => x.managerid == userId).ToList();
                    //ViewBag.msg = query2;
                    //return View();
                    return query2;
                }
                else if (queryy.Count != 0)
                {
                    // var result = query1.Except(query).ToList();
                    //var result = query1.RemoveAll(x => query.Contains(x));
                    foreach (var item in query11.ToList())
                    {
                        if (queryy.Any(x => x.Id == item.Id))
                        {
                            query11.Remove(item);
                        }
                    }
                    //ViewBag.msg = query11;
                    //return View();
                    return query11;
                }
            }
            return null;
        }
    }
}
