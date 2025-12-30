using AdminX.Data;
using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface IEpicPatientReferenceDataAsync
    {
        public Task<EpicPatientReference> GetEpicPatient(int mpi);
    }

    public class EpicPatientReferenceDataAsync : IEpicPatientReferenceDataAsync
    {
        private readonly AdminContext _adminContext;

        public EpicPatientReferenceDataAsync(AdminContext adminContext)
        {
            _adminContext = adminContext;
        }

        public async Task<EpicPatientReference> GetEpicPatient(int mpi)
        {
            EpicPatientReference patient = await _adminContext.EpicPatientReferences.FirstOrDefaultAsync(p => p.MPI == mpi);

            return patient;
        }
    }
}
