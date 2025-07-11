using Newtonsoft.Json;
using RestSharp;
using APIControllers.Data;
using APIControllers.Meta;
using APIControllers.Models;
using Microsoft.Data.SqlClient;

namespace APIControllers.Controllers
{
    public class APIControllerLOCAL
    {
        private readonly APIContext _context;
        
        private readonly IConfiguration _config;
        private readonly IAPIPatientData _APIPatientData;
        private readonly IAPIConstantsData _constants;
        private readonly IAPIPhenotipsMirrorData _phenotipsMirrorData;
        private readonly APIHPOCodeData _hpo;
        private string apiURLBase;
        private string apiURL;
        private string authKey;
        private string apiKey;

        public APIControllerLOCAL(APIContext context, IConfiguration config) //to test API - LIVE to use the .dll file
        {
            _context = context;
            _APIPatientData = new APIPatientData(_context);
            _constants = new APIConstantsData(_context);
            _hpo = new APIHPOCodeData(_context);
            _phenotipsMirrorData = new APIPhenotipsMirrorData(_context);
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

        public async Task<string> GetPhenotipsPatientID(int id) //gets Phenotips ID from an existing patient (takes MPI)
        {
            //bool isExists = false;
            string phenotipsID = "";
            var patient = _APIPatientData.GetPatientDetails(id);
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
                phenotipsID = dynJson.id;
            }

            return phenotipsID;
        }

        public async Task<string> GetPhenotipsFamilyID(int id) //gets Phenotips ID from an existing patient (takes MPI)
        {
            //bool isExists = false;
            string phenotipsID = "";
            var patient = _APIPatientData.GetPatientDetails(id);
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
            var patient = _APIPatientData.GetPatientDetails(id);
            //bool isSuccess = false;
            //string sMessage = "";
            Int16 result = 0; //if patient exists, result will remain as 0

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
                apiCall = apiCall + ",\"sex\":\"" + $"{patient.SEX.Substring(0, 1)}" + "\",\"external_id\":\"" + $"{patient.CGU_No}" + "\"";
                apiCall = apiCall + ",\"labeled_eids\":[{\"label\":\"NHS Number\",\"value\":\"" + $"{patient.SOCIAL_SECURITY}" + "\"}";
                apiCall = apiCall + ",{\"label\":\"MPI\",\"value\":\"" + $"{patient.MPI}" + "\"}]";
                apiCall = apiCall + "}";

                request.AddJsonBody(apiCall, false);
                var response = await client.PostAsync(request);

                //SetPhenotipsOwner(patient.MPI, User.Identity.Name); - not currently necessary
                                
                if (response.Content == "")
                {
                    //isSuccess = true;
                    //sMessage = "Push to Phenotips successful";
                    result = 1; //success result

                    string ptID = await GetPhenotipsPatientID(patient.MPI);
                    AddPatientToPhenotipsMirrorTable(ptID, patient.MPI, patient.CGU_No, patient.FIRSTNAME, patient.LASTNAME, patient.DOB.GetValueOrDefault(), 
                        patient.POSTCODE, patient.SOCIAL_SECURITY);
                }
                else
                {
                    result = -1; //failure result
                    //sMessage = "Push to Phenotips failed :(";
                }
            }            
            return result;
        }

