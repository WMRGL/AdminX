using AdminX.Data;
using AdminX.Models;
using System.Data;

namespace AdminX.Meta
{
    interface ITestData
    {        
        public List<Test> GetTestListByPatient(int mpi);        
    }
    public class TestData : ITestData
    {
        private readonly ClinicalContext _clinContext;
    

        public TestData(ClinicalContext context)
        {
            _clinContext = context;
        }
        

        public List<Test> GetTestListByPatient(int mpi)
        {
            IQueryable<Test> tests = from t in _clinContext.Test
                        where t.MPI.Equals(mpi)
                        select t;

            return tests.ToList();
        }
    }
}
