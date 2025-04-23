using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DigitalTasbeehWithFriendsApi.Models;

namespace DigitalTasbeehWithFriendsApi.Controllers
{
    public class CreateTasbeehController : ApiController
    {
        DTWFEntities Db = new DTWFEntities();
        //All Tasbeeh Function
        [HttpGet]
        public HttpResponseMessage Alltasbeeh(int userid)
        {
            try
            {

                var data = Db.Tasbeeh.Where(t=>t.User_id==userid).ToList();
                if (data == null)
                {
                    
                    return Request.CreateResponse(HttpStatusCode.NotFound,"NO Any Tasbeeh Yet");
                }
                return Request.CreateResponse(HttpStatusCode.OK,data);
            }
            catch (Exception ex)
            {
return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        // Delete tasbeeh Function
        [HttpGet]
        public HttpResponseMessage Deletetasbeeh(int userid,int tabseehid)
        {
            try
            {
                var data = Db.Tasbeeh.Where(a => a.ID == tabseehid && a.User_id == userid).FirstOrDefault();
                var data1 = Db.Tasbeeh_Detailes.Where(a => a.Tasbeeh_id == tabseehid).ToList();
                if (data1 == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found");
                }
                var qurantextid = data1.Select(d => d.Quran_Tasbeeh_id).ToList();
                var data2 = Db.Quran_Tasbeeh.Where(a => qurantextid.Contains(a.ID)).ToList();
                Db.Quran_Tasbeeh.RemoveRange(data2);
                Db.Tasbeeh_Detailes.RemoveRange(data1);
                Db.Tasbeeh.Remove(data);
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK,"Delete Succesfully");

            }catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
   //Create Tasbeeh Title Function
        [HttpPost]
        public HttpResponseMessage CreateTasbeehtitle(Tasbeeh T)
        {
            try
            {
                Db.Tasbeeh.Add(T);
                Db.SaveChanges();
               

                return Request.CreateResponse(HttpStatusCode.OK, T.ID);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }

        }
        //search Tasbeeh By title
        [HttpGet]
        public HttpResponseMessage SearchTasbeeh(String name)
        {
            try
            {
                var tasbeeh = Db.Tasbeeh.FirstOrDefault(a => a.Tasbeeh_Title == name);
                if (tasbeeh == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No Tasbeeh Found for the Specfix Name");
                }

                return Request.CreateResponse(HttpStatusCode.OK, tasbeeh);
            }catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        //Add ayah from Quran function
      [HttpPost]
public HttpResponseMessage AddQuranTasbeeh(string surahName, int ayahNumberFrom, int ayahNumberTo, int count)
{
    try
    {
        
        var result = Db.quran_text
            .Join(Db.Sura,
                qt => qt.sura,  
                s => s.ID,
                (qt, s) => new { qt, s })
            .Where(joined => joined.s.sura_name.ToLower() == surahName.ToLower() &&
                             joined.qt.aya >= ayahNumberFrom &&
                             joined.qt.aya <= ayahNumberTo)
            .GroupBy(joined => joined.s.sura_name)
            .Select(group => new
            {
                totalCount = group.Count(),
                surah_name = group.Key,
                ayah_number_from = group.Min(g => g.qt.aya),
                ayah_number_to = group.Max(g => g.qt.aya),
                ayah_texts = group.OrderBy(g => g.qt.aya).Select(g => g.qt.text).ToList()
            }).ToList();

      
        var quranEntries = result.Select(r => new Quran_Tasbeeh
        {
            Count = count,
            Sura_name = r.surah_name,
            Ayah_number_from = r.ayah_number_from,
            Ayah_number_to = r.ayah_number_to,
            Ayah_text = string.Join(" ", r.ayah_texts)
        }).ToList();

        
        Db.Quran_Tasbeeh.AddRange(quranEntries);
        Db.SaveChanges();

       
        return Request.CreateResponse(HttpStatusCode.OK, quranEntries);
    }
    catch (Exception ex)
    {
        
        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
    }
}
        //Delete Quran Tasbeeh Function
        [HttpGet]
        public HttpResponseMessage Deletequrantasbeeh(int id)
        {
            try
            {
                var data = Db.Quran_Tasbeeh.Where(a => a.ID == id).FirstOrDefault();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found");
                }
                Db.Quran_Tasbeeh.Remove(data);
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK,data.ID);

            }catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // Attect Ayah   To Tasbeeh title I mean Coumpund tasbeeh Function
        [HttpPost]
        public HttpResponseMessage CreateCoumpoundTasbeeh(List<Tasbeeh_Detailes> td)
        {
            try
            {
                Db.Tasbeeh_Detailes.AddRange(td);
                Db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Tasbeeh data saved");

            }catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



    }
}
