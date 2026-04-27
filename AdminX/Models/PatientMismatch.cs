using Microsoft.EntityFrameworkCore;

namespace AdminX.Models
{
    [Keyless]
    public class PatientMismatch
    {
        public string LocalMRN { get; set; }
        public string Caboodle_ReferralID { get; set; }
        public string Caboodle_NHSNo { get; set; }
        public string Caboodle_Forename { get; set; }
        public string Caboodle_Surname { get; set; }
        public DateTime Caboodle_DOB { get; set; }
        public string CGU_No { get; set; }
        public string CGUDB_RefID { get; set; }
        public string CGUDB_Forename { get; set; }
        public string CGUDB_Surname { get; set; }
        public DateTime CGUDB_DOB { get; set; }
        public string CGUDB_NHSNo { get; set; }
    }
}
