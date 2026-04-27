using ClinicalXPDataConnections.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.ViewModels
{
    [Keyless]
    public class PatientMergeVM
    {
        public Patient patientFrom { get; set; }
        public Patient patientTo { get; set; }
        public StaffMember staffMember { get; set; }
    }
}