        public async Task<Int16> SynchroniseMirrorWithPhenotips(int id)
        {
            Int16 result = 0; 
            var patient = _APIPatientData.GetPatientDetails(id);

            PhenotipsPatient ptpt = _phenotipsMirrorData.GetPhenotipsPatientByID(patient.MPI);

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
                SqlConnection con = new SqlConnection(_config.GetConnectionString("ConString"));
                SqlCommand com = new SqlCommand("", con);
                dynamic dynJson = JsonConvert.DeserializeObject(response.Content);
                Console.WriteLine(dynJson);

                string ptID = dynJson.id;
                string famID = dynJson.family_id;
                if (famID != null)
                {
                    famID = famID.Substring(famID.Length - 10); //because it puts "xwiki..." at the start
                }
                string firstName = dynJson.patient_name.first_name;
                string lastName = dynJson.patient_name.last_name;

                int dobYear = dynJson.date_of_birth.year;
                int dobMonth = dynJson.date_of_birth.month;
                int dobDay = dynJson.date_of_birth.day;
                DateTime dob = DateTime.Parse(dobYear + "-" + dobMonth + "-" + dobDay);                
                
                string nhsNo = "";

                if (dynJson.ContainsKey("labeled_eids"))
                {
                    foreach (var item in dynJson.labeled_eids)
                    {
                        if (item.label == "NHS Number")
                        {
                            nhsNo = item.value;
                        }
                    }
                }

                nhsNo = nhsNo.Replace(" ", "");                

                if (firstName == patient.FIRSTNAME && lastName == patient.LASTNAME && dob == patient.DOB && nhsNo == patient.SOCIAL_SECURITY)
                {
                    result = 1; //check if PT result matches CGU_DB records
                }
                
                if(ptpt == null) //check if patient in PT Mirror table, add if not, modify accordingly if required
                {
                    con.Open();
                    com.CommandText = $"insert into PhenotipsPatients (FirstName, LastName, DOB, NHSNo, MPI, CGUNumber, PhenotipsID, FamilyID) " +
                        $"values ('{firstName}', '{lastName}', '{dob.ToString("yyyy-MM-dd")}', '{nhsNo}', {id}, '{patient.CGU_No}', '{ptID}', '{famID}')";
                    com.ExecuteNonQuery();
                    con.Close();

                    ptpt = _phenotipsMirrorData.GetPhenotipsPatientByID(patient.MPI); //once pt added, have to get the patient again
                }

                if(ptpt.FirstName != firstName || ptpt.LastName != lastName || ptpt.DOB != dob || ptpt.NHSNo != nhsNo || ptpt.NHSNo == "")
                {
                    con.Open();
                    if (nhsNo == null || nhsNo == "")
                    {
                        com.CommandText = $"Update PhenotipsPatients set FirstName='{firstName}', LastName='{lastName}', DOB='{dob.ToString("yyyy-MM-dd")}', NHSNo=null " +
                            $"where MPI={id}";
                    }
                    else
                    {
                        com.CommandText = $"Update PhenotipsPatients set FirstName='{firstName}', LastName='{lastName}', DOB='{dob.ToString("yyyy-MM-dd")}', NHSNo='{nhsNo}' " +
                            $"where MPI={id}";
                    }
                    com.ExecuteNonQuery();
                    con.Close();
                } //update mirror table to match Phenotips

                if(ptpt.PhenotipsID != ptID)
                {
                    con.Open();
                    com.CommandText = $"Update PhenotipsPatients set PhenotipsID='{ptID}' where MPI={id}";
                    com.ExecuteNonQuery();
                    con.Close();
                }

                if(ptpt.FamilyID != famID)
                {
                    con.Open();
                    if (famID == null)
                    {
                        com.CommandText = $"Update PhenotipsPatients set FamilyID=null where MPI={id}";
                    }
                    else
                    {
                        com.CommandText = $"Update PhenotipsPatients set FamilyID='{famID}' where MPI={id}";
                    }
                    com.ExecuteNonQuery();
                    con.Close();
                }

            }

