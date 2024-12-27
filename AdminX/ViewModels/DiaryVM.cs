using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;

namespace AdminX.ViewModels
{
    [Keyless]
    public class DiaryVM
    {        
        public List<StaffMember> clinicians { get; set; }
        public Diary diary { get; set; }
        public List<Diary> diaryList { get; set; }
        public Patient patient { get; set; }
        public List<Referral> referralsList { get; set; }
        public List<Document> documents { get; set; }
        public string message { get; set; }
        public bool success { get; set; }
    }
}
