using AdminX.Models;
using ClinicalXPDataConnections.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.ViewModels
{
    [Keyless]
    public class PatientSearchVM
    { 
        public List<Patient> patientsList { get; set; }
        public List<PatientSearchResults> patientSearchResultsList { get; set; }
        public List<PatientSearchResults> relativeSearchResultsList { get; set; }
        public List<PatientSearchResults> pedigreeSearchResultsList { get; set; }
        public StaffMember staffMember { get; set; }
        public string cguNumberSearch { get; set; }
        public string forenameSearch { get; set; }
        public string surnameSearch { get; set;}
        public DateTime dobSearch { get; set; }
        public string nhsNoSearch { get; set; }
        public string postcodeSearch { get; set; }

    }
}
