using StudentThesis.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace StudentThesis.Controllers
{
    [Authorize]
    public class ThesesController : Controller
    {
        private Student_ThesisEntities db = new Student_ThesisEntities();

        // GET: Theses
        public ActionResult Index()
        {
            var statuses = db.GetThesesWithStatus().ToList();
            var theses = db.Thesis.Include(t => t.ThesisFiles).Where(t => t.AuthorCategoryID == 2 && t.IsArchived == null).ToList();
            foreach (var status in theses)
            {
                var report = statuses.Where(m => m.ThesisID == status.ThesisID).FirstOrDefault();
                if (report != null)
                {
                    status.Status = report.Status;
                }
            } 

            return View(theses);
        }
        public ActionResult StudentArchiveIndex()
        {
            var statuses = db.GetThesesWithStatus().ToList();
            var theses = db.Thesis.Include(t => t.ThesisFiles).Where(t => t.AuthorCategoryID == 2 && t.IsArchived != null).ToList();
            foreach (var status in theses)
            {
                var report = statuses.Where(m => m.ThesisID == status.ThesisID).FirstOrDefault();
                if (report != null)
                {
                    status.Status = report.Status;
                }
            }

            return View(theses);
        }
        public ActionResult InitialThesis()
        {
            var theses = db.Thesis.Include(t => t.ThesisFiles)
                                  .Where(t => t.Room == null && t.ScheduleDate == null && t.AuthorCategoryID == 2)
                                  .ToList();
            return View(theses);
        }

        public ActionResult DefenseSchedule()
        {
            var theses = db.Thesis.Include(t => t.ThesisFiles)
                                  .Where(t => t.Room != null && t.ScheduleDate != null && t.IsApproved == null && t.AuthorCategoryID == 2)
                                  .ToList();
            return View(theses);
        }

        public ActionResult Development()
        {
            var theses = db.Thesis.Include(t => t.ThesisFiles)
                                  .Where(t => t.FinalRoom == null && t.FinalScheduleDate == null && t.IsApproved != null && t.AuthorCategoryID == 2)
                                  .ToList();
            return View(theses);
        }

        public ActionResult FinalDefense()
        {
            var theses = db.Thesis.Include(t => t.ThesisFiles)
                                  .Where(t => t.FinalRoom != null && t.FinalScheduleDate != null && t.IsApproved != null && t.AuthorCategoryID == 2 && t.FinalApproval == null)
                                  .ToList();
            return View(theses);
        }

        public ActionResult ApprovedManuscript()
        {
            var theses = db.Thesis.Include(t => t.ThesisFiles)
                                  .Where(t => t.FinalRoom != null && t.FinalScheduleDate != null && t.IsApproved != null && t.FinalApproval != null && t.AuthorCategoryID == 2)
                                  .ToList();
            return View(theses);
        }

        public ActionResult Approve(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Thesis thesis = db.Thesis.Find(id);
            if (thesis == null)
            {
                return HttpNotFound();
            }
            thesis.AuthorCategoryID = 2;
            thesis.IsApproved = true; // Set IsApproved to true
            db.SaveChanges();

            return RedirectToAction("Development");
        }

        public ActionResult FinalApproval(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Thesis thesis = db.Thesis.Find(id);
            if (thesis == null)
            {
                return HttpNotFound();
            }
            thesis.AuthorCategoryID = 2;
            thesis.FinalApproval = true; // Set FinalApproval to true
            db.SaveChanges();

            return RedirectToAction("ApprovedManuscript");
        }

        public ActionResult Archive(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Thesis thesis = db.Thesis.Find(id);
            if (thesis == null)
            {
                return HttpNotFound();
            }
            thesis.Year = DateTime.Now;
            thesis.AuthorCategoryID = 2;
            thesis.IsApproved = true;
            thesis.IsArchived = true;// Set IsApproved to true
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult UndoArchive(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Thesis thesis = db.Thesis.Find(id);
            if (thesis == null)
            {
                return HttpNotFound();
            }
            thesis.Year = DateTime.Now;
            thesis.AuthorCategoryID = 2;
            thesis.IsApproved = true;
            thesis.IsArchived = null;// Set IsApproved to true
            db.SaveChanges();

            return RedirectToAction("StudentArchiveIndex");
        }

        // GET: Theses/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Theses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Thesis thesis)
        {
            if (!ModelState.IsValid)
            {
                return View(thesis);
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    thesis.Year = DateTime.Now;
                    thesis.AuthorCategoryID = 2;
                    var membername = thesis.MemberName;
                    var files = thesis.Files;

                    db.Thesis.Add(thesis);
                    db.SaveChanges();

                    var pattern = @"\s*;\s*";
                    var memberNames = Regex.Split(membername, pattern)
                                            .Select(s => s.Trim())
                                            .Distinct()
                                            .ToList();

                    foreach (var memberName in memberNames)
                    {
                        var member = db.Member.FirstOrDefault(mn => mn.MemberName == memberName);

                        if (member == null)
                        {
                            member = new Member { MemberName = memberName };
                            db.Member.Add(member);
                            db.SaveChanges(); // Save the new member before using its MemberID
                        }

                        var thesisMember = new ThesisMembers { ThesisID = thesis.ThesisID, MemberID = member.MemberID };
                        db.ThesisMembers.Add(thesisMember);
                    }

                    if (files != null && files.Count > 0)
                    {
                        foreach (var file in files)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                using (var ms = new MemoryStream())
                                {
                                    file.InputStream.CopyTo(ms);
                                    var fileBytes = ms.ToArray();

                                    var pdfFiles = new Files { PDFFiles = fileBytes };
                                    db.Files.Add(pdfFiles);
                                    db.SaveChanges();

                                    var thesisFiles = new ThesisFiles { ThesisID = thesis.ThesisID, FilesID = pdfFiles.FilesID };
                                    db.ThesisFiles.Add(thesisFiles);
                                }
                            }
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

        // GET: Theses/Edit/5
        public ActionResult FinalSched(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Thesis thesis = db.Thesis.Find(id);
            if (thesis == null)
            {
                return HttpNotFound();
            }
            return View(thesis);
        }

        // POST: Theses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FinalSched(Thesis thesis)
        {
            if (ModelState.IsValid)
            {
                thesis.Year = DateTime.Now;
                thesis.AuthorCategoryID = 2;
                db.Entry(thesis).State = EntityState.Modified;

                // Add panelist handling logic
                var finalpanelistname = thesis.FinalPanelistName;
                var pattern = @"\s*;\s*";
                var finalpanelistNames = Regex.Split(finalpanelistname, pattern)
                                        .Select(s => s.Trim())
                                        .Distinct()
                                        .ToList();

                foreach (var finalpanelistName in finalpanelistNames)
                {
                    var finalpanelist = db.FinalPanelist.FirstOrDefault(pn => pn.FinalPanelistName == finalpanelistName);

                    if (finalpanelist == null)
                    {
                        finalpanelist = new FinalPanelist { FinalPanelistName = finalpanelistName };
                        db.FinalPanelist.Add(finalpanelist);
                        db.SaveChanges();
                    }

                    var finalpanelistID = finalpanelist.FinalPanelistID;

                    var existingPanelistMember = db.FinalPanelistMembers.FirstOrDefault(pm => pm.ThesisID == thesis.ThesisID && pm.FinalPanelistID == finalpanelistID);

                    if (existingPanelistMember == null)
                    {
                        var finalpanelistMembers = new FinalPanelistMembers { ThesisID = thesis.ThesisID, FinalPanelistID = finalpanelistID };
                        db.FinalPanelistMembers.Add(finalpanelistMembers);
                        db.SaveChanges();
                    }
                }
                return RedirectToAction("Index");
            }
            return View(thesis);
        }
        // GET: Theses1/Edit/5
        public ActionResult ThesisEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Thesis thesis = db.Thesis.Find(id);
            if (thesis == null)
            {
                return HttpNotFound();
            }
            return View(thesis);
        }

        // POST: Theses1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThesisEdit(Thesis thesis)
        {
            if (ModelState.IsValid)
            {
                thesis.Year = DateTime.Now;
                thesis.AuthorCategoryID = 2;
                db.Entry(thesis).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(thesis);
        }

        // GET: Theses/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Thesis thesis = db.Thesis.Find(id);
            if (thesis == null)
            {
                return HttpNotFound();
            }
            return View(thesis);
        }

        // POST: Theses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Thesis thesis)
        {
            if (ModelState.IsValid)
            {
                thesis.Year = DateTime.Now;
                thesis.AuthorCategoryID = 2;
                db.Entry(thesis).State = EntityState.Modified;

                // Add panelist handling logic
                var panelistname = thesis.PanelistName;
                var pattern = @"\s*;\s*";
                var panelistNames = Regex.Split(panelistname, pattern)
                                        .Select(s => s.Trim())
                                        .Where(s => !string.IsNullOrEmpty(s))
                                        .Distinct()
                                        .ToList();

                foreach (var panelistName in panelistNames)
                {
                    var panelist = db.Panelist.FirstOrDefault(pn => pn.PanelistName == panelistName);

                    if (panelist == null)
                    {
                        panelist = new Panelist { PanelistName = panelistName };
                        db.Panelist.Add(panelist);
                        db.SaveChanges();
                    }

                    var panelistID = panelist.PanelistID;

                    var existingPanelistMember = db.PanelistMembers.FirstOrDefault(pm => pm.ThesisID == thesis.ThesisID && pm.PanelistID == panelistID);

                    if (existingPanelistMember == null)
                    {
                        var panelistMembers = new PanelistMembers { ThesisID = thesis.ThesisID, PanelistID = panelistID };
                        db.PanelistMembers.Add(panelistMembers);
                        db.SaveChanges();
                    }
                }

                return RedirectToAction("Index");
            }

            return View(thesis);
        }

        // GET: Theses/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Thesis thesis = db.Thesis.Find(id);
            if (thesis == null)
            {
                return HttpNotFound();
            }
            return View(thesis);
        }

        // POST: Theses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Thesis thesis = db.Thesis.Find(id);
            db.Thesis.Remove(thesis);
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
