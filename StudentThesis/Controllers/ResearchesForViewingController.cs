using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using StudentThesis.Models;

namespace StudentThesis.Controllers
{
    public class ResearchesForViewingController : Controller
    {
        private Student_ThesisEntities db = new Student_ThesisEntities();

        // GET: ResearchesForViewing
        public ActionResult Index()
        {
            var research = db.Research.Include(r => r.AuthorCategory);
            return View(research.ToList());
        }

        public ActionResult ViewAbstract(int id)
        {
            var research = db.Research.Find(id);
            if (research != null && research.Abstract != null)
            {
                var base64Pdf = Convert.ToBase64String(research.Abstract);
                byte[] pdfData = Convert.FromBase64String(base64Pdf);
                return new FileContentResult(pdfData, "application/pdf");
            }

            return HttpNotFound();
        }

        public ActionResult ViewFullPaper(int id)
        {
            var research = db.Research.Find(id);
            if (research != null && research.FullPaper != null)
            {
                var base64Pdf = Convert.ToBase64String(research.FullPaper);
                byte[] pdfData = Convert.FromBase64String(base64Pdf);
                return new FileContentResult(pdfData, "application/pdf");
            }

            return HttpNotFound();
        }

    }
}