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
                var Selectedtasbeeh = Db.Tasbeeh.Where(a => a.Flag == false && a.ID == id).FirstOrDefault();

                if (Selectedtasbeeh == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found");
                }

                if (Selectedtasbeeh.Type == "Quran")
                {
                    var tasbeehdeatiles = Db.Tasbeeh
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
                            Count = data.Qurantasbeeh.Count,
                            Type = "Quran"
                        })
                        .ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, tasbeehdeatiles);
                }
                else if (Selectedtasbeeh.Type == "Wazifa")
                {
                    var tasbeehdeatiles = Db.Tasbeeh
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
                            Count = w.wazifaText.count ?? 0,
                            Type = "Wazifa"
                        })
                        .ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, tasbeehdeatiles);
                }
                else if (Selectedtasbeeh.Type == "Compund")
                {
                    var compoundResult = new List<object>();
                    var chainTasbeehDetails = Db.Chaintasbeehdeatiles.Where(a => a.Tasbeeh_id == id).ToList();

                    foreach (var chainItem in chainTasbeehDetails)
                    {
                        var linkedTasbeeh = Db.Tasbeeh.Where(a => a.ID == chainItem.Existing_Tasbeehid).FirstOrDefault();

                        if (linkedTasbeeh != null)
                        {
                            if (linkedTasbeeh.Type == "Wazifa")
                            {
                                var wazifaDetails = Db.wazifa_Deatiles
                                    .Join(Db.wazifa_text, wd => wd.wazifa_text_id, wt => wt.id, (wd, wt) => new { wd, wt })
                                    .Where(x => x.wd.Wazifa_id == linkedTasbeeh.ID)
                                    .Select(x => new
                                    {
                                        Text = x.wt.text,
                                        Count = x.wt.count ?? 0,
                                        Type = "Wazifa"
                                    })
                                    .ToList();
                                compoundResult.AddRange(wazifaDetails);
                            }
                            else if (linkedTasbeeh.Type == "Quran")
                            {
                                var quranDetails = Db.Tasbeeh_Detailes
                                    .Join(Db.Quran_Tasbeeh, td => td.Quran_Tasbeeh_id, qt => qt.ID, (td, qt) => new { td, qt })
                                    .Where(x => x.td.Tasbeeh_id == linkedTasbeeh.ID)
                                    .Select(x => new
                                    {
                                        Text = x.qt.Ayah_text, 
                                        Count = x.qt.Count,
                                        Type = "Quran"
                                    })
                                    .ToList();
                                compoundResult.AddRange(quranDetails);
                            }
                            else if (linkedTasbeeh.Type == "Compund")
                            {
                                var nestedResponse = Gettasbeehwazifadeatiles(linkedTasbeeh.ID);
                                if (nestedResponse.StatusCode == HttpStatusCode.OK)
                                {
                                    var nestedContent = nestedResponse.Content.ReadAsAsync<List<object>>().Result;
                                    compoundResult.AddRange(nestedContent);
                                }
                            }
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, compoundResult);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unsupported Tasbeeh type");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

    }

}
