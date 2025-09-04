using System.Text;
using Newtonsoft.Json;
using SGS.Projects.Api.Models;

namespace SGS.Projects.Api.Services
{
    public class SapB1ServiceLayerService : ISapB1ServiceLayerService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SapB1ServiceLayerService> _logger;
        private string? _sessionId;

        public SapB1ServiceLayerService(HttpClient httpClient, IConfiguration configuration, ILogger<SapB1ServiceLayerService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            
            var baseUrl = configuration["SapB1:ServiceLayerUrl"] 
                ?? throw new ArgumentNullException(nameof(configuration), "SapB1:ServiceLayerUrl not found");
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<string> GetSessionIdAsync()
        {
            if (!string.IsNullOrEmpty(_sessionId))
                return _sessionId;

            try
            {
                var loginData = new
                {
                    CompanyDB = _configuration["SapB1:CompanyDB"],
                    UserName = _configuration["SapB1:UserName"],
                    Password = _configuration["SapB1:Password"]
                };

                var json = JsonConvert.SerializeObject(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/Login", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    _sessionId = loginResponse?.SessionId;
                    
                    if (!string.IsNullOrEmpty(_sessionId))
                    {
                        _httpClient.DefaultRequestHeaders.Add("Cookie", $"B1SESSION={_sessionId}");
                        return _sessionId;
                    }
                }

                throw new Exception($"Failed to login to SAP B1 Service Layer. Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session ID from SAP B1 Service Layer");
                throw;
            }
        }

        public async Task<Timesheet> CreateTimesheetAsync(TimesheetCreateRequest request)
        {
            try
            {
                await GetSessionIdAsync();

                var timesheetData = new
                {
                    U_Date = request.Date.ToString("yyyy-MM-dd"),
                    U_EmployeeId = request.EmployeeId,
                    U_ProjectId = request.ProjectId,
                    U_ActivityId = request.ActivityId,
                    U_Hours = request.Hours,
                    U_Description = request.Description ?? string.Empty,
                    U_Status = "Draft"
                };

                var json = JsonConvert.SerializeObject(timesheetData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/UDO/TIMESHEET", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    return new Timesheet
                    {
                        DocEntry = result?.DocEntry,
                        DocNum = result?.DocNum?.ToString() ?? string.Empty,
                        Date = request.Date,
                        EmployeeId = request.EmployeeId,
                        ProjectId = request.ProjectId,
                        ActivityId = request.ActivityId,
                        Hours = request.Hours,
                        Description = request.Description,
                        Status = "Draft",
                        CreatedDate = DateTime.Now
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create timesheet. Status: {response.StatusCode}, Error: {errorContent}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating timesheet in SAP B1 Service Layer");
                throw;
            }
        }

        public async Task<Timesheet> UpdateTimesheetAsync(TimesheetUpdateRequest request)
        {
            try
            {
                await GetSessionIdAsync();

                var timesheetData = new Dictionary<string, object>();
                
                if (request.Date.HasValue)
                    timesheetData["U_Date"] = request.Date.Value.ToString("yyyy-MM-dd");
                if (!string.IsNullOrEmpty(request.EmployeeId))
                    timesheetData["U_EmployeeId"] = request.EmployeeId;
                if (!string.IsNullOrEmpty(request.ProjectId))
                    timesheetData["U_ProjectId"] = request.ProjectId;
                if (!string.IsNullOrEmpty(request.ActivityId))
                    timesheetData["U_ActivityId"] = request.ActivityId;
                if (request.Hours.HasValue)
                    timesheetData["U_Hours"] = request.Hours.Value;
                if (!string.IsNullOrEmpty(request.Description))
                    timesheetData["U_Description"] = request.Description;

                var json = JsonConvert.SerializeObject(timesheetData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync($"/UDO/TIMESHEET({request.DocEntry})", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    return new Timesheet
                    {
                        DocEntry = request.DocEntry,
                        DocNum = result?.DocNum?.ToString() ?? string.Empty,
                        Date = request.Date ?? DateTime.MinValue,
                        EmployeeId = request.EmployeeId ?? string.Empty,
                        ProjectId = request.ProjectId ?? string.Empty,
                        ActivityId = request.ActivityId ?? string.Empty,
                        Hours = request.Hours ?? 0,
                        Description = request.Description,
                        ModifiedDate = DateTime.Now
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to update timesheet. Status: {response.StatusCode}, Error: {errorContent}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating timesheet with DocEntry {DocEntry} in SAP B1 Service Layer", request.DocEntry);
                throw;
            }
        }

        public async Task<bool> DeleteTimesheetAsync(int docEntry)
        {
            try
            {
                await GetSessionIdAsync();

                var response = await _httpClient.DeleteAsync($"/UDO/TIMESHEET({docEntry})");
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to delete timesheet with DocEntry {DocEntry}. Status: {StatusCode}, Error: {Error}", 
                    docEntry, response.StatusCode, errorContent);
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting timesheet with DocEntry {DocEntry} from SAP B1 Service Layer", docEntry);
                throw;
            }
        }
    }
}
