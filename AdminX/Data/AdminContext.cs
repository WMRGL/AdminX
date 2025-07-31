using Microsoft.EntityFrameworkCore;
using AdminX.Models;


namespace AdminX.Data
{
    public class AdminContext : DbContext //The CLinicalContext class is the data context for all clinical related data.
    {
        public AdminContext(DbContextOptions<AdminContext> options) : base(options) { }

        //public DbSet<ActivityType> ActivityType { get; set; }
        public DbSet<Language> Language { get; set; }
        public DbSet<ListStatusAdmin> ListStatusAdmin { get; set; }
        public DbSet<ListDisease> ListDiseases { get; set; }
        public DbSet<PatientSearches> PatientSearches { get; set; }
        public DbSet<PatientSearchResults> PatientSearchResults { get; set; }
        public DbSet<ListCity> ListCity { get; set; }
        public DbSet<DictatedlettersReportClinicians> DictatedlettersReportClinicians { get; set; }
        public DbSet<DictatedLettersSecTeamReport> DictatedLettersSecTeamReport { get; set; }
        public DbSet<DictatedLettersReport> DictatedLettersReport { get; set; }
        public DbSet<DiaryAction> DiaryAction { get; set; }
        public DbSet<AlertTypes> AlertTypes { get; set; }
        public DbSet<MergeHistory> MergeHistory { get; set; }
        
    }
}
