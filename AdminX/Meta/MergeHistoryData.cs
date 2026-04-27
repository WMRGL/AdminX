using AdminX.Data;
using AdminX.Models;

namespace AdminX.Meta
{
    interface IMergeHistoryData
    {
        public List<MergeHistory> GetMergeHistoryByMPI(int mpi);
        public List<MergeHistory> GetMergeHistoryByNewFileNo(string pedNo);
        public List<MergeHistory> GetMergeHistoryByOldFileNo(string pedNo);
        //public List<MergeHistory> GetMergeHistoryByPatientDemographics(string? firstName, string? lastName, string? nhsNo, DateTime? dob);
    }
    public class MergeHistoryData : IMergeHistoryData
    {
        //private readonly ClinicalContext _clinContext;
        private readonly AdminContext _context;

        public MergeHistoryData(AdminContext context)
        {
            //_clinContext = clinContext;
            _context = context;
        }

        public List<MergeHistory> GetMergeHistoryByMPI(int mpi)
        {
            IQueryable<MergeHistory> mh = _context.MergeHistory.Where(h => h.MPI == mpi);

            return mh.ToList();
        }

        public List<MergeHistory> GetMergeHistoryByNewFileNo(string pedNo)
        {
            IQueryable<MergeHistory> mh = _context.MergeHistory.Where(h => h.NewPedigreeNumber == pedNo);

            return mh.ToList();
        }

        public List<MergeHistory> GetMergeHistoryByOldFileNo(string pedNo)
        {
            IQueryable<MergeHistory> mh = _context.MergeHistory.Where(h => h.OldPedigreeNumber == pedNo);

            return mh.ToList();
        }

        /*
        public List<MergeHistory> GetMergeHistoryByPatientDemographics(string? firstName, string? lastName, string? nhsNo, DateTime? dob)
        {
            int iMPI = 0;
            Patient patient = new Patient();
            IPatientData pd = new PatientData(_clinContext);

            patient = pd.GetPatientDetailsByDemographicData(firstName, lastName, nhsNo, dob.GetValueOrDefault());
            iMPI = patient.MPI;

            List<MergeHistory> mh = GetMergeHistoryByMPI(iMPI);

            return mh.ToList();
        }
        */
    }
}
