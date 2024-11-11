using AdminX.Data;
using AdminX.Models;
using System.Data;

namespace AdminX.Meta
{
    interface IRelativeDiagnosisData
    {
        public List<RelativesDiagnosis> GetRelativeDiagnosisList(int id);
        public RelativesDiagnosis GetRelativeDiagnosisDetails(int id);
        
    }
    public class RelativeDiagnosisData : IRelativeDiagnosisData
    {
        private readonly ClinicalContext _clinContext;

        public RelativeDiagnosisData(ClinicalContext context)
        {
            _clinContext = context;
        }        

        public List<RelativesDiagnosis> GetRelativeDiagnosisList(int id)
        {
            IQueryable<RelativesDiagnosis> reldiag = from r in _clinContext.RelativesDiagnoses
                           where r.RelsID == id
                           select r;
            
            return reldiag.ToList();
        }

        public RelativesDiagnosis GetRelativeDiagnosisDetails(int id)
        {
            RelativesDiagnosis item = _clinContext.RelativesDiagnoses.FirstOrDefault(rd => rd.TumourID == id);
            return item;
        }

        

        

    }
}
