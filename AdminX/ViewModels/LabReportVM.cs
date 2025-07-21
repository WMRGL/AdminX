using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;

namespace AdminX.ViewModels
{
    [Keyless]
    public class LabReportVM
    {
        public LabPatient patient {  get; set; }
        public List<LabPatient> patientsList { get; set; }
        public LabDNALab dnaReport { get; set; }
        public List<LabDNALab> dnaReportsList { get; set; }
        public List<LabDNALabData> labDNALabDataList { get; set; }
        public LabLab cytoReport { get; set; }
        public List<LabLab> cytoReportsList { get; set; }
        public LabDNAReport dnaReportDetails { get; set; }
        public string? searchTerms { get; set; }
    }
}
