using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using AdminX.Data;
using AdminX.Meta;
using AdminX.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;

namespace ClinicX.Controllers
{
    public class APIController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly AdminContext _aContext;
        private readonly DocumentContext _docContext;
        private readonly IConfiguration _config;
        private readonly IPatientData _patientData;
        private readonly IConstantsData _constants;
        private string apiURLBase;
        private string apiURL;
        private string authKey;
        private string apiKey;
        private readonly ICRUD _crud;

        public APIController(ClinicalContext clinContext, AdminContext aContext, DocumentContext docContext, IConfiguration config)
        {
            _clinContext = clinContext;
            _aContext = aContext;
            _docContext = docContext;
            _patientData = new PatientData(_clinContext);
            _constants = new ConstantsData(_docContext);
            apiURLBase = _constants.GetConstant("PhenotipsURL", 1).Trim();
            authKey = _constants.GetConstant("PhenotipsAPIAuthKey", 1).Trim();
            apiKey = _constants.GetConstant("PhenotipsAPIAuthKey", 2).Trim();
            _config = config;
            _crud = new CRUD(_config);
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
            var response = await client.GetAsync(request); //secret key is broken so we can't currently get past this line

            if (CheckResponseValid(response.Content)) 
            {
                dynamic dynJson = JsonConvert.DeserializeObject(response.Content);
                phenotipsID = dynJson.family_id;
            }

            return phenotipsID;
        }

        public async Task<IActionResult> PushPtToPhenotips(int id)
        {
            var patient = _patientData.GetPatientDetails(id);
            bool isSuccess = false;
            string sMessage = "";

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
                    isSuccess = true;
                    sMessage = "Push to Phenotips successful";

                    string ptID = await GetPhenotipsPatientID(patient.MPI);
                    //_crud.AddPatientToPhenotipsMirrorTable(ptID, patient.MPI, patient.CGU_No, patient.FIRSTNAME, patient.LASTNAME, patient.DOB.GetValueOrDefault());
                }
                else
                {
                    sMessage = "Push to Phenotips failed :(";
                }
            }
            else
            {
                sMessage = "Patient already exists in Phenotips!";
            }
            return RedirectToAction("PatientDetails", "Patient", new { id = patient.MPI, success = isSuccess, message = sMessage });
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

    }
}
