using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using RestSharp;

namespace AdminX.Controllers
{
    public class APIController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly DocumentContext _docContext;
        private readonly IConfiguration _config;
        private readonly IPatientData _patientData;
        private readonly IConstantsData _constants;
        private readonly HPOCodeData _hpo;
        private string apiURLBase;
        private string apiURL;
        private string authKey;
        private string apiKey;

        public APIController(ClinicalContext clinContext, DocumentContext docContext, IConfiguration config)
        {
            _clinContext = clinContext;
            _docContext = docContext;
            _patientData = new PatientData(_clinContext);
            _constants = new ConstantsData(_docContext);
            _hpo = new HPOCodeData(_clinContext);
            apiURLBase = _constants.GetConstant("PhenotipsURL", 1).Trim();
            authKey = _constants.GetConstant("PhenotipsAPIAuthKey", 1).Trim();
            apiKey = _constants.GetConstant("PhenotipsAPIAuthKey", 2).Trim();
            _config = config;
        }

        public bool CheckResponseValid(string response)
        {
            if (response.Contains("<html>")) //this seems to be the only way to check - if no results, it will return "<html>"
            {
                return false;
            }

            return true;
        }

        public async Task<string> GetPhenotipsPatientID(int id)
        {
            //bool isExists = false;
            string phenotipsID = "";
            var patient = _patientData.GetPatientDetails(id);
            apiURL = apiURLBase + $":443/rest/patients/eid/{patient.CGU_No}";
            var options = new RestClientOptions(apiURL);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", "Basic " + authKey);
            request.AddHeader("X-Gene42-Secret", apiKey);
            var response = await client.GetAsync(request);

            if (CheckResponseValid(response.Content)) 
            {
                dynamic dynJson = JsonConvert.DeserializeObject(response.Content);
                phenotipsID = dynJson.family_id;
            }

            return phenotipsID;
        }

        public async Task<Int16> PushPtToPhenotips(int id)
        {
            var patient = _patientData.GetPatientDetails(id);
            //bool isSuccess = false;
            //string sMessage = "";
            Int16 result = 0;

            if (GetPhenotipsPatientID(id).Result == "")
            {
                

                DateTime DOB = patient.DOB.GetValueOrDefault();
                int yob = DOB.Year;
                int mob = DOB.Month;
                int dob = DOB.Day;

                apiURL = apiURLBase + ":443/rest/patients";
                var options = new RestClientOptions(apiURL);
                var client = new RestClient(options);
                var request = new RestRequest("");
                request.AddHeader("authorization", $"Basic {authKey}");
                request.AddHeader("X-Gene42-Secret", apiKey);
                string apiCall = "{\"patient_name\":{\"first_name\":\"" + $"{patient.FIRSTNAME}" + "\",\"last_name\":\"" + $"{patient.LASTNAME}" + "\"}";
                apiCall = apiCall + ",\"date_of_birth\":{\"year\":" + yob.ToString() + ",\"month\":" + mob.ToString() + ",\"day\":" + dob.ToString() + "}";
                apiCall = apiCall + ",\"sex\":\"" + $"{patient.SEX.Substring(0, 1)}" + "\",\"external_id\":\"" + $"{patient.CGU_No}" + "\"}";

                request.AddJsonBody(apiCall, false);
                var response = await client.PostAsync(request);

                //SetPhenotipsOwner(patient.MPI, User.Identity.Name); - not currently necessary
                                
                if (response.Content == "")
                {
                    //isSuccess = true;
                    //sMessage = "Push to Phenotips successful";
                    result = 1;

                    string ptID = await GetPhenotipsPatientID(patient.MPI);
                    AddPatientToPhenotipsMirrorTable(ptID, patient.MPI, patient.CGU_No, patient.FIRSTNAME, patient.LASTNAME, patient.DOB.GetValueOrDefault(), 
                        patient.POSTCODE, patient.SOCIAL_SECURITY);
                }
                else
                {
                    result = -1;
                    //sMessage = "Push to Phenotips failed :(";
                }
            }
            //else
            //{
                //sMessage = "Patient already exists in Phenotips!";
            //}
            //return RedirectToAction("Confirm", "Phenotips", new { id = patient.MPI, success = isSuccess, message = sMessage });
            return result;
        }

