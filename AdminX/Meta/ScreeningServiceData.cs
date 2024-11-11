using AdminX.Data;
using AdminX.Models;
using System.Data;

namespace AdminX.Meta
{
    interface IScreeningServiceData
    {
        
    }
    public class ScreeningServiceData : IScreeningServiceData
    {
        private readonly ClinicalContext _clinContext;
        
        public ScreeningServiceData(ClinicalContext context)
        {
            _clinContext = context;
        }
                
        
    }
}
