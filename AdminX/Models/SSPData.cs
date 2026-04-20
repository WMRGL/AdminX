using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminX.Models
{
    [Table("ViewPatientSocialServicePathways", Schema = "dbo")]
    public class SSP
    {
        [Key]
        public int SSPID { get; set; }        
        public string SSPStatus { get; set; }
        public bool IsActive { get; set; }        
        public string? ParentalResponsibility { get; set; }
        public DateTime? DateInformationReq { get; set; }
        public DateTime? DateInformationReqFU { get; set; }
        public DateTime? DateInformationReceived { get; set; }
        public string? Outcome { get; set; }
        public string? OtherOutcomeDetails { get; set; }
        public string? CGU_No { get; set; }
        public string? PatientFirstName { get; set; }
        public string? PatientLastName { get; set; }
        public DateTime? DOB { get; set; }
        public string? SOCIAL_SECURITY { get; set; }
        public string? SocialWorkerTitle { get; set; }
        public string? SocialWorkerFirstName { get; set; }
        public string? SocialWorkerLastName { get; set; }
        public string? SocialWorkerJobTitle { get; set; }
        public string? Name { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Address3 { get; set; }
        public string? Address4 { get; set; }
        public string? Address5 { get; set; }
    }

    [Table("ListSocialServices", Schema = "dbo")]
    public class SocialServices
    {
        [Key]
        public string SocialServicesID { get; set; }
        public string? Name { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Address3 { get; set; }
        public string? Address4 { get; set; }
        public string? Address5 { get; set; }
        public string? PostCode { get; set; }
    }

    [Table("ListSocialWorkers", Schema = "dbo")]
    public class SocialWorkers
    {
        [Key]
        public string SocialWorkerID { get; set; }
        public string? Title { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? JobTitle { get; set; }
        public string? SocialServicesID { get; set; }
    }

    [Table("ListSocialServicePathwayOutcomes", Schema = "dbo")]
    public class SocialServicePathwayOutcome
    {
        [Key]
        public int ID { get; set; }
        public string Outcome { get; set; }
    }

    [Table("ListSSPStatus", Schema = "dbo")]
    public class SocialServicePathwayStatus
    {
        [Key]
        public int SSPStatusID { get; set; }
        public string SSPStatus { get; set; }
    }
}
