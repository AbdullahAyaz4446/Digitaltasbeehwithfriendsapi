using DigitalTasbeehWithFriendsApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Web;
//using System.Web.Http.Cors;
using System.Web.Http;

namespace DigitalTasbeehWithFriendsApi.Controllers
{
    //[EnableCors(origins: "*", headers: "*", methods:"*")]
    public class RequestController : ApiController
    { 
        DTWFEntities Db = new DTWFEntities();
        //Group Members fucntion you want to distribute tasbeeh
        [HttpGet]
        public HttpResponseMessage ShowGroupmembers(int groupid)
        {
            try
            {
                var members = Db.GroupUsers.Join(Db.Groups, gu => gu.Group_id, g => g.ID, (gu, g) => new
                {
                    GroupUser = gu,
                    Group = g
                }).Join(Db.Users, gu => gu.GroupUser.Members_id, u => u.ID, (gu, u) => new
                {
                    GroupUser = gu.GroupUser,
                    Group = gu.Group,
                    User = u 
                }).Where(res => res.Group.ID == groupid).Select(res => new
                {
                    Groupid = res.Group.ID,
                    GroupTitle = res.Group.Group_Title,
                    Admin = res.Group.Admin_id,
                    Memberid=res.User.ID,
                    Memmber = res.User.Username
                }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, members);
            }
            catch (Exception ex) { 
            return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }
        //Tasbeeh Distribute Manully Send Request Function
        [HttpPost]
        public HttpResponseMessage DistributeTasbeehManually()
        {
            try
            {
              
                var form = HttpContext.Current.Request.Form;

                int groupid = int.Parse(form["groupid"]); 

               
                List<int> id = JsonConvert.DeserializeObject<List<int>>(form["id"]);
                List<int> count = JsonConvert.DeserializeObject<List<int>>(form["count"]);

                var adminGroup = Db.Groups.FirstOrDefault(g => g.ID == groupid);
                if (adminGroup == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Group not found.");
                }

                var tasbeehGoal = Db.GroupTasbeeh 
                    .FirstOrDefault(a => a.Group_id == groupid);
                if (tasbeehGoal == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No active tasbeeh goal found for this group.");
                }

                for (int i = 0; i < id.Count; i++)
                {
                    int memberId = id[i];
                    int assignedCount = count[i];
                    bool isAdmin = memberId == adminGroup.Admin_id;

                    var newRequest = new Request
                    {
                        Tasbeeh_Id = tasbeehGoal.ID,
                        Sender_id = adminGroup.Admin_id,
                        Receiver_id = memberId,
                        Group_id = adminGroup.ID,
                        Assigned_count = assignedCount,
                        Send_at = DateTime.Now,
                        Status = isAdmin ? "Accept" : "Pending",
                        Accept_at = isAdmin ? DateTime.Now : (DateTime?)null
                    };

                    Db.Request.Add(newRequest);

                    if (isAdmin)
                    {
                        var gutd = new groupusertasbeehdeatiles
                        {
                            Group_Tasbeeh_Id = tasbeehGoal.ID,
                            Group_user_id = Db.GroupUsers
                    .Where(a => a.Members_id == memberId&&a.Group_id==groupid)
                    .Select(res => res.ID)
                    .FirstOrDefault(),
                            Assign_count = assignedCount,
                            startdate = DateTime.Now,
                            Current_count = 0
                        };
                        Db.groupusertasbeehdeatiles.Add(gutd);
                    }
                }

                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Requests sent successfully.");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //Tasbeeh Distribute Equally Send Request Function
        [HttpPost]
        public HttpResponseMessage DistributeTasbeehEqually(int groupid)
        {
            try
            {
                var adminGroup = Db.Groups.FirstOrDefault(g => g.ID == groupid);

                var tasbeehgoal = Db.GroupTasbeeh
                    .FirstOrDefault(a => a.Group_id == groupid);
                var groupmembers = Db.GroupUsers
                    .Where(g => g.Group_id == groupid)
                    .Join(Db.Users, gu => gu.Members_id, u => u.ID, (gu, u) => new { gu, u })
                    .Where(member => member.u.Status == "avalible")
                    .ToList();
                int count = groupmembers.Count;
                if (count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No avalible members found.");
                }
                int assignCount = tasbeehgoal.Goal / count;
                int assigncountreminder = tasbeehgoal.Goal % count;
                if (assigncountreminder > 0)
                {
                    int i = 0; 

                    foreach (var member in groupmembers)
                    {
                        var Admin = adminGroup.Admin_id == member.gu.Members_id;

                      
                        int extraCount = (i < assigncountreminder) ? 1 : 0;
                        i++; 

                        var newRequest = new Request
                        {
                            Tasbeeh_Id = tasbeehgoal.ID,
                            Sender_id = adminGroup.Admin_id,
                            Receiver_id = member.gu.Members_id,
                            Group_id = adminGroup.ID,
                            Assigned_count = assignCount + extraCount, 
                            Send_at = DateTime.Now,
                            Status = Admin ? "Accept" : "Pending",
                            Accept_at = Admin ? DateTime.Now : (DateTime?)null
                        };

                        if (Admin)
                        {
                            var gutd = new groupusertasbeehdeatiles
                            {
                                Group_Tasbeeh_Id = tasbeehgoal.ID,
                                Group_user_id = member.gu.ID,
                                Assign_count = assignCount + extraCount,
                                startdate = DateTime.Now,
                                Current_count = 0
                            };
                            Db.groupusertasbeehdeatiles.Add(gutd);
                        }

                        Db.Request.Add(newRequest);
                    }
                }
                if (assigncountreminder == 0)
                {
                    foreach (var member in groupmembers)
                    {
                        var Admin = adminGroup.Admin_id == member.gu.Members_id;
                        var newRequest = new Request
                        {
                            Tasbeeh_Id = tasbeehgoal.ID,
                            Sender_id = adminGroup.Admin_id,
                            Receiver_id = member.gu.Members_id,
                            Group_id = adminGroup.ID,
                            Assigned_count = assignCount,
                            Send_at = DateTime.Now,
                            Status = Admin ? "Accept" : "Pending",
                            Accept_at = Admin ? DateTime.Now : (DateTime?)null
                        };
                        if (Admin)
                        {
                            var gutd = new groupusertasbeehdeatiles
                            {
                                Group_Tasbeeh_Id = tasbeehgoal.ID,
                                Group_user_id = member.gu.ID,
                                Assign_count = assignCount,
                                startdate = DateTime.Now,
                                Current_count = 0
                            };
                            Db.groupusertasbeehdeatiles.Add(gutd);
                        }
                        Db.Request.Add(newRequest);
                    }
                }
           
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Tasbeeh Distributed Equally and Records Added.");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, $"Error: {ex.Message}");
            }
        }

        // Accept Request Function
        [HttpPost]
        public HttpResponseMessage AcceptRequest(int requestid, int userid)
        {
            try
            {
                
                var data = Db.Request
                    .Join(Db.Groups, r => r.Group_id, g => g.ID, (r, g) => new
                    {
                        Request = r,
                        Group = g
                    })
                    .Join(Db.GroupUsers, gr => gr.Group.ID, gu => gu.Group_id, (gr, gu) => new
                    {
                        Request = gr.Request,
                        Group = gr.Group,
                        Groupuser = gu
                    })
                    .Where(d => d.Request.ID == requestid && d.Request.Status == "Pending" && d.Groupuser.Members_id == userid)
                    .FirstOrDefault();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Request not found or already processed.");
                }
                data.Request.Status = "Accept";
                data.Request.Accept_at = DateTime.Now;

                var gutddata = Db.groupusertasbeehdeatiles.Where(a => a.Group_user_id == data.Groupuser.ID && a.Group_Tasbeeh_Id == data.Request.Tasbeeh_Id).FirstOrDefault();
                if(gutddata != null)
                {
                    gutddata.Assign_count = gutddata.Assign_count + data.Request.Assigned_count;
                    Db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Request accepted successfully.");
                }
                var gutd = new groupusertasbeehdeatiles
                {
                    Group_Tasbeeh_Id = data.Request.Tasbeeh_Id,
                    Group_user_id = data.Groupuser.ID,
                    Assign_count = data.Request.Assigned_count,
                    startdate=DateTime.Now,
                    Current_count = 0
                };
                Db.groupusertasbeehdeatiles.Add(gutd);
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Request accepted successfully.");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        //Reject Request Function
        [HttpGet]
        public HttpResponseMessage RejectRequest(int requestid)
        {
            try
            {
                var rejectrequest = Db.Request.Where(r => r.ID == requestid&&r.Status== "Pending").FirstOrDefault();
                if (rejectrequest == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found");
                }
                rejectrequest.Status = "Reject";
                var groupmembers = Db.GroupUsers
                    .Where(g => g.Group_id == rejectrequest.Group_id&&g.Members_id!=rejectrequest.Receiver_id)
                    .Join(Db.Users, gu => gu.Members_id, u => u.ID, (gu, u) => new { gu, u })
                    .Where(member => member.u.Status == "avalible")
                    .ToList();
                var count = rejectrequest.Assigned_count;
                var members = groupmembers.Count();
                var assigncount = count / members;
                foreach (var member in groupmembers)
                {
                    var newRequest = new Request
                    {
                        Tasbeeh_Id = rejectrequest.Tasbeeh_Id,
                        Sender_id = rejectrequest.Sender_id,
                        Receiver_id = member.gu.Members_id,
                        Group_id = rejectrequest.Group_id,
                        Assigned_count = assigncount,
                        Send_at = DateTime.Now,
                        Status ="Pending",
                    };
                    Db.Request.Add(newRequest);
                }
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK,"rejected");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        //Notification Send If tasbeeh date End
        [HttpPost]
        public HttpResponseMessage TasbeehEndNotification(int groupid,DateTime t)
        {
            try
            {
                var adminid=Db.Groups.Where(a=>a.ID==groupid).Select(a=> new
                {
                    AdminId=a.Admin_id,
                    Grouptitle=a.Group_Title
                }).FirstOrDefault();
                var tasbeeh = Db.groupusertasbeehdeatiles
                  .Join(Db.GroupTasbeeh, gutd => gutd.Group_Tasbeeh_Id, gt => gt.ID, (gutd, gt) => new
                  {
                      GroupTasbeeh = gt,
                      GroupUserTasbeehDetails = gutd
                  })
                  .Where(res => res.GroupTasbeeh.Group_id == groupid )
                  .Join(Db.GroupUsers, gt => gt.GroupUserTasbeehDetails.Group_user_id, gu => gu.ID, (gt, gu) => new
                  {
                      GroupTasbeeh = gt.GroupTasbeeh,
                      GroupUserTasbeehDetails = gt.GroupUserTasbeehDetails,
                      GroupUser = gu
                  })
                  .Join(Db.Users, g => g.GroupUser.Members_id, u => u.ID, (g, u) => new
                  {
                      Enddate = g.GroupTasbeeh.End_date,
                      Groupid = g.GroupUser.Group_id,
                      TasbeehGoal = g.GroupTasbeeh.Goal,
                      Achieved = g.GroupTasbeeh.Achieved,
                      Username = u.Username,
                      Status = u.Status,
                      AssignCount = g.GroupUserTasbeehDetails.Assign_count,
                      CurrentCount = g.GroupUserTasbeehDetails.Current_count,
                      Userid=u.ID
                  }).Where(res=>res.Status=="avalible")
                  .ToList();
                var enddate = tasbeeh.Select(a => a.Enddate).FirstOrDefault();
                var tomorrowdate = t.AddDays(1);
                var admin = adminid.AdminId;
                if (enddate != null)
                {
                    foreach (var a in tasbeeh)
                    {
                        string Message = $"{adminid.Grouptitle} Active Tasbeeh Going To End Tomorrow";
                        string Message2 = $"{adminid.Grouptitle} Active Tasbeeh Is End ";
                        if (enddate == tomorrowdate)
                        {
                            var no = new Notification
                            {
                                Sender_id = admin,
                                Receiver_id = a.Userid,
                                Detail = Message

                            };
                            Db.Notification.Add(no);
                        }
                        if (enddate == t)
                        {
                            var no = new Notification
                            {
                                Sender_id = admin,
                                Receiver_id = a.Userid,
                                Detail = Message2

                            };
                            Db.Notification.Add(no);
                        }
                    }
                    Db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Notification Send Succesfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        //Delete Notification Function
        [HttpGet]
        public HttpResponseMessage Deletenotification(int id)
        {
            try
            {
                var notification = Db.Notification.FirstOrDefault(a => a.ID == id);
                if (notification == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found");
                }
                Db.Notification.Remove(notification);
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Delete Succesfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        // Distribute maunnaly Tasbeeh get Users who is avalible
        [HttpGet]
        public HttpResponseMessage ShowGroupm(int groupid)
        {
            try
            {
                var members = Db.GroupUsers.Join(Db.Groups, gu => gu.Group_id, g => g.ID, (gu, g) => new
                {
                    GroupUser = gu,
                    Group = g
                }).Join(Db.Users, gu => gu.GroupUser.Members_id, u => u.ID, (gu, u) => new
                {
                    GroupUser = gu.GroupUser,
                    Group = gu.Group,
                    User = u
                }).Where(res => res.Group.ID == groupid && res.User.Status == "avalible").Select(res => new
                {
                    Groupid = res.Group.ID,
                    GroupTitle = res.Group.Group_Title,
                    Admin = res.Group.Admin_id,
                    Memberid = res.User.ID,
                    Memmber = res.User.Username
                }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, members);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }
    }
}