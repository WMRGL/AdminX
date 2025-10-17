using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Models
{
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
        public DateTime? DOB { get; set; }
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

    [Table("ListDiaryAction", Schema = "dbo")]
    public class DiaryAction
    {
        [Key]
        public string ActionCode { get; set; }
        public string? Action { get; set; }
    }

    [Table("AlertTypes", Schema = "dbo")]
    public class AlertTypes
    {
        [Key]
        public int AlertTypeID { get; set; }
        public string AlertType { get; set; }
        public bool InUse { get; set; }
    }

    [Keyless]
    [Table("ViewMergeHistory", Schema = "dbo")]
    public class MergeHistory
    {
        public int MPI { get; set; }
        public int? OldMPI { get; set; }
        public string NewPedigreeNumber { get; set; }
        public string OldPedigreeNumber { get; set; }
        public string PatientName { get; set; }
        public string Type { get; set; }
        public string MergedBy { get; set; }
    }

    [Keyless]
   // [Table("GenderIdentity")]
    public class GenderIdentity
    {
        public string Gender { get; set; }
    }

    [Keyless]
    [Table("Clinicians_Clinics", Schema = "dbo")]
    public class CliniciansClinics
    {
        public string Facility { get; set; }
        public string? Addressee { get; set; }
        public string? Position { get; set; }
        public string? A_Address { get; set; }
        public string? A_Town { get; set; }
        public string? A_County { get; set; }
        public string? A_PostCode { get; set; }
        public string? A_Salutation { get; set; }
        public string? Preamble { get; set; }
        public string? Postlude { get; set; }
        public string? Copies_To { get; set; }
        // DateOnRef
        public string? ClinicSite { get; set; }
        public int ShowLocalRef { get; set; }
        public bool IncludeSpR { get; set; }
        public string? TimeOfDay { get; set; }
        public DateTime? StartTime { get; set; }
        public int? ApptDuration { get; set; }
        public string? DayOfWeek { get; set; }
        public string? WeeksOfMonth { get; set; }
        public string? MonthsOfYear { get; set; }
        public string? TelNo { get; set; }
        public string? Initials { get; set; }
        public string? Secretary { get; set; }
        public string? WaitingListKind { get; set; }
        public string? PCT { get; set; }
        public Int16 LookAheadMths { get; set; }
        public string? ClinicListEmail { get; set; }
        public string? EmailCCs { get; set; }
        public bool IsCallToBookClinic { get; set; }
        public bool DateOnRef { get; set; }        
    }
}
