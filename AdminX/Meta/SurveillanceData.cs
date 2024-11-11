using AdminX.Data;
using AdminX.Models;
using System.Data;

namespace AdminX.Meta
{
    interface ISurveillanceData
    {
        public List<Surveillance> GetSurveillanceList(int? mpi);
        public List<Surveillance> GetSurveillanceListByRiskID(int? riskID);
        public Surveillance GetSurvDetails(int? riskID);
        
    }
    public class SurveillanceData : ISurveillanceData
    {
        private readonly ClinicalContext _clinContext;
        
        public SurveillanceData(ClinicalContext context)
        {
            _clinContext = context;        
        }
        
        public List<Surveillance> GetSurveillanceList(int? mpi) //Get list of all surveillance recommendations for an ICP (by MPI)
        {
            IQueryable<Surveillance> surveillances = from r in _clinContext.Surveillance
                               where r.MPI == mpi
                               select r;

            return surveillances.ToList();
        }

        public List<Surveillance> GetSurveillanceListByRiskID(int? riskID) //Get list of all surveillance recommendations for a  risk item (by RiskID)
        {
            IQueryable<Surveillance> surveillances = from r in _clinContext.Surveillance
                                where r.RiskID == riskID
                                select r;

            return surveillances.ToList();
        }

        

        public Surveillance GetSurvDetails(int? survID) //Get details of surveillance recommendation by RiskID
        {
            Surveillance surv = _clinContext.Surveillance.FirstOrDefault(c => c.SurvRecID == survID);
            return surv;
        }        

    }
}
