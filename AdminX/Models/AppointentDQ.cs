using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminX.Models
{
    [Table("Clinical_XP_MasterActivityTable", Schema = "dbo")]
    [Keyless]
    public class CgudbAppointmentDetails
    {
        public int MPI { get; set; }
        public string STAFF_CODE_1 { get; set; }
        public string FACILITY { get; set; }
        [Column("type")]
        public string type { get; set; }
        public DateTime BOOKED_DATE { get; set; }
        public DateTime BOOKED_TIME { get; set; }
        public string COUNSELED { get; set; }
        public DateTime? REFERRAL_DATE { get; set; }
        public int RefID { get; set; }
        public short? EST_DURATION_MINS { get; set; }
        public string? complete { get; set; }
        public int LogicalDelete { get; set; }
        public string? ClockStartDate { get; set; }
        public string? ClockStopDate { get; set; }
    }


    [Table("Clinical_XP_MasterPatientTable", Schema = "dbo")]
    public class MasterPatientTable
    {
        [Key] 
        public int MPI { get; set; }
        public string FIRSTNAME { get; set; }
        public string LASTNAME { get; set; }
        public string SOCIAL_SECURITY { get; set; }
        public string CGU_No { get; set; }
        public DateTime DOB { get; set; }
        public DateTime REFERRAL_DATE { get; set; }
        public int RefID { get; set; }
        public string type { get; set; }

    }

    [Table("Clinical_XP_Staff", Schema = "dbo")]
    [Keyless]
    public class Staff
    {
        public string STAFF_CODE { get; set; }
        public string NAME { get; set; }
    }

    [Table("Clinical_XP_CLIN_FACILITIES", Schema = "dbo")]
    [Keyless]
    public class ClinFacility
    {
        public string FACILITY { get; set; }
        public string LOCATION { get; set; }
    }

    [Table("ViewEpicOutpatients", Schema = "dbo")]
    [Keyless]
    public class EpicAppointmentDetail
    {
        public string localmrn { get; set; }
        public string SpecialtyName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string AppointmentOutcome { get; set; }
        public string AppointmentLocation { get; set; }
        public string ClinicCode { get; set; }
        public string ClinicName { get; set; }
        public string ClinicleadClinicianType { get; set; }
        public string AppointmentAttendance { get; set; }
        public string ProviderName { get; set; }
        public string ConsultationMechanism { get; set; }
    }

    [Table("VeiwEpicPatients", Schema = "dbo")]
    public class EpicPatient
    {
        [Key] 
        public string localmrn { get; set; }
        public string Forename { get; set; }
        public string Surname { get; set; }
        public DateTime DOB { get; set; }
        public string NHSNo { get; set; }
        public string Address { get; set; }
        public string Postcode { get; set; }
        public string GenderIdentity { get; set; }
        public string Sex { get; set; }
        public string SexAssignedAtBirth { get; set; }
        public string Ethnicity { get; set; }
        public string TelephoneNumber { get; set; }
        public string CurrentGPPracticeCode { get; set; }
        public string CurrentGPPracticeName { get; set; }
        public string ReferralID { get; set; }
        public DateTime ReferralReceivedDate { get; set; }
        public string ReferredBy { get; set; }
        public string ReferredTo { get; set; }

    }

    [Table("ViewEpicReferrals", Schema = "dbo")]
    [Keyless]
    public class EpicReferrals
    {
        public string localmrn { get; set; }
        public string ReferralID { get; set; }
        public DateTime ReferralReceivedDate { get; set; }
        public string ReferredBy { get; set; }
        public string ReferredTo { get; set; }
        public string ReferredToSpecialty { get; set; }
    }
}