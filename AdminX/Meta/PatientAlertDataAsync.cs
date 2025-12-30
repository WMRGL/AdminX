using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface IPatientAlertDataAsync
    {
        public Task<Alert> GetProtectedAdress(int id);
    }

    public class PatientAlertDataAsync : IPatientAlertDataAsync
    {
        private readonly ClinicalContext _clinContext;

        public PatientAlertDataAsync(ClinicalContext context)
        {
            _clinContext = context;
        }

        public async Task<Alert> GetProtectedAdress(int id)
        {
            Alert alert = await _clinContext.Alert.FirstAsync(i => i.MPI == id);

            return alert;
        }
    }

}
