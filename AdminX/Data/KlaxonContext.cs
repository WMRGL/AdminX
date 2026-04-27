using AdminX.Models;
using Microsoft.EntityFrameworkCore;
namespace AdminX.Data
{
    public class KlaxonContext : DbContext
    {
        public KlaxonContext(DbContextOptions<KlaxonContext> options) : base(options)
        {
        }
        
        public DbSet<EpicAppointmentDetail> EpicAppointmentDetails { get; set; }
        public DbSet<EpicPatient> EpicPatients { get; set; }
    }
    
}
