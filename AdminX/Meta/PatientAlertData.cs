using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Models;

namespace AdminX.Meta
{
    public interface IPatientAlertData 
    {
        public Alert GetProtectedAdress(int id);
    }

    public class PateintAlertData : IPatientAlertData
    {
        private readonly ClinicalContext _clinContext;

        public PateintAlertData(ClinicalContext context)
        {
            _clinContext = context;
        }

        public Alert GetProtectedAdress(int id)
        {
            Alert alert = _clinContext.Alert.FirstOrDefault(i => i.MPI == id);
            return alert;
        }
    }

}
