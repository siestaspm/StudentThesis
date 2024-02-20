using StudentThesis.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.Web.Mvc;

namespace StudentThesis.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // Variables to store the number of theses
            int numberOfThesis;
            int numberOfStudentsThesis;
            int numberOfOverallThesis;

            using (var context = new Student_ThesisEntities())
            {
                // Retrieving the number of student theses using a stored procedure
                var results = context.Database.SqlQuery<int>("EXEC GetStudentThesis").FirstOrDefault();
                // Retrieving the number of faculty theses using a stored procedure
                var result = context.Database.SqlQuery<int>("EXEC GetFacultyThesis").FirstOrDefault();
                // Retrieving the overall number of theses using a stored procedure
                var overall = context.Database.SqlQuery<int>("EXEC GetOverallThesis").FirstOrDefault();

                // Assigning the retrieved values to the respective variables, or defaulting to 0 if null
                numberOfOverallThesis = overall != null ? overall : 0;
                numberOfThesis = result != null ? result : 0;
                numberOfStudentsThesis = results != null ? results : 0;
            }

            // Assigning the variables to ViewBag properties for access in the view
            ViewBag.NumberOfStudentThesis = numberOfStudentsThesis;
            ViewBag.NumberOfThesis = numberOfThesis;
            ViewBag.NumberOfOverallThesis = numberOfOverallThesis;

            return View();
        }

        public JsonResult GetChartData()
        {
            using (var context = new Student_ThesisEntities())
            {
                // Retrieving chart data using a stored procedure
                var chartData = context.CountThesesByStatus().ToList();

                // Formatting the chart data to a desired format
                var chartDataFormatted = chartData.Select(data => new
                {
                    Status = data.Status,
                    Count = data.Count
                }).ToList();

                // Returning the formatted chart data as JSON
                return Json(chartDataFormatted, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetThematicAreasData()
        {
            using (var context = new Student_ThesisEntities())
            {
                // Retrieving thematic areas data using a stored procedure
                var thematicAreasData = context.CountThematicAreas().ToList();

                // Extracting labels (thematic areas) and counts from the retrieved data
                var labels = thematicAreasData.Select(data => data.ThematicArea).ToList();
                var counts = thematicAreasData.Select(data => data.Count).ToList();

                // Creating a new object to hold the labels and counts
                var chartData = new
                {
                    Labels = labels,
                    Counts = counts
                };

                // Returning the chart data as JSON
                return Json(chartData, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
