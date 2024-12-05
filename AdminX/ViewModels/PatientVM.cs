using ClinicalXPDataConnections.Models;
using Microsoft.EntityFrameworkCore;
using AdminX.Models;

namespace AdminX.ViewModels
{
    [Keyless]
    public class PatientVM
    {
        public Patient patient { get; set; }
        public List<Relative> relatives { get; set; }
        public List<PatientTitle> titles { get; set; }
        public List<Ethnicity> ethnicities { get; set; }
        public List<YesNo> yesno { get; set; }         
        public List<Referral> referrals { get; set; }
        public PatientPathway patientPathway { get; set; }
        public List<Alert> alerts { get; set; }
        public List<Diary> diary { get; set; }
        public List<ExternalClinician> GPList { get; set; }
        public List<ExternalFacility> GPPracticeList { get; set; }
        public StaffMember staffMember { get; set; }
        public string message { get; set; }
        public string? diedage { get; set; }
        public string? currentage { get; set; }
        public List<Language> languages { get; set; }
        public Alert protectedAddress { get; set; }
        public bool success { get; set; }
    }
}
