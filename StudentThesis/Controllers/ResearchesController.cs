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
    [Authorize]
    public class ResearchesController : Controller
    {
        private Student_ThesisEntities db = new Student_ThesisEntities();

        // GET: Researches
        public ActionResult Index()
        {
            var research = db.Research.Include(r => r.AuthorCategory).Where(r => r.AuthorCategoryID == 1 && r.IsArchived == null).ToList();
            return View(research);
        }
        public ActionResult IndexStudent()
        {
            var research = db.Research.Include(r => r.AuthorCategory).Where(r => r.AuthorCategoryID == 2 && r.IsArchived == null).ToList();

            return View(research);
        }

        public ActionResult StudentResearchArchive()
        {
            var research = db.Research.Include(r => r.AuthorCategory).Where(r => r.AuthorCategoryID == 2 && r.IsArchived != null).ToList();

            return View(research);
        }

        public ActionResult FacultyResearchArchive()
        {
            var research = db.Research.Include(r => r.AuthorCategory).Where(r => r.AuthorCategoryID == 1 && r.IsArchived != null).ToList();

            return View(research);
        }

        public ActionResult Archive(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Research research = db.Research.Find(id);
            if (research == null)
            {
                return HttpNotFound();
            }
            research.IsArchived = true;// Set IsApproved to true
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult UndoArchive(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Research research = db.Research.Find(id);
            if (research == null)
            {
                return HttpNotFound();
            }
            research.IsArchived = null;// Set IsApproved to true
            db.SaveChanges();

            return RedirectToAction("FacultyResearchArchive");
        }

        public ActionResult StudentArchive(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Research research = db.Research.Find(id);
            if (research == null)
            {
                return HttpNotFound();
            }
            research.IsArchived = true;// Set IsApproved to true
            db.SaveChanges();

            return RedirectToAction("IndexStudent");
        }

        public ActionResult StudentUndoArchive(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Research research = db.Research.Find(id);
            if (research == null)
            {
                return HttpNotFound();
            }
            research.IsArchived = null;// Set IsApproved to true
            db.SaveChanges();

            return RedirectToAction("StudentResearchArchive");
        }



        public FileResult DownloadAbstract(int id)
        {
            var research = db.Research.Find(id);
            if (research != null && research.Abstract != null)
            {
                return File(research.Abstract, "application/pdf", "abstract.pdf");
            }

            return null;
        }

        public FileResult DownloadFullPaper(int id)
        {
            var research = db.Research.Find(id);
            if (research != null && research.FullPaper != null)
            {
                return File(research.FullPaper, "application/pdf", "fullpaper.pdf");
            }

            return null;
        }


        // GET: Researches/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Research research = db.Research.Find(id);
            if (research == null)
            {
                return HttpNotFound();
            }
            return View(research);
        }

        // GET: Researches/Create
        public ActionResult Create()
        {
            ViewBag.AuthorCategoryID = new SelectList(db.AuthorCategory, "AuthorCategoryID", "AuthorCategory1");
            return View();
        }

        // POST: Researches/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Research research, HttpPostedFileBase fullPaperFile, HttpPostedFileBase abstractFile)
        {
            if (ModelState.IsValid)
            {
                if (fullPaperFile != null && fullPaperFile.ContentLength > 0)
                {
                    using (var binaryReader = new BinaryReader(fullPaperFile.InputStream))
                    {
                        research.FullPaper = binaryReader.ReadBytes(fullPaperFile.ContentLength);
                    }
                }
                if (abstractFile != null && abstractFile.ContentLength > 0)
                {
                    using (var binaryReader = new BinaryReader(abstractFile.InputStream))
                    {
                        research.Abstract = binaryReader.ReadBytes(abstractFile.ContentLength);
                    }
                }

                db.Research.Add(research);
                db.SaveChanges();

                var authorNames = research.AuthorName.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(name => name.Trim())
                                           .Distinct()
                                           .ToList();

                foreach (var authorName in authorNames)
                {
                    var author = db.Author.FirstOrDefault(a => a.AuthorName == authorName);

                    if (author == null)
                    {
                        author = new Author { AuthorName = authorName };
                        db.Author.Add(author);
                        db.SaveChanges(); // Save the new author before using its AuthorID
                    }

                    var thesisAuthor = new ThesisAuthor { ResearchID = research.ResearchID, AuthorID = author.AuthorID };
                    db.ThesisAuthor.Add(thesisAuthor);
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AuthorCategoryID = new SelectList(db.AuthorCategory, "AuthorCategoryID", "AuthorCategory1", research.AuthorCategoryID);
            return View(research);
        }

        // GET: Researches/Create
        public ActionResult CreateTheses()
        {
            ViewBag.AuthorCategoryID = new SelectList(db.AuthorCategory, "AuthorCategoryID", "AuthorCategory1");
            return View();
        }

        // POST: Researches/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTheses(Research research, HttpPostedFileBase fullPaperFile, HttpPostedFileBase abstractFile, int Id = 0)
        {
            if (!ModelState.IsValid)
            {
                return View(research);
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (fullPaperFile != null && fullPaperFile.ContentLength > 0)
                    {
                        using (var binaryReader = new BinaryReader(fullPaperFile.InputStream))
                        {
                            research.FullPaper = binaryReader.ReadBytes(fullPaperFile.ContentLength);
                        }
                    }
                    if (abstractFile != null && abstractFile.ContentLength > 0)
                    {
                        using (var binaryReader = new BinaryReader(abstractFile.InputStream))
                        {
                            research.Abstract = binaryReader.ReadBytes(abstractFile.ContentLength);
                        }
                    }

                    // Pre-populate research object with data from thesis if thesisId is provided
                    if (Id != 0)
                    {
                        var thesis = db.Thesis.Find(Id);
                        if (thesis != null)
                        {
                            research.Year = thesis.Year;
                            research.Title = thesis.Title;
                            research.ThesisAdviser = thesis.ThesisAdviser;
                            research.AuthorCategoryID = thesis.AuthorCategoryID;
                            research.ThematicArea = thesis.ThematicArea;
                            research.TechnicalCritic = thesis.TechnicalCritic;
                            // Populate other relevant properties from the thesis object
                        }

                        db.Research.Add(research);
                        db.SaveChanges();

                        var memberNames = db.ThesisMembers.Where(t => t.ThesisID == Id)
                                                            .Select(t => t.Member.MemberName)
                                                            .Distinct()
                                                            .ToList();

                        foreach (var memberName in memberNames)
                        {
                            var author = db.Author.FirstOrDefault(mn => mn.AuthorName == memberName);

                            if (author == null)
                            {
                                author = new Author { AuthorName = memberName };
                                db.Author.Add(author);
                                db.SaveChanges(); // Save the new member before using its MemberID
                            }

                            var thesisAuthor = new ThesisAuthor { ResearchID = research.ResearchID, AuthorID = author.AuthorID };
                            db.ThesisAuthor.Add(thesisAuthor);
                        }
                    }

                    
                    db.SaveChanges();
                    transaction.Commit();
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }



        // GET: Researches/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Research research = db.Research.Find(id);
            if (research == null)
            {
                return HttpNotFound();
            }
            ViewBag.AuthorCategoryID = new SelectList(db.AuthorCategory, "AuthorCategoryID", "AuthorCategory1", research.AuthorCategoryID);
            return View(research);
        }

        // POST: Researches/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Research research)
        {
            if (ModelState.IsValid)
            {
                db.Entry(research).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AuthorCategoryID = new SelectList(db.AuthorCategory, "AuthorCategoryID", "AuthorCategory1", research.AuthorCategoryID);
            return View(research);
        }

        // GET: Researches/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Research research = db.Research.Find(id);
            if (research == null)
            {
                return HttpNotFound();
            }
            return View(research);
        }

        // POST: Researches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Research research = db.Research.Find(id);
            db.Research.Remove(research);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
