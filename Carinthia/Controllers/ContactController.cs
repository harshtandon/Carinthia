using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Carinthia.SolrNet;

namespace Carinthia.Controllers
{
    public class ContactController : Controller
    {
        public ActionResult Search()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(Carinthia.Domain.Contact contact)
        {
            if (!ModelState.IsValid)
                return View();

            string FilePath = "";

            //if (flUpload.PostedFile.ContentLength > 0)
            //{
            //    FilePath = System.Configuration.ConfigurationManager.AppSettings["SolrDocRepository"].ToString() + sContStub + flUpload.PostedFile.FileName.Substring(flUpload.PostedFile.FileName.LastIndexOf('.'));

            //    try
            //    {
            //        flUpload.PostedFile.SaveAs(FilePath);
            //    }
            //    catch (Exception ex)
            //    {
            //        Response.Write("Something went wrong - " + ex.Message);
            //    }
            //}

            contact.ContStub = new System.Guid().ToString();
            contact.AccountId = 123456;
            contact.AttachmentPath = FilePath;

            new Indexer().IndexContact(contact);
            return RedirectToAction("Index");
        }
    }
}