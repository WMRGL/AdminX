using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;
using AdminX.Models;

namespace AdminX.ViewModels
{
    [Keyless]
    public class SysAdminVM
    {        
        public List<StaffMember> staffMembers { get; set; }
        public List<StaffMember> consultants { get; set; }
        public List<StaffMember> gcs { get; set; }
        public List<StaffMember> sprs { get; set; }
        public List<ExternalClinician> clinicians { get; set; }
        public List<ExternalFacility> facilities { get; set; }
        public List<ClinicVenue> venues { get; set; }   
        public List<string> teams { get; set; }
        public List<string> types { get; set; }
        public StaffMember staffMember { get; set; }
        public ExternalClinician clinician { get; set; }
        public ExternalFacility facility { get; set; }
        public ClinicVenue venue { get; set; }
        public List<PatientTitle> titles { get; set; }
        public List<CliniciansClinics> cliniciansClinicList { get; set; }
        public CliniciansClinics cliniciansClinic { get; set; }
        public string message { get; set; }
        public bool success { get; set; }
        public bool isEditStaff { get; set; }
        public bool hasAssociatedClinicDetails { get; set; }
        public string clinCodeToCreate { get; set; }
        public StaffMember dutyConsultant { get; set; }
        public StaffMember dutyGC { get; set; }
        public StaffMember dutySPR { get; set; }

    }
}
