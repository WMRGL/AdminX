using AdminX.Data;
using AdminX.Models;


namespace AdminX.Meta
{
    interface IPatientData
    {
        public Patient GetPatientDetails(int id);
        public Patient GetPatientDetailsByWMFACSID(int id);
        public Patient GetPatientDetailsByDemographicData(string firstname, string lastname, string nhsno, DateTime dob);
        public List<Ethnicity> GetEthnicitiesList();
        public List<PatientTitle> GetTitlesList();
    }
    public class PatientData : IPatientData
    {
        private readonly ClinicalContext _clinContext;

        public PatientData(ClinicalContext context)
        {
            _clinContext = context;
        }

        public Patient GetPatientDetails(int id)
        {
            Patient patient = _clinContext.Patients.FirstOrDefault(i => i.MPI == id);
            return patient;
        } //Get patient details from MPI

        public Patient GetPatientDetailsByWMFACSID(int id)
        {
            Patient patient = _clinContext.Patients.FirstOrDefault(i => i.WMFACSID == id);
            return patient;
        } //Get patient details from WMFACSID               

        public Patient GetPatientDetailsByDemographicData(string firstname, string lastname, string nhsno, DateTime dob)
        {
            Patient patient = _clinContext.Patients.FirstOrDefault(i => i.FIRSTNAME == firstname && i.LASTNAME == lastname && 
                                                        i.SOCIAL_SECURITY == nhsno && i.DOB == dob);
            return patient;
        }

        public List<Ethnicity> GetEthnicitiesList()
        {
            IQueryable<Ethnicity> ethnicities = from e in _clinContext.Ethnicity
                                                orderby e.Ethnic
                                                select e;

            return ethnicities.ToList();
        }

        public List<PatientTitle> GetTitlesList()
        {
            IQueryable<PatientTitle> titles = from e in _clinContext.PatientTitles                                                
                                                select e;

            return titles.ToList();
        }
    }
}
