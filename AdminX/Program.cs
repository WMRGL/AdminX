using AdminX.Controllers;
using AdminX.Data;
using AdminX.Meta;
using APIControllers.Controllers;
using APIControllers.Data;
using Audit.Core;
using Audit.Core.Providers; 
using Audit.EntityFramework;
using Audit.EntityFramework.Providers;
using Audit.Mvc;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Snippets;

var builder = WebApplication.CreateBuilder(args);
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("secrets.json", optional: false)
    .Build();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ClinicalContext>(options => options.UseSqlServer(config.GetConnectionString("ConString")));
builder.Services.AddDbContext<AdminContext>(options => options.UseSqlServer(config.GetConnectionString("ConString")));
builder.Services.AddDbContext<LabContext>(options => options.UseSqlServer(config.GetConnectionString("ConStringLab")));
builder.Services.AddDbContext<APIContext>(options => options.UseSqlServer(config.GetConnectionString("ConString")));
builder.Services.AddDbContext<KlaxonContext>(options => options.UseSqlServer(config.GetConnectionString("ConStringEpic")));
builder.Services.AddDbContext<DocumentContext>(options => options.UseSqlServer(config.GetConnectionString("ConString")));
builder.Services.AddDbContext<DQContext>(options => options.UseSqlServer(config.GetConnectionString("DQLab")));

