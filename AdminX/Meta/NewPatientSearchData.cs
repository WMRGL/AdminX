using AdminX.Data;
using AdminX.Models;
using System.Data;

namespace AdminX.Meta
{
    interface INewPatientSearchData
    {
        public int GetPatientSearchID(string staffCode);
        public List<PatientSearchResults> GetPatientSearchResults(int searchID);
    }
    public class NewPatientSearchData : INewPatientSearchData
    {
        private readonly AdminContext _adminContext;

        public NewPatientSearchData(AdminContext adminContext)
        {
            _adminContext = adminContext;
        }

        public int GetPatientSearchID(string staffCode)
        {
            int id = _adminContext.PatientSearches.OrderByDescending(s => s.SearchID).Where(s => s.SearchBy == staffCode).FirstOrDefault().SearchID;

            return id;
        }

        public List<PatientSearchResults> GetPatientSearchResults(int searchID)
        {
            IQueryable<PatientSearchResults> results = _adminContext.PatientSearchResults.Where(s => s.SearchID == searchID);

            return results.ToList();
        }
    }
}