        public async Task<List<Relative>> ImportRelativesFromPhenotips(int id)
        {
            var patient = _patientData.GetPatientDetails(id);
            List<Relative> relatives = new List<Relative>(); //because it has to return something

            string phenotipsID = GetPhenotipsPatientID(id).Result;
            
            if (phenotipsID != null && phenotipsID != "")
            {
                phenotipsID = phenotipsID.Substring(phenotipsID.Length - 10);

                List<Relative> relListAll = new List<Relative>();

                apiURL = apiURLBase + $":443/get/PhenoTips/FamilyPedigreeInterface?action=familyinfo&document_id={phenotipsID}";

                var options = new RestClientOptions(apiURL);
                var client = new RestClient(options);
                var request = new RestRequest("");
                request.AddHeader("accept", "application/json");
                request.AddHeader("authorization", "Basic " + authKey);
                request.AddHeader("X-Gene42-Secret", apiKey);
                var response = await client.GetAsync(request);
                dynamic dynJson = JsonConvert.DeserializeObject(response.Content);
                //http://localhost:7168/Patient/PatientDetails?id=227775

                int relID = 0;
                
                foreach (var item in dynJson.pedigree.members)
                {
                    if ((item.properties.patient_name.first_name != null && item.properties.patient_name.first_name != "")
                            && (item.properties.patient_name.last_name != null && item.properties.patient_name.last_name != ""))
                    {
                        relID += 1;
                        DateTime dob = DateTime.Parse("1900-01-01");
                        DateTime dod = DateTime.Parse("1900-01-01");
                        string gender;
                        if (item.properties.date_of_birth.year != null && item.properties.date_of_birth.month != null && item.properties.date_of_birth.day != null)
                        {
                            dob = DateTime.Parse(item.properties.date_of_birth.year.ToString() + "-" +
                                            item.properties.date_of_birth.month.ToString() + "-" +
                                            item.properties.date_of_birth.day.ToString());
                        }
                        if (item.properties.date_of_death.year != null && item.properties.date_of_death.month != null && item.properties.date_of_death.day != null)
                        {
                            dod = DateTime.Parse(item.properties.date_of_death.year.ToString() + "-" +
                                            item.properties.date_of_death.month.ToString() + "-" +
                                            item.properties.date_of_death.day.ToString());
                        }

                        gender = item.properties.sex;

                        if (gender != "M" && gender != "F")
                        {
                            gender = "U";
                        }

                        relListAll.Add(new Relative
                        {
                            RelForename1 = item.properties.patient_name.first_name,
                            RelSurname = item.properties.patient_name.last_name,
                            DOB = dob,
                            DOD = dod,
                            RelSex = gender,
                            WMFACSID = patient.WMFACSID,
                            relsid = relID
                        });
                    }
                }

                RelativeData relData = new RelativeData(_clinContext); 

                foreach (var rel in relListAll)
                {
                    if (relData.GetRelativeDetailsByName(rel.RelForename1, rel.RelSurname).Count() == 0 &&
                            rel.RelForename1 != patient.FIRSTNAME && rel.RelSurname != patient.LASTNAME)
                    {
                        relatives.Add(new Relative
                        {
                            relsid = rel.relsid,
                            WMFACSID = rel.WMFACSID,
                            RelForename1 = rel.RelForename1,
                            RelSurname = rel.RelSurname,
                            DOB = rel.DOB,
                            DOD = rel.DOD,
                            RelSex = rel.RelSex
                        });
                    }
                }
            }            

            return relatives.ToList();
        }


        public async Task SetPhenotipsOwner(int id, string userName) //might not be necessary anymore...
        {
            if (id != null && id != 0)
            {
                string phenotipsID = GetPhenotipsPatientID(id).Result;
                                
                apiURL = apiURLBase + $":443/rest/patients/{phenotipsID}/permissions";
                var options = new RestClientOptions(apiURL);
                var client = new RestClient(options);
                var request = new RestRequest("");
                request.AddHeader("content-type", "application/json");
                request.AddHeader("authorization", authKey);
                request.AddHeader("X-Gene42-Secret", apiKey);
                request.AddJsonBody("{\"owner\":{\"id\":\"xwiki:XWiki." + userName + "\"}}", false);
                var response = client.PutAsync(request);
            }
        }



