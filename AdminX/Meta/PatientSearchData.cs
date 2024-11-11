using AdminX.Data;
using AdminX.Models;


namespace AdminX.Meta
{
    interface IPatientSearchData
    {        
        public List<Patient> GetPatientsListByCGUNo(string? cguNo);
        public List<Patient> GetPatientsListByName(string? firstname, string? lastname);
        public List<Patient> GetPatientsListByNHS(string? nhsNo);
        public List<Patient> GetPatientsListByDOB(DateTime dob);
        //the reason for multiple "GetPatientsLists", and not one with multiple parameters, is because in order to do that,
        //the "patients" list would have to be created first and then narrowed by criteria.
        //This would result in very long loading times, as there are a LOT of patients, and I don't really want to select them all
        //only to have to filter them.
    }
    public class PatientSearchData : IPatientSearchData
    {
        private readonly ClinicalContext _clinContext;       

        public PatientSearchData(ClinicalContext context)
        {
            _clinContext = context;
        }       
               
        
        public List<Patient> GetPatientsListByCGUNo(string cguNo)
        {
            IQueryable<Patient> patients = _clinContext.Patients.Where(p => p.CGU_No.Contains(cguNo));            
            
            return patients.ToList();
        }
        public List<Patient> GetPatientsListByName(string? firstname, string? lastname)
        {
            IQueryable<Patient> patients = _clinContext.Patients.Where(p => p.FIRSTNAME.Contains(firstname) || p.LASTNAME.Contains(lastname));
            
            return patients.ToList();
        }

        public List<Patient> GetPatientsListByNHS(string nhsNo)
        {
            IQueryable<Patient> patients = _clinContext.Patients.Where(p => p.SOCIAL_SECURITY.Contains(nhsNo));

            return patients.ToList();
        }

        public List<Patient> GetPatientsListByDOB(DateTime dob)        
        {
            IQueryable<Patient> patients = _clinContext.Patients.Where(p => p.DOB == dob);

            return patients.ToList();
        }
    }
}
