using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using StudentThesis.Models;

namespace StudentThesis.Controllers
{
    [Authorize] // Requires authorization for accessing this controller
    public class BarChartController : Controller
    {
        private Student_ThesisEntities db = new Student_ThesisEntities(); // Creating an instance of the Student_ThesisEntities database context

        // GET: BarChart
        public ActionResult Index()
        {
            var thesis = db.Thesis.Include(t => t.AuthorCategory); // Fetching all the theses and including the related AuthorCategory
            return View(thesis.ToList()); // Passing the list of theses to the view
        }

        // Retrieves thesis title and year data for student users
        public JsonResult GetThesisTitleAndYear()
        {
            using (var context = new Student_ThesisEntities()) // Creating a new instance of the Student_ThesisEntities database context
            {
                var thesisData = context.GetThesisTitleAndYearStudent().ToList(); // Calling a stored procedure or a function named GetThesisTitleAndYearStudent to retrieve thesis data for students

                var yearsCount = thesisData
                    .GroupBy(data => data.Year) // Grouping the thesis data by year
                    .Select(group => new
                    {
                        Year = group.Key,
                        Count = group.Count() // Counting the number of theses in each group (year)
                    })
                    .OrderBy(item => item.Year) // Sorting the groups by year
                    .ToList();

                var years = yearsCount.Select(item => item.Year).ToList(); // Extracting the years from the groups
                var counts = yearsCount.Select(item => item.Count).ToList(); // Extracting the counts from the groups

                var chartData = new
                {
                    Years = years, // Assigning the years list to the "Years" property
                    Counts = counts // Assigning the counts list to the "Counts" property
                };

                return Json(chartData, JsonRequestBehavior.AllowGet); // Returning the chart data as JSON
            }
        }

        // Displays faculty-related data
        public ActionResult Faculty()
        {
            var thesis = db.Thesis.Include(t => t.AuthorCategory); // Fetching all the theses and including the related AuthorCategory
            return View(thesis.ToList()); // Passing the list of theses to the view
        }

        // Retrieves thesis title and year data for faculty users
        public JsonResult GetThesisTitleAndYearFaculty()
        {
            using (var context = new Student_ThesisEntities()) // Creating a new instance of the Student_ThesisEntities database context
            {
                var thesisData = context.GetThesisTitleAndYear().ToList(); // Calling a stored procedure or a function named GetThesisTitleAndYear to retrieve thesis data for faculty

                var yearsCount = thesisData
                    .GroupBy(data => data.Year) // Grouping the thesis data by year
                    .Select(group => new
                    {
                        Year = group.Key,
                        Count = group.Count() // Counting the number of theses in each group (year)
                    })
                    .OrderBy(item => item.Year) // Sorting the groups by year
                    .ToList();

                var years = yearsCount.Select(item => item.Year).ToList(); // Extracting the years from the groups
                var counts = yearsCount.Select(item => item.Count).ToList(); // Extracting the counts from the groups

                var chartData = new
                {
                    Years = years, // Assigning the years list to the "Years" property
                    Counts = counts // Assigning the counts list to the "Counts" property
                };

                return Json(chartData, JsonRequestBehavior.AllowGet); // Returning the chart data as JSON
            }
        }
    }
}