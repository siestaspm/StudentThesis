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
    
    public partial class PanelistMembers
    {
        public int PanelistMembersID { get; set; }
        public int ThesisID { get; set; }
        public int PanelistID { get; set; }
    
        public virtual Panelist Panelist { get; set; }
        public virtual Thesis Thesis { get; set; }
    }
}
