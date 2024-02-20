using StudentThesis.Models; // Importing the necessary models
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
    [Authorize] // Restricts access to authenticated users
    public class FacultyThesesController : Controller
    {
        private Student_ThesisEntities db = new Student_ThesisEntities(); // Creating an instance of the database context

        // GET: Theses
        public ActionResult Index()
        {
            // Retrieve the list of theses with their corresponding statuses
            var statuses = db.GetThesesWithStatus().ToList();

            // Retrieve the theses authored by faculty members that are not archived
            var theses = db.Thesis.Include(t => t.ThesisFiles).Where(t => t.AuthorCategoryID == 1 && t.IsArchived == null).ToList();

            // Update the status of each thesis based on the retrieved statuses
            foreach (var status in theses)
            {
                var report = statuses.Where(m => m.ThesisID == status.ThesisID).FirstOrDefault();
                if (report != null)
                {
                    status.Status = report.Status;
                }
            }

            return View(theses); // Pass the list of theses to the corresponding view
        }

        // GET: Theses/FacultyArchiveIndex
        public ActionResult FacultyArchiveIndex()
        {
            // Retrieve the list of theses with their corresponding statuses
            var statuses = db.GetThesesWithStatus().ToList();

            // Retrieve the archived theses authored by faculty members
            var theses = db.Thesis.Include(t => t.ThesisFiles).Where(t => t.AuthorCategoryID == 1 && t.IsArchived != null).ToList();

            // Update the status of each thesis based on the retrieved statuses
            foreach (var status in theses)
            {
                var report = statuses.Where(m => m.ThesisID == status.ThesisID).FirstOrDefault();
                if (report != null)
                {
                    status.Status = report.Status;
                }
            }

            return View(theses); // Pass the list of archived theses to the corresponding view
        }

        // GET: Theses/Ongoing
        public ActionResult Ongoing()
        {
            // Retrieve the ongoing theses authored by faculty members
            var theses = db.Thesis.Include(t => t.ThesisFiles)
                                  .Where(t => t.IsFinished == null && t.AuthorCategoryID == 1)
                                  .ToList();

            return View(theses); // Pass the list of ongoing theses to the corresponding view
        }

        // GET: Theses/Complete
        public ActionResult Complete()
        {
            // Retrieve the completed theses authored by faculty members
            var theses = db.Thesis.Include(t => t.ThesisFiles)
                                  .Where(t => t.IsFinished != null && t.AuthorCategoryID == 1)
                                  .ToList();

            return View(theses); // Pass the list of completed theses to the corresponding view
        }

        // GET: Theses/DownloadFile/5
        public ActionResult DownloadFile(int id)
        {
            // Find the file with the given ID in the database
            Files file = db.Files.Find(id);
            if (file == null)
            {
                return HttpNotFound(); // If the file is not found, return a "Not Found" HTTP response
            }

            // Return the file for download
            return File(file.PDFFiles, "application/pdf"); // Return the file with the appropriate content type
        }

        // GET: Theses/IsFinished/5
        public ActionResult IsFinished(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest); // If the ID is not provided, return a "Bad Request" HTTP response
            }

            // Find the thesis with the given ID in the database
            Thesis thesis = db.Thesis.Find(id);
            if (thesis == null)
            {
                return HttpNotFound(); // If the thesis is not found, return a "Not Found" HTTP response
            }

            // Update the thesis's properties to mark it as finished
            thesis.AuthorCategoryID = 1;
            thesis.IsFinished = true;
            db.SaveChanges(); // Save the changes to the database

            return RedirectToAction("Complete"); // Redirect the user to the "Complete" action
        }

        // GET: Theses/Archive/5
        public ActionResult Archive(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest); // If the ID is not provided, return a "Bad Request" HTTP response
            }

            // Find the thesis with the given ID in the database
            Thesis thesis = db.Thesis.Find(id);
            if (thesis == null)
            {
                return HttpNotFound(); // If the thesis is not found, return a "Not Found" HTTP response
            }

            // Update the thesis's properties to mark it as approved and archived
            thesis.AuthorCategoryID = 1;
            thesis.IsApproved = true;
            thesis.IsArchived = true;
            db.SaveChanges(); // Save the changes to the database

            return RedirectToAction("Index"); // Redirect the user to the "Index" action
        }

        // GET: Theses/UndoArchive/5
        public ActionResult UndoArchive(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest); // If the ID is not provided, return a "Bad Request" HTTP response
            }

            // Find the thesis with the given ID in the database
            Thesis thesis = db.Thesis.Find(id);
            if (thesis == null)
            {
                return HttpNotFound(); // If the thesis is not found, return a "Not Found" HTTP response
            }

            // Update the thesis's properties to mark it as approved and remove the archive status
            thesis.AuthorCategoryID = 1;
            thesis.IsApproved = true;
            thesis.IsArchived = null;
            db.SaveChanges(); // Save the changes to the database

            return RedirectToAction("FacultyArchiveIndex"); // Redirect the user to the "FacultyArchiveIndex" action
        }

        // GET: Theses/Create
        public ActionResult Create()
        {
            return View(); // Return the view for creating a new thesis
        }

        // POST: Theses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Thesis thesis)
        {
            if (!ModelState.IsValid)
            {
                return View(thesis); // If the model state is not valid, return the create view with the invalid model
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    thesis.Year = DateTime.Now;
                    thesis.AuthorCategoryID = 1;
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

        // GET: Theses/AddFiles/{thesisId}
        public ActionResult AddFiles(int? id)
        {
            var thesis = db.Thesis.FirstOrDefault(t => t.ThesisID == id);
            if (thesis == null)
            {
                return HttpNotFound(); // If the thesis is not found, return a "Not Found" HTTP response
            }

            return View(thesis); // Return the view for adding files to the thesis
        }

        // POST: Theses/AddFiles/{thesisId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddFiles(int id, Thesis thesis)
        {
            if (!ModelState.IsValid)
            {
                return View(thesis); // If the model state is not valid, return the add files view with the invalid model
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var existingThesis = db.Thesis.FirstOrDefault(t => t.ThesisID == id);
                    if (existingThesis == null)
                    {
                        return HttpNotFound(); // If the thesis is not found, return a "Not Found" HTTP response
                    }

                    var newFiles = thesis.NewFiles;

                    if (newFiles != null && newFiles.Count > 0)
                    {
                        foreach (var file in newFiles)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                using (var ms = new MemoryStream())
                                {
                                    file.InputStream.CopyTo(ms);
                                    var fileBytes = ms.ToArray();

                                    var pdfFile = new Files { PDFFiles = fileBytes };
                                    db.Files.Add(pdfFile);
                                    db.SaveChanges();

                                    var thesisFile = new ThesisFiles { ThesisID = existingThesis.ThesisID, FilesID = pdfFile.FilesID };
                                    db.ThesisFiles.Add(thesisFile);
                                }
                            }
                        }
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    return RedirectToAction("Index"); // Redirect the user to the "Index" action
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        // GET: Theses/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest); // If the ID is not provided, return a "Bad Request" HTTP response
            }

            // Find the thesis with the given ID in the database
            Thesis thesis = db.Thesis.Find(id);
            if (thesis == null)
            {
                return HttpNotFound(); // If the thesis is not found, return a "Not Found" HTTP response
            }

            return View(thesis); // Return the view for editing the thesis
        }

        // POST: Theses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Thesis thesis)
        {
            if (!ModelState.IsValid)
            {
                return View(thesis); // If the model state is not valid, return the edit view with the invalid model
            }

            db.Entry(thesis).State = EntityState.Modified;
            db.SaveChanges(); // Save the changes to the database

            return RedirectToAction("Index"); // Redirect the user to the "Index" action
        }
    }
}
