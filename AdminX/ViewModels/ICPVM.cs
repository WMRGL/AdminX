using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;

namespace AdminX.ViewModels
{
    [Keyless]
    public class ICPVM
    {
        public List<StaffMember> staffMembers { get; set; }
        public List<StaffMember> consultants { get; set; }
        public List<StaffMember> GCs { get; set; }
        public List<Triage> triages { get; set; }
        public List<ICPCancer> icpCancerList { get; set; }                
        public Triage triage { get; set; }
        public ICPGeneral? icpGeneral { get; set; }
        public ICPCancer? icpCancer { get; set; }
        public List<Document> documentList { get; set; }
        public Referral referralDetails { get; set; }
        public List<ICPAction> cancerActionsList { get; set; }
        public List<ICPGeneralAction> generalActionsList { get; set; }
        public List<ICPGeneralAction2> generalActionsList2 { get; set; }
        public List<ICPCancerReviewAction> cancerReviewActionsLists { get; set; }        
        public List<ExternalCliniciansAndFacilities> clinicians { get; set; }
        public List<ExternalCliniciansAndFacilities> screeningCoordinators { get; set; }
        public List<string> specialities { get; set; }
        public string staffCode { get; set; }
        public bool isICPTriageStarted { get; set; }
        public int referralAgeDays { get; set; }
        public int referralAgeWeeks { get; set; }
        public bool canDeleteICP { get; set; }
    }
}
