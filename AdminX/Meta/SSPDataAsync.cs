using AdminX.Data;
using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface ISSPDataAsync
    {
        public Task<List<SocialServicePathwayOutcome>> GetSSPOutcomes();
        public Task<List<SocialServicePathwayStatus>> GetSSPStatuses();
        public Task<List<SocialService>> GetSocialServices();
        public Task<List<SocialWorker>> GetSocialWorkers();
        public Task<SSP> GetSSPDetails(int id);
        public Task<List<SSP>> GetSSPList();
    }

    public class SSPDataAsync : ISSPDataAsync
    {
        private readonly AdminContext _adminContext;

        public SSPDataAsync(AdminContext context)
        {
            _adminContext = context;
        }

       

        public async Task<List<SocialServicePathwayOutcome>> GetSSPOutcomes()
        {
            List<SocialServicePathwayOutcome> outcomeSSPs = await _adminContext.SocialServicePathwayOutcome.OrderBy(a => a.ID).ToListAsync();

            return outcomeSSPs;
        }

        public async Task<List<SocialServicePathwayStatus>> GetSSPStatuses()
        {
            List<SocialServicePathwayStatus> statusSSPs = await _adminContext.SocialServicePathwayStatus.OrderBy(a => a.SSPStatusID).ToListAsync();

            return statusSSPs;
        }

        public async Task<List<SocialService>> GetSocialServices()
        {
            List<SocialService> ss = await _adminContext.SocialService.OrderBy(a => a.SocialServicesID).ToListAsync();

            return ss;
        }
        public async Task<List<SocialWorker>> GetSocialWorkers()
        {
            List<SocialWorker> sw = await _adminContext.SocialWorker.OrderBy(a => a.SocialWorkerID).ToListAsync();

            return sw;
        }

        public async Task<SSP> GetSSPDetails(int id)
        {
            SSP ssp = await _adminContext.SSP.FirstOrDefaultAsync(s => s.SSPID == id);
            
            return ssp;
        }

        public async Task<List<SSP>> GetSSPList()
        {
            List<SSP> ssps = await _adminContext.SSP.OrderBy(s => s.SSPID).Where(s => s.IsActive == true).ToListAsync();

            return ssps;
        }
    }
}
