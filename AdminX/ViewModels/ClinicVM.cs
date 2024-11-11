using Microsoft.EntityFrameworkCore;
using AdminX.Models;

namespace AdminX.ViewModels
{
    [Keyless]
    public class ClinicVM
    {
        public List<StaffMember> staffMembers { get; set; }
        public List<ActivityItem> activityItems { get; set; }
        public ActivityItem activityItem { get; set; }
        public Appointment Clinic { get; set; }
        public Referral linkedReferral { get; set; }
        public List<Outcome> outcomes { get; set; }
        public Patient patients { get; set; }        
        public List<Appointment> pastClinicsList { get; set; }
        public DateTime clinicFilterDate { get; set; }
    }
}
