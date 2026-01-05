using AdminX.Data;
using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface IEpicReferralReferenceDataAsync
    {
        public Task<EpicReferralReference> GetEpicReferral(int refID);
        public Task<List<EpicReferralReference>> GetEpicReferralsList(int mpi);
    }

    public class EpicReferralReferenceDataAsync : IEpicReferralReferenceDataAsync
    {
        private readonly AdminContext _adminContext;

        public EpicReferralReferenceDataAsync(AdminContext adminContext)
        {
            _adminContext = adminContext;
        }

        public async Task<EpicReferralReference> GetEpicReferral(int refID)
        {
            EpicReferralReference referral = await _adminContext.EpicReferralReference.FirstOrDefaultAsync(r => r.RefID == refID);

            return referral;
        }

        public async Task<List<EpicReferralReference>> GetEpicReferralsList(int mpi)
        {
            IQueryable<EpicReferralReference> erefs = _adminContext.EpicReferralReference.Where(r => r.MPI == mpi);
            
            return await erefs.ToListAsync();
        }
    }
}
