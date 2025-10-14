using AdminX.Models;
using ClinicalXPDataConnections.Models;

namespace AdminX.ViewModels
{
    public class NewEpicPatientVM
    {
        public List<Patient> patientList { get; set; }
        public List<PatientSearchResults> patientSearchResultsList { get; set; }
        public List<PatientSearchResults> relativeSearchResultsList { get; set; }
        public List<PatientSearchResults> pedigreeSearchResultsList { get; set; }
        public Patient patient { get; set; }
    }
}
