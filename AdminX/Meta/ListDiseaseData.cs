using AdminX.Data;
using AdminX.Models;
using ClinicalXPDataConnections.Models;
using Microsoft.Identity.Client;

namespace AdminX.Meta
{
    public interface IListDiseaseData
    {
        public List<ListDisease> GetDiseases();
    }

    public class ListDiseaseData : IListDiseaseData
    {
        private readonly AdminContext _adminContext;

        public ListDiseaseData(AdminContext adminContext)
        {
            _adminContext = adminContext;
        }

        public List<ListDisease> GetDiseases()
        {
             List<ListDisease> listDiseases = _adminContext.ListDiseases.ToList();
            return listDiseases;
        }
    }
}