builder.Services.AddScoped<ICaseloadDataAsync, CaseloadDataAsync>();
builder.Services.AddScoped<IStaffUserDataAsync, StaffUserDataAsync>();
builder.Services.AddScoped<INotificationDataAsync, NotificationDataAsync>();
builder.Services.AddSingleton<IVersionData, VersionData>();
builder.Services.AddScoped<IPatientDataAsync, PatientDataAsync>();
builder.Services.AddScoped<IReferralDataAsync, ReferralDataAsync>();
builder.Services.AddScoped<IActivityDataAsync, ActivityDataAsync>();
builder.Services.AddScoped<IClinicDataAsync, ClinicDataAsync>();
builder.Services.AddScoped<IOutcomeDataAsync, OutcomeDataAsync>();
builder.Services.AddScoped<IClinicVenueDataAsync, ClinicVenueDataAsync>();
builder.Services.AddScoped<IActivityTypeDataAsync, ActivityTypeDataAsync>();
builder.Services.AddScoped<IDiseaseDataAsync, DiseaseDataAsync>();
builder.Services.AddScoped<IDictatedLetterDataAsync, DictatedLetterDataAsync>();
builder.Services.AddScoped<IExternalClinicianDataAsync, ExternalClinicianDataAsync>();
builder.Services.AddScoped<IExternalFacilityDataAsync, ExternalFacilityDataAsync>();
builder.Services.AddScoped<IConstantsDataAsync, ConstantsDataAsync>();
builder.Services.AddScoped<ILabDataAsync, LabReportDataAsync>();
builder.Services.AddScoped<IDocumentsDataAsync, DocumentsDataAsync>();
builder.Services.AddScoped<IDiaryDataAsync, DiaryDataAsync>();
builder.Services.AddScoped<ILeafletDataAsync, LeafletDataAsync>();
builder.Services.AddScoped<ISupervisorDataAsync, SupervisorDataAsync>();
builder.Services.AddScoped<IAreaNamesDataAsync, AreaNamesDataAsync>();
builder.Services.AddScoped<IPathwayDataAsync, PathwayDataAsync>();
builder.Services.AddScoped<IRelativeDataAsync, RelativeDataAsync>();
builder.Services.AddScoped<IRelativeDiaryDataAsync, RelativeDiaryDataAsync>();
builder.Services.AddScoped<IRelativeDiagnosisDataAsync, RelativeDiagnosisDataAsync>();
builder.Services.AddScoped<IAlertDataAsync, AlertDataAsync>();
builder.Services.AddScoped<IAgeCalculator, AgeCalculator>();
builder.Services.AddScoped<ITriageDataAsync, TriageDataAsync>();
builder.Services.AddScoped<IPhenotipsMirrorDataAsync, PhenotipsMirrorDataAsync>();
builder.Services.AddScoped<IPatientSearchDataAsync, PatientSearchDataAsync>();
builder.Services.AddScoped<IRiskDataAsync, RiskDataAsync>();
builder.Services.AddScoped<ISurveillanceDataAsync, SurveillanceDataAsync>();
builder.Services.AddScoped<IStudyDataAsync, StudyDataAsync>();
builder.Services.AddScoped<ITestEligibilityDataAsync, TestEligibilityDataAsync>();
builder.Services.AddScoped<IWaitingListDataAsync, WaitingListDataAsync>();
builder.Services.AddScoped<IFHSummaryDataAsync, FHSummaryDataAsync>();
builder.Services.AddScoped<IReviewDataAsync, ReviewDataAsync>();
builder.Services.AddScoped<IICPActionDataAsync, ICPActionDataAsync>();
builder.Services.AddScoped<ITitleDataAsync, TitleDataAsync>();
builder.Services.AddScoped<IPriorityDataAsync, PriorityDataAsync>();
builder.Services.AddScoped<IAppointmentDataAsync, AppointmentDataAsync>();
builder.Services.AddScoped<IAlertTypeDataAsync, AlertTypeDataAsync>();
builder.Services.AddScoped<IDiaryActionDataAsync, DiaryActionDataAsync>();
builder.Services.AddScoped<IDictatedLettersReportDataAsync, DictatedLettersReportDataAsync>();
builder.Services.AddScoped<IHSDataAsync, HSDataAsync>();
builder.Services.AddScoped<IMergeHistoryDataAsync, MergeHistoryDataAsync>();
builder.Services.AddScoped<INewPatientSearchDataAsync, NewPatientSearchDataAsync>();
builder.Services.AddScoped<ILanguageDataAsync, LanguageDataAsync>();
builder.Services.AddScoped<IPatientAlertDataAsync, PatientAlertDataAsync>();
builder.Services.AddScoped<ICityDataAsync, CityDataAsync>();
builder.Services.AddScoped<IGenderDataAsync, GenderDataAsync>();
builder.Services.AddScoped<IGenderIdentityDataAsync, GenderIdentityDataAsync>();
builder.Services.AddScoped<IReferralStagingDataAsync, ReferralStagingDataAsync>();
builder.Services.AddScoped<IEpicPatientReferenceDataAsync, EpicPatientReferenceDataAsync>();
builder.Services.AddScoped<IPedigreeDataAsync, PedigreeDataAsync>();
builder.Services.AddScoped<IEthnicityDataAsync, EthnicityDataAsync>();
builder.Services.AddScoped<IAdminStatusDataAsync, AdminStatusDataAsync>();
builder.Services.AddScoped<IListDiseaseDataAsync, ListDiseaseDataAsync>();
builder.Services.AddScoped<ICliniciansClinicDataAsync, CliniciansClinicDataAsync>();
builder.Services.AddScoped<IRefReasonDataAsync, RefReasonDataAsync>();
builder.Services.AddScoped<IDocKindsDataAsync, DocKindsDataAsync>();
builder.Services.AddScoped<IEpicReferralReferenceDataAsync, EpicReferralReferenceDataAsync>();

builder.Services.AddScoped<IAuditServiceAsync, AuditServiceAsync>();
builder.Services.AddScoped<ICRUD, CRUD>();
builder.Services.AddScoped<LetterController>();
builder.Services.AddScoped<APIController>();
builder.Services.AddScoped<HSController>();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
   .AddCookie(options =>
   {
       options.LoginPath = "/Login/UserLogin";
   });

builder.Services.AddScoped<IAppointmentDQData, AppointmentDQData>();
builder.Services.AddScoped<IPatientDQData, PatientDQData>();
builder.Services.AddScoped<IGenderIdentityData, GenderIdentityData>();

