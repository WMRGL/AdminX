using AdminX.Models;

namespace AdminX.ViewModels
{
    public class DiscrepancyReportVM
    {
        
        public List<CgudbNotInEpic> CgudbResults { get; set; }
        public List<EpicNotInCgudb> EpicResults { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        public DiscrepancyReportVM()
        {
            CgudbResults = new List<CgudbNotInEpic>();
            EpicResults = new List<EpicNotInCgudb>();
        }
       
    }

}