        public void AddPatientToPhenotipsMirrorTable(string ptID, int mpi, string cguno, string firstname, string lastname, DateTime DOB, string postCode, string nhsNo)
        {
            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("Insert into dbo.PhenotipsPatients (PhenotipsID, MPI, CGUNumber, FirstName, Lastname, DOB, PostCode, NHSNo) values('"
                + ptID + "', " + mpi + ", '" + cguno + "', '" + firstname + "', '" + lastname + "', '" + DOB.ToString("yyyy-MM-dd") + "', '" + postCode +
                "', '" + nhsNo + "')", conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }


        public async Task<List<HPOTerm>> GetHPOCodes(string searchTerm)
        {
            List<HPOTerm> hpoCodes = new List<HPOTerm>();

            searchTerm = searchTerm.Replace(" ", "%20");
            int pageNo = 1;
            int setSize = 10;

            apiURL = $"https://ontology.jax.org/api/hp/search?q={searchTerm}&page={pageNo.ToString()}&limit={setSize.ToString()}";  //I don't know what the page and limit actually means!

            var options = new RestClientOptions(apiURL);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");

            var response = await client.GetAsync(request);

            dynamic dynJson = JsonConvert.DeserializeObject(response.Content);
            int totalCount = dynJson.totalCount;

            int i = 0;

            if (totalCount > setSize) //because I can't just have a number that returns all results - that would be far too convenient!
            {
                var batch = BatchInteger(totalCount, setSize);

                foreach (var item in batch)
                {
                    foreach (var term in dynJson.terms)
                    {
                        i += 1; //the model needs an ID field, but as this is not supplied by ontology themselves, we need to fabricate it here
                        string hpoID = term.id;
                        if (hpoCodes.Where(t => t.TermCode == hpoID).Count() == 0) //because it's duplicating them all for some reason!!!
                        {
                            hpoCodes.Add(new HPOTerm { TermCode = term.id, Term = term.name, ID = i });
                        }
                    }
                    pageNo++;
                    apiURL = $"https://ontology.jax.org/api/hp/search?q={searchTerm}&page={pageNo.ToString()}&limit={item.ToString()}";
                    options = new RestClientOptions(apiURL);
                    client = new RestClient(options);
                    request = new RestRequest("");
                    request.AddHeader("accept", "application/json");

                    response = await client.GetAsync(request);

                    dynJson = JsonConvert.DeserializeObject(response.Content);


                }
            }
            else
            {
                foreach (var term in dynJson.terms)
                {
                    i += 1;
                    Console.WriteLine(term.id);
                    hpoCodes.Add(new HPOTerm { TermCode = term.id, Term = term.name, ID = i });
                }
            }


            return hpoCodes.OrderBy(c => c.TermCode).ToList();
        }

        public async Task<HPOTerm> GetHPODataByTermCode(string hpoTermCode)
        {
            HPOTerm hPOTerm;

            apiURL = $"https://ontology.jax.org/api/hp/terms/{hpoTermCode.Replace(":", "%3A")}";

            var options = new RestClientOptions(apiURL);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");

            var response = await client.GetAsync(request);


            dynamic dynJson = JsonConvert.DeserializeObject(response.Content);

            hPOTerm = new HPOTerm { ID = 1, TermCode = dynJson.id, Term = dynJson.name };


            return hPOTerm;
        }

        public async Task<List<string>> GetHPOSynonymsByTermCode(string hpoTermCode)
        {
            List<string> hpoSynonyms = new List<string>();

            apiURL = $"https://ontology.jax.org/api/hp/terms/{hpoTermCode.Replace(":", "%3A")}";

            var options = new RestClientOptions(apiURL);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");

            var response = await client.GetAsync(request);


            dynamic dynJson = JsonConvert.DeserializeObject(response.Content);

            foreach (var item in dynJson.synonyms)
            {
                hpoSynonyms.Add(item.ToString());
            }

            return hpoSynonyms.ToList();
        }

        public async Task<IActionResult> GetAllHPOTerms()
        {
            apiURL = $"https://ontology.jax.org/api/hp/terms";

            var options = new RestClientOptions(apiURL);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");

            var response = await client.GetAsync(request);

            dynamic dynJson = JsonConvert.DeserializeObject(response.Content);

            foreach (var item in dynJson)
            {
                string hpoID = item.id;
                string hpoName = item.name;

                if (_hpo.GetHPOTermByTermCode(hpoID) == null)
                {
                    _hpo.AddHPOTermToDatabase(hpoID, hpoName.Replace("'", "''"), User.Identity.Name, _config);

                    int hpocodeid = _hpo.GetHPOTermByTermCode(hpoID).ID;

                    foreach (var synonym in item.synonyms)
                    {
                        string syn = synonym.ToString();
                        syn = syn.Replace("{", "").Replace("}", "");
                        _hpo.AddHPOSynonymToDatabase(hpocodeid, syn, User.Identity.Name, _config);
                    }
                }
            }

            return RedirectToAction("Index", "Home");
        }

        private IEnumerable<int> BatchInteger(int total, int batchSize)
        { //this is supposed to split the total into batches of 10, so each one can be run as a separate page - but the API itself is a bit janky
            if (batchSize == 0)
            {
                yield return 0;
            }

            if (batchSize >= total)
            {
                yield return total;
            }
            else
            {
                int rest = total % batchSize;
                int divided = total / batchSize;
                if (rest > 0)
                {
                    divided += 1;
                }

                for (int i = 0; i < divided; i++)
                {
                    if (rest > 0 && i == divided - 1)
                    {
                        yield return rest;
                    }
                    else
                    {
                        yield return batchSize;
                    }
                }
            }
        }
    }
}
