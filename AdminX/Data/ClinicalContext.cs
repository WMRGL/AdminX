using Microsoft.EntityFrameworkCore;
using AdminX.Models;

namespace AdminX.Data
{
    public class ClinicalContext : DbContext //The ClinicalContext class is the data context for all clinical related data.
    {
        public ClinicalContext (DbContextOptions<ClinicalContext> options) :base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityItem>().ToTable(ThreadPoolBoundHandle => ThreadPoolBoundHandle.HasTrigger("TriggerName"));
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Ethnicity> Ethnicity { get; set; }
        public DbSet<Relative> Relatives { get; set; }    
        public DbSet<RelativesDiagnosis> RelativesDiagnoses { get; set; }
        public DbSet<Appointment> Clinics { get; set; }
        public DbSet<Referral> Referrals { get; set; }
        public DbSet<Triage> Triages { get; set; }
        public DbSet<ICP> ICP { get; set; }
        public DbSet<ICPGeneral> ICPGeneral { get; set; }
        public DbSet<ICPCancer> ICPCancer { get; set; }
        public DbSet<StaffMember> StaffMembers { get; set; }
        public DbSet<ActivityItem> ActivityItems { get; set; }
        public DbSet<Diary> Diary { get; set; }
        public DbSet<Note> ClinicalNotes { get; set; }
        public DbSet<NoteItem> NoteItems { get; set; }
        public DbSet<Diagnosis> Diagnosis { get; set; }
        public DbSet<Test> Test { get; set; }
        public DbSet<Outcome> Outcomes { get; set; }
        public DbSet<HPOTermDetails> HPOTermDetails { get; set; }
        public DbSet<DiseaseStatus> DiseaseStatusList { get; set; }
        public DbSet<DictatedLetter> DictatedLetters { get; set; }
        public DbSet<DictatedLettersPatient> DictatedLettersPatients { get; set; }
        public DbSet<DictatedLettersCopy> DictatedLettersCopies { get; set; }
        public DbSet<PatientPathway> PatientPathway { get; set; }
        public DbSet<Caseload> Caseload { get; set; }
        public DbSet<Alert> Alert { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ICPAction> ICPCancerActionsList { get; set; }
        public DbSet<ICPGeneralAction> ICPGeneralActionsList { get; set; }
        public DbSet<ICPGeneralAction2> ICPGeneralActionsList2 { get; set; }
        public DbSet <ICPCancerReviewAction> ICPCancerReviewActionsList { get; set; }
        public DbSet<Risk> Risk { get; set; }
        public DbSet<Surveillance> Surveillance { get; set; }
        public DbSet<Eligibility> Eligibility { get; set; }
        public DbSet<ExternalFacility> ExternalFacility { get; set; }
        public DbSet<ExternalClinician> ExternalClinician { get; set; }        
        public DbSet<Relation> Relations { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<Pathway> Pathways { get; set; }
        public DbSet<Priority> Priority { get; set; }
        public DbSet<Notification> Notifications { get; set; }        
        public DbSet<Constant> Constants { get; set; }
        public DbSet<ExternalCliniciansAndFacilities> ExternalCliniciansAndFacilities { get; set; }
        public DbSet<PatientTitle> PatientTitles { get; set; }
        public DbSet<YesNo> YesNo { get; set; }
        public DbSet<ActivityType> ActivityTypes { get; set; }
    }
}
