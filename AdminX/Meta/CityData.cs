using AdminX.Data;
using AdminX.Models;

namespace AdminX.Meta
{
    public interface ICityData
    {
        public string GetCounty(string city);
        public List<ListCity> GetAllCities();
    }
    public class CityData : ICityData
    {        
        private readonly AdminContext _adminContext;

        public CityData(AdminContext adminContext)
        {            
            _adminContext = adminContext;
        }
        
        public string GetCounty(string city)
        {
            string county = _adminContext.ListCity.FirstOrDefault(c => c.TownCity == city).County;

            return county;
        }

        public List<ListCity> GetAllCities()
        {
            IQueryable<ListCity> cities = _adminContext.ListCity.Where(c => c.County != null);

            return cities.ToList();
        }
    }
}
