using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.ViewModels
{
    [Keyless]
    public class TestDiseaseVM
    {        
        public Patient patient { get; set; }
        public Test test { get; set; }
        public List<Test> tests { get; set; }        
        public Diagnosis diagnosis { get; set; }
        public List<Diagnosis> diagnosisList { get; set; }        
        public List<DiseaseStatus> statusList { get; set; }
        public string? searchTerm { get; set; }
    }
}
