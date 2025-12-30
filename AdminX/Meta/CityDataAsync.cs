using AdminX.Data;
using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface ICityDataAsync
    {
        public Task<string> GetCounty(string city);
        public Task<List<ListCity>> GetAllCities();
    }
    public class CityDataAsync : ICityDataAsync
    {        
        private readonly AdminContext _adminContext;

        public CityDataAsync(AdminContext adminContext)
        {            
            _adminContext = adminContext;
        }
        
        public async Task<string> GetCounty(string city)
        {
            string county = "";
            var cityCounty = await _adminContext.ListCity.FirstAsync(c => c.TownCity == city);
            if(cityCounty != null) { county = cityCounty.County; }

            return county;
        }

        public async Task<List<ListCity>> GetAllCities()
        {
            IQueryable<ListCity> cities = _adminContext.ListCity.Where(c => c.County != null);

            return await cities.ToListAsync();
        }
    }
}
