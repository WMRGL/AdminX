using AdminX.Data;
using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface ICliniciansClinicDataAsync
    {
        public Task<CliniciansClinics> GetCliniciansClinic(string facilityCode);
        public Task<List<CliniciansClinics>> GetCliniciansClinicList();
    }
    public class CliniciansClinicDataAsync : ICliniciansClinicDataAsync
    {
        private readonly AdminContext _adminContext;

        public CliniciansClinicDataAsync(AdminContext context)
        {
            _adminContext = context;
        }

        public async Task<CliniciansClinics> GetCliniciansClinic(string facilityCode)
        {
            CliniciansClinics cd = await _adminContext.CliniciansClinics.FirstAsync(c => c.Facility ==  facilityCode);

            return cd;
        }

        public async Task<List<CliniciansClinics>> GetCliniciansClinicList()
        {
            IQueryable<CliniciansClinics> ccs = _adminContext.CliniciansClinics;

            return await ccs.ToListAsync();
        }
    }
}
