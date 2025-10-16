using AdminX.Data;
using AdminX.Models;

namespace AdminX.Meta
{
    public interface ILanguageData
    {
        public List<Language> GetLanguages();
    }

    public class LanguageData : ILanguageData
    {
        private readonly AdminContext _adminContext;

        public LanguageData(AdminContext adminContext)
        {
            _adminContext = adminContext;
        }

        public List<Language> GetLanguages()
        {
            List<Language> listLanguages = _adminContext.Language.ToList();
            return listLanguages;
        }
    }
}


