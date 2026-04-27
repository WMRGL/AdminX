using AdminX.Models;
using ClinicalXPDataConnections.Models;

namespace AdminX.ViewModels
{
    public class SSPVM
    {
        public SSP ssp { get; set; }
        public List<SSP> ssps { get; set; }
        public List<SocialServicePathwayOutcome> sspOutcomes { get; set; }
        public List<SocialServicePathwayStatus> sspStatuses { get; set; }
        public List<SocialService> socialServices { get; set; }
        public List<SocialWorker> socialWorkers { get; set; }
        public Patient patient { get; set; }
        public List<PatientTitle> titles { get; set; }
        public SocialService socialService { get; set; }
        public SocialWorker socialWorker { get; set; }
    }
}
