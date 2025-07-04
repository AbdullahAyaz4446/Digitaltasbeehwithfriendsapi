using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using DigitalTasbeehWithFriendsApi.Models;
using Newtonsoft.Json;
namespace DigitalTasbeehWithFriendsApi.Controllers
{
    public class UserController : ApiController
    {


        DTWFEntities Db=new DTWFEntities();

        ////update
        //[HttpPost]
        //public HttpResponseMessage UpdateUser(int id)
        //{r
        //    try
        //    {
        //        var request = HttpContext.Current.Request;
        //        var myImage = request.Files[0];
        //        var userData = request.Form["User"];
        //        Users updatedUser = JsonConvert.DeserializeObject<Users>(userData);


        //        var existingUser = Db.Users.FirstOrDefault(u => u.ID == id);
        //        if (existingUser == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound, "User not found.");
        //        }


        //        existingUser.Username = updatedUser.Username;
        //        existingUser.Email = updatedUser.Email; 
        //        existingUser.Status = existingUser.Status;


        //        if (myImage != null)
        //        {

        //            var filePath = HttpContext.Current.Server.MapPath("~/Images/");
        //            var fileName = id.ToString() + "-" + updatedUser.Username + ".jpg";
        //            var fullFilePath = filePath + fileName;


        //            myImage.SaveAs(fullFilePath);


        //            existingUser.Image = fileName;
        //        }


        //        Db.SaveChanges();

        //        return Request.CreateResponse(HttpStatusCode.OK, existingUser);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
        //    }
        //}



