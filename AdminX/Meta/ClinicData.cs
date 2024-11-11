using AdminX.Data;
using AdminX.Models;
using System.Data;

namespace AdminX.Meta
{
    interface IClinicData
    {
        public List<Appointment> GetClinicList(DateTime dateFrom, DateTime dateTo);
        public List<Appointment> GetClinicByPatientsList(int mpi);
        public Appointment GetClinicDetails(int refID);
        public List<Outcome> GetOutcomesList();
    }
    public class ClinicData : IClinicData
    {
        private readonly ClinicalContext _clinContext;        
        private readonly StaffUserData _staffUser;

        public ClinicData(ClinicalContext context)
        {
            _clinContext = context;
            _staffUser = new StaffUserData(_clinContext);
        }
        
             

        public List<Appointment> GetClinicList(DateTime dateFrom, DateTime dateTo) //Get list of your clinics
        {

            IQueryable<Appointment> clinics = from c in _clinContext.Clinics
                          where c.AppType.Contains("App") && c.Attendance == "NOT RECORDED"
                          && c.BOOKED_DATE >= dateFrom && c.BOOKED_DATE <= dateTo
                          select c;

            return clinics.ToList();
        }

        public List<Appointment> GetClinicByPatientsList(int mpi)
        {
            IQueryable<Appointment> appts = from c in _clinContext.Clinics
                        where c.MPI.Equals(mpi)
                        orderby c.BOOKED_DATE descending
                        select c;

            return appts.ToList();
        }

        public Appointment GetClinicDetails(int refID) //Get details of an appointment for display only
        {
            Appointment appt = _clinContext.Clinics.FirstOrDefault(a => a.RefID == refID);

            return appt;
        }

        public List<Outcome> GetOutcomesList() //Get list of outcomes for clinic appointments
        {
            IQueryable<Outcome> outcomes = from o in _clinContext.Outcomes
                          where o.DEFAULT_CLINIC_STATUS.Equals("Active")
                          select o;

            return outcomes.ToList();
        }

             
        
    }
}
