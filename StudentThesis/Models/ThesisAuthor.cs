//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StudentThesis.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ThesisAuthor
    {
        public int ThesisAuthorID { get; set; }
        public int ResearchID { get; set; }
        public int AuthorID { get; set; }
    
        public virtual Author Author { get; set; }
        public virtual Research Research { get; set; }
    }
}
