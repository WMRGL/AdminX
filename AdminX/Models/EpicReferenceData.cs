using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace AdminX.Models
{
    [Table("DownstreamPatientReferenceTable", Schema = "dbo")]
    public class EpicPatientReference
    {
        [Key]
        public int ID { get; set; }
        public int MPI { get; set; }
        public string? ExternalPatientID { get; set; }
        public string? Title { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DOB { get; set; }
        public string? PtSex { get; set; }
        public string? EthnicCode { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Address3 { get; set; }
        public string? Address4 { get; set; }
        public string? Address5 { get; set; }
        public string? PostCode { get; set; }
        public string? NHSNo { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? GPPractice { get; set; }
        public string? GP { get; set; }
        public string? MiddleName { get; set; }
        public string? PhoneHome { get; set; }
        public string? PhoneWork { get; set; }
        public string? PhoneMobile { get; set; }
        public string? Email { get; set; }
        public int UpdateSts { get; set; }
        public string? RecordStatus { get; set; }
        public string? LocalUpdateBy { get; set; }
        public DateTime? LocalUpdateDate { get; set; }
        public string? PrimaryLanguage { get; set; }
    }

    [Table("DownstreamReferralStagingTable", Schema = "dbo")]
    public class EpicReferralStaging
    {
        [Key]
        public int ID { get; set; }
        public string PatientID { get; set; }
        //public string ReferralID { get; set; }
        public int ReferralID { get; set; }
        public DateTime ReferralDate { get; set; }
        public string? ReferredBy { get; set; }
        public string? ReferredTo { get; set; }
        public string? Urgency { get; set; }
        public string? Pathway { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? createdby { get; set; }
        public string? Pregnancy { get; set; }
        public string? Indication { get; set; }
        public string? Speciality { get; set; }
        public Int16? UpdateSts { get; set; }
        public string? UpdateMessage { get; set; }
        public string? ReferralStatus { get; set; }
    }

    [Table("DownstreamReferralReferenceTable", Schema = "dbo")]
    public class EpicReferralReference
    {
        [Key]
        public int ID { get; set; }
        public int? MPI { get; set; }
        //public string? ExternalReferralID { get; set; }
        public int? ExternalReferralID { get; set; }
        public DateTime ReferralDate { get; set; }
        public DateTime? ClockStartDate { get; set; }
        public DateTime? ClockStopDate { get; set; }
        public string? ReferralType { get; set; }
        public string? Consultant { get; set; }
        public string? GC { get; set; }
        public string? AdminContact { get; set; }
        public string? Pathway { get; set; }
        public string? ReferredBy { get; set; }
        public string? Class { get; set; }
        public string? Pregnant { get; set; }
        public string? Indication { get; set; }
        public string? LocalUpdateBy { get; set; }
        public DateTime? LocalUpdateDate { get; set; }
        public int? RefID { get; set; }
        public int? UpdateSts { get; set; }
        public string? PatientID { get; set; }
        public string? ReferralStatus { get; set; }
        public int LocalUpdateSts { get; set; }
    }
}
