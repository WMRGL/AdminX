using AdminX.Data;
using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface IMergeHistoryDataAsync
    {
        public Task<List<MergeHistory>> GetMergeHistoryByMPI(int mpi);
        public Task<List<MergeHistory>> GetMergeHistoryByNewFileNo(string pedNo);
        public Task<List<MergeHistory>> GetMergeHistoryByOldFileNo(string pedNo);
        //public List<MergeHistory> GetMergeHistoryByPatientDemographics(string? firstName, string? lastName, string? nhsNo, DateTime? dob);
    }
    public class MergeHistoryDataAsync : IMergeHistoryDataAsync
    {
        //private readonly ClinicalContext _clinContext;
        private readonly AdminContext _context;

        public MergeHistoryDataAsync(AdminContext context)
        {
            //_clinContext = clinContext;
            _context = context;
        }

        public async Task<List<MergeHistory>> GetMergeHistoryByMPI(int mpi)
        {
            IQueryable<MergeHistory> mh = _context.MergeHistory.Where(h => h.MPI == mpi);

            return await mh.ToListAsync();
        }

        public async Task<List<MergeHistory>> GetMergeHistoryByNewFileNo(string pedNo)
        {
            IQueryable<MergeHistory> mh = _context.MergeHistory.Where(h => h.NewPedigreeNumber == pedNo);

            return await mh.ToListAsync();
        }

        public async Task<List<MergeHistory>> GetMergeHistoryByOldFileNo(string pedNo)
        {
            IQueryable<MergeHistory> mh = _context.MergeHistory.Where(h => h.OldPedigreeNumber == pedNo);

            return await mh.ToListAsync();
        }       
    }
}
