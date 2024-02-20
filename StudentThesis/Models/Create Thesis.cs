using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StudentThesis.Models
{
    public partial class Thesis
    {
        public string MemberName { get; set; }
        public string PanelistName { get; set; }
        public string FinalPanelistName { get; set; }
        public List<HttpPostedFileBase> Files { get; set; }
        public List<HttpPostedFileBase> NewFiles { get; set; }

        public string Status { get; set; }
    }
}