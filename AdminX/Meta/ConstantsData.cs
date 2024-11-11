using AdminX.Data;
using AdminX.Models;


namespace AdminX.Meta
{
    interface IConstantsData
    {
        public string GetConstant(string constantCode, int constantValue);
    }
    public class ConstantsData : IConstantsData
    {        
        private readonly ClinicalContext? _clinContext;
        
        public ConstantsData(ClinicalContext docContext)
        {
            _clinContext = docContext;
        }        
       

        public string GetConstant(string constantCode, int constantValue)
        {
            Constant item = _clinContext.Constants.FirstOrDefault(c => c.ConstantCode == constantCode);

            if (constantValue == 1)
            {
                return item.ConstantValue;
            }
            else
            {
                return item.ConstantValue2;
            }
        }
    }
}
