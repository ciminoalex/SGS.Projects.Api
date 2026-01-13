using System.Text;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Newtonsoft.Json;
using SGS.Projects.Api.Models;

namespace SGS.Projects.Api.Services
{
    public class SapB1ServiceLayerService : ISapB1ServiceLayerService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SapB1ServiceLayerService> _logger;
        private readonly IDbOdbcService _dbOdbcService;
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICredentialStore _credentialStore;
        private string? _sessionId;
        private static readonly SemaphoreSlim _loginLock = new SemaphoreSlim(1, 1);

        public SapB1ServiceLayerService(HttpClient httpClient, IConfiguration configuration, ILogger<SapB1ServiceLayerService> logger, IDbOdbcService dbOdbcService, IMemoryCache memoryCache, IHttpContextAccessor httpContextAccessor, ICredentialStore credentialStore)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _dbOdbcService = dbOdbcService;
            _memoryCache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
            _credentialStore = credentialStore;
            
            var baseUrl = configuration["SapB1:ServiceLayerUrl"] 
                ?? throw new ArgumentNullException(nameof(configuration), "SapB1:ServiceLayerUrl not found");
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<string> GetSessionIdAsync()
        {
            var userKey = GetCurrentTokenJti();
            if (string.IsNullOrEmpty(userKey))
            {
                throw new InvalidOperationException("Richiesta non autenticata o token privo di JTI");
            }

            var (cookieKey, sessionKey) = GetCacheKeys(userKey);

            // Prova a leggere dalla cache per utente
            if (_memoryCache.TryGetValue(cookieKey, out string? cachedCookie) &&
                _memoryCache.TryGetValue(sessionKey, out string? cachedSessionId) &&
                !string.IsNullOrEmpty(cachedCookie) && !string.IsNullOrEmpty(cachedSessionId))
            {
                // Applica il cookie all'HttpClient corrente
                _httpClient.DefaultRequestHeaders.Remove("Cookie");
                _httpClient.DefaultRequestHeaders.Add("Cookie", cachedCookie);
                _sessionId = cachedSessionId;

                if (await IsSessionValidAsync())
                {
                    return _sessionId!;
                }
                else
                {
                    _logger.LogWarning("Cached session expired for user {UserKey}, attempting to reconnect", userKey);
                    InvalidateSessionLocal(userKey);
                }
            }

            // Evita login concorrenti
            await _loginLock.WaitAsync();
            try
            {
                // Ricontrolla perché un altro thread potrebbe aver effettuato il login
                if (_memoryCache.TryGetValue(cookieKey, out cachedCookie) &&
                    _memoryCache.TryGetValue(sessionKey, out string? cachedSessionId2) &&
                    !string.IsNullOrEmpty(cachedCookie) && !string.IsNullOrEmpty(cachedSessionId2))
                {
                    _httpClient.DefaultRequestHeaders.Remove("Cookie");
                    _httpClient.DefaultRequestHeaders.Add("Cookie", cachedCookie);
                    _sessionId = cachedSessionId2;
                    return _sessionId!;
                }

                // Effettua il login
                return await LoginAsync(userKey);
            }
            finally
            {
                _loginLock.Release();
            }
        }

