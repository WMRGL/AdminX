using AdminX.Data;
using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Meta
{
    public interface ILanguageDataAsync
    {
        public Task<List<Language>> GetLanguages();
    }

    public class LanguageDataAsync : ILanguageDataAsync
    {
        private readonly AdminContext _adminContext;

        public LanguageDataAsync(AdminContext adminContext)
        {
            _adminContext = adminContext;
        }

        public async Task<List<Language>> GetLanguages()
        {
            List<Language> listLanguages = await _adminContext.Language.ToListAsync();

            return listLanguages;
        }
    }
}


