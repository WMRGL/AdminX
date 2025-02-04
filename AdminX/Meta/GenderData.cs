using AdminX.Data;
using AdminX.Models;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Models;

namespace AdminX.Meta
{
    public interface IGenderData
    {
        public List<Gender> GetGenderList();
    }
    public class GenderData : IGenderData
    {       
                
            private readonly ClinicalContext _context;

            public GenderData(ClinicalContext context)
            {
            _context = context;
            }

           public List<Gender> GetGenderList()
            {
                IQueryable<Gender> genders = _context.Genders;

                return genders.ToList();
            }

    }
}
