using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using DigitalTasbeehWithFriendsApi.Models;
using Microsoft.Ajax.Utilities;
namespace DigitalTasbeehWithFriendsApi.Controllers
{
    public class GroupController : ApiController
    {
        DTWFEntities Db = new DTWFEntities();
        // Create Group Function
        [HttpPost]
        public  HttpResponseMessage CreateGroup(Groups G)
        {
            try
            {
                G.Flag = false;
                Db.Groups.Add(G);    
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK,G);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        //search group By Title
        [HttpGet]
        public HttpResponseMessage Searchgroup(String name)
        {
            try
            {
                var group=Db.Groups.FirstOrDefault(a=>a.Group_Title==name);
                if (group == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Group FInd For The Specfix Name");
                }
                return Request.CreateResponse(HttpStatusCode.OK, group);

            }catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }
        // Update Group Function
        [HttpPost]
        public HttpResponseMessage Updategroup(Groups G)
        {
            try
            {
                var group = Db.Groups.Where(g => g.ID == G.ID).FirstOrDefault();

                if (group == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Group Not Found");
                }


                group.Group_Title = G.Group_Title;
                group.Admin_id = G.Admin_id;

                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Group Updated Succesfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        ////Delete Group Function
        [HttpGet]
        public HttpResponseMessage Deletegroup(int id)
        {
            try
            {
                var data = Db.Groups.Where(g => g.ID == id).FirstOrDefault();
                data.Flag = true;
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Delete Succesfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        // Add Members In Group Function
        [HttpPost]
        public HttpResponseMessage GroupMembers(List<GroupUsers> Gu)
        {
            try
            { 
                 Db.GroupUsers.AddRange(Gu);
                 Db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Group User successfully added");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
 
  
        //Tasbeeh Progress and all group members Progress Function
        [HttpGet]
        public HttpResponseMessage TasbeehProgressAndMembersProgress(int groupId,int tasbeehid)
        { 
            try
            {
                var tasbeehProgress = Db.groupusertasbeehdeatiles
                    .Join(Db.GroupTasbeeh, gutd => gutd.Group_Tasbeeh_Id, gt => gt.ID, (gutd, gt) => new
                    {
                        GroupTasbeeh = gt,
                        GroupUserTasbeehDetails = gutd
                    })
                    .Join(Db.GroupUsers, gt => gt.GroupUserTasbeehDetails.Group_user_id, gu => gu.ID, (gt, gu) => new
                    {
                        GroupTasbeeh = gt.GroupTasbeeh,
                        GroupUserTasbeehDetails = gt.GroupUserTasbeehDetails,
                        GroupUser = gu
                    }).Where(res => res.GroupTasbeeh.ID==tasbeehid&&res.GroupTasbeeh.Flag==0&&res.GroupUserTasbeehDetails.Flag==0|| res.GroupTasbeeh.ID == tasbeehid && res.GroupTasbeeh.Flag == 2 && res.GroupUserTasbeehDetails.Flag != 3 )
                    .Join(Db.Users, g => g.GroupUser.Members_id, u => u.ID, (g, u) => new
                    {
                        TasbeehID = g.GroupTasbeeh.Tasbeeh_id,
                        Groupid = g.GroupUser.Group_id,
                        TasbeehGoal = g.GroupTasbeeh.Goal,
                        Achieved = g.GroupTasbeeh.Achieved,
                        Username = u.Username, 
                        Status = u.Status,
                        AssignCount = g.GroupUserTasbeehDetails.Assign_count,
                        CurrentCount = g.GroupUserTasbeehDetails.Current_count,
                        Adminid = Db.Groups.Where(a => a.ID == groupId).Select(a => a.Admin_id).FirstOrDefault(),
                        userid = u.ID
                    })
                    .ToList();
                 
                return Request.CreateResponse(HttpStatusCode.OK, tasbeehProgress);
            }
            catch (Exception ex)
            {
                
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "An unexpected error occurred: " + ex.Message);
            }
        }
        //Tasbeeh Achived Function To Post Achived count Function
        [HttpGet]
        public HttpResponseMessage IncremnetInTasbeeh(int groupid,int tasbeehid)
        {
            try
            {

                var tasbeehProgress = Db.groupusertasbeehdeatiles
                  .Join(Db.GroupTasbeeh, gutd => gutd.Group_Tasbeeh_Id, gt => gt.ID, (gutd, gt) => new
                  {
                      GroupTasbeeh = gt,
                      GroupUserTasbeehDetails = gutd
                  })
                  .Join(Db.GroupUsers, gt => gt.GroupUserTasbeehDetails.Group_user_id, gu => gu.ID, (gt, gu) => new
                  {
                      GroupTasbeeh = gt.GroupTasbeeh,
                      GroupUserTasbeehDetails = gt.GroupUserTasbeehDetails,
                      GroupUser = gu
                  }).Where(res => res.GroupTasbeeh.Group_id == groupid && res.GroupTasbeeh.ID == tasbeehid && res.GroupTasbeeh.Flag == 0 && res.GroupUserTasbeehDetails.Flag == 0 || res.GroupTasbeeh.Group_id == groupid && res.GroupTasbeeh.ID == tasbeehid  && res.GroupTasbeeh.Flag == 0 && res.GroupUserTasbeehDetails.Group_Tasbeeh_Id == tasbeehid && res.GroupUserTasbeehDetails.Flag == 0 ||res.GroupTasbeeh.Group_id == groupid && res.GroupTasbeeh.ID == tasbeehid && res.GroupTasbeeh.Flag == 0 && res.GroupUserTasbeehDetails.Flag == 3)
                  .Join(Db.Users, g => g.GroupUser.Members_id, u => u.ID, (g, u) => new
                  {
                      CurrentCount = g.GroupUserTasbeehDetails.Current_count
                  })
                  .ToList();
                var changeflag = Db.groupusertasbeehdeatiles.Where(a => a.Group_Tasbeeh_Id == tasbeehid && a.Flag == 0 ).ToList();
                var totalcount = tasbeehProgress.Sum(t=>t.CurrentCount);
                var GroupTasbeehdata = Db.GroupTasbeeh.Where(gt => gt.Group_id == groupid && gt.ID == tasbeehid&&gt.Flag==0|| gt.Group_id == groupid && gt.ID == tasbeehid && gt.Flag == 2).FirstOrDefault();
                GroupTasbeehdata.Achieved = totalcount ?? 0;
                if(GroupTasbeehdata.Achieved== GroupTasbeehdata.Goal)
                {
                    GroupTasbeehdata.Flag = 2;
                   
                    foreach (var chnageflag in changeflag)
                    {
                        chnageflag.Flag = 2;
                        

                    }

                    Db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Tasbeh Goal Has be completed");
                }

                Db.SaveChanges();
                
                return Request.CreateResponse(HttpStatusCode.OK, "Tasbeh Progress Updated Succesfully");
                   

            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,ex.Message);
            }
        }
        // Tasbeeh Logs on the base of Group Id Function
        [HttpGet]
        public HttpResponseMessage Tasbeehlogs(int groupid, int userid)
        {
            try
            {
                var memberId = Db.GroupUsers
                    .Where(a => a.Group_id == groupid && a.Members_id == userid)
                    .Select(a => a.ID)
                    .FirstOrDefault();

                if (memberId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User not found in this group");
                }

                var groupTasbeehs = Db.GroupTasbeeh
                    .Where(a => a.Group_id == groupid && a.Flag != 3&& a.Group_id == groupid && a.Flag != 4)
                    .ToList();

                var result = new List<object>();

                foreach (var groupTasbeeh in groupTasbeehs)
                {
                    
                    var userHasTasbeeh = Db.groupusertasbeehdeatiles
                        .Any(a => a.Group_Tasbeeh_Id == groupTasbeeh.ID &&
                                  a.Group_user_id == memberId &&
                                  a.Flag !=3);

                    if (userHasTasbeeh)
                    {
                        var tasbeeh = Db.Tasbeeh.FirstOrDefault(t => t.ID == groupTasbeeh.Tasbeeh_id);

                        if (tasbeeh != null)
                        {
                            result.Add(new
                            {
                                id = groupTasbeeh.ID,
                                title = tasbeeh.Tasbeeh_Title,
                                Goal = groupTasbeeh.Goal,
                                Achieved = groupTasbeeh.Achieved,
                                deadline = groupTasbeeh.End_date,
                                Flag = groupTasbeeh.Flag,
                                tid = groupTasbeeh.Tasbeeh_id,
                                day = groupTasbeeh.schedule
                            });
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        ////going to work and save tasbeeh log
        [HttpPost]
        public HttpResponseMessage Savetasbeehlog(tasbeehlogs tbl)
        {
            try
            {
                tbl.leaveat = DateTime.Now;
                tbl.Flag = 0;
                Db.tasbeehlogs.Add(tbl);
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Succsully save data");

            }catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        // Show pop up when you come back 
        [HttpGet]
        public HttpResponseMessage fetchtasbeehlog(int Userid,int grouptasbeehid)
        { 
            try
            {
                var data = Db.tasbeehlogs.Where(a => a.Userid == Userid && a.grouptasbeehid == grouptasbeehid&&a.Flag==0).FirstOrDefault();
                data.startat = DateTime.Now;
                data.Flag = 1;
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, data);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        // update date of group tasbeeh
        [HttpGet]
        public HttpResponseMessage updatedata(int id,DateTime  newenddate)
        {
            try
            {

                var data=Db.GroupTasbeeh.FirstOrDefault(a=>a.ID == id);
                data.End_date = newenddate;
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "update succesfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        //update tasbeeh day
        [HttpGet]
        public HttpResponseMessage updateday(int id,int day)
        {
            try
            {

                var data = Db.GroupTasbeeh.FirstOrDefault(a => a.ID == id);
                data.schedule = day;
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "update succesfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }



    }
}
