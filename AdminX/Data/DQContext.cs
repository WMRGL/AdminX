using Microsoft.EntityFrameworkCore;
using AdminX.Models;
namespace AdminX.Data
{
    public class DQContext : DbContext
    {
        public DQContext(DbContextOptions<DQContext> options) : base(options) { }
        public DbSet<EpicNotInCgudb> EpicNotInCgudb { get; set; }
        public DbSet<CgudbNotInEpic> CgudbNotInEpic { get; set; }
        public DbSet<CgudbAppointmentDetails> CgudbAppointmentsDetails { get; set; }
        public DbSet<MasterPatientTable> MasterPatientTable { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<ClinFacility> ClinFacilities { get; set; }
        public DbSet<EpicAppointmentDetail> EpicAppointmentDetails { get; set; }
        public DbSet<EpicPatient> EpicPatients { get; set; }
    }
}
