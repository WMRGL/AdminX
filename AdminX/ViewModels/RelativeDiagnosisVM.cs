using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.ViewModels
{
    [Keyless]
    public class RelativeDiagnosisVM
    {
        public Relative relativeDetails { get; set; }
        public RelativesDiagnosis relativesDiagnosis { get; set; }
        public List<RelativesDiagnosis> relativesDiagnosisList { get; set; }
        public List<StaffMember> staffList { get; set; }
        public List<StaffMember> clinicianList { get; set; }        
        public List<Relation> relationList { get; set; }
        public List<Gender> genderList { get; set; }
        public int MPI { get; set; }
        public int WMFACSID { get; set; }

    }
}
