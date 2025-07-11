﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Permissions;
using System.Web.Http;
using DigitalTasbeehWithFriendsApi.Models;
namespace DigitalTasbeehWithFriendsApi.Controllers
{
    public class SigleController : ApiController
    {
         DTWFEntities Db = new DTWFEntities();
        [HttpPost]
        public HttpResponseMessage CreateSingletasbeeh(SingleTasbeeh t)
        {
            try
            {
                t.Flag = false;
                Db.SingleTasbeeh.Add(t);
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Single Tasbeeh Create Succesfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost] 
        public HttpResponseMessage SearchSingletasbeeh(String name)
        {
            try
            {
                var data = Db.SingleTasbeeh.Where(a => a.Title == name).FirstOrDefault();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found");
                }
                return Request.CreateResponse(HttpStatusCode.OK, data); 

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        public HttpResponseMessage GetAllSingletasbeehbyid(int userid)
        {
            try
            {
                var data = Db.SingleTasbeeh.Where(a => a.User_id == userid&&a.Flag==false).ToList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No Signle Tasbeeh Found For Specfix Member");

                }
                return Request.CreateResponse(HttpStatusCode.OK,data);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        public HttpResponseMessage deletesingle(int id)
        {
            try
            {
                var data=Db.SingleTasbeeh.Where(a => a.ID == id).FirstOrDefault();
                data.Flag = true;
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }
        // Get Single Tasbeeh Logs 
        [HttpGet]
        public HttpResponseMessage Getallsingletasbeehlog(int id)
        {
            try
            {
               
                var data = Db.Tasbeeh.Join(Db.AssignToSingleTasbeeh, t => t.ID, ast => ast.Tasbeeh_id, (t, ast) => new { Tasbeeh = t, Asigntasbeehdata = ast }).Where(result => result.Asigntasbeehdata.SingleTasbeeh_id == id&&result.Asigntasbeehdata.Flag!=3&& result.Asigntasbeehdata.SingleTasbeeh_id == id && result.Asigntasbeehdata.Flag != 4).Select(res => new
                {
                    ID=res.Asigntasbeehdata.ID,
                    title = res.Tasbeeh.Tasbeeh_Title,
                    Goal = res.Asigntasbeehdata.Goal,
                    Achieved = res.Asigntasbeehdata.Achieved,
                    Enddate=res.Asigntasbeehdata.Enddate,
                    Flag=res.Asigntasbeehdata.Flag, 
                    tid=res.Tasbeeh.ID,
                    day=res.Asigntasbeehdata.schedule,

                }).ToList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No Data Found");
                }
                return Request.CreateResponse(HttpStatusCode.OK, data);

            }catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,ex.Message);
            }

        }
        //Read Single Tasbeeh
        [HttpGet]
        public HttpResponseMessage Readsingletasbeeh(int id, int tasbeehid)
        {
            try
            {
                var data = Db.AssignToSingleTasbeeh.Where(a => a.SingleTasbeeh_id == id && a.ID == tasbeehid&&a.Flag==0).FirstOrDefault();
                if (data.Goal == data.Achieved+1)
                {
                    data.Flag = 2;
                    data.Achieved = data.Achieved + 1;
                    Db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK,"Completed");
                }
                else
                {  
                    data.Achieved = data.Achieved + 1;
                    Db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK,data);
                }
                
             

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }
        //Get Single Tasbeeh data
        [HttpGet]
        public HttpResponseMessage Getsingletasbeehdata(int id, int tasbeehid)
         {
            try
            {
                var data=Db.AssignToSingleTasbeeh.Where(a=>a.SingleTasbeeh_id==id&&a.ID==tasbeehid).FirstOrDefault();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found");
                }
                return Request.CreateResponse(HttpStatusCode.OK,data);

            }catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }
        [HttpPost]
        public HttpResponseMessage Assigntosingletasbeeh(AssignToSingleTasbeeh td)
        {
            try
            {
                td.Flag = 0;
                td.Startdate = DateTime.Now;
                Db.AssignToSingleTasbeeh.Add(td);
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Single Assign Succesfully");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        //[HttpGet]
        //public HttpResponseMessage Resumetasbeeh(int id)
        //{
        //    try
        //    {
        //        var selectedTasbeeh = Db.AssignToSingleTasbeeh.FirstOrDefault(a => a.ID == id);
        //        if (selectedTasbeeh == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound, "Tasbeeh not found.");
        //        }
        //        var allsingletasbeeh = Db.AssignToSingleTasbeeh
        //            .Where(a=>a.SingleTasbeeh_id == selectedTasbeeh.SingleTasbeeh_id && a.ID != id)
        //            .ToList();

        //        foreach (var tasbeeh in allsingletasbeeh)
        //        {
        //            tasbeeh.status = "Unactive";
        //        }
        //        selectedTasbeeh.status = "Active";

        //        Db.SaveChanges();

        //        return Request.CreateResponse(HttpStatusCode.OK, "Successfully Activated");

        //    }
        //    catch(Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        // Close Tasbeeh Function
        [HttpGet]
        public HttpResponseMessage Closetasbeeh(int id)
        {
            try
            {
                var data = Db.AssignToSingleTasbeeh.FirstOrDefault(a => a.ID == id);
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
                var data = Db.AssignToSingleTasbeeh.FirstOrDefault(a => a.ID == id);
                data.Flag = 0;
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Tasbeeh open");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        public HttpResponseMessage Reactivesingletasbeeh(int id, DateTime? enddate = null)
        {
            try
            {
                var data = Db.AssignToSingleTasbeeh.FirstOrDefault(a => a.ID == id);
                data.Flag = 3;
                var reactivetasbeeh = new AssignToSingleTasbeeh
                {
                    SingleTasbeeh_id = data.SingleTasbeeh_id,
                    Tasbeeh_id = data.Tasbeeh_id,
                    Goal = data.Goal,
                    Achieved = 0,
                    Startdate = DateTime.Now,
                    Enddate = enddate ?? data.Enddate,
                    Flag = 0

                };
                Db.AssignToSingleTasbeeh.Add(reactivetasbeeh);
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Tasbeeh Reactived Succesfully");

            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        public HttpResponseMessage delete(int id)
        {
            try
            {
                var tasbeeh = Db.AssignToSingleTasbeeh.Where(a => a.ID == id).FirstOrDefault();
                if (tasbeeh == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found");
                }
                tasbeeh.Flag = 4;
                Db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Delete");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

    }
}
 