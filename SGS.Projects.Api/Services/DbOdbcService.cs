using System.Data.Odbc;
using SGS.Projects.Api.Models;

namespace SGS.Projects.Api.Services
{
    public class DbOdbcService : IDbOdbcService
    {
        private readonly string _connectionString;
        private readonly ILogger<DbOdbcService> _logger;

        public DbOdbcService(IConfiguration configuration, ILogger<DbOdbcService> logger)
        {
            _connectionString = configuration.GetConnectionString("SqlServer") 
                ?? throw new ArgumentNullException(nameof(configuration), "SqlServer connection string not found");
            _logger = logger;
        }

        public async Task<IEnumerable<Timesheet>> GetTimesheetsAsync()
        {
            var timesheets = new List<Timesheet>();
            
            try
            {
                using var connection = new OdbcConnection(_connectionString);
                await connection.OpenAsync();
                
                var query = @"
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
                    FROM ""@SGS_PRJ_OTMS""
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
                
                var query = @"
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
                    FROM ""@SGS_PRJ_OTMS""
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
                
                var query = @"
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
                    FROM ""@SGS_PRJ_OTMS""
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
                
                var query = @"
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
                    FROM ""@SGS_PRJ_OTMS""
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
                
                var query = @"
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
                    FROM ""@SGS_PRJ_OTMS""
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
                
                var query = @"
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
                    FROM ""@SGS_PRJ_OTMS""
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
                var query = @"
                    SELECT 
                        CASE 
                            WHEN MAX(CAST(""Code"" AS INTEGER)) IS NULL THEN 1
                            ELSE MAX(CAST(""Code"" AS INTEGER)) + 1
                        END
                    FROM ""@SGS_PRJ_OTMS""
                    WHERE ""Code"" IS NOT NULL 
                    AND ""Code"" != ''";
                
                using var command = new OdbcCommand(query, connection);
                var result = await command.ExecuteScalarAsync();
                
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
    }
}
