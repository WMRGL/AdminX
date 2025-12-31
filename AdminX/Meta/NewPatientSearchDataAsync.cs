using AdminX.Data;
using AdminX.Models;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface INewPatientSearchDataAsync
    {
        public Task<int> GetPatientSearchID(string staffCode);
        public Task<List<PatientSearchResults>> GetPatientSearchResults(int searchID);
    }
    public class NewPatientSearchDataAsync : INewPatientSearchDataAsync
    {
        private readonly AdminContext _adminContext;

        public NewPatientSearchDataAsync(AdminContext adminContext)
        {
            _adminContext = adminContext;
        }

        public async Task<int> GetPatientSearchID(string staffCode)
        {
            var search = await _adminContext.PatientSearches.OrderByDescending(s => s.SearchID).Where(s => s.SearchBy == staffCode).FirstOrDefaultAsync();

            return search.SearchID;
        }

        public async Task<List<PatientSearchResults>> GetPatientSearchResults(int searchID)
        {
            IQueryable<PatientSearchResults> results = _adminContext.PatientSearchResults.Where(s => s.SearchID == searchID);

            return await results.ToListAsync();
        }
    }
}