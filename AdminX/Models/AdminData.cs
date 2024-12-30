using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AdminX.Models
{
    [Table("APPTYPE", Schema = "dbo")]
    public class ActivityType
    {
        [Key]
        public string APP_TYPE { get; set; }
        public Int16 NON_ACTIVE { get; set; }
        public bool ISREFERRAL { get; set; }
        public bool ISAPPT { get; set; }
        public bool ISSTUDY { get; set; }
    }

    [Table("PatientSearches", Schema = "dbo")]
    public class PatientSearches
    {
        [Key]
        public int SearchID { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public DateTime DOB { get; set; }
        public string NHSNo { get; set; }
        public string PostCode { get; set; }
        public DateTime SearchDate { get; set; }
        public string SearchBy { get; set; }
        public int SearchStatus { get; set; }
        public string SearchDescription { get; set; }
        public string SearchType { get; set; }
    }

    [Table("PatientSearchResults", Schema = "dbo")]
    public class PatientSearchResults
    {
        [Key]
        public int SearchResultID { get; set; }
        public int SearchID { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public DateTime? DOB  { get; set; }
        public string? NHSNo { get; set; }
        public string? PostCode { get; set; }
        public string? CGUNo { get; set; }
        public int SearchLevel { get; set; }
        public int? MPI { get; set; }
        public string ResultSource { get; set; }
        public int? RelsID { get; set; }
        public int? InternalIndividualID { get; set; }
    }   
    
}
