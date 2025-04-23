using DigitalTasbeehWithFriendsApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;

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
                var previoustasbeehList = Db.GroupTasbeeh
                    .Where(a => a.Status == "Active" && a.Group_id == Gt.Group_id)
                    .ToList();


                foreach (var tasbeeh in previoustasbeehList)
                {
                    tasbeeh.Status = "Unactive";
                }


                Gt.Start_date = DateTime.Now;
                Gt.Status = "Active";
                Db.GroupTasbeeh.Add(Gt);


                Db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Tasbeeh assigned successfully.");
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        //Wannt Stop Active Tasbeeh Function
        [HttpGet]
        public HttpResponseMessage StopTasbeeh(int id)
        {
            try
            {
                var data = Db.GroupTasbeeh.Where(a => a.ID == id).FirstOrDefault();
                data.Status = "Unactive";
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Succesfully Unactive");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }
        //Want To Resume Stop Tasbeeh Function
        [HttpGet]
        public HttpResponseMessage ActiveTasbeeh(int id)
        {
            try
            {
                var selectedTasbeeh = Db.GroupTasbeeh.FirstOrDefault(a => a.ID == id);
                if (selectedTasbeeh == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Tasbeeh not found.");
                }
                var allGroupTasbeehs = Db.GroupTasbeeh
                    .Where(gt => gt.Group_id == selectedTasbeeh.Group_id && gt.ID != id)
                    .ToList();

                foreach (var tasbeeh in allGroupTasbeehs)
                {
                    tasbeeh.Status = "Unactive";
                }
                selectedTasbeeh.Status = "Active";

                Db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Successfully Activated");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

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
    }
}
