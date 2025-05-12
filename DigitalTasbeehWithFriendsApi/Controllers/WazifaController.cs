using DigitalTasbeehWithFriendsApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DigitalTasbeehWithFriendsApi.Controllers
{
    public class WazifaController : ApiController
    {
        DTWFEntities Db = new DTWFEntities();
    
     

        //Add wazifa Text
        [HttpPost]
        public HttpResponseMessage Addwazifatext(wazifa_text txt)
        {
            try
            {
                Db.wazifa_text.Add(txt);
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, txt);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        //Delete Wazifa Text
        [HttpGet]
        public HttpResponseMessage Deletwazifatext(int id)
        {
            try
            {
                var data = Db.wazifa_text.Where(a => a.id == id).FirstOrDefault();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found");
                }
                Db.wazifa_text.Remove(data);
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Wazifa Text Succesfully Deleted");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [HttpPost]
        public HttpResponseMessage Createcompundwazifa(List<wazifa_Deatiles> wd)
        {
            try
            {
                Db.wazifa_Deatiles.AddRange(wd);
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Wazifa Create Succsfully");
            }
            catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message); }
        }
        //Delete wazifa api function
        [HttpGet]
        public HttpResponseMessage Deletecompletewazifa(int id, int userid)
        {
            try
            {
                var data = Db.Tasbeeh.FirstOrDefault(a => a.ID == id && a.User_id == userid);
                data.Flag = true;
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Delete Succesfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        //get all tasbeeh or wazifa deatiles api function to show in notification
        [HttpGet]
        public HttpResponseMessage Gettasbeehwazifadeatiles(int id)
        {
            try
            {
                var tasbeehdeatiles = Db.Tasbeeh.Any(t => t.ID == id)
    ? Db.Tasbeeh
        .Join(Db.Tasbeeh_Detailes, t => t.ID, td => td.Tasbeeh_id, (t, td) => new
        {
            Tasbeeh = t,
            Tasbeehdeatiles = td
        })
        .Join(Db.Quran_Tasbeeh, ttd => ttd.Tasbeehdeatiles.Quran_Tasbeeh_id, qt => qt.ID, (ttd, qt) => new
        {
            Qurantasbeeh = qt,
            Tasbeeh = ttd.Tasbeeh
        })
        .Where(a => a.Tasbeeh.ID == id)
        .Select(data => new
        {
            Text = data.Qurantasbeeh.Ayah_text,
            Count = data.Qurantasbeeh.Count
        })
        .ToList()
    : Db.Tasbeeh
        .Join(Db.wazifa_Deatiles, w => w.ID, wd => wd.Wazifa_id, (w, wd) => new
        {
            wazifa = w,
            wazifadeatiles = wd
        })
        .Join(Db.wazifa_text, wwd => wwd.wazifadeatiles.wazifa_text_id, wt => wt.id, (wwd, wt) => new
        {
            wwd.wazifa,
            wazifaText = wt
        })
        .Where(x => x.wazifa.ID == id)
        .Select(w => new
        {
            Text = w.wazifaText.text,
            Count = w.wazifaText.count ?? 0
        })
        .ToList();




                if (tasbeehdeatiles == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found");
                }
                return Request.CreateResponse(HttpStatusCode.OK, tasbeehdeatiles);

            }
            catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message); }
        }
        
    }

}
