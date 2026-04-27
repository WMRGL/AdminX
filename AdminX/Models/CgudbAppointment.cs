using Microsoft.EntityFrameworkCore;

namespace AdminX.Models
{
    [Keyless]
    public class CgudbNotInEpic
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? SocialSecurity { get; set; }
        public DateTime BookedDate { get; set; }
        public DateTime BookedTime { get; set; }
        public string? Type { get; set; }
        public string? Location { get; set; }
        public string? CliniciansName { get; set; }
        public string? Attendance { get; set; }
        public DateTime? ReferralDate { get; set; }
        public short? EstDurationMins { get; set; }
        public string? Facility { get; set; }
    }
}