            return result;
        }
        
        public async Task<List<Relative>> ImportRelativesFromPhenotips(int id)
        {
            var patient = _APIPatientData.GetPatientDetails(id);
            List<Relative> relatives = new List<Relative>(); //because it has to return something so need to make the empty list first

            string phenotipsID = GetPhenotipsFamilyID(id).Result;
            
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

                APIRelativeData relData = new APIRelativeData(_context); 

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

        public async Task<Int16> SchedulePPQ(int id, string ppqType)
        {
            var patient = _APIPatientData.GetPatientDetails(id);
            //bool isSuccess = false;
            //string sMessage = "";
            Int16 result = 0;

            string pID = "";
            string ppqID;
            if (ppqType == "Cancer")
            {
                ppqID = "bwch_cancer";
            }
            else
            {
                ppqID = "bwch_general";
            }

            pID = GetPhenotipsPatientID(id).Result;
            Console.WriteLine(GetPPQUrl(patient.MPI, ppqType));

            if (!CheckPPQExists(id, ppqType).Result)
            {
                Console.WriteLine("PPQ doesn't exist");
                if (pID != "" && pID != null)
                {
                    DateTime DOB = patient.DOB.GetValueOrDefault();
                    int yob = DOB.Year;
                    int mob = DOB.Month;
                    int dob = DOB.Day;

                    apiURL = apiURLBase + ":443/rest/questionnaire-scheduler/create";
                    var options = new RestClientOptions(apiURL);
                    var client = new RestClient(options);
                    var request = new RestRequest("");
                    request.AddHeader("authorization", $"Basic {authKey}");
                    request.AddHeader("X-Gene42-Secret", apiKey);
                    string apiCall = "{\"questionnaireId\":\"" + $"{ppqID}" + "\",";
                    apiCall = apiCall + "\"mrn\":\"" + $"{patient.CGU_No}" + "\","; //mrn needs to be the "External Identifier" - the CGU number in this case
                    apiCall = apiCall + "\"firstName\":\"" + $"{patient.FIRSTNAME}" + "\",\"lastName\":\"" + $"{patient.LASTNAME}" + "\"";
                    apiCall = apiCall + ",\"dateOfBirth\":{\"day\":" + dob.ToString() + ",\"month\":" + mob.ToString() + ",\"year\":" + yob.ToString() + "},";
                    apiCall = apiCall + "\"associatedStudy\":null}";

                    Console.WriteLine(apiCall);

                    request.AddJsonBody(apiCall, false);
                    var response = await client.PostAsync(request);

                    if (response.Content.Contains("{\"workflowStatus\""))
                    {
                        result = 1; //success result                    
                    }
                    else
                    {
                        result = -1; //failure result                    
                    }
                }
            }
            return result;
        }

        public async Task<bool> CheckPPQExists(int id, string ppqType)
        {
            var patient = _APIPatientData.GetPatientDetails(id);
            //bool isSuccess = false;
            //string sMessage = "";
            bool result = false;

            string pID = "";
            string ppqID;
            if (ppqType == "Cancer")
            {
                ppqID = "bwch_cancer";
            }
            else
            {
                ppqID = "bwch_general";
            }

            pID = GetPhenotipsPatientID(id).Result;

            if (pID != "" && pID != null)
            {
                DateTime DOB = patient.DOB.GetValueOrDefault();
                int yob = DOB.Year;
                int mob = DOB.Month;
                int dob = DOB.Day;

                apiURL = apiURLBase + ":443/rest/questionnaire-scheduler/search";
                var options = new RestClientOptions(apiURL);
                var client = new RestClient(options);
                var request = new RestRequest("");
                request.AddHeader("authorization", $"Basic {authKey}");
                request.AddHeader("X-Gene42-Secret", apiKey);
                //string apiCall = "{\"search\":{\"questionnaireId\":\"" + $"{ppqID}" + "\",\"mrn\":\"" + $"{patient.CGU_No}" + "\",\"orderBy\":\"\",\"orderDir\":\"\",\"offset\":0,\"limit\":25}}";
                string apiCall = "{\"search\":{\"questionnaireId\":\"" + $"{ppqID}" + "\",\"mrn\":\"" + $"{patient.CGU_No}" + "\",\"status\":\"active\",\"orderBy\":\"\",\"orderDir\":\"\",\"offset\":0,\"limit\":25}}";

                request.AddJsonBody(apiCall, false);
                var response = await client.PostAsync(request);

                string requestResult = "";


                if (!response.Content.Contains("{\"scheduleRequests\":[]"))
                {
                    result = true;
                }
            }
            return result;
        }

        public async Task<string> GetPPQUrl(int id, string ppqType)
        {
            var patient = _APIPatientData.GetPatientDetails(id);
            //bool isSuccess = false;
            //string sMessage = "";
            string result = "";

            string pID = "";
            string ppqID;
            if (ppqType == "Cancer")
            {
                ppqID = "bwch_cancer";
            }
            else
            {
                ppqID = "bwch_general";
            }

            pID = GetPhenotipsPatientID(id).Result;

            if (pID != "" && pID != null)
            {
                if (CheckPPQExists(id, ppqType).Result)
                {
                    DateTime DOB = patient.DOB.GetValueOrDefault();
                    int yob = DOB.Year;
                    int mob = DOB.Month;
                    int dob = DOB.Day;

                    apiURL = apiURLBase + ":443/rest/questionnaire-scheduler/search";
                    var options = new RestClientOptions(apiURL);
                    var client = new RestClient(options);
                    var request = new RestRequest("");
                    request.AddHeader("authorization", $"Basic {authKey}");
                    request.AddHeader("X-Gene42-Secret", apiKey);
                    string apiCall = "{\"search\":{\"questionnaireId\":\"" + $"{ppqID}" + "\",\"mrn\":\"" + $"{patient.CGU_No}" + "\",\"orderBy\":\"\",\"orderDir\":\"\",\"offset\":0,\"limit\":25}}";

                    request.AddJsonBody(apiCall, false);
                    var response = await client.PostAsync(request);

                    if (response.Content.Contains("{\"scheduleRequests\""))
                    {
                        dynamic dynJson = JsonConvert.DeserializeObject(response.Content);
                        result = dynJson.scheduleRequests[0].questionnaireUri;
                    }                    
                }

            }
            return result;
        }

        public async Task<string> GetPPQQRCode(int id, string ppqType)
        {
            var patient = _APIPatientData.GetPatientDetails(id);
            //bool isSuccess = false;
            //string sMessage = "";
            string result = "";

            string pID = "";
            string ppqID;
            if (ppqType == "Cancer")
            {
                ppqID = "bwch_cancer";
            }
            else
            {
                ppqID = "bwch_general";
            }

            pID = GetPhenotipsPatientID(id).Result;

            if (pID != "" && pID != null)
            {
                if (CheckPPQExists(id, ppqType).Result)                
                {
                    
                    DateTime DOB = patient.DOB.GetValueOrDefault();
                    int yob = DOB.Year;
                    int mob = DOB.Month;
                    int dob = DOB.Day;

                    apiURL = apiURLBase + ":443/rest/questionnaire-scheduler/search";
                    var options = new RestClientOptions(apiURL);
                    var client = new RestClient(options);
                    var request = new RestRequest("");
                    request.AddHeader("authorization", $"Basic {authKey}");
                    request.AddHeader("X-Gene42-Secret", apiKey);
                    string apiCall = "{\"search\":{\"questionnaireId\":\"" + $"{ppqID}" + "\",\"mrn\":\"" + $"{patient.CGU_No}" + "\",\"orderBy\":\"\",\"orderDir\":\"\",\"offset\":0,\"limit\":25}}";

                    request.AddJsonBody(apiCall, false);
                    var response = await client.PostAsync(request);

                    if (response.Content.Contains("{\"scheduleRequests\""))
                    {
                        dynamic dynJson = JsonConvert.DeserializeObject(response.Content);
                        result = dynJson.scheduleRequests[0].questionnaireQR;
                    }                 
                }                
            }
            return result;
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

        public async Task<string> GetAllHPOTerms(string userName)
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
                    _hpo.AddHPOTermToDatabase(hpoID, hpoName.Replace("'", "''"), userName, _config);

                    int hpocodeid = _hpo.GetHPOTermByTermCode(hpoID).ID;

                    foreach (var synonym in item.synonyms)
                    {
                        string syn = synonym.ToString();
                        syn = syn.Replace("{", "").Replace("}", "");
                        _hpo.AddHPOSynonymToDatabase(hpocodeid, syn.Replace("'", "''"), userName, _config);
                    }
                }
            }

            //return RedirectToAction("Index", "Home");
            return "success";
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



        public async Task<string> SearchSCRPatients(string firstName, string lastName) 
        {
            //apiURL = apiURLBase + $":443/rest/patients/eid/XXXXX";
            apiURL = "";
            var options = new RestClientOptions(apiURL);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", "Basic " + authKey);
            request.AddHeader("X-Gene42-Secret", apiKey);
            var response = await client.GetAsync(request);

            

            return response.Content.ToString();
        }
    }
}