builder.Services.AddControllersWithViews(options =>
{
    // Adds auditing to ALL controllers automatically
    options.Filters.Add(new Audit.Mvc.AuditAttribute()
    {
        IncludeHeaders = true,
        IncludeRequestBody = true,
        IncludeModel = true, 
        EventTypeName = "{verb} {controller}/{action}" 
    });
});

builder.Services.AddMvc();
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("secrets.json");

// this code is all for the shared authentication
var directoryInfo = new DirectoryInfo(@"C:\Websites\Authentication");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(directoryInfo)
    .SetApplicationName("GeneticsWebAppHome");

builder.Services.ConfigureApplicationCookie(options => {
    options.Cookie.Name = ".AspNet.GeneticsWebAppHome";
    options.Cookie.Path = "/";
});
//
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=Login}/{action=UserLogin}/{id?}");
    pattern: "{controller=Home}/{action=Index}");


Audit.Core.Configuration.Setup()
    .UseConditional(c => c
        // 1. HANDLE DATABASE CHANGES (Insert/Update/Delete triggered by SaveChanges)
        .When(ev => ev is AuditEventEntityFramework,
            new EntityFrameworkDataProvider(ef => ef
                .UseDbContext<AdminX.Data.AdminContext>()
                .AuditTypeMapper(t => typeof(AdminX.Models.AuditLog))
                .AuditEntityAction<AdminX.Models.AuditLog>((ev, entry, audit) =>
                {
                    audit.UserId = ev.Environment.UserName;
                    audit.EventType = ev.EventType;
                    audit.DateTime = DateTime.UtcNow;
                    audit.TableName = entry.Table;
                    audit.Action = entry.Action;
                    audit.OldValues = entry.ColumnValues.ContainsKey("Old") ? System.Text.Json.JsonSerializer.Serialize(entry.ColumnValues["Old"]) : null;
                    audit.NewValues = entry.ColumnValues.ContainsKey("New") ? System.Text.Json.JsonSerializer.Serialize(entry.ColumnValues["New"]) : null;
                    audit.IpAddress = ev.Environment.CustomFields.ContainsKey("IpAddress") ? ev.Environment.CustomFields["IpAddress"]?.ToString() : null;
                })
                .IgnoreMatchedProperties(true)
            )
        )
        // 2. HANDLE MVC ACTIONS (Search, View, Get triggered by Controller)
        .When(ev => ev is AuditEventMvcAction,
            new DynamicAsyncDataProvider(d => d
                .OnInsert(async ev =>
                {
                    // We need to manually resolve the DbContext to save this "View" event
                    using (var scope = app.Services.CreateScope())
                    {
                        var ctx = scope.ServiceProvider.GetRequiredService<AdminX.Data.AdminContext>();
                        var mvcData = ev.GetMvcAuditAction(); // Helper to get MVC specific data

                        var log = new AdminX.Models.AuditLog
                        {
                            UserId = ev.Environment.UserName ?? "Anonymous",
                            EventType = ev.EventType, 
                            DateTime = DateTime.UtcNow,
                            TableName = mvcData.ControllerName, // Treat Controller as the "Table"
                            Action = mvcData.ActionName,        // Treat Action as the "Action" (Search/Index)

                            // Save the Search Parameters (e.g. query strings) into NewValues
                            NewValues = mvcData.ActionParameters != null
                                        ? System.Text.Json.JsonSerializer.Serialize(mvcData.ActionParameters)
                                        : null,

                            IpAddress = ev.Environment.CustomFields.ContainsKey("IpAddress")
                                        ? ev.Environment.CustomFields["IpAddress"]?.ToString()
                                        : null
                        };

                        ctx.AuditLogs.Add(log);
                        await ctx.SaveChangesAsync();
                    }
                })
            )
        )
    );



app.Run();
