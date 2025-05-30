using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

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

    [Table("View_DictatedLettersReport_Clinicians", Schema = "dbo")]
    public class DictatedlettersReportClinicians
    {
        [Key]
        public string LetterFromCode { get; set; }
        public string Name { get; set; }
        [Column("Draft Count")]
        public int DraftCount { get; set; }
        [Column("Oldest Draft")]
        public string? OldestDraft { get; set; }
        [Column("For Approval Count")]
        public int ForApprovalCount { get; set; }
        [Column("Oldest For Approval")]
        public string? OldestForApproval { get; set; }

    }

    [Table("View_DictatedLetters_SecTeam_Report", Schema = "dbo")]
    public class DictatedLettersSecTeamReport
    {
        [Key]
        [Column("Secretarial Team")]
        public string SecretarialTeam { get; set; }
        [Column("Draft Count")]
        public int DraftCount { get; set; }
        [Column("Oldest Draft")]
        public string? OldestDraft { get; set; }
        [Column("For Approval Count")]
        public int ForApprovalCount { get; set; }
        [Column("Oldest For Approval")]
        public string? OldestForApproval { get; set; }
        [Column("For Corrections")]
        public int ForCorrections { get; set; }
        [Column("Oldest For Corrections")]
        public string? OldestForCorrections { get; set; }
        [Column("For Printing Count")]
        public int ForPrintingCount { get; set; }
        [Column("Oldest For Printing")]
        public string? OldestForPrinting { get; set; }
    }

    [Keyless]
    [Table("DictatedLettersReport", Schema = "dbo")]
    public class DictatedLettersReport
    {
        public string LetterFromCode { get; set; }
        public string Name { get; set; }
        [Column("Secretarial Team")]
        public string SecretarialTeam { get; set; }
        [Column("Draft Count")]
        public int DraftCount { get; set; }
        [Column("Oldest Draft")]
        public string? OldestDraft { get; set; }
        [Column("For Approval Count")]
        public int ForApprovalCount { get; set; }
        [Column("Oldest For Approval")]
        public string? OldestForApproval { get; set; }
        [Column("For Corrections")]
        public int ForCorrections { get; set; }
        [Column("Oldest For Corrections")]
        public string? OldestForCorrections { get; set; }
        [Column("For Printing Count")]
        public int ForPrintingCount { get; set; }
        [Column("Oldest For Printing")]
        public string? OldestForPrinting { get; set; }
    }

    [Table("ListDiaryAction", Schema = "dbo" )]
    public class DiaryAction
    {
        [Key]
        public string ActionCode { get; set; }
        public string? Action { get; set; }
    }


}
