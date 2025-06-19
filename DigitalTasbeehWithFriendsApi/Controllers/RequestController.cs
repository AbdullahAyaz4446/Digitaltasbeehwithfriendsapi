using DigitalTasbeehWithFriendsApi.Models;
using Microsoft.Ajax.Utilities;
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
                int tasbeehid = int.Parse(form["tasbeehid"]);


                List<int> id = JsonConvert.DeserializeObject<List<int>>(form["id"]);
                List<int> count = JsonConvert.DeserializeObject<List<int>>(form["count"]);

                var adminGroup = Db.Groups.FirstOrDefault(g => g.ID == groupid);
                if (adminGroup == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Group not found.");
                }

                var tasbeehGoal = Db.GroupTasbeeh
               .FirstOrDefault(a => a.ID == tasbeehid && a.Flag == 0);

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
                            Current_count = 0,
                            Flag=0
                            
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
        public HttpResponseMessage DistributeTasbeehEqually(int groupid,int tasbeehid)
        {
            try
            {
                var adminGroup = Db.Groups.FirstOrDefault(g => g.ID == groupid);

                var tasbeehgoal = Db.GroupTasbeeh
                    .FirstOrDefault(a =>a.ID==tasbeehid&&a.Flag==0);
                var groupmembers = Db.GroupUsers
                    .Where(g => g.Group_id == groupid)
                    .Join(Db.Users, gu => gu.Members_id, u => u.ID, (gu, u) => new { gu, u })
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
                                Current_count = 0,
                                Flag=0
                               
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
                                Current_count = 0,
                                Flag=0
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

                var gutddata = Db.groupusertasbeehdeatiles.Where(a => a.Group_user_id == data.Groupuser.ID && a.Group_Tasbeeh_Id == data.Request.Tasbeeh_Id&&a.Flag==0).FirstOrDefault();
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
                    Current_count = 0,
                    Flag=0
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
        [HttpGet]
        public HttpResponseMessage TasbeehEndNotification(int groupid)
        {
            try
            {
                var data = Db.GroupTasbeeh.Join(Db.Groups,
                 gt => gt.Group_id,
                 g => g.ID,
                 (gt, g) => new
                 {
                     GroupTasbeeh = gt,
                     Group = g
                 })
             .Join(Db.Tasbeeh,
                 gt => gt.GroupTasbeeh.Tasbeeh_id,
                 t => t.ID,
                 (gt, t) => new
                 {
                     GroupTasbeeh = gt.GroupTasbeeh,
                     Group = gt.Group,
                     Tasbeeh = t
                 }).Join(Db.Request,gt=>gt.GroupTasbeeh.ID, r => r.Tasbeeh_Id, (gt, r) => new
                 {
                     GroupTasbeeh = gt.GroupTasbeeh,
                     Group = gt.Group,
                     Tasbeeh = gt.Tasbeeh,
                     Request = r
                 }
                 )
             .Where(result => result.GroupTasbeeh.Flag == 0 && result.Group.ID == groupid&&result.Request.Group_id==result.Group.ID&&result.Request.Tasbeeh_Id==result.GroupTasbeeh.ID&&result.Request.Status=="Accept")
             .Select(res => new
             {
                 Groupid = res.Group.ID,
                 Grouptitle = res.Group.Group_Title,
                 Adminid = res.Group.Admin_id,
                 GroupTasbeehid = res.GroupTasbeeh.ID,
                 Enddate = res.GroupTasbeeh.End_date,
                 Tasbeehid = res.Tasbeeh.ID,
                 Tasbeehname = res.Tasbeeh.Tasbeeh_Title,
                  receiverid=res.Request.Receiver_id
             }).ToList();

                
                var Nowdate = DateTime.Now.Date;
                var tomorrowdate = DateTime.Now.AddDays(1).Date;
                foreach (var chkdate in data)
                {
                    var grouptasbeeh = Db.GroupTasbeeh.Where(a => a.ID == chkdate.GroupTasbeehid&&a.Flag==0).FirstOrDefault();
                    var message = $"Tasbeeh '{chkdate.Tasbeehname}' in group '{chkdate.Grouptitle}' is ending tomorrow.";
                    var message1 = $"Tasbeeh '{chkdate.Tasbeehname}' in group '{chkdate.Grouptitle}' is ending Today.";
                    var chk = chkdate.Enddate.Value.Date;
                    var tasbeehdate= chkdate.Enddate.Value.Date;
                    if (chk == tomorrowdate)
                    {
                        var notification = new Notification
                        {
                            Receiver_id=chkdate.receiverid,
                            Detail=message
                        };
                        Db.Notification.Add(notification);
                        Db.SaveChanges();

                    }
                    if (tasbeehdate == Nowdate)
                    {
                        var notification = new Notification
                        {
                            Receiver_id = chkdate.receiverid,
                            Detail = message1
                        };
                        Db.Notification.Add(notification);
                        grouptasbeeh.Flag = 2;
                        Db.SaveChanges();

                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK,"Succesfully Date chk");
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
                }).Where(res => res.Group.ID == groupid).Select(res => new
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


        //Reassign tasbeeh equally to all group members
        [HttpGet]
        public HttpResponseMessage reassigntasbehequally(int groupid,int Goal,int grouptasbeehid,int leaveid)
        {
            try
            {


                var adminGroup = Db.Groups.FirstOrDefault(g => g.ID == groupid);
                var groupmembers = Db.GroupUsers
                    .Where(g => g.Group_id == groupid)
                    .Join(Db.Users, gu => gu.Members_id, u => u.ID, (gu, u) => new { gu, u })
                    .ToList();
                int count = groupmembers.Count;
                if (count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No avalible members found.");
                }
                int assignCount = Goal / count;
                int assigncountreminder = Goal % count;
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
                            Tasbeeh_Id = grouptasbeehid,
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
                            var gutddata = Db.groupusertasbeehdeatiles.Where(a => a.Group_user_id == member.gu.ID && a.Group_Tasbeeh_Id == grouptasbeehid && a.Flag == 0).FirstOrDefault();
                            gutddata.Assign_count = gutddata.Assign_count + assignCount+extraCount;
                            Db.SaveChanges();
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
                            Tasbeeh_Id = grouptasbeehid,
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
                            var gutddata = Db.groupusertasbeehdeatiles.Where(a => a.Group_user_id == member.gu.ID && a.Group_Tasbeeh_Id == grouptasbeehid && a.Flag == 0).FirstOrDefault();
                            gutddata.Assign_count = gutddata.Assign_count + assignCount;
                            Db.SaveChanges();
                        }
                        Db.Request.Add(newRequest);
                    }
                }
                var data = Db.leavegroupusertasbeehdeatiles.Where(a => a.ID == leaveid).FirstOrDefault();
                data.Flag = 1;
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Tasbeeh Distributed Equally and Records Added.");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }
        ////Reassign tasbeeh munally to all group members
        //Tasbeeh Distribute Manully Send Request Function
        [HttpPost]
        public HttpResponseMessage reassignDistributeTasbeehManually()
        {
            try
            {

                var form = HttpContext.Current.Request.Form;

                int groupid = int.Parse(form["groupid"]);
                int tasbeehid = int.Parse(form["tasbeehid"]);
                var tasbeehGoal = int.Parse(form["goal"]);
                int leaveid= int.Parse(form["leaveid"]);

                List<int> id = JsonConvert.DeserializeObject<List<int>>(form["id"]);
                List<int> count = JsonConvert.DeserializeObject<List<int>>(form["count"]);

                var adminGroup = Db.Groups.FirstOrDefault(g => g.ID == groupid);
                if (adminGroup == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Group not found.");
                }

                if (tasbeehGoal==0)
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
                        Tasbeeh_Id = tasbeehid,
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
                        var mid = Db.GroupUsers.FirstOrDefault(a => a.Group_id == groupid && a.Members_id == memberId);
                        var gutddata = Db.groupusertasbeehdeatiles.Where(a => a.Group_user_id == mid.ID && a.Group_Tasbeeh_Id == tasbeehid && a.Flag == 0).FirstOrDefault();
                        gutddata.Assign_count = gutddata.Assign_count + assignedCount;
                        Db.SaveChanges();
                    }
                }
                var data = Db.leavegroupusertasbeehdeatiles.Where(a => a.ID == leaveid).FirstOrDefault();
                data.Flag = 1;
                Db.SaveChanges(); 
              
                return Request.CreateResponse(HttpStatusCode.OK, "Requests sent successfully.");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

    }
}