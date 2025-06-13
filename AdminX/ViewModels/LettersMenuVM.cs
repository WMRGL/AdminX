using ClinicalXPDataConnections.Models;
using AdminX.Models;

namespace AdminX.ViewModels
{
    public class LettersMenuVM
    {
        public List<Document> docsListStandard {  get; set; }
        public List<Document> docsListMedRec { get; set; }
        public List<Document> docsListDNA { get; set; }
        public List<Document> docsListOutcome { get; set; }
        public Patient patient {  get; set; }
        public Relative relative { get; set; }
        public Referral referral { get; set; }
        public List<Leaflet> leaflets { get; set; }
        public List<Referral> referralList { get; set; }
        public List<ExternalCliniciansAndFacilities> clinicianList { get; set; }
        public bool isRelative { get; set; }
        public List<StaffMember> staffMemberList { get; set; } 
        public string patientAddress { get; set; }
    }
}
