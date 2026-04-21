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
        public List<SocialServices> socialServices { get; set; }
        public List<SocialWorkers> socialWorkers { get; set; }
        public Patient patient { get; set; }
    }
}
