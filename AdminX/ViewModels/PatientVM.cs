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
        public Patient nextPatient { get; set; }
        public Patient previousPatient { get; set; }
        public List<Relative> relatives { get; set; }
        public List<PatientTitle> titles { get; set; }
        public List<Ethnicity> ethnicities { get; set; }
        public List<YesNo> yesno { get; set; }         
        public List<Referral> activeReferrals { get; set; }
        public List<Referral> inactiveReferrals { get; set; }
        public List<Referral> tempReges { get; set; }
        public List<Referral> incompleteReferrals { get; set; }
        public List<Appointment> appointments { get; set; }
        public PatientPathway patientPathway { get; set; }
        public List<Alert> alerts { get; set; }
        public List<Diary> diary { get; set; }
        public List<ExternalClinician> GPList { get; set; }
        public List<ExternalClinician> currentGPList { get; set; }
        public ExternalClinician GP { get; set; }
        public List<ExternalFacility> GPPracticeList { get; set; }
        public ExternalFacility GPPractice { get; set; }
        public StaffMember staffMember { get; set; }
        public string message { get; set; }
        public List<string>? messages { get; set; }
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
        public List<Gender> genders { get; set; }
        public string edmsLink { get; set; }
        public string phenotipsLink { get; set; }
        public bool isPhenotipsAvailable { get; set; }
        public bool isCancerPPQScheduled { get; set; }
        public bool isCancerPPQComplete { get; set; }
        public bool isGeneralPPQScheduled { get; set; }
        public bool isGeneralPPQComplete { get; set; }
        public bool isPatientInPhenotips { get; set; }
        public PhenotipsPatient ptPatient { get; set; }
        public List<AlertTypes> alertTypes { get; set; }
        public Alert alert { get; set; }
        public List<Referral> referralsList { get; set; }
        public List<DiaryAction> diaryActionsList { get; set; }
        public List<Document> documentsList { get; set; }
        public Referral defaultRef { get; set; }
        public string? gpPracticeCode { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public List<Gender>? genderAtBirth { get; set; }
        public List<GenderIdentity> genderIdentities { get; set; }
        public string? GenderIdentity { get; set; }
        public bool isEpicPatientChanged { get; set; }
        public bool isEpicReferralChanged { get; set; }
        public EpicPatientReference epicPatient {  get; set; }
        public EpicReferralReference epicReferral { get; set; }
        public List<EpicReferralStaging> epicReferralStaging { get; set; }
        public List<EpicReferralReference> epicReferrals { get; set; }
        public int selectedAreaID { get; set; }
    }
}
