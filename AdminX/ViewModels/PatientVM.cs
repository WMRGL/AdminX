using AdminX.Models;
using Microsoft.EntityFrameworkCore;

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
        public List<HPOTermDetails> hpoTermDetails { get; set; }        
        public List<Referral> referrals { get; set; }
        public PatientPathway patientPathway { get; set; }
        public List<Alert> alerts { get; set; }
        public List<Diary> diary { get; set; }
        public List<ExternalClinician> GPList { get; set; }
        public List<ExternalFacility> GPPracticeList { get; set; }
        public StaffMember staffMember { get; set; }
        public string message { get; set; }
        public bool success { get; set; }
    }
}
