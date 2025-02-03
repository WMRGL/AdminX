using ClinicalXPDataConnections.Models;
using Microsoft.EntityFrameworkCore;
using AdminX.Models;

namespace AdminX.ViewModels
{
    [Keyless]
    public class PatientVM
    {
        public Patient patient { get; set; }
        public List<Patient> patientsList { get; set; }
        public List<Relative> relatives { get; set; }
        public List<PatientTitle> titles { get; set; }
        public List<Ethnicity> ethnicities { get; set; }
        public List<YesNo> yesno { get; set; }         
        public List<Referral> activeReferrals { get; set; }
        public List<Referral> inactiveReferrals { get; set; }
        public List<Appointment> appointments { get; set; }
        public PatientPathway patientPathway { get; set; }
        public List<Alert> alerts { get; set; }
        public List<Diary> diary { get; set; }
        public List<ExternalClinician> GPList { get; set; }
        public ExternalClinician GP { get; set; }
        public List<ExternalFacility> GPPracticeList { get; set; }
        public ExternalFacility GPPractice { get; set; }
        public StaffMember staffMember { get; set; }
        public string message { get; set; }
        public string? diedage { get; set; }
        public string? currentage { get; set; }
        public List<Language> languages { get; set; }
        public Alert protectedAddress { get; set; }
        public bool success { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public DateTime? dob { get; set; }
        public string? postCode { get; set; }
        public string? nhs { get; set; }
        public string? cguNumber { get; set; }
        public string? ethnicCode { get; set; }
        public Referral referral { get; set; }
        public List<Review> reviewList { get; set; }
        public List<ListCity> cityList { get; set; }
        public List<AreaNames> areaNamesList { get; set; }
    }
}
