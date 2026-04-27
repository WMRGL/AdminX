using Microsoft.EntityFrameworkCore;

namespace AdminX.Models
{
    [Keyless]
    public class EpicNotInCgudb
    {
        public string Forename { get; set; }
        public string Surname { get; set; }
        public string NHSNo { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string AppointmentLocation { get; set; }
        public string ClinicCode { get; set; }
        public string ClinicName { get; set; }
        public string ClinicleadClinicianType { get; set; }
        public string AppointmentAttendance { get; set; }
        public string AppointmentOutcome { get; set; }
        public string ProviderName { get; set; }
        public string specialtyname { get; set; }
        public string ConsultationMechanism { get; set; }
    }
}
