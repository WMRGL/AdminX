using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminX.Models
{
    [Table("MasterActivityTable", Schema = "dbo")]
    [Keyless]
    public class CgudbAppointmentDetails
    {
        public string MPI { get; set; }
        public string STAFF_CODE_1 { get; set; }
        public string FACILITY { get; set; }
        [Column("type")]
        public string type { get; set; }
        public DateTime BOOKED_DATE { get; set; }
        public DateTime BOOKED_TIME { get; set; }
        public string COUNSELED { get; set; }
        public DateTime? REFERRAL_DATE { get; set; }
        public short? EST_DURATION_MINS { get; set; }
    }

 
  //  [Table("MasterPatientTable", Schema = "dbo")]
    public class MasterPatientTable
    {
        [Key] 
        public string MPI { get; set; }
        public string FIRSTNAME { get; set; }
        public string LASTNAME { get; set; }
        public string SOCIAL_SECURITY { get; set; }
    }

    [Table("STAFF", Schema = "dbo")]
    [Keyless]
    public class Staff
    {
        public string STAFF_CODE { get; set; }
        public string NAME { get; set; }
    }

    [Table("CLIN_FACILITIES", Schema = "dbo")]
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
        public string NHSNo { get; set; }
    }
}