using DigitalTasbeehWithFriendsApi.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace DigitalTasbeehWithFriendsApi.Controllers
{
    public class AssignTasbeehController : ApiController
    {
        DTWFEntities Db = new DTWFEntities();
        //Assign Tasbeeh To Group Function
        [HttpPost]
        public HttpResponseMessage AssignTasbeeh(GroupTasbeeh Gt)
        {
            try
            {
                Gt.Flag = 0;
                Gt.Start_date = DateTime.Now;
                Db.GroupTasbeeh.Add(Gt);


                Db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, Gt);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        //Wannt Stop Active Tasbeeh Function
        //[HttpGet]
        //public HttpResponseMessage StopTasbeeh(int id)
        //{
        //    try
        //    {
        //        var data = Db.GroupTasbeeh.Where(a => a.ID == id).FirstOrDefault();
        //        data.Status = "Unactive";
        //        Db.SaveChanges();
        //        return Request.CreateResponse(HttpStatusCode.OK, "Succesfully Unactive");

        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

        //    }
        //}
        //Want To Resume Stop Tasbeeh Function
        //[HttpGet]
        //public HttpResponseMessage ActiveTasbeeh(int id)
        //{
        //    try
        //    {
        //        var selectedTasbeeh = Db.GroupTasbeeh.FirstOrDefault(a => a.ID == id);
        //        if (selectedTasbeeh == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound, "Tasbeeh not found.");
        //        }
        //        var allGroupTasbeehs = Db.GroupTasbeeh
        //            .Where(gt => gt.Group_id == selectedTasbeeh.Group_id && gt.ID != id)
        //            .ToList();

        //        foreach (var tasbeeh in allGroupTasbeehs)
        //        {
        //            tasbeeh.Status = "Unactive"; 
        //        }
        //        selectedTasbeeh.Status = "Active";

        //        Db.SaveChanges();

        //        return Request.CreateResponse(HttpStatusCode.OK, "Successfully Activated");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        //Update Tasbeeh in group Function
        [HttpGet]
        public HttpResponseMessage UpdateTasbeehdate(GroupTasbeeh gt)
        {
            try
            {
                var tasbeeh = Db.GroupTasbeeh.FirstOrDefault(a => a.ID == gt.ID);
                if (tasbeeh == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found");
                }
                tasbeeh.Goal = gt.Goal;
                tasbeeh.End_date = gt.End_date;
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Updated");


            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        //Delete Tasbeeh in group Function
        [HttpGet]
        public HttpResponseMessage Deletetasbeehingroup(int id)
        {
            try
            {
                var tasbeeh = Db.GroupTasbeeh.Where(a => a.ID == id).FirstOrDefault();
                if (tasbeeh == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found");
                }
                var groupusertasbeehdeatiles = Db.groupusertasbeehdeatiles.Where(a => a.Group_Tasbeeh_Id == id).ToList();
                var request = Db.Request.Where(a => a.Tasbeeh_Id == id).ToList();
                Db.GroupTasbeeh.Remove(tasbeeh);
                Db.groupusertasbeehdeatiles.RemoveRange(groupusertasbeehdeatiles);
                Db.Request.RemoveRange(request);
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Delete Succesfully");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }
        // Close Tasbeeh Function
        [HttpGet]
        public HttpResponseMessage Closetasbeeh(int id)
        {
            try
            {
                var data = Db.GroupTasbeeh.FirstOrDefault(a => a.ID == id);
                data.Flag = 1;
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Tasbeeh Close");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }



        }
        //open tasbeeh
        [HttpGet]
        public HttpResponseMessage Opentasbeeh(int id)
        {
            try
            {
                var data = Db.GroupTasbeeh.FirstOrDefault(a => a.ID == id);
                data.Flag = 0;
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Tasbeeh open");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //Reactive tasbeeh
        [HttpGet]
        public HttpResponseMessage Reactivecompetetasbeeh(int id, DateTime? enddate = null)
        {
            try
            {

                var data = Db.GroupTasbeeh.FirstOrDefault(a => a.ID == id && a.Flag == 2);
                if (data == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Tasbeeh not found");


                var adminid = Db.Groups.FirstOrDefault(a => a.ID == data.Group_id);
                if (adminid == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Group not found");

                var admingroupuserid = Db.GroupUsers.FirstOrDefault(a => a.Group_id == adminid.ID && a.Members_id == adminid.Admin_id);
                if (admingroupuserid == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Admin group user not found");
                var Grouptasbeeh = new GroupTasbeeh
                {
                    Group_id = data.Group_id,
                    Tasbeeh_id = data.Tasbeeh_id,
                    Goal = data.Goal,
                    Achieved = 0,
                    Start_date = DateTime.Now,
                    Flag = 0,
                    End_date = enddate ?? data.End_date
                };


                Db.GroupTasbeeh.Add(Grouptasbeeh);
                Db.SaveChanges();


                data.Flag = 3;
                Db.Entry(data).State = EntityState.Modified;


                var oldrequests = Db.Request.Where(a => a.Tasbeeh_Id == data.ID && a.Group_id == data.Group_id).ToList();
                foreach (var oldrequest in oldrequests)
                {
                    bool isAdmin = (adminid.Admin_id == oldrequest.Receiver_id);

                    var newRequest = new Request
                    {
                        Tasbeeh_Id = Grouptasbeeh.ID,
                        Sender_id = adminid.Admin_id,
                        Receiver_id = oldrequest.Receiver_id,
                        Group_id = oldrequest.Group_id,
                        Assigned_count = oldrequest.Assigned_count,
                        Send_at = DateTime.Now,
                        Status = isAdmin ? "Accept" : "Pending",
                        Accept_at = isAdmin ? DateTime.Now : (DateTime?)null
                    };
                    Db.Request.Add(newRequest);
                    if (isAdmin)
                    {
                        var gutd = new groupusertasbeehdeatiles
                        {
                            Group_Tasbeeh_Id = Grouptasbeeh.ID,
                            Group_user_id = admingroupuserid.ID,
                            Assign_count = oldrequest.Assigned_count,
                            startdate = DateTime.Now,
                            Current_count = 0,
                            Flag = 0
                        };
                        Db.groupusertasbeehdeatiles.Add(gutd);
                    }
                }
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Successfully reactivated");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        public HttpResponseMessage Reassigntasbeehtospecficmember()
        {
            try
            {
                var form = HttpContext.Current.Request.Form;
                int userid= int.Parse(form["userid"]);
                int groupid = int.Parse(form["groupid"]);
                int grouptasbeehid = int.Parse(form["grouptasbeehid"]);
                int adminid= int.Parse(form["adminid"]);
                int assigncount = int.Parse(form["assigncount"]);
                int id = int.Parse(form["id"]);
                var addnewmembers = new GroupUsers
                {
                    Group_id = groupid,
                    Members_id = userid
                };
                var gernaterequest = new Request
                {
                    Tasbeeh_Id = grouptasbeehid,
                    Sender_id = adminid,
                    Receiver_id = userid,
                    Group_id = groupid,
                    Assigned_count = assigncount,
                    Send_at = DateTime.Now,
                    Status = "Pending"
                };
                Db.GroupUsers.Add(addnewmembers);
                Db.Request.Add(gernaterequest);
                var data=Db.leavegroupusertasbeehdeatiles.Where(a=>a.ID==id).FirstOrDefault();
                data.Flag = 1;
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Succesfully Reassign to specfic members");

            } catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message); }
        }
        }
    }

    

