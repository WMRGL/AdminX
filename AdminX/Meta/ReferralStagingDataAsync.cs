using AdminX.Data;
using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface IReferralStagingDataAsync
    {
        public Task<List<EpicReferralStaging>> GetParkedReferralUpdates(string epicID);
    }
    public class ReferralStagingDataAsync : IReferralStagingDataAsync
    {
        private readonly AdminContext _context;

        public ReferralStagingDataAsync(AdminContext context)
        {
            _context = context;
        }
        public async Task<List<EpicReferralStaging>> GetParkedReferralUpdates(string epicID)
        {
            IQueryable<EpicReferralStaging> stagedRefs = _context.EpicReferralStaging.Where(r => r.PatientID == epicID && r.UpdateSts < 5).OrderBy(r => r.CreatedDate);

            return await stagedRefs.ToListAsync();
        }
    }
}
