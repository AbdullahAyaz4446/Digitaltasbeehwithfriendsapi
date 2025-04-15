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
        //[HttpGet]
        //public HttpResponseMessage Deletegroup(int id)
        //{
        //    try
        //    {
        //        var group = Db.Groups.Where(a => a.ID == id).FirstOrDefault();
        //        if (group == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found");
        //        }
        //        var groupusers = Db.GroupUsers.Where(a => a.Group_id == id).ToList();
        //        var grouptasbeeh = Db.GroupTasbeeh.Where(a => a.Group_id == id).ToList();
        //        var request = Db.Request.Where(a => a.Group_id == id).ToList();
        //        //var groupusertasbeehdeatiles = Db.
        //        foreach (var groupuser in groupusers)
        //        {
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}
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
        public HttpResponseMessage TasbeehProgressAndMembersProgress(int groupId)
        {
            try
            {
               

                var tasbeehProgress = Db.groupusertasbeehdeatiles
                    .Join(Db.GroupTasbeeh, gutd => gutd.Group_Tasbeeh_Id, gt => gt.ID, (gutd, gt) => new
                    {
                        GroupTasbeeh = gt,
                        GroupUserTasbeehDetails = gutd
                    })
                    .Where(res => res.GroupTasbeeh.Group_id == groupId && res.GroupTasbeeh.Status == "Active")
                    .Join(Db.GroupUsers, gt => gt.GroupUserTasbeehDetails.Group_user_id, gu => gu.ID, (gt, gu) => new
                    {
                        GroupTasbeeh = gt.GroupTasbeeh,
                        GroupUserTasbeehDetails = gt.GroupUserTasbeehDetails,
                        GroupUser = gu
                    })
                    .Join(Db.Users, g => g.GroupUser.Members_id, u => u.ID, (g, u) => new
                    {
                        TasbeehID = g.GroupTasbeeh.Tasbeeh_id,
                        Groupid =g.GroupUser.Group_id,
                        TasbeehGoal = g.GroupTasbeeh.Goal,
                        Achieved = g.GroupTasbeeh.Achieved,
                        Username = u.Username,
                        Status=u.Status,
                        AssignCount = g.GroupUserTasbeehDetails.Assign_count,
                        CurrentCount = g.GroupUserTasbeehDetails.Current_count,
                        
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
        [HttpPost]
        public HttpResponseMessage IncremnetInTasbeeh(int groupid)
        {
            try
            {

                var tasbeehProgress = Db.groupusertasbeehdeatiles
                  .Join(Db.GroupTasbeeh, gutd => gutd.Group_Tasbeeh_Id, gt => gt.ID, (gutd, gt) => new
                  {
                      GroupTasbeeh = gt,
                      GroupUserTasbeehDetails = gutd
                  })
                  .Where(res => res.GroupTasbeeh.Group_id == groupid && res.GroupTasbeeh.Status == "Active")
                  .Join(Db.GroupUsers, gt => gt.GroupUserTasbeehDetails.Group_user_id, gu => gu.ID, (gt, gu) => new
                  {
                      GroupTasbeeh = gt.GroupTasbeeh,
                      GroupUserTasbeehDetails = gt.GroupUserTasbeehDetails,
                      GroupUser = gu
                  })
                  .Join(Db.Users, g => g.GroupUser.Members_id, u => u.ID, (g, u) => new
                  {
                      CurrentCount = g.GroupUserTasbeehDetails.Current_count
                  })
                  .ToList();
                var totalcount = tasbeehProgress.Sum(t=>t.CurrentCount);
                //var grouptasbeehID = tasbeehProgress.Select(t => t.GrouptasbeehId).Distinct().ToList();
                var GroupTasbeehdata = Db.GroupTasbeeh.Where(gt => gt.Group_id == groupid && gt.Status == "Active").FirstOrDefault();
                GroupTasbeehdata.Achieved = totalcount ?? 0;
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
        public HttpResponseMessage Tasbeehlogs(int groupid)
        {
            try
            {
                var data = Db.GroupTasbeeh.Where(a => a.Group_id == groupid).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, data);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
