using AdminX.Data;
using AdminX.Models;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;

namespace AdminX.Meta
{
    public interface INewPatientSearchDataAsync
    {
        public Task<int> GetPatientSearchID(string staffCode);
        public Task<List<PatientSearchResults>> GetPatientSearchResults(int searchID);
        public Task<List<Patient>> GetRecentlyViewedPatients(string username);
    }
    public class NewPatientSearchDataAsync : INewPatientSearchDataAsync
    {
        private readonly AdminContext _adminContext;
        private readonly ClinicalContext _clinContext;

        public NewPatientSearchDataAsync(AdminContext adminContext, ClinicalContext clinContext)
        {
            _adminContext = adminContext;
            _clinContext = clinContext;
        }

        public async Task<int> GetPatientSearchID(string staffCode)
        {
            var search = await _adminContext.PatientSearches.OrderByDescending(s => s.SearchID).Where(s => s.SearchBy == staffCode).FirstOrDefaultAsync();

            return search.SearchID;
        }

        public async Task<List<PatientSearchResults>> GetPatientSearchResults(int searchID)
        {
            IQueryable<PatientSearchResults> results = _adminContext.PatientSearchResults.Where(s => s.SearchID == searchID);
            var mpis = results.Where(r => r.MPI.HasValue).Select(r => r.MPI.Value).ToList();

            if (mpis.Any())
            {
                var addresses = _clinContext.Patients.Where(p => mpis.Contains(p.MPI))
                    .Select(p => new { p.MPI, p.ADDRESS1, p.ADDRESS2, p.ADDRESS3, p.ADDRESS4 }).ToList();

                foreach (var result in results)
                {
                    if (result.MPI.HasValue)
                    {
                        var match = addresses.FirstOrDefault(a => a.MPI == result.MPI.Value);
                        if (match != null)
                        {
                            result.Address = match.ADDRESS1;
                        }
                    }
                }
            }

            return await results.ToListAsync();
        }

        public async Task<List<Patient>> GetRecentlyViewedPatients(string username)
        {
            var logs = await _adminContext.AuditLogs
                .Where(l => l.TableName == "Patient"       
                         && l.Action == "PatientDetails"    
                         && l.UserId == username
                         && !string.IsNullOrEmpty(l.NewValues))
                .OrderByDescending(l => l.DateTime)
                .Take(10)
                .ToListAsync();

            var recentPatients = new List<Patient>();
            var patientIds = new List<int>();

            foreach (var log in logs)
            {
                try
                {
                    var json = JObject.Parse(log.NewValues);
                    if (json["id"] != null && int.TryParse(json["id"].ToString(), out int mpi))
                    {
                        if (!patientIds.Contains(mpi)) 
                        {
                            patientIds.Add(mpi);
                        }
                    }
                }
                catch { }
            }

            if (!patientIds.Any()) return recentPatients;

            var patients = await _clinContext.Patients
                .Where(p => patientIds.Contains(p.MPI))
                .Select(p => new Patient
                {
                    MPI = p.MPI,
                    FIRSTNAME = p.FIRSTNAME,
                    LASTNAME = p.LASTNAME,
                    CGU_No = p.CGU_No,
                    DOB = p.DOB,
                    SOCIAL_SECURITY = p.SOCIAL_SECURITY
                })
                .ToListAsync();

            foreach (var id in patientIds)
            {
                var match = patients.FirstOrDefault(p => p.MPI == id);
                if (match != null) recentPatients.Add(match);
            }

            return recentPatients;
        }
    }
}