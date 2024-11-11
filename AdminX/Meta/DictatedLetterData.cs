using AdminX.Data;
using AdminX.Models;
using System.Data;

namespace AdminX.Meta
{
    interface IDictatedLetterData
    {
        public List<DictatedLetter> GetDictatedLettersList(string staffcode);
        public List<DictatedLetter> GetDictatedLettersForPatient(int mpi);
        public DictatedLetter GetDictatedLetterDetails(int dotID);
        public List<DictatedLettersPatient> GetDictatedLettersPatientsList(int dotID);
        public List<DictatedLettersCopy> GetDictatedLettersCopiesList(int dotID);
        public DictatedLettersCopy GetDictatedLetterCopyDetails(int id);
        public List<Patient> GetDictatedLetterPatientsList(int dotID);
    }
    public class DictatedLetterData : IDictatedLetterData
    {
        private readonly ClinicalContext _clinContext;

        public DictatedLetterData(ClinicalContext context)
        {
            _clinContext = context;
        }
               

        public List<DictatedLetter> GetDictatedLettersList(string staffcode)
        {
            IQueryable<DictatedLetter> letters = from l in _clinContext.DictatedLetters
                          where l.LetterFromCode == staffcode && l.MPI != null && l.RefID != null && l.Status != "Printed"
                          orderby l.DateDictated descending
                          select l;

            return letters.ToList();
        }

        public List<DictatedLetter> GetDictatedLettersForPatient(int mpi)
        {
            IQueryable<DictatedLetter> letters = from l in _clinContext.DictatedLetters
                                                 where l.MPI == mpi && l.Status != "Printed"
                                                 orderby l.DateDictated descending
                                                 select l;

            return letters.ToList();
        }

        public DictatedLetter GetDictatedLetterDetails(int dotID) //Get details of DOT letter by its DotID
        {
            DictatedLetter letter = _clinContext.DictatedLetters.FirstOrDefault(l => l.DoTID == dotID);

            return letter;
        }

        public List<DictatedLettersPatient> GetDictatedLettersPatientsList(int dotID) //Get list of patients added to a DOT letter by the DotID
        {
            IQueryable<DictatedLettersPatient> patient = from p in _clinContext.DictatedLettersPatients
                          where p.DOTID == dotID
                          select p;

            List<DictatedLettersPatient> patients = new List<DictatedLettersPatient>();

            foreach (var p in patient)
            {
                patients.Add(new DictatedLettersPatient() { DOTID = p.DOTID });
            }

            return patients;
        }

        public List<DictatedLettersCopy> GetDictatedLettersCopiesList(int dotID) //Get list of all CCs added to a DOT letter by DotID
        {
            IQueryable<DictatedLettersCopy> copies = from c in _clinContext.DictatedLettersCopies
                       where c.DotID == dotID
                       select c;            

            return copies.ToList();
        }        
        
        public DictatedLettersCopy GetDictatedLetterCopyDetails(int id)  //Get details of a CC on a letter for deletion
        {
            DictatedLettersCopy letter = _clinContext.DictatedLettersCopies.FirstOrDefault(x => x.CCID == id);

            return letter;
        }

        public List<Patient> GetDictatedLetterPatientsList(int dotID) //Get list of all patients in the family that can be added to a DOT, by the DotID
        {
            DictatedLetter letter = _clinContext.DictatedLetters.FirstOrDefault(l => l.DoTID == dotID);
            int? mpi = letter.MPI;
            Patient pat = _clinContext.Patients.FirstOrDefault(p => p.MPI == mpi.GetValueOrDefault());

            IQueryable<Patient> patients = from p in _clinContext.Patients
                           where p.PEDNO == pat.PEDNO
                           select p;

            return patients.ToList();
        }
    }
}