        ////SignUp Function
        //[HttpPost]
        //public HttpResponseMessage SignUp()
        //{
        //    try
        //    {
        //        var request = HttpContext.Current.Request;
        //        var myImage = request.Files[0];
        //        var User = request.Form["User"];
        //        Users u = JsonConvert.DeserializeObject<Users>(User);
        //        var filePath = HttpContext.Current.Server.MapPath("~/Images/");
        //        int maxID = Db.Users.Max(m => m.ID);
        //        var fileName = (maxID + 1).ToString() + "-" + u.Username + ".jpg";
        //        myImage.SaveAs(filePath + fileName);
        //        u.Status= "Offline";
        //        u.Image = fileName;
        //        Db.Users.Add(u);
        //        Db.SaveChanges();
        //        return Request.CreateResponse(HttpStatusCode.OK, "Data Inserted:");
        //    }
        //    catch (Exception ea)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, ea.Message);
        //    }
        //}
        //SignUp Function
        [HttpPost]
        public HttpResponseMessage SignUp(Users U)
        {
            try
            {
                U.Status = "online";
                Db.Users.Add(U);
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "SignUp Succesfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }
        //LogIn Function
        [HttpGet] 
        public HttpResponseMessage Login(String email,String password)
        {

            try 
            {
                var User = Db.Users.Where(a => a.Email.ToLower() == email.ToLower() && a.Password == password).FirstOrDefault();
                if (User == null) {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User NOt Found");
                }
                User.Status = "online";
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, User);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }
        //Friend Function
        [HttpGet]
        public HttpResponseMessage AllUser() {
            try
            {
                var users = Db.Users.Select(u => new
                {
                    ID = u.ID,
                    name = u.Username,
                    Status = u.Status
                }).ToList();
               
                if (User == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User NOt Found"); 
                }
                return Request.CreateResponse(HttpStatusCode.OK, users);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }



        }
        //Update Your Deatiles Function
        [HttpPost]
        public HttpResponseMessage ModifyUser(Users U)
        {
            try
            {
                var user = Db.Users.FirstOrDefault(u => u.ID == U.ID);
                if (user == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User NOt Found");
                }
                user.Username=U.Username;
                user.Password = U.Password;
                user.Status = user.Status;
                user.Email = U.Email;
                Db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "User Data Updated");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        // Active Your Self Function
        [HttpGet]
        public HttpResponseMessage Online(int ID)
        {
            try
            {
                var user = Db.Users.FirstOrDefault(u => u.ID == ID);
                if (user == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User not found");
                }
                return Request.CreateResponse(HttpStatusCode.OK, "online");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }


        }
        // update online state
        [HttpGet]
        public HttpResponseMessage UpdateOnlineStatus(int UserID, String Status)
        {
            try
            {
                var user = Db.Users.FirstOrDefault(u => u.ID == UserID);
                if (user == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User not found");
                }
                user.Status = Status;
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "online");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }
        // UnActive Your Self Function
        //[HttpPost]
        //public HttpResponseMessage UnActive(int ID)
        //{
        //    try
        //    {
        //        var user = Db.Users.FirstOrDefault(u => u.ID == ID);
        //        if (user == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound, "User not found");
        //        }
        //        user.Status = "unavalible";
        //        Db.SaveChanges();


        //        return Request.CreateResponse(HttpStatusCode.OK, "unavalible");

        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }


        //}
        // Search Function
        [HttpGet]
        public HttpResponseMessage Searchuser(String email)
        {
            try
            {
                var user = Db.Users.FirstOrDefault(a => a.Email == email);

                if(user == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User Not Found");
                }
                return Request.CreateResponse(HttpStatusCode.OK, user);

            }catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }
        // Delete Your Account Function
        [HttpGet]
        public HttpResponseMessage DeleteUser(int id)
        {
            try
            {
                var user = Db.Users.FirstOrDefault(u => u.ID == id); 
                if (user != null)
                {
                    Db.Users.Remove(user);
                    Db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "User deleted successfully");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User not found");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        //All Group on The Base Of your ID
        [HttpGet]
        public HttpResponseMessage GroupTitles(int memberId)
        {
            try
            {
                //            var groupTitles = Db.GroupUsers
                //.Where(gu => gu.Members_id == memberId)
                //.Select(gu => new
                //{
                //    Group_ID = gu.Group_id,
                //    Group_Title = Db.Groups
                //        .Where(g => g.ID == gu.Group_id)
                //        .Select(g => g.Group_Title)
                //        .FirstOrDefault()
                //})
                //.Distinct() // Ensure unique group titles
                //.ToList();
                var groupTitles = Db.GroupUsers
.Join(Db.Groups,
   gu => gu.Group_id,
   g => g.ID,
   (gu, g) => new
   {
       gu, 
       g
   })
.Where(w => w.gu.Members_id == memberId&&w.g.Flag==false)  // add group flag
.Select(x => new
{
    Groupid=x.g.ID,
    Adminid=x.g.Admin_id,
    Grouptitle=x.g.Group_Title
   

})
.Distinct()
.ToList();
                if (!groupTitles.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No groups found for the specified member.");
                }

                return Request.CreateResponse(HttpStatusCode.OK, groupTitles);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        //Your Tasbeh Progress Function
        [HttpGet]
        public HttpResponseMessage TasbeehProgressLogedMember(int userId, int groupId,int tasbeehid)
        {
            try
            {

                var groupuserid = Db.GroupUsers
                    .Where(gu => gu.Members_id == userId && gu.Group_id == groupId)
                    .Select(gu => gu.ID)
                    .FirstOrDefault();

                if (groupuserid == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User is not a member of any group.");
                }


                var tasbeehProgress = Db.groupusertasbeehdeatiles
                    .Join(Db.GroupTasbeeh, gutd => gutd.Group_Tasbeeh_Id, gt => gt.ID, (gutd, gt) => new
                    {
                        GroupTasbeeh = gt,
                        groupusertasbeehdeatiles = gutd 
                    })
                    .Where(res => res.GroupTasbeeh.Group_id == groupId&&res.GroupTasbeeh.ID==tasbeehid&&res.groupusertasbeehdeatiles.Flag==0)
                    .Join(Db.Groups, gt => gt.GroupTasbeeh.Group_id, g => g.ID, (gt, g) => new
                    {
                        GroupTasbeeh = gt.GroupTasbeeh,
                        groupusertasbeehdeatiles = gt.groupusertasbeehdeatiles,
                        Group = g
                    })
                    .Join(Db.GroupUsers, gt => gt.Group.ID, gu => gu.Group_id, (gt, gu) => new
                    {
                        GroupTasbeeh = gt.GroupTasbeeh,
                        groupusertasbeehdeatiles = gt.groupusertasbeehdeatiles,
                        Group = gt.Group,
                        GroupUsers = gu
                    })
                    .Where(res => res.groupusertasbeehdeatiles.Group_user_id == groupuserid && res.Group.ID == groupId && res.GroupTasbeeh.ID==tasbeehid&&res.groupusertasbeehdeatiles.Flag==0)
                    .Join(Db.Users, g => g.GroupUsers.Members_id, u => u.ID, (g, u) => new
                    {
                        GroupTasbeeh = g.GroupTasbeeh,
                        groupusertasbeehdeatiles = g.groupusertasbeehdeatiles,
                        Group = g.Group,
                        GroupUsers = g.GroupUsers,
                        users = Db.Users.Where(a => a.ID == userId).FirstOrDefault()
                    })
                    .Select(res => new
                    {
                        groupusertasbeehdeatilesid=res.groupusertasbeehdeatiles.ID,
                        Grouptasbeehid=res.GroupTasbeeh.ID,
                        TasbeehID = res.GroupTasbeeh.Tasbeeh_id,
                        GroupTitle = res.Group.Group_Title,
                        Username = res.users.Username,
                        Goal = res.groupusertasbeehdeatiles.Assign_count,
                        Current = res.groupusertasbeehdeatiles.Current_count,
                        deadline = res.GroupTasbeeh.End_date
                    })
                    .FirstOrDefault(); 
                 
                if (tasbeehProgress == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No Tasbeeh progress found for the user in this group.");
                }

                return Request.CreateResponse(HttpStatusCode.OK, tasbeehProgress);


            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
            }
        }
        //Read Active Tasbeeh And Notification end if u achived your tasbeeh goal
        [HttpGet]
        public HttpResponseMessage ReadTasbehandnotificationsend(int userid, int groupid,int tasbeehid)
        {
            try
            { 
                var groupuserid = Db.GroupUsers
                .Where(gu => gu.Members_id == userid&&gu.Group_id== groupid)
                .Select(gu => gu.ID)
                .FirstOrDefault();

                if (groupuserid == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User is not a member of any group.");
                }


                var tasbeehProgress = Db.groupusertasbeehdeatiles
                    .Join(Db.GroupTasbeeh, gutd => gutd.Group_Tasbeeh_Id, gt => gt.ID, (gutd, gt) => new
                    {
                        GroupTasbeeh = gt,
                        groupusertasbeehdeatiles = gutd
                    })
                    .Join(Db.Groups, gt => gt.GroupTasbeeh.Group_id, g => g.ID, (gt, g) => new
                    {
                        GroupTasbeeh = gt.GroupTasbeeh,
                        groupusertasbeehdeatiles = gt.groupusertasbeehdeatiles,
                        Group = g
                    })
                    .Join(Db.GroupUsers, gt => gt.Group.ID, gu => gu.Group_id, (gt, gu) => new
                    {
                        GroupTasbeeh = gt.GroupTasbeeh,
                        groupusertasbeehdeatiles = gt.groupusertasbeehdeatiles,
                        Group = gt.Group,
                        GroupUsers = gu
                    })
                    .Join(Db.Users, g => g.GroupUsers.Members_id, u => u.ID, (g, u) => new
                    {
                        GroupTasbeeh = g.GroupTasbeeh,
                        groupusertasbeehdeatiles = g.groupusertasbeehdeatiles,
                        Group = g.Group,
                        GroupUsers = g.GroupUsers,
                        users = Db.Users.Where(a => a.ID == userid).FirstOrDefault()
                    }).Where(a=>a.GroupTasbeeh.ID==a.groupusertasbeehdeatiles.Group_Tasbeeh_Id&& groupuserid == a.groupusertasbeehdeatiles.Group_user_id&&a.groupusertasbeehdeatiles.Group_Tasbeeh_Id==a.GroupTasbeeh.ID&&a.GroupTasbeeh.ID==tasbeehid&&a.groupusertasbeehdeatiles.Flag==0)
                    .Select(res => new
                    {
                        TasbeehID = res.GroupTasbeeh.ID,
                        Enddate = res.GroupTasbeeh.End_date,
                        Groupuserid = res.GroupUsers.ID,
                        memberid = res.groupusertasbeehdeatiles.Group_user_id,
                        Current = res.groupusertasbeehdeatiles.Current_count,
                        Adminid = res.Group.Admin_id,
                        Goal = res.groupusertasbeehdeatiles.Assign_count,
                        username = res.users.Username,
                        Grouptitle = res.Group.Group_Title,
                        ID=res.groupusertasbeehdeatiles.ID
                        
                        

                    }).FirstOrDefault();
                var updateenddate=Db.groupusertasbeehdeatiles.Where(a=>a.ID==tasbeehProgress.ID).FirstOrDefault();
       
                if (tasbeehProgress.Current+1 == tasbeehProgress.Goal)
                {
                   
                    updateenddate.Enddate = DateTime.Now;
                    var data = Db.groupusertasbeehdeatiles.Where(a => a.Group_Tasbeeh_Id == tasbeehid && a.Group_user_id == groupuserid).FirstOrDefault();
                    data.Current_count = tasbeehProgress.Current + 1;
                    Db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Your Tasbeeh Goal is Completed");
                }
                else
                {
                    
                    var data = Db.groupusertasbeehdeatiles.Where(a => a.Group_Tasbeeh_Id == tasbeehid && a.Group_user_id == groupuserid).FirstOrDefault();
                    data.Current_count = tasbeehProgress.Current + 1;
                    Db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, tasbeehProgress);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        // SHow all Request user Function
        [HttpGet]
        public HttpResponseMessage Showallrequest(int userid)
        {
            try 
            {
                var allrequest = Db.Request.Join(Db.GroupTasbeeh, r => r.Tasbeeh_Id, gt => gt.ID, (r, gt) => new
                {
                    Request = r,
                    Grouptasbeeh = gt
                }).Join(Db.Groups, gtr => gtr.Grouptasbeeh.Group_id, g => g.ID, (gtr, g) => new
                {
                    Request = gtr.Request,
                    Grouptasbeeh = gtr.Grouptasbeeh,
                    Group = g
                }).Join(Db.GroupUsers, gtrgu => gtrgu.Group.ID, gu => gu.Group_id, (gtrgu, gu) => new
                {
                    Request = gtrgu.Request,
                    Grouptasbeeh = gtrgu.Grouptasbeeh,
                    Group = gtrgu.Group,
                    Groupuser = gu
                }).Join(Db.Users, g => g.Groupuser.Members_id, u => u.ID, (g, u) => new
                {
                    Request = g.Request,
                    Grouptasbeeh = g.Grouptasbeeh,
                    Group = g.Group,
                    Groupuser = g.Groupuser,
                    User = u
                }).Where(d => d.Request.Status == "Pending" && d.Request.Receiver_id == userid&&d.Group.Flag==false&&d.Grouptasbeeh.Flag==0)
                .Select(data => new
                {
                    id = data.Request.ID,
                    GroupTitle = data.Group.Group_Title,
                    Count = data.Request.Assigned_count,
                    Tasbeehname =Db.Tasbeeh.Where(a => a.ID == data.Grouptasbeeh.Tasbeeh_id).Select(a => new { Title = a.Tasbeeh_Title, Id = a.ID }).FirstOrDefault(),
                    Type="request"
    })
    .Distinct()
   .OrderByDescending(x => x.id)
    .ToList();
                if (allrequest == null) 
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No Notification Found");
                }
                return Request.CreateResponse(HttpStatusCode.OK, allrequest);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        //Leave Group and Send Notification i mean reason Your Are ill or something
        [HttpGet]
        public HttpResponseMessage Leavetasbeeh(int id, String Message,int userid)
        {
            try
            {
                //var gernatenotification=Db.groupusertasbeehdeatiles
                //  .Where(gutd => gutd.ID == id && gutd.Flag == 0)
                //  .Join(Db.GroupTasbeeh, gutd => gutd.Group_Tasbeeh_Id, gt => gt.ID, (gutd, gt) => new
                //  {
                //      GroupTasbeeh = gt,
                //      groupusertasbeehdeatiles = gutd
                //  })
                //  .Join(Db.Groups, gt => gt.GroupTasbeeh.Group_id, g => g.ID, (gt, g) => new
                //  {
                //      GroupTasbeeh = gt.GroupTasbeeh,
                //      groupusertasbeehdeatiles = gt.groupusertasbeehdeatiles,
                //      Group = g
                //  }).Where(a => a.GroupTasbeeh.ID == a.groupusertasbeehdeatiles.Group_Tasbeeh_Id)
                //  .Join(Db.GroupUsers, gt => gt.Group.ID, gu => gu.Group_id, (gt, gu) => new
                //  {
                //      GroupTasbeeh = gt.GroupTasbeeh,
                //      groupusertasbeehdeatiles = gt.groupusertasbeehdeatiles,
                //      Group = gt.Group,
                //      GroupUsers = gu
                //  })
                //  .Join(Db.Users, g => g.GroupUsers.Members_id, u => u.ID, (g, u) => new
                //  {
                //      GroupTasbeeh = g.GroupTasbeeh,
                //      groupusertasbeehdeatiles = g.groupusertasbeehdeatiles,
                //      Group = g.Group,
                //      GroupUsers = g.GroupUsers,
                //      users = u
                //  })
                //  .Select(res => new
                //  {
                //      groupusertasbeehdeatilesid = res.groupusertasbeehdeatiles.ID,
                //      Groupid=res.Group.ID,
                //      groupTasbeehID = res.GroupTasbeeh.ID,
                //      memberid = res.groupusertasbeehdeatiles.Group_user_id,
                //      Current = res.groupusertasbeehdeatiles.Current_count,
                //      Adminid = res.Group.Admin_id,
                //      Goal = res.groupusertasbeehdeatiles.Assign_count,
                //      username = res.users.Username,
                //      Grouptitle = res.Group.Group_Title,
                //      tasbeehtitle=Db.Tasbeeh.Where(a => a.ID == res.GroupTasbeeh.Tasbeeh_id).Select(a => new { Title = a.Tasbeeh_Title}).FirstOrDefault(),
                //      startdate=res.groupusertasbeehdeatiles.startdate,
                //      Enddate=res.groupusertasbeehdeatiles.Enddate

                //  }).FirstOrDefault();


                var username = Db.Users.Where(a => a.ID == userid).Select(a => a.Username).FirstOrDefault();

                
                var deletegroupuser = Db.groupusertasbeehdeatiles.Where(a => a.ID == id).FirstOrDefault();
                var data = Db.groupusertasbeehdeatiles.Join(Db.GroupTasbeeh, a => a.Group_Tasbeeh_Id, gt => gt.ID, (a, gt) => new
                {
                    groupusertasbeehdeatiles = a,
                    GroupTasbeeh = gt
                }).Where(res => res.GroupTasbeeh.ID == deletegroupuser.Group_Tasbeeh_Id).Join(Db.Groups, res => res.GroupTasbeeh.Group_id, g => g.ID, (res, g) => new
                {
                    groupusertasbeehdeatiles = res.groupusertasbeehdeatiles,
                    GroupTasbeeh = res.GroupTasbeeh,
                    Group = g
                }).Join(Db.Tasbeeh, res => res.GroupTasbeeh.Tasbeeh_id, t => t.ID, (res, t) => new
                {
                    groupusertasbeehdeatiles = res.groupusertasbeehdeatiles,
                    GroupTasbeeh = res.GroupTasbeeh,
                    Group = res.Group,
                    TasbeehTitle = t.Tasbeeh_Title
                }).Where(a => a.groupusertasbeehdeatiles.ID == id).FirstOrDefault();


                   var  message = $"User '{username}' has left the tasbeeh '{data.TasbeehTitle}' in group '{data.Group.Group_Title}'. Reason: {Message}";
                var leavemember = new leavegroupusertasbeehdeatiles
                {
                    Group_Tasbeeh_id = deletegroupuser.Group_Tasbeeh_Id,
                    Group_user_id = deletegroupuser.Group_user_id,
                    Enddate = deletegroupuser.Enddate,
                    startdate = deletegroupuser.startdate,
                    Assign_count= deletegroupuser.Assign_count,
                    Current_count= deletegroupuser.Current_count,
                    Flag = 0,
                    Message= message,
                    Groupadminid=data.Group.Admin_id,
                    groupid=data.Group.ID
                };
                Db.leavegroupusertasbeehdeatiles.Add(leavemember);
                deletegroupuser.Flag = 3;
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, message);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // Get the list of All group members
        [HttpGet]
        public HttpResponseMessage Allgroupmember(int groupid)
        {
            try
            {
                var data = Db.GroupUsers.Where(a => a.Group_id == groupid).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, data);
            }catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        // Get the list of All leave group members deatiles
        [HttpGet]
        public HttpResponseMessage Allleavegroupmember(int userid)
        {
            try
            {
                var data = Db.leavegroupusertasbeehdeatiles.Where(a => a.Groupadminid == userid && a.Flag == 0).OrderByDescending(a => a.ID).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
