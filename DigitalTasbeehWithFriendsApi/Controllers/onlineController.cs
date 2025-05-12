using DigitalTasbeehWithFriendsApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http; 

namespace DigitalTasbeehWithFriendsApi.Controllers
{
    public class onlineController : ApiController
    {
        DTWFEntities Db = new DTWFEntities();
        public HttpResponseMessage Getip(int userid)
        {
            var data=Db.Users.Where(a => a.ID == userid).FirstOrDefault();
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound,"Offline");
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Online"); 
         }
    }
}
 