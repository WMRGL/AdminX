using AdminX.Data;
using AdminX.Models;
using AdminX.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AdminX.Meta
{
    interface IHPOCodeData
    {
        public List<HPOTermDetails> GetHPOTermsAddedList(int id);
        

    }
    public class HPOCodeData : IHPOCodeData
    {
        private readonly ClinicalContext _clinContext;

        public HPOCodeData(ClinicalContext context)
        {
            _clinContext = context;
        }
        
        public List<HPOTermDetails> GetHPOTermsAddedList(int id) //Get list of HPO codes added to a patient by MPI
        {
            IQueryable<HPOTermDetails> notes = from n in _clinContext.HPOTermDetails
                        where n.MPI == id
                        select n;            

            return notes.ToList();
        }        
    }
}
