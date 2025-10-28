using AdminX.Data;
using AdminX.Models;

namespace AdminX.Meta
{
    public interface IReferralStagingData
    {
        public List<EpicReferralStaging> GetParkedReferralUpdates(string epicID);
    }
    public class ReferralStagingData : IReferralStagingData
    {
        private readonly AdminContext _context;

        public ReferralStagingData(AdminContext context)
        {
            _context = context;
        }
        public List<EpicReferralStaging> GetParkedReferralUpdates(string epicID)
        {
            IQueryable<EpicReferralStaging> stagedRefs = _context.EpicReferralStaging.Where(r => r.PatientID == epicID).OrderBy(r => r.CreatedDate);

            return stagedRefs.ToList();
        }
    }
}
