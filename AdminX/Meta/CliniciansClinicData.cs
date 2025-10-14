using AdminX.Data;
using AdminX.Models;

namespace AdminX.Meta
{
    public interface ICliniciansClinicData
    {
        public CliniciansClinics GetCliniciansClinic(string facilityCode);
        public List<CliniciansClinics> GetCliniciansClinicList();
    }
    public class CliniciansClinicData : ICliniciansClinicData
    {
        private readonly AdminContext _adminContext;

        public CliniciansClinicData(AdminContext context)
        {
            _adminContext = context;
        }

        public CliniciansClinics GetCliniciansClinic(string facilityCode)
        {
            CliniciansClinics cd = _adminContext.CliniciansClinics.FirstOrDefault(c => c.Facility ==  facilityCode);

            return cd;
        }

        public List<CliniciansClinics> GetCliniciansClinicList()
        {
            IQueryable<CliniciansClinics> ccs = _adminContext.CliniciansClinics;

            return ccs.ToList();
        }
    }
}
