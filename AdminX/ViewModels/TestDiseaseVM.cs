using ClinicalXPDataConnections.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.ViewModels
{
    [Keyless]
    public class TestDiseaseVM
    {        
        public Patient patient { get; set; }        
        public Diagnosis diagnosis { get; set; }
        public List<Diagnosis> diagnosisList { get; set; }        
        public List<DiseaseStatus> statusList { get; set; }
        public string? searchTerm { get; set; }
    }
}
