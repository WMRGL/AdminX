using AdminX.Data;
using AdminX.Models;
using System.Data;

namespace AdminX.Meta
{
    interface IDiseaseData
    {        
        public List<Diagnosis> GetDiseaseListByPatient(int mpi);        
    }
    public class DiseaseData : IDiseaseData
    {
        private readonly ClinicalContext _clinContext;        
        public DiseaseData(ClinicalContext context)
        {
            _clinContext = context;
        }
        
        
        public List<Diagnosis> GetDiseaseListByPatient(int mpi) //Get list of all diseases recorded against a patient
        {            

            IQueryable<Diagnosis> items = from i in _clinContext.Diagnosis
                        where i.MPI == mpi
                        orderby i.DESCRIPTION
                        select i;

            return items.ToList();
        }
    }
}
