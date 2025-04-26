using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
                var data = Db.SingleTasbeeh.Where(a => a.User_id == userid).ToList();
                if(data == null)
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



    }
}
 