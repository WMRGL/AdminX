﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace AdminX.Models
{
    [Table("IPMReferenceTable", Schema = "dbo")]
    public class EpicPatientReference

    {
        [Key]
        public int ID { get; set; }
        public int MPI { get; set; }
        public string? IPMID { get; set; }
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
        public int IPMUpdateSts { get; set; }
        public string? IPMRecordStatus { get; set; }
        public string? LocalUpdateBy { get; set; }
        public DateTime? LocalUpdateDate { get; set; }
        public string? PrimaryLanguage { get; set; }
    }

    [Table("IPMReferralStagingTable", Schema = "dbo")]
    public class EpicReferralStaging
    {
        [Key]
        public int ID { get; set; }
        public string FacilID { get; set; }
        public int RefID { get; set; }
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
        public Int16? IPMUpdateSts { get; set; }
        public string? IPMUpdateMessage { get; set; }
    }
}
