using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminX.Models
{
    [Table("ViewPatientHSData", Schema = "dbo")]
    [Keyless]
    public class HS
    {
        
        public int MPI {  get; set; }
        public string MasterFileNo { get; set; }
        public int RelsID { get; set; }
        public int WMFACSID { get; set; }
        public string? PtName { get; set; }
        public string? RelName { get; set; }
        public string? RelAdd { get; set; }
        public string? RelPC { get; set; }
        public string? RelSex { get; set; }
        public string? RelDOBy { get; set; } //it'll almost certainly be a string, and have nulls
        public string? RelDOB { get; set; }
        public string? RelDOD { get; set; }
        public string? AgeDeath { get; set; }
        public string? Alive { get; set; }
        public string? RelAffected { get; set; }
        public string? Affected { get; set; }
        public int? TumourID { get; set; }
        public string? Diagnosis { get; set; }
        public string? AgeDiag { get; set; }
        public string? Hospital { get; set; }
        public DateTime? DateReq { get; set; }
        public DateTime? DateRec { get; set; }
        public string? CRegCode { get; set; }
        public string? Status { get; set; }
        [Column("Consent?")]
        public string? Consent { get; set; }
        public string? InfoReq { get; set; }
        public string? WhyNot { get; set; }
        public string? Confirmed { get; set; }
        public string? Conf { get; set; }
        public float? ConfDiagAge { get; set; }
        public string? ConfDiagDate { get; set; }
        public string? Notes { get; set; }
        public string? RelSurname { get; set; }
        public string? RelForename1 { get; set; }
        public string? SiteCode { get; set; }
        public string? LatCode { get; set; }
        public string? MorphCode { get; set; }
        public string? Site { get; set; }
        public string? Lat { get; set; }
        public string? Morph { get; set; }
        public string? Registry { get; set; }

    }
}
