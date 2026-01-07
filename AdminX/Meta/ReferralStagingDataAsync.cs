using AdminX.Data;
using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface IReferralStagingDataAsync
    {
        public Task<List<EpicReferralStaging>> GetParkedReferralUpdates(string epicID);
        public Task<EpicReferralStaging> GetParkedUpdate(int id);
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

        public async Task<EpicReferralStaging> GetParkedUpdate(int id)
        {
            EpicReferralStaging stagedUpdate = await _context.EpicReferralStaging.AsNoTracking().FirstAsync(r => r.ID == id);
            //Force it to get the updated value
            return stagedUpdate;
        }
    }
}
