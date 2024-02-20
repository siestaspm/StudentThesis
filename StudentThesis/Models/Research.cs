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
    
    public partial class Research
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Research()
        {
            this.ThesisAuthor = new HashSet<ThesisAuthor>();
        }
    
        public int ResearchID { get; set; }
        public Nullable<int> AuthorCategoryID { get; set; }
        public string Title { get; set; }
        public Nullable<System.DateTime> Year { get; set; }
        public string Keywords { get; set; }
        public string ThesisAdviser { get; set; }
        public string TechnicalCritic { get; set; }
        public byte[] Abstract { get; set; }
        public string TypeOfResearch { get; set; }
        public string ThematicArea { get; set; }
        public byte[] FullPaper { get; set; }
        public Nullable<bool> IsArchived { get; set; }
    
        public virtual AuthorCategory AuthorCategory { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ThesisAuthor> ThesisAuthor { get; set; }
    }
}
