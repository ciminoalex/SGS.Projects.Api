using System.Data.Odbc;
using SGS.Projects.Api.Models;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SGS.Projects.Api.Services
{
    public class DbOdbcService : IDbOdbcService
    {
        private readonly string _connectionString;
        private readonly string _schema;
        private readonly ILogger<DbOdbcService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DbOdbcService(IConfiguration configuration, ILogger<DbOdbcService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _connectionString = configuration.GetConnectionString("DefaultDatabase") 
                ?? throw new ArgumentNullException(nameof(configuration), "DefaultDatabase connection string not found");

            _schema = configuration["SapB1:CompanyDB"]
                ?? throw new ArgumentNullException(nameof(configuration), "Schema not defined");


            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<OdbcConnection> CreateOpenConnectionAsync()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var connection = new OdbcConnection(_connectionString);
            try
            {
                _logger.LogDebug("Opening ODBC connection to schema {Schema}", _schema);
                await connection.OpenAsync();
                stopwatch.Stop();
                _logger.LogInformation("ODBC connection opened in {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
                return connection;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed to open ODBC connection after {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
                connection.Dispose();
                throw;
            }
        }

        public async Task<IEnumerable<Timesheet>> GetTimesheetsAsync()
        {
            var timesheets = new List<Timesheet>();
            
            try
            {
                using var connection = await CreateOpenConnectionAsync();
                
                var query = $@"
                    SELECT 
                        ""DocEntry"",
                        ""Code"",
                        ""U_ResId"" AS ""ResId"",
                        ""U_CardCode"" AS ""CardCode"",
                        ""U_CardName"" AS ""CardName"",
                        ""U_RefId"" AS ""RefId"",
                        ""U_RefData"" AS ""RefData"",
                        ""U_Project"" AS ""Project"",
                        ""U_ProjectName"" AS ""ProjectName"",
                        ""U_SubProject"" AS ""SubProject"",
                        ""U_Activity"" AS ""Activity"",
                        ""U_ActivityId"" AS ""ActivityId"",
                        ""U_SubActivity"" AS ""SubActivity"",
                        ""U_ActivityName"" AS ""ActivityName"",
                        ""U_Date"" AS ""Date"",
                        ""U_TimeStart"" AS ""TimeStart"",
                        ""U_TimeEnd"" AS ""TimeEnd"",
                        ""U_TimePa"" AS ""TimePa"",
                        ""U_TimeNF"" AS ""TimeNF"",
                        ""U_TimeNrPa"" AS ""TimeNrPa"",
                        ""U_TimeNrNF"" AS ""TimeNrNF"",
                        ""U_TimeNrTot"" AS ""TimeNrTot"",
                        ""U_TimeNrNet"" AS ""TimeNrNet"",
                        ""U_DescExt"" AS ""DescExt"",
                        ""U_DescInt"" AS ""DescInt"",
                        ""U_Status"" AS ""Status""
                    FROM ""{_schema}"".""@SGS_PRJ_OTMS""
                    ORDER BY ""U_Date"" DESC";
                
                using var command = new OdbcCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    timesheets.Add(new Timesheet
                    {
                        DocEntry = reader.IsDBNull(0) ? null : (int)reader.GetInt32(0),
                        Code = reader.IsDBNull(1) ? null : reader.GetString(1),
                        ResId = reader.IsDBNull(2) ? null : reader.GetString(2),
                        CardCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CardName = reader.IsDBNull(4) ? null : reader.GetString(4),
                        RefId = reader.IsDBNull(5) ? null : reader.GetString(5),
                        RefData = reader.IsDBNull(6) ? null : reader.GetString(6),
                        Project = reader.IsDBNull(7) ? null : reader.GetString(7),
                        ProjectName = reader.IsDBNull(8) ? null : reader.GetString(8),
                        SubProject = reader.IsDBNull(9) ? null : reader.GetString(9),
                        Activity = reader.IsDBNull(10) ? null : reader.GetString(10),
                        ActivityId = reader.IsDBNull(11) ? null : reader.GetString(11),
                        SubActivity = reader.IsDBNull(12) ? null : reader.GetString(12),
                        ActivityName = reader.IsDBNull(13) ? null : reader.GetString(13),
                        Date = reader.GetDateTime(14),
                        TimeStart = reader.IsDBNull(15) ? null : (int)reader.GetInt16(15),
                        TimeEnd = reader.IsDBNull(16) ? null : (int)reader.GetInt16(16),
                        TimePa = reader.IsDBNull(17) ? null : (int)reader.GetInt16(17),
                        TimeNF = reader.IsDBNull(18) ? null : (int)reader.GetInt16(18),
                        TimeNrPa = reader.IsDBNull(19) ? null : reader.GetDecimal(19),
                        TimeNrNF = reader.IsDBNull(20) ? null : reader.GetDecimal(20),
                        TimeNrTot = reader.IsDBNull(21) ? null : reader.GetDecimal(21),
                        TimeNrNet = reader.IsDBNull(22) ? null : reader.GetDecimal(22),
                        DescExt = reader.IsDBNull(23) ? null : reader.GetString(23),
                        DescInt = reader.IsDBNull(24) ? null : reader.GetString(24),
                        Status = reader.IsDBNull(25) ? null : reader.GetString(25),
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheets from database");
                throw;
            }
            
            return timesheets;
        }

        public async Task<Timesheet?> GetTimesheetByIdAsync(int docEntry)
        {
            try
            {
                using var connection = new OdbcConnection(_connectionString);
                await connection.OpenAsync();
                
                var query = $@"
                    SELECT 
                        ""DocEntry"",
                        ""Code"",
                        ""U_ResId"" AS ""ResId"",
                        ""U_CardCode"" AS ""CardCode"",
                        ""U_CardName"" AS ""CardName"",
                        ""U_RefId"" AS ""RefId"",
                        ""U_RefData"" AS ""RefData"",
                        ""U_Project"" AS ""Project"",
                        ""U_ProjectName"" AS ""ProjectName"",
                        ""U_SubProject"" AS ""SubProject"",
                        ""U_Activity"" AS ""Activity"",
                        ""U_ActivityId"" AS ""ActivityId"",
                        ""U_SubActivity"" AS ""SubActivity"",
                        ""U_ActivityName"" AS ""ActivityName"",
                        ""U_Date"" AS ""Date"",
                        ""U_TimeStart"" AS ""TimeStart"",
                        ""U_TimeEnd"" AS ""TimeEnd"",
                        ""U_TimePa"" AS ""TimePa"",
                        ""U_TimeNF"" AS ""TimeNF"",
                        ""U_TimeNrPa"" AS ""TimeNrPa"",
                        ""U_TimeNrNF"" AS ""TimeNrNF"",
                        ""U_TimeNrTot"" AS ""TimeNrTot"",
                        ""U_TimeNrNet"" AS ""TimeNrNet"",
                        ""U_DescExt"" AS ""DescExt"",
                        ""U_DescInt"" AS ""DescInt"",
                        ""U_Status"" AS ""Status""
                    FROM ""{_schema}"".""@SGS_PRJ_OTMS""
                    WHERE ""DocEntry"" = ?";
                
                using var command = new OdbcCommand(query, connection);
                command.Parameters.AddWithValue("@DocEntry", docEntry);
                
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    return new Timesheet
                    {
                        DocEntry = reader.IsDBNull(0) ? null : (int)reader.GetInt32(0),
                        Code = reader.IsDBNull(1) ? null : reader.GetString(1),
                        ResId = reader.IsDBNull(2) ? null : reader.GetString(2),
                        CardCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CardName = reader.IsDBNull(4) ? null : reader.GetString(4),
                        RefId = reader.IsDBNull(5) ? null : reader.GetString(5),
                        RefData = reader.IsDBNull(6) ? null : reader.GetString(6),
                        Project = reader.IsDBNull(7) ? null : reader.GetString(7),
                        ProjectName = reader.IsDBNull(8) ? null : reader.GetString(8),
                        SubProject = reader.IsDBNull(9) ? null : reader.GetString(9),
                        Activity = reader.IsDBNull(10) ? null : reader.GetString(10),
                        ActivityId = reader.IsDBNull(11) ? null : reader.GetString(11),
                        SubActivity = reader.IsDBNull(12) ? null : reader.GetString(12),
                        ActivityName = reader.IsDBNull(13) ? null : reader.GetString(13),
                        Date = reader.GetDateTime(14),
                        TimeStart = reader.IsDBNull(15) ? null : (int)reader.GetInt16(15),
                        TimeEnd = reader.IsDBNull(16) ? null : (int)reader.GetInt16(16),
                        TimePa = reader.IsDBNull(17) ? null : (int)reader.GetInt16(17),
                        TimeNF = reader.IsDBNull(18) ? null : (int)reader.GetInt16(18),
                        TimeNrPa = reader.IsDBNull(19) ? null : reader.GetDecimal(19),
                        TimeNrNF = reader.IsDBNull(20) ? null : reader.GetDecimal(20),
                        TimeNrTot = reader.IsDBNull(21) ? null : reader.GetDecimal(21),
                        TimeNrNet = reader.IsDBNull(22) ? null : reader.GetDecimal(22),
                        DescExt = reader.IsDBNull(23) ? null : reader.GetString(23),
                        DescInt = reader.IsDBNull(24) ? null : reader.GetString(24),
                        Status = reader.IsDBNull(25) ? null : reader.GetString(25)
                    };
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheet with DocEntry {DocEntry} from database", docEntry);
                throw;
            }
        }

        public async Task<IEnumerable<Timesheet>> GetTimesheetsByEmployeeAsync(string employeeId)
        {
            var timesheets = new List<Timesheet>();
            
            try
            {
                using var connection = new OdbcConnection(_connectionString);
                await connection.OpenAsync();
                
                var query = $@"
                    SELECT 
                        ""DocEntry"",
                        ""Code"",
                        ""U_ResId"" AS ""ResId"",
                        ""U_CardCode"" AS ""CardCode"",
                        ""U_CardName"" AS ""CardName"",
                        ""U_RefId"" AS ""RefId"",
                        ""U_RefData"" AS ""RefData"",
                        ""U_Project"" AS ""Project"",
                        ""U_ProjectName"" AS ""ProjectName"",
                        ""U_SubProject"" AS ""SubProject"",
                        ""U_Activity"" AS ""Activity"",
                        ""U_ActivityId"" AS ""ActivityId"",
                        ""U_SubActivity"" AS ""SubActivity"",
                        ""U_ActivityName"" AS ""ActivityName"",
                        ""U_Date"" AS ""Date"",
                        ""U_TimeStart"" AS ""TimeStart"",
                        ""U_TimeEnd"" AS ""TimeEnd"",
                        ""U_TimePa"" AS ""TimePa"",
                        ""U_TimeNF"" AS ""TimeNF"",
                        ""U_TimeNrPa"" AS ""TimeNrPa"",
                        ""U_TimeNrNF"" AS ""TimeNrNF"",
                        ""U_TimeNrTot"" AS ""TimeNrTot"",
                        ""U_TimeNrNet"" AS ""TimeNrNet"",
                        ""U_DescExt"" AS ""DescExt"",
                        ""U_DescInt"" AS ""DescInt"",
                        ""U_Status"" AS ""Status""
                    FROM ""{_schema}"".""@SGS_PRJ_OTMS""
                    WHERE ""U_ResId"" = ?
                    ORDER BY ""U_Date"" DESC";
                
                using var command = new OdbcCommand(query, connection);
                command.Parameters.AddWithValue("@ResId", employeeId);
                
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    timesheets.Add(new Timesheet
                    {
                        DocEntry = reader.IsDBNull(0) ? null : (int)reader.GetInt32(0),
                        Code = reader.IsDBNull(1) ? null : reader.GetString(1),
                        ResId = reader.IsDBNull(2) ? null : reader.GetString(2),
                        CardCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CardName = reader.IsDBNull(4) ? null : reader.GetString(4),
                        RefId = reader.IsDBNull(5) ? null : reader.GetString(5),
                        RefData = reader.IsDBNull(6) ? null : reader.GetString(6),
                        Project = reader.IsDBNull(7) ? null : reader.GetString(7),
                        ProjectName = reader.IsDBNull(8) ? null : reader.GetString(8),
                        SubProject = reader.IsDBNull(9) ? null : reader.GetString(9),
                        Activity = reader.IsDBNull(10) ? null : reader.GetString(10),
                        ActivityId = reader.IsDBNull(11) ? null : reader.GetString(11),
                        SubActivity = reader.IsDBNull(12) ? null : reader.GetString(12),
                        ActivityName = reader.IsDBNull(13) ? null : reader.GetString(13),
                        Date = reader.GetDateTime(14),
                        TimeStart = reader.IsDBNull(15) ? null : (int)reader.GetInt16(15),
                        TimeEnd = reader.IsDBNull(16) ? null : (int)reader.GetInt16(16),
                        TimePa = reader.IsDBNull(17) ? null : (int)reader.GetInt16(17),
                        TimeNF = reader.IsDBNull(18) ? null : (int)reader.GetInt16(18),
                        TimeNrPa = reader.IsDBNull(19) ? null : reader.GetDecimal(19),
                        TimeNrNF = reader.IsDBNull(20) ? null : reader.GetDecimal(20),
                        TimeNrTot = reader.IsDBNull(21) ? null : reader.GetDecimal(21),
                        TimeNrNet = reader.IsDBNull(22) ? null : reader.GetDecimal(22),
                        DescExt = reader.IsDBNull(23) ? null : reader.GetString(23),
                        DescInt = reader.IsDBNull(24) ? null : reader.GetString(24),
                        Status = reader.IsDBNull(25) ? null : reader.GetString(25),
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheets for employee {EmployeeId} from database", employeeId);
                throw;
            }
            
            return timesheets;
        }

        public async Task<IEnumerable<Timesheet>> GetTimesheetsByProjectAsync(string projectId)
        {
            var timesheets = new List<Timesheet>();
            
            try
            {
                using var connection = new OdbcConnection(_connectionString);
                await connection.OpenAsync();
                
                var query = $@"
                    SELECT 
                        ""DocEntry"",
                        ""Code"",
                        ""U_ResId"" AS ""ResId"",
                        ""U_CardCode"" AS ""CardCode"",
                        ""U_CardName"" AS ""CardName"",
                        ""U_RefId"" AS ""RefId"",
                        ""U_RefData"" AS ""RefData"",
                        ""U_Project"" AS ""Project"",
                        ""U_ProjectName"" AS ""ProjectName"",
                        ""U_SubProject"" AS ""SubProject"",
                        ""U_Activity"" AS ""Activity"",
                        ""U_ActivityId"" AS ""ActivityId"",
                        ""U_SubActivity"" AS ""SubActivity"",
                        ""U_ActivityName"" AS ""ActivityName"",
                        ""U_Date"" AS ""Date"",
                        ""U_TimeStart"" AS ""TimeStart"",
                        ""U_TimeEnd"" AS ""TimeEnd"",
                        ""U_TimePa"" AS ""TimePa"",
                        ""U_TimeNF"" AS ""TimeNF"",
                        ""U_TimeNrPa"" AS ""TimeNrPa"",
                        ""U_TimeNrNF"" AS ""TimeNrNF"",
                        ""U_TimeNrTot"" AS ""TimeNrTot"",
                        ""U_TimeNrNet"" AS ""TimeNrNet"",
                        ""U_DescExt"" AS ""DescExt"",
                        ""U_DescInt"" AS ""DescInt"",
                        ""U_Status"" AS ""Status""
                    FROM ""{_schema}"".""@SGS_PRJ_OTMS""
                    WHERE ""U_Project"" = ?
                    ORDER BY ""U_Date"" DESC";
                
                using var command = new OdbcCommand(query, connection);
                command.Parameters.AddWithValue("@Project", projectId);
                
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    timesheets.Add(new Timesheet
                    {
                        DocEntry = reader.IsDBNull(0) ? null : (int)reader.GetInt32(0),
                        Code = reader.IsDBNull(1) ? null : reader.GetString(1),
                        ResId = reader.IsDBNull(2) ? null : reader.GetString(2),
                        CardCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CardName = reader.IsDBNull(4) ? null : reader.GetString(4),
                        RefId = reader.IsDBNull(5) ? null : reader.GetString(5),
                        RefData = reader.IsDBNull(6) ? null : reader.GetString(6),
                        Project = reader.IsDBNull(7) ? null : reader.GetString(7),
                        ProjectName = reader.IsDBNull(8) ? null : reader.GetString(8),
                        SubProject = reader.IsDBNull(9) ? null : reader.GetString(9),
                        Activity = reader.IsDBNull(10) ? null : reader.GetString(10),
                        ActivityId = reader.IsDBNull(11) ? null : reader.GetString(11),
                        SubActivity = reader.IsDBNull(12) ? null : reader.GetString(12),
                        ActivityName = reader.IsDBNull(13) ? null : reader.GetString(13),
                        Date = reader.GetDateTime(14),
                        TimeStart = reader.IsDBNull(15) ? null : (int)reader.GetInt16(15),
                        TimeEnd = reader.IsDBNull(16) ? null : (int)reader.GetInt16(16),
                        TimePa = reader.IsDBNull(17) ? null : (int)reader.GetInt16(17),
                        TimeNF = reader.IsDBNull(18) ? null : (int)reader.GetInt16(18),
                        TimeNrPa = reader.IsDBNull(19) ? null : reader.GetDecimal(19),
                        TimeNrNF = reader.IsDBNull(20) ? null : reader.GetDecimal(20),
                        TimeNrTot = reader.IsDBNull(21) ? null : reader.GetDecimal(21),
                        TimeNrNet = reader.IsDBNull(22) ? null : reader.GetDecimal(22),
                        DescExt = reader.IsDBNull(23) ? null : reader.GetString(23),
                        DescInt = reader.IsDBNull(24) ? null : reader.GetString(24),
                        Status = reader.IsDBNull(25) ? null : reader.GetString(25),
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheets for project {ProjectId} from database", projectId);
                throw;
            }
            
            return timesheets;
        }

        public async Task<IEnumerable<Timesheet>> GetTimesheetsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var timesheets = new List<Timesheet>();
            
            try
            {
                using var connection = new OdbcConnection(_connectionString);
                await connection.OpenAsync();
                
                var query = $@"
                    SELECT 
                        ""DocEntry"",
                        ""Code"",
                        ""U_ResId"" AS ""ResId"",
                        ""U_CardCode"" AS ""CardCode"",
                        ""U_CardName"" AS ""CardName"",
                        ""U_RefId"" AS ""RefId"",
                        ""U_RefData"" AS ""RefData"",
                        ""U_Project"" AS ""Project"",
                        ""U_ProjectName"" AS ""ProjectName"",
                        ""U_SubProject"" AS ""SubProject"",
                        ""U_Activity"" AS ""Activity"",
                        ""U_ActivityId"" AS ""ActivityId"",
                        ""U_SubActivity"" AS ""SubActivity"",
                        ""U_ActivityName"" AS ""ActivityName"",
                        ""U_Date"" AS ""Date"",
                        ""U_TimeStart"" AS ""TimeStart"",
                        ""U_TimeEnd"" AS ""TimeEnd"",
                        ""U_TimePa"" AS ""TimePa"",
                        ""U_TimeNF"" AS ""TimeNF"",
                        ""U_TimeNrPa"" AS ""TimeNrPa"",
                        ""U_TimeNrNF"" AS ""TimeNrNF"",
                        ""U_TimeNrTot"" AS ""TimeNrTot"",
                        ""U_TimeNrNet"" AS ""TimeNrNet"",
                        ""U_DescExt"" AS ""DescExt"",
                        ""U_DescInt"" AS ""DescInt"",
                        ""U_Status"" AS ""Status""
                    FROM ""{_schema}"".""@SGS_PRJ_OTMS""
                    WHERE ""U_Date"" BETWEEN ? AND ?
                    ORDER BY ""U_Date"" DESC";
                
                using var command = new OdbcCommand(query, connection);
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);
                
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    timesheets.Add(new Timesheet
                    {
                        DocEntry = reader.IsDBNull(0) ? null : (int)reader.GetInt32(0),
                        Code = reader.IsDBNull(1) ? null : reader.GetString(1),
                        ResId = reader.IsDBNull(2) ? null : reader.GetString(2),
                        CardCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CardName = reader.IsDBNull(4) ? null : reader.GetString(4),
                        RefId = reader.IsDBNull(5) ? null : reader.GetString(5),
                        RefData = reader.IsDBNull(6) ? null : reader.GetString(6),
                        Project = reader.IsDBNull(7) ? null : reader.GetString(7),
                        ProjectName = reader.IsDBNull(8) ? null : reader.GetString(8),
                        SubProject = reader.IsDBNull(9) ? null : reader.GetString(9),
                        Activity = reader.IsDBNull(10) ? null : reader.GetString(10),
                        ActivityId = reader.IsDBNull(11) ? null : reader.GetString(11),
                        SubActivity = reader.IsDBNull(12) ? null : reader.GetString(12),
                        ActivityName = reader.IsDBNull(13) ? null : reader.GetString(13),
                        Date = reader.GetDateTime(14),
                        TimeStart = reader.IsDBNull(15) ? null : (int)reader.GetInt32(15),
                        TimeEnd = reader.IsDBNull(16) ? null : (int)reader.GetInt32(16),
                        TimePa = reader.IsDBNull(17) ? null : (int)reader.GetInt32(17),
                        TimeNF = reader.IsDBNull(18) ? null : (int)reader.GetInt32(18),
                        TimeNrPa = reader.IsDBNull(19) ? null : reader.GetDecimal(19),
                        TimeNrNF = reader.IsDBNull(20) ? null : reader.GetDecimal(20),
                        TimeNrTot = reader.IsDBNull(21) ? null : reader.GetDecimal(21),
                        TimeNrNet = reader.IsDBNull(22) ? null : reader.GetDecimal(22),
                        DescExt = reader.IsDBNull(23) ? null : reader.GetString(23),
                        DescInt = reader.IsDBNull(24) ? null : reader.GetString(24),
                        Status = reader.IsDBNull(25) ? null : reader.GetString(25),
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheets for date range {StartDate} to {EndDate} from database", startDate, endDate);
                throw;
            }
            
            return timesheets;
        }

        public async Task<IEnumerable<Timesheet>> GetTimesheetsByEmployeeAndDateRangeAsync(string employeeId, DateTime startDate, DateTime endDate)
        {
            var timesheets = new List<Timesheet>();
            
            try
            {
                using var connection = new OdbcConnection(_connectionString);
                await connection.OpenAsync();
                
                var query = $@"
                    SELECT 
                        ""DocEntry"",
                        ""Code"",
                        ""U_ResId"" AS ""ResId"",
                        ""U_CardCode"" AS ""CardCode"",
                        ""U_CardName"" AS ""CardName"",
                        ""U_RefId"" AS ""RefId"",
                        ""U_RefData"" AS ""RefData"",
                        ""U_Project"" AS ""Project"",
                        ""U_ProjectName"" AS ""ProjectName"",
                        ""U_SubProject"" AS ""SubProject"",
                        ""U_Activity"" AS ""Activity"",
                        ""U_ActivityId"" AS ""ActivityId"",
                        ""U_SubActivity"" AS ""SubActivity"",
                        ""U_ActivityName"" AS ""ActivityName"",
                        ""U_Date"" AS ""Date"",
                        ""U_TimeStart"" AS ""TimeStart"",
                        ""U_TimeEnd"" AS ""TimeEnd"",
                        ""U_TimePa"" AS ""TimePa"",
                        ""U_TimeNF"" AS ""TimeNF"",
                        ""U_TimeNrPa"" AS ""TimeNrPa"",
                        ""U_TimeNrNF"" AS ""TimeNrNF"",
                        ""U_TimeNrTot"" AS ""TimeNrTot"",
                        ""U_TimeNrNet"" AS ""TimeNrNet"",
                        ""U_DescExt"" AS ""DescExt"",
                        ""U_DescInt"" AS ""DescInt"",
                        ""U_Status"" AS ""Status""
                    FROM ""{_schema}"".""@SGS_PRJ_OTMS""
                    WHERE ""U_ResId"" = ? AND ""U_Date"" BETWEEN ? AND ?
                    ORDER BY ""U_Date"" DESC";
                
                using var command = new OdbcCommand(query, connection);
                command.Parameters.AddWithValue("@ResId", employeeId);
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);
                
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    timesheets.Add(new Timesheet
                    {
                        DocEntry = reader.IsDBNull(0) ? null : (int)reader.GetInt32(0),
                        Code = reader.IsDBNull(1) ? null : reader.GetString(1),
                        ResId = reader.IsDBNull(2) ? null : reader.GetString(2),
                        CardCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CardName = reader.IsDBNull(4) ? null : reader.GetString(4),
                        RefId = reader.IsDBNull(5) ? null : reader.GetString(5),
                        RefData = reader.IsDBNull(6) ? null : reader.GetString(6),
                        Project = reader.IsDBNull(7) ? null : reader.GetString(7),
                        ProjectName = reader.IsDBNull(8) ? null : reader.GetString(8),
                        SubProject = reader.IsDBNull(9) ? null : reader.GetString(9),
                        Activity = reader.IsDBNull(10) ? null : reader.GetString(10),
                        ActivityId = reader.IsDBNull(11) ? null : reader.GetString(11),
                        SubActivity = reader.IsDBNull(12) ? null : reader.GetString(12),
                        ActivityName = reader.IsDBNull(13) ? null : reader.GetString(13),
                        Date = reader.GetDateTime(14),
                        TimeStart = reader.IsDBNull(15) ? null : (int)reader.GetInt16(15),
                        TimeEnd = reader.IsDBNull(16) ? null : (int)reader.GetInt16(16),
                        TimePa = reader.IsDBNull(17) ? null : (int)reader.GetInt16(17),
                        TimeNF = reader.IsDBNull(18) ? null : (int)reader.GetInt16(18),
                        TimeNrPa = reader.IsDBNull(19) ? null : reader.GetDecimal(19),
                        TimeNrNF = reader.IsDBNull(20) ? null : reader.GetDecimal(20),
                        TimeNrTot = reader.IsDBNull(21) ? null : reader.GetDecimal(21),
                        TimeNrNet = reader.IsDBNull(22) ? null : reader.GetDecimal(22),
                        DescExt = reader.IsDBNull(23) ? null : reader.GetString(23),
                        DescInt = reader.IsDBNull(24) ? null : reader.GetString(24),
                        Status = reader.IsDBNull(25) ? null : reader.GetString(25),
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheets for employee {EmployeeId} in date range {StartDate} to {EndDate} from database", employeeId, startDate, endDate);
                throw;
            }
            
            return timesheets;
        }

        public async Task<string> GetNextTimesheetCodeAsync()
        {
            try
            {
                using var connection = new OdbcConnection(_connectionString);
                await connection.OpenAsync();
                
                // Query to get the maximum Code value converted to numeric, then add 1
                var query = $@"
                    SELECT 
                        CASE 
                            WHEN MAX(CAST(""Code"" AS INTEGER)) IS NULL THEN 1
                            ELSE MAX(CAST(""Code"" AS INTEGER)) + 1
                        END
                    FROM ""{_schema}"".""@SGS_PRJ_OTMS""
                    WHERE ""Code"" IS NOT NULL 
                    AND ""Code"" != ''";
                
                using var command = new OdbcCommand(query, connection);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var result = await command.ExecuteScalarAsync();
                sw.Stop();
                _logger.LogInformation("ODBC ExecuteScalar completed in {ElapsedMs} ms", sw.ElapsedMilliseconds);
                
                if (result != null && result != DBNull.Value)
                {
                    return result.ToString()!;
                }
                
                return "1"; // Default to 1 if no records found
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next timesheet code from database");
                throw;
            }
        }

        public async Task<IEnumerable<CustomerSummary>> GetCustomersAsync()
        {
            var customers = new List<CustomerSummary>();
            try
            {
                using var connection = await CreateOpenConnectionAsync();

                var query = $@"
                    SELECT 
                        ""CardCode"",
                        ""CardName""
                    FROM ""{_schema}"".""OCRD""
                    WHERE ""CardType"" = 'C' 
                    ORDER BY ""CardName""";

                using var command = new OdbcCommand(query, connection);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                using var reader = await command.ExecuteReaderAsync();
                sw.Stop();
                _logger.LogInformation("ODBC ExecuteReader(customers) completed in {ElapsedMs} ms", sw.ElapsedMilliseconds);
                while (await reader.ReadAsync())
                {
                    customers.Add(new CustomerSummary
                    {
                        CardCode = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                        CardName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers from database");
                throw;
            }
            return customers;
        }

        public async Task<IEnumerable<ContactSummary>> GetContactsByCustomerAsync(string cardCode)
        {
            var contacts = new List<ContactSummary>();
            try
            {
                using var connection = await CreateOpenConnectionAsync();

                var query = $@"
                    SELECT 
                        ""CntctCode"" AS ""Code"",
                        ""Name""
                    FROM ""{_schema}"".""OCPR""
                    WHERE ""CardCode"" = ?
                    ORDER BY ""Name""";

                using var command = new OdbcCommand(query, connection);
                command.Parameters.AddWithValue("@CardCode", cardCode);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                using var reader = await command.ExecuteReaderAsync();
                sw.Stop();
                _logger.LogInformation("ODBC ExecuteReader(contacts) completed in {ElapsedMs} ms", sw.ElapsedMilliseconds);
                while (await reader.ReadAsync())
                {
                    contacts.Add(new ContactSummary
                    {
                        Code = reader.IsDBNull(0) ? string.Empty : reader.GetInt32(0).ToString(),
                        Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contacts for customer {CardCode} from database", cardCode);
                throw;
            }
            return contacts;
        }

        public async Task<IEnumerable<ProjectSummary>> GetProjectsAsync()
        {
            var projects = new List<ProjectSummary>();
            try
            {
                using var connection = new OdbcConnection(_connectionString);
                await connection.OpenAsync();

                var query = $@"
                    SELECT
                        T.""AbsEntry"" AS ""Code"",
                        T.""NAME"" AS ""Name""
                    FROM ""{_schema}"".""OPMG"" T
                    ORDER BY ""NAME""";

                using var command = new OdbcCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    projects.Add(new ProjectSummary
                    {
                        Code = reader.IsDBNull(0) ? string.Empty : reader.GetInt32(0).ToString(),
                        Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects from database");
                throw;
            }
            return projects;
        }

        public async Task<IEnumerable<ActivitySummary>> GetActivitiesByProjectAsync(string projectCode)
        {
            var activities = new List<ActivitySummary>();
            try
            {
                using var connection = new OdbcConnection(_connectionString);
                await connection.OpenAsync();

                var query = $@"
                    SELECT 
                        ""LineID"" AS ""Code"",
                        ""DSCRIPTION"" AS ""Name"",
                        ""U_SGS_PRJ_UnitMsr"",
                        ""U_SGS_PRJ_Price""
                    FROM ""{_schema}"".""PMG1""
                    WHERE ""AbsEntry"" = ?
                    ORDER BY ""LineID""";

                using var command = new OdbcCommand(query, connection);
                command.Parameters.AddWithValue("@Project", projectCode);
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    activities.Add(new ActivitySummary
                    {
                        Code = reader.IsDBNull(0) ? string.Empty : reader.GetInt32(0).ToString(),
                        Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        UoM = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        Price = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activities for project {ProjectCode} from database", projectCode);
                throw;
            }
            return activities;
        }

        public async Task<IEnumerable<ProjectSummary>> GetProjectsByCustomerAsync(string cardCode)
        {
            var projects = new List<ProjectSummary>();
            try
            {
                using var connection = new OdbcConnection(_connectionString);
                await connection.OpenAsync();

                // Projects linked to a BP via SAP B1 standard tables: OINV/RDR/OPRJ linkage varies by implementation.
                // Here we leverage the timesheet source table if projects are referenced there by CardCode, else fallback to OPRJ + OCRD link via custom relations.
                var query = $@"
                    SELECT
                        T.""AbsEntry"" AS ""Code"",
                        T.""NAME"" AS ""Name""
                    FROM ""{_schema}"".""OPMG"" T
                    WHERE T.""CARDCODE"" = ?
                    ORDER BY T.""NAME""";

                using var command = new OdbcCommand(query, connection);
                command.Parameters.AddWithValue("@CardCode", cardCode);
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    projects.Add(new ProjectSummary
                    {
                        Code = reader.IsDBNull(0) ? string.Empty : reader.GetInt32(0).ToString(),
                        Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects for customer {CardCode} from database", cardCode);
                throw;
            }
            return projects;
        }

        public async Task<IEnumerable<ResourceSummary>> GetResourcesAsync()
        {
            var resources = new List<ResourceSummary>();
            try
            {
                using var connection = new OdbcConnection(_connectionString);
                await connection.OpenAsync();

                // Read from common JWT claims to handle mapping differences
                var principal = _httpContextAccessor.HttpContext?.User;
                var jti = principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                var userName =
                    principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? principal?.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value
                    ?? principal?.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value
                    ?? principal?.FindFirst(ClaimTypes.Name)?.Value;
                _logger.LogDebug("GetResourcesAsync principal resolved: sub/name={User}, jti={Jti}", userName ?? "", jti ?? "");
                if (string.IsNullOrWhiteSpace(userName))
                {
                    _logger.LogWarning("GetResourcesAsync: missing JWT 'sub' claim (Authorization header may contain an old/invalid token)");
                    return resources;
                }

                var query = $@"
                    SELECT 
                        T0.""ResCode"" AS ""Code"",
                        T0.""ResName"" AS ""Name""
                    FROM ""{_schema}"".""ORSC"" T0
                    INNER JOIN ""{_schema}"".""RSC4"" T1 ON T0.""ResCode"" = T1.""ResCode""
                    INNER JOIN ""{_schema}"".""OHEM"" T2 ON T1.""EmpID"" = T2.""empID""
                    INNER JOIN ""{_schema}"".""OUSR"" T3 ON T2.""userId"" = T3.""USERID""
                    LEFT JOIN
                    (
                        ""{_schema}"".""OHEM"" T4
                        INNER JOIN ""{_schema}"".""OUSR"" T5 ON T4.""userId"" = T5.""USERID""
                    ) ON T4.""empID"" = T2.""manager""
                    WHERE T0.""validFor"" = 'Y' AND T2.""Active"" = 'Y'
                    AND (T3.""USER_CODE"" = ? OR IFNULL(T5.""USER_CODE"",'') = ?)
                    ORDER BY T0.""ResName""";

                using var command = new OdbcCommand(query, connection);
                command.Parameters.AddWithValue("@User1", userName);
                command.Parameters.AddWithValue("@User2", userName);
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var code = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                    var name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    resources.Add(new ResourceSummary { Code = code, Name = name });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving resources from database");
                throw;
            }
            return resources;
        }

        public async Task<ActivityTimeTotal?> GetActivityTimeTotAsync(string projectId, string activityId)
        {
            try
            {
                using var connection = new OdbcConnection(_connectionString);
                await connection.OpenAsync();

                var query = $@"
                    SELECT 
                        T0.""U_Project"", 
                        T0.""U_ActivityId"", 
                        SUM(T0.""U_TimeNrTot"") AS ""TimeTot""
                    FROM ""{_schema}"".""@SGS_PRJ_OTMS"" T0
                    WHERE T0.""U_Project"" = ? AND T0.""U_ActivityId"" = ?
                    GROUP BY 
                        T0.""U_Project"", T0.""U_ActivityId""";

                using var command = new OdbcCommand(query, connection);
                command.Parameters.AddWithValue("@ProjectId", projectId);
                command.Parameters.AddWithValue("@ActivityId", activityId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new ActivityTimeTotal
                    {
                        Project = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                        ActivityId = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        TimeTot = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2)
                    };
                }

                return new ActivityTimeTotal
                {
                    Project = projectId,
                    ActivityId = activityId,
                    TimeTot = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity time total for project {ProjectId} and activity {ActivityId}", projectId, activityId);
                throw;
            }
        }
    }
}
