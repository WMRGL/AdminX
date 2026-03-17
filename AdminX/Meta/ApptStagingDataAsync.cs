using AdminX.Data;
using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface IApptStagingDataAsync
    {
        public Task<List<EpicApptStaging>> GetParkedApptUpdates(string epicID);
        public Task<EpicApptStaging> GetParkedUpdate(int id);
    }
    public class ApptStagingDataAsync : IApptStagingDataAsync
    {
        private readonly AdminContext _context;

        public ApptStagingDataAsync(AdminContext context)
        {
            _context = context;
        }
        public async Task<List<EpicApptStaging>> GetParkedApptUpdates(string epicID)
        {
            IQueryable<EpicApptStaging> stagedRefs = _context.EpicApptStaging.Where(r => r.PatientID == epicID && r.UpdateSts < 5 && r.ApptID != null).OrderBy(r => r.Appt_DTTM);

            return await stagedRefs.ToListAsync();
        }

        public async Task<EpicApptStaging> GetParkedUpdate(int id)
        {
            EpicApptStaging stagedUpdate = await _context.EpicApptStaging.AsNoTracking().FirstAsync(r => r.ID == id);
            //Force it to get the updated value
            return stagedUpdate;
        }
    }
}
