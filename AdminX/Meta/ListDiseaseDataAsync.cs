using AdminX.Data;
using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface IListDiseaseDataAsync
    {
        public Task<List<ListDisease>> GetDiseases();
    }

    public class ListDiseaseDataAsync : IListDiseaseDataAsync
    {
        private readonly AdminContext _adminContext;

        public ListDiseaseDataAsync(AdminContext adminContext)
        {
            _adminContext = adminContext;
        }

        public async Task<List<ListDisease>> GetDiseases()
        {
             List<ListDisease> listDiseases = await _adminContext.ListDiseases.ToListAsync();
            return listDiseases;
        }
    }
}
