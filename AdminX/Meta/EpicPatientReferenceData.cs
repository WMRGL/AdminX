using AdminX.Data;
using AdminX.Models;

namespace AdminX.Meta
{
    public interface IEpicPatientReferenceData
    {
        EpicPatientReference GetEpicPatient(int mpi);
    }

    public class EpicPatientReferenceData : IEpicPatientReferenceData
    {
        private readonly AdminContext _adminContext;

        public EpicPatientReferenceData(AdminContext adminContext)
        {
            _adminContext = adminContext;
        }

        public EpicPatientReference GetEpicPatient(int mpi)
        {
            EpicPatientReference patient = _adminContext.EpicPatientReferences.FirstOrDefault(p => p.MPI == mpi);

            return patient;
        }
    }
}
