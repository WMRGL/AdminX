using ClinicalXPDataConnections.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.ViewModels
{
    [Keyless]
    public class RelativeVM
    {
        public Patient patient { get; set; }
        public Relative relativeDetails { get; set; }
        public List<Relative> phenotipsRelativesList { get; set; }
        public List<Relative> cgudbRelativesList { get; set; }
        public List<Relation> relationsList { get; set; }
        public List<Gender> genderList { get; set; }
        public int MPI { get; set; }
        public int WMFACSID { get; set; }

    }
}
