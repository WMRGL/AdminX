using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;

namespace AdminX.ViewModels
{
    [Keyless]
    public class CaseloadVM
    {
        public List<Caseload> caseLoad { get; set; }
        public List<StaffMember> clinicians { get; set; }
        public int countClinics { get; set; }
        public int countTriages { get; set; }
        public int countCancerICPs { get; set; }
        public int countTests { get; set; }
        public int countReviews { get; set; }
        public int countLetters { get; set; }
        public string name { get; set; }
        public string staffCode { get; set; }
        public bool isLive { get; set; }
        public string notificationMessage { get; set; }
    }
}
