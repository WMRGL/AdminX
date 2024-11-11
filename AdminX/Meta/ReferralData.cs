using AdminX.Data;
using AdminX.Models;
using System.Data;

namespace AdminX.Meta
{
    interface IReferralData
    {
        public Referral GetReferralDetails(int id);
        public List<Referral> GetReferralsList(int id);
    }
    public class ReferralData : IReferralData
    {
        private readonly ClinicalContext _clinContext;

        public ReferralData(ClinicalContext context)
        {
            _clinContext = context;
        }
                
        public Referral GetReferralDetails(int id) //Get details of referral by RefID
        {
            Referral referral = _clinContext.Referrals?.FirstOrDefault(i => i.refid == id);
            return referral;
        } 
                
        public List<Referral> GetReferralsList(int id) //Get list of active referrals for patient by MPI
        {
            IQueryable<Referral> referrals = from r in _clinContext.Referrals
                           where r.MPI == id & r.RefType.Contains("Referral") & r.COMPLETE != "Complete"
                           orderby r.RefDate
                           select r;            

            return referrals.ToList();
        }       
    }
}