        private async Task<bool> IsSessionValidAsync()
        {
            try
            {
                // Prova a fare una chiamata semplice per verificare se la sessione è valida
                var response = await _httpClient.GetAsync("UserFields");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> LoginAsync(string userKey)
        {
            try
            {
                var credsOpt = _credentialStore.GetCredentials(userKey);
                if (credsOpt == null)
                {
                    throw new InvalidOperationException("Credenziali non trovate per il token");
                }
                var (userName, password) = credsOpt.Value;
                var loginData = new
                {
                    CompanyDB = _configuration["SapB1:CompanyDB"],
                    UserName = userName,
                    Password = password
                };

                var json = JsonConvert.SerializeObject(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("Login", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    _sessionId = loginResponse?.SessionId;
                    
                    if (!string.IsNullOrEmpty(_sessionId))
                    {
                        // Recupera anche eventuale ROUTEID dai Set-Cookie (se presente)
                        var setCookieHeaders = response.Headers.TryGetValues("Set-Cookie", out var values)
                            ? string.Join(" ", values)
                            : string.Empty;
                        string routeId = ExtractRouteId(setCookieHeaders);

                        var cookieString = string.IsNullOrEmpty(routeId)
                            ? $"B1SESSION={_sessionId}"
                            : $"B1SESSION={_sessionId}; ROUTEID={routeId}";

                        _httpClient.DefaultRequestHeaders.Remove("Cookie");
                        _httpClient.DefaultRequestHeaders.Add("Cookie", cookieString);

                        CacheSession(userKey, cookieString, _sessionId);
                        _logger.LogInformation("Successfully logged in to SAP B1 Service Layer for user {UserKey}", userKey);
                        return _sessionId;
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to login to SAP B1 Service Layer. Status: {response.StatusCode}, Error: {errorContent}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in to SAP B1 Service Layer for user {UserKey}", userKey);
                throw;
            }
        }

        private async Task<HttpResponseMessage> ExecuteWithRetryAsync(Func<Task<HttpResponseMessage>> apiCall)
        {
            try
            {
                var response = await apiCall();
                
                // Se la risposta indica una sessione scaduta, prova a riconnettersi
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    var userKey = GetCurrentTokenJti();
                    _logger.LogWarning("API call failed with unauthorized status, attempting to reconnect for user {UserKey}", userKey);
                    if (!string.IsNullOrEmpty(userKey))
                    {
                        InvalidateSessionLocal(userKey);
                        await GetSessionIdAsync();
                        response = await apiCall();
                    }
                    
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing API call with retry");
                throw;
            }
        }

        private void CacheSession(string userKey, string cookieString, string sessionId)
        {
            var defaultMinutes = 25; // fallback in caso non configurato
            var ttlMinutesConfig = _configuration["SapB1:SessionTimeoutMinutes"];
            if (!int.TryParse(ttlMinutesConfig, out int ttlMinutes))
            {
                ttlMinutes = defaultMinutes;
            }

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(ttlMinutes)
            };

            var (cookieKey, sessionKey) = GetCacheKeys(userKey);
            _memoryCache.Set(cookieKey, cookieString, options);
            _memoryCache.Set(sessionKey, sessionId, options);
        }

        private void InvalidateSessionLocal(string userKey)
        {
            _sessionId = null;
            _httpClient.DefaultRequestHeaders.Remove("Cookie");
            var (cookieKey, sessionKey) = GetCacheKeys(userKey);
            _memoryCache.Remove(cookieKey);
            _memoryCache.Remove(sessionKey);
        }

        private string ExtractRouteId(string setCookie)
        {
            if (string.IsNullOrEmpty(setCookie)) return string.Empty;
            // Cerca ROUTEID=xxxx; o ROUTEID=xxxx,
            var token = "ROUTEID=";
            var idx = setCookie.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return string.Empty;
            idx += token.Length;
            var endIdx = setCookie.IndexOfAny(new[] { ';', ',', ' ' }, idx);
            if (endIdx < 0) endIdx = setCookie.Length;
            return setCookie.Substring(idx, endIdx - idx);
        }

        public async Task<List<Timesheet>> GetTimesheetsAsync()
        {
            try
            {
                await GetSessionIdAsync();

                _httpClient.DefaultRequestHeaders.Add("Prefer", "odata.maxpagesize=50000");
                var response = await ExecuteWithRetryAsync(() => _httpClient.GetAsync("SGS_PRJ_OTMS"));
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    var timesheets = new List<Timesheet>();
                    if (result?.value != null)
                    {
                        foreach (var item in result.value)
                        {
                            timesheets.Add(MapSapResponseToTimesheet(item!));
                        }
                    }
                    
                    return timesheets;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to get timesheets. Status: {response.StatusCode}, Error: {errorContent}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timesheets from SAP B1 Service Layer");
                throw;
            }
        }

        public async Task<Timesheet?> GetTimesheetAsync(string code)
        {
            try
            {
                await GetSessionIdAsync();

                _httpClient.DefaultRequestHeaders.Add("Prefer", "odata.maxpagesize=50000");
                var response = await ExecuteWithRetryAsync(() => _httpClient.GetAsync($"SGS_PRJ_OTMS('{code}')"));
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    return MapSapResponseToTimesheet(result);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to get timesheet with code {code}. Status: {response.StatusCode}, Error: {errorContent}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timesheet with code {Code} from SAP B1 Service Layer", code);
                throw;
            }
        }

        public async Task<Timesheet> CreateTimesheetAsync(TimesheetCreateRequest request)
        {
            try
            {
                await GetSessionIdAsync();

                // Get the next Code value from the database
                var nextCode = await _dbOdbcService.GetNextTimesheetCodeAsync();

                var timesheetData = new
                {
                    Code = nextCode,
                    U_Date = request.Date.ToString("yyyy-MM-dd"),
                    U_ResId = request.ResId,
                    U_CardCode = request.CardCode,
                    U_CardName = request.CardName,
                    U_RefId = request.RefId,
                    U_RefData = request.RefData,
                    U_Project = request.Project,
                    U_ProjectName = request.ProjectName,
                    U_SubProject = request.SubProject,
                    U_Activity = request.Activity,
                    U_ActivityId = request.ActivityId,
                    U_SubActivity = request.SubActivity,
                    U_ActivityName = request.ActivityName,
                    U_TimeStart = request.TimeStart,
                    U_TimeEnd = request.TimeEnd,
                    U_TimePa = request.TimePa,
                    U_TimeNF = request.TimeNF,
                    U_TimeNrPa = request.TimeNrPa,
                    U_TimeNrNF = request.TimeNrNF,
                    U_TimeNrTot = request.TimeNrTot,
                    U_TimeNrNet = request.TimeNrTot,
                    U_DescExt = request.DescExt,
                    U_DescInt = request.DescInt,
                    U_Status = "Inserito"
                };

                var json = JsonConvert.SerializeObject(timesheetData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await ExecuteWithRetryAsync(() => _httpClient.PostAsync("SGS_PRJ_OTMS", content));
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    return MapSapResponseToTimesheet(result);
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
                if (!string.IsNullOrEmpty(request.ResId))
                    timesheetData["U_ResId"] = request.ResId;
                if (!string.IsNullOrEmpty(request.CardCode))
                    timesheetData["U_CardCode"] = request.CardCode;
                if (!string.IsNullOrEmpty(request.CardName))
                    timesheetData["U_CardName"] = request.CardName;
                if (!string.IsNullOrEmpty(request.RefId))
                    timesheetData["U_RefId"] = request.RefId;
                if (!string.IsNullOrEmpty(request.RefData))
                    timesheetData["U_RefData"] = request.RefData;
                if (!string.IsNullOrEmpty(request.Project))
                    timesheetData["U_Project"] = request.Project;
                if (!string.IsNullOrEmpty(request.ProjectName))
                    timesheetData["U_ProjectName"] = request.ProjectName;
                if (!string.IsNullOrEmpty(request.SubProject))
                    timesheetData["U_SubProject"] = request.SubProject;
                if (!string.IsNullOrEmpty(request.Activity))
                    timesheetData["U_Activity"] = request.Activity;
                if (!string.IsNullOrEmpty(request.ActivityId))
                    timesheetData["U_ActivityId"] = request.ActivityId;
                if (!string.IsNullOrEmpty(request.SubActivity))
                    timesheetData["U_SubActivity"] = request.SubActivity;
                if (!string.IsNullOrEmpty(request.ActivityName))
                    timesheetData["U_ActivityName"] = request.ActivityName;
                if (request.TimeStart.HasValue)
                    timesheetData["U_TimeStart"] = request.TimeStart.Value;
                if (request.TimeEnd.HasValue)
                    timesheetData["U_TimeEnd"] = request.TimeEnd.Value;
                if (request.TimePa.HasValue)
                    timesheetData["U_TimePa"] = request.TimePa.Value;
                if (request.TimeNF.HasValue)
                    timesheetData["U_TimeNF"] = request.TimeNF.Value;
                if (request.TimeNrPa.HasValue)
                    timesheetData["U_TimeNrPa"] = request.TimeNrPa.Value;
                if (request.TimeNrNF.HasValue)
                    timesheetData["U_TimeNrNF"] = request.TimeNrNF.Value;
                if (request.TimeNrTot.HasValue){
                    timesheetData["U_TimeNrTot"] = request.TimeNrTot.Value;
                    timesheetData["U_TimeNrNet"] = request.TimeNrTot.Value;
                }
                if (!string.IsNullOrEmpty(request.DescExt))
                    timesheetData["U_DescExt"] = request.DescExt;
                if (!string.IsNullOrEmpty(request.DescInt))
                    timesheetData["U_DescInt"] = request.DescInt;
                if (!string.IsNullOrEmpty(request.Status))
                    timesheetData["U_Status"] = request.Status;

                var json = JsonConvert.SerializeObject(timesheetData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // First get the Code from DocEntry
                var existingTimesheet = await GetTimesheetByDocEntryAsync(request.DocEntry);
                if (existingTimesheet?.Code == null)
                {
                    throw new Exception($"Timesheet with DocEntry {request.DocEntry} not found");
                }
                
                var response = await ExecuteWithRetryAsync(() => _httpClient.PatchAsync($"SGS_PRJ_OTMS('{existingTimesheet.Code}')", content));
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    return MapSapResponseToTimesheet(result);
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

        public async Task<bool> DeleteTimesheetAsync(string code)
        {
            try
            {
                await GetSessionIdAsync();

                var response = await ExecuteWithRetryAsync(() => _httpClient.DeleteAsync($"SGS_PRJ_OTMS('{code}')"));
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to delete timesheet with Code {Code}. Status: {StatusCode}, Error: {Error}", 
                    code, response.StatusCode, errorContent);
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting timesheet with Code {Code} from SAP B1 Service Layer", code);
                throw;
            }
        }

        private Timesheet MapSapResponseToTimesheet(dynamic sapResponse)
        {
            return new Timesheet
            {
                DocEntry = sapResponse?.DocEntry,
                Code = sapResponse?.Code,
                ResId = sapResponse?.U_ResId,
                CardCode = sapResponse?.U_CardCode,
                CardName = sapResponse?.U_CardName,
                RefId = sapResponse?.U_RefId,
                RefData = sapResponse?.U_RefData,
                Project = sapResponse?.U_Project,
                ProjectName = sapResponse?.U_ProjectName,
                SubProject = sapResponse?.U_SubProject,
                Activity = sapResponse?.U_Activity,
                ActivityId = sapResponse?.U_ActivityId,
                SubActivity = sapResponse?.U_SubActivity,
                ActivityName = sapResponse?.U_ActivityName,
                Date = sapResponse?.U_Date != null ? DateTime.Parse(sapResponse.U_Date.ToString()) : DateTime.MinValue,
                TimeStart = ConvertHmsToHundreds(sapResponse?.U_TimeStart),
                TimeEnd = ConvertHmsToHundreds(sapResponse?.U_TimeEnd),
                TimePa = ConvertHmsToHundreds(sapResponse?.U_TimePa),
                TimeNF = sapResponse?.U_TimeNF,
                TimeNrPa = sapResponse?.U_TimeNrPa,
                TimeNrNF = sapResponse?.U_TimeNrNF,
                TimeNrTot = sapResponse?.U_TimeNrTot,
                TimeNrNet = sapResponse?.U_TimeNrNet,
                DescExt = sapResponse?.U_DescExt,
                DescInt = sapResponse?.U_DescInt,
                Status = sapResponse?.U_Status
            };
        }

        private int? ConvertHmsToHundreds(dynamic timeValue)
        {
            if (timeValue == null)
            {
                return null;
            }

            var raw = timeValue.ToString();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            if (TimeSpan.TryParse(raw, out TimeSpan ts))
            {
                return (ts.Hours * 100) + ts.Minutes;
            }

            // Fallback: attempt to parse HH:mm or HHmm formats
            var cleaned = raw.Trim();
            if (cleaned.Contains(':'))
            {
                var parts = cleaned.Split(':');
                if (parts.Length >= 2)
                {
                    int h = 0, m = 0;
                    if (int.TryParse(parts[0], out h) && int.TryParse(parts[1], out m))
                    {
                        return (h * 100) + m;
                    }
                }
            }

            if (int.TryParse(cleaned, out int asInt))
            {
                return asInt;
            }

            return null;
        }

        private async Task<Timesheet?> GetTimesheetByDocEntryAsync(int docEntry)
        {
            try
            {
                await GetSessionIdAsync();

                _httpClient.DefaultRequestHeaders.Add("Prefer", "odata.maxpagesize=50000");
                var response = await ExecuteWithRetryAsync(() => _httpClient.GetAsync($"SGS_PRJ_OTMS?$filter=DocEntry eq {docEntry}"));
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    if (result?.value != null && result.value.Count > 0)
                    {
                        return MapSapResponseToTimesheet(result.value[0]!);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timesheet by DocEntry {DocEntry} from SAP B1 Service Layer", docEntry);
                throw;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        private string? GetCurrentTokenJti()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirstValue("jti") ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private (string cookieKey, string sessionKey) GetCacheKeys(string userKey)
        {
            return ($"SapB1:SessionCookie:{userKey}", $"SapB1:SessionId:{userKey}");
        }
    }
}
