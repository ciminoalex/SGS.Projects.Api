using System.Data.Odbc;
using SGS.Projects.Api.Models;

namespace SGS.Projects.Api.Services
{
    public class HanaOdbcService : IHanaOdbcService
    {
        private readonly string _connectionString;
        private readonly ILogger<HanaOdbcService> _logger;

        public HanaOdbcService(IConfiguration configuration, ILogger<HanaOdbcService> logger)
        {
            _connectionString = configuration.GetConnectionString("SapHana") 
                ?? throw new ArgumentNullException(nameof(configuration), "SapHana connection string not found");
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
                        DocEntry,
                        DocNum,
                        Code,
                        U_ResId as ResId,
                        U_CardCode as CardCode,
                        U_CardName as CardName,
                        U_RefId as RefId,
                        U_RefData as RefData,
                        U_Project as Project,
                        U_ProjectName as ProjectName,
                        U_SubProject as SubProject,
                        U_Activity as Activity,
                        U_ActivityId as ActivityId,
                        U_SubActivity as SubActivity,
                        U_ActivityName as ActivityName,
                        U_Date as Date,
                        U_TimeStart as TimeStart,
                        U_TimeEnd as TimeEnd,
                        U_TimePa as TimePa,
                        U_TimeNF as TimeNF,
                        U_TimeNrPa as TimeNrPa,
                        U_TimeNrNF as TimeNrNF,
                        U_TimeNrTot as TimeNrTot,
                        U_TimeNrNet as TimeNrNet,
                        U_Price as Price,
                        U_LineTotal as LineTotal,
                        U_DescExt as DescExt,
                        U_DescInt as DescInt,
                        U_ItemCode as ItemCode,
                        U_Status as Status,
                        U_Approver as Approver,
                        U_InvEntry as InvEntry,
                        U_DestType as DestType,
                        U_DestEntry as DestEntry,
                        U_DestLine as DestLine,
                        U_BaseType as BaseType,
                        U_BaseEntry as BaseEntry,
                        U_BaseLine as BaseLine,
                        U_DescExtOri as DescExtOri,
                        U_TimeStartOri as TimeStartOri,
                        U_TimeEndOri as TimeEndOri,
                        U_TimePaOri as TimePaOri,
                        U_TimeNFOri as TimeNFOri,
                        U_TimeNrPaOri as TimeNrPaOri,
                        U_TimeNrNFOri as TimeNrNFOri,
                        U_TimeNrTotOri as TimeNrTotOri,
                        U_TimeNrNetOri as TimeNrNetOri,
                        CreateDate as CreatedDate,
                        CreateTS as CreatedBy,
                        UpdateDate as ModifiedDate,
                        UpdateTS as ModifiedBy
                    FROM [@TIMESHEET]
                    ORDER BY U_Date DESC";
                
                using var command = new OdbcCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    timesheets.Add(new Timesheet
                    {
                        DocEntry = reader.IsDBNull(0) ? null : (int)reader.GetInt32(0),
                        DocNum = reader.GetString(1),
                        Code = reader.IsDBNull(2) ? null : reader.GetString(2),
                        ResId = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CardCode = reader.IsDBNull(4) ? null : reader.GetString(4),
                        CardName = reader.IsDBNull(5) ? null : reader.GetString(5),
                        RefId = reader.IsDBNull(6) ? null : reader.GetString(6),
                        RefData = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Project = reader.IsDBNull(8) ? null : reader.GetString(8),
                        ProjectName = reader.IsDBNull(9) ? null : reader.GetString(9),
                        SubProject = reader.IsDBNull(10) ? null : reader.GetString(10),
                        Activity = reader.IsDBNull(11) ? null : reader.GetString(11),
                        ActivityId = reader.IsDBNull(12) ? null : reader.GetString(12),
                        SubActivity = reader.IsDBNull(13) ? null : reader.GetString(13),
                        ActivityName = reader.IsDBNull(14) ? null : reader.GetString(14),
                        Date = reader.GetDateTime(15),
                        TimeStart = reader.IsDBNull(16) ? null : TimeSpan.Parse(reader.GetString(16)),
                        TimeEnd = reader.IsDBNull(17) ? null : TimeSpan.Parse(reader.GetString(17)),
                        TimePa = reader.IsDBNull(18) ? null : TimeSpan.Parse(reader.GetString(18)),
                        TimeNF = reader.IsDBNull(19) ? null : TimeSpan.Parse(reader.GetString(19)),
                        TimeNrPa = reader.IsDBNull(20) ? null : reader.GetDecimal(20),
                        TimeNrNF = reader.IsDBNull(21) ? null : reader.GetDecimal(21),
                        TimeNrTot = reader.IsDBNull(22) ? null : reader.GetDecimal(22),
                        TimeNrNet = reader.IsDBNull(23) ? null : reader.GetDecimal(23),
                        Price = reader.IsDBNull(24) ? null : reader.GetDecimal(24),
                        LineTotal = reader.IsDBNull(25) ? null : reader.GetDecimal(25),
                        DescExt = reader.IsDBNull(26) ? null : reader.GetString(26),
                        DescInt = reader.IsDBNull(27) ? null : reader.GetString(27),
                        ItemCode = reader.IsDBNull(28) ? null : reader.GetString(28),
                        Status = reader.IsDBNull(29) ? null : reader.GetString(29),
                        Approver = reader.IsDBNull(30) ? null : reader.GetString(30),
                        InvEntry = reader.IsDBNull(31) ? null : reader.GetString(31),
                        DestType = reader.IsDBNull(32) ? null : reader.GetString(32),
                        DestEntry = reader.IsDBNull(33) ? null : reader.GetString(33),
                        DestLine = reader.IsDBNull(34) ? null : (int)reader.GetInt32(34),
                        BaseType = reader.IsDBNull(35) ? null : reader.GetString(35),
                        BaseEntry = reader.IsDBNull(36) ? null : reader.GetString(36),
                        BaseLine = reader.IsDBNull(37) ? null : (int)reader.GetInt32(37),
                        DescExtOri = reader.IsDBNull(38) ? null : reader.GetString(38),
                        TimeStartOri = reader.IsDBNull(39) ? null : TimeSpan.Parse(reader.GetString(39)),
                        TimeEndOri = reader.IsDBNull(40) ? null : TimeSpan.Parse(reader.GetString(40)),
                        TimePaOri = reader.IsDBNull(41) ? null : TimeSpan.Parse(reader.GetString(41)),
                        TimeNFOri = reader.IsDBNull(42) ? null : TimeSpan.Parse(reader.GetString(42)),
                        TimeNrPaOri = reader.IsDBNull(43) ? null : reader.GetDecimal(43),
                        TimeNrNFOri = reader.IsDBNull(44) ? null : reader.GetDecimal(44),
                        TimeNrTotOri = reader.IsDBNull(45) ? null : reader.GetDecimal(45),
                        TimeNrNetOri = reader.IsDBNull(46) ? null : reader.GetDecimal(46),
                        CreatedDate = reader.IsDBNull(47) ? null : reader.GetDateTime(47),
                        CreatedBy = reader.IsDBNull(48) ? null : reader.GetString(48),
                        ModifiedDate = reader.IsDBNull(49) ? null : reader.GetDateTime(49),
                        ModifiedBy = reader.IsDBNull(50) ? null : reader.GetString(50),
                        // Campi di compatibilità
                        EmployeeId = reader.IsDBNull(3) ? string.Empty : reader.GetString(3), // Mappato su ResId
                        ProjectId = reader.IsDBNull(8) ? string.Empty : reader.GetString(8), // Mappato su Project
                        Hours = reader.IsDBNull(23) ? 0 : reader.GetDecimal(23), // Mappato su TimeNrNet
                        Description = reader.IsDBNull(26) ? null : reader.GetString(26) // Mappato su DescExt
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheets from SAP HANA");
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
                        DocEntry,
                        DocNum,
                        Code,
                        U_ResId as ResId,
                        U_CardCode as CardCode,
                        U_CardName as CardName,
                        U_RefId as RefId,
                        U_RefData as RefData,
                        U_Project as Project,
                        U_ProjectName as ProjectName,
                        U_SubProject as SubProject,
                        U_Activity as Activity,
                        U_ActivityId as ActivityId,
                        U_SubActivity as SubActivity,
                        U_ActivityName as ActivityName,
                        U_Date as Date,
                        U_TimeStart as TimeStart,
                        U_TimeEnd as TimeEnd,
                        U_TimePa as TimePa,
                        U_TimeNF as TimeNF,
                        U_TimeNrPa as TimeNrPa,
                        U_TimeNrNF as TimeNrNF,
                        U_TimeNrTot as TimeNrTot,
                        U_TimeNrNet as TimeNrNet,
                        U_Price as Price,
                        U_LineTotal as LineTotal,
                        U_DescExt as DescExt,
                        U_DescInt as DescInt,
                        U_ItemCode as ItemCode,
                        U_Status as Status,
                        U_Approver as Approver,
                        U_InvEntry as InvEntry,
                        U_DestType as DestType,
                        U_DestEntry as DestEntry,
                        U_DestLine as DestLine,
                        U_BaseType as BaseType,
                        U_BaseEntry as BaseEntry,
                        U_BaseLine as BaseLine,
                        U_DescExtOri as DescExtOri,
                        U_TimeStartOri as TimeStartOri,
                        U_TimeEndOri as TimeEndOri,
                        U_TimePaOri as TimePaOri,
                        U_TimeNFOri as TimeNFOri,
                        U_TimeNrPaOri as TimeNrPaOri,
                        U_TimeNrNFOri as TimeNrNFOri,
                        U_TimeNrTotOri as TimeNrTotOri,
                        U_TimeNrNetOri as TimeNrNetOri,
                        CreateDate as CreatedDate,
                        CreateTS as CreatedBy,
                        UpdateDate as ModifiedDate,
                        UpdateTS as ModifiedBy
                    FROM [@TIMESHEET]
                    WHERE DocEntry = ?";
                
                using var command = new OdbcCommand(query, connection);
                command.Parameters.AddWithValue("@DocEntry", docEntry);
                
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    return new Timesheet
                    {
                        DocEntry = reader.IsDBNull(0) ? null : (int)reader.GetInt32(0),
                        DocNum = reader.GetString(1),
                        Code = reader.IsDBNull(2) ? null : reader.GetString(2),
                        ResId = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CardCode = reader.IsDBNull(4) ? null : reader.GetString(4),
                        CardName = reader.IsDBNull(5) ? null : reader.GetString(5),
                        RefId = reader.IsDBNull(6) ? null : reader.GetString(6),
                        RefData = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Project = reader.IsDBNull(8) ? null : reader.GetString(8),
                        ProjectName = reader.IsDBNull(9) ? null : reader.GetString(9),
                        SubProject = reader.IsDBNull(10) ? null : reader.GetString(10),
                        Activity = reader.IsDBNull(11) ? null : reader.GetString(11),
                        ActivityId = reader.IsDBNull(12) ? null : reader.GetString(12),
                        SubActivity = reader.IsDBNull(13) ? null : reader.GetString(13),
                        ActivityName = reader.IsDBNull(14) ? null : reader.GetString(14),
                        Date = reader.GetDateTime(15),
                        TimeStart = reader.IsDBNull(16) ? null : TimeSpan.Parse(reader.GetString(16)),
                        TimeEnd = reader.IsDBNull(17) ? null : TimeSpan.Parse(reader.GetString(17)),
                        TimePa = reader.IsDBNull(18) ? null : TimeSpan.Parse(reader.GetString(18)),
                        TimeNF = reader.IsDBNull(19) ? null : TimeSpan.Parse(reader.GetString(19)),
                        TimeNrPa = reader.IsDBNull(20) ? null : reader.GetDecimal(20),
                        TimeNrNF = reader.IsDBNull(21) ? null : reader.GetDecimal(21),
                        TimeNrTot = reader.IsDBNull(22) ? null : reader.GetDecimal(22),
                        TimeNrNet = reader.IsDBNull(23) ? null : reader.GetDecimal(23),
                        Price = reader.IsDBNull(24) ? null : reader.GetDecimal(24),
                        LineTotal = reader.IsDBNull(25) ? null : reader.GetDecimal(25),
                        DescExt = reader.IsDBNull(26) ? null : reader.GetString(26),
                        DescInt = reader.IsDBNull(27) ? null : reader.GetString(27),
                        ItemCode = reader.IsDBNull(28) ? null : reader.GetString(28),
                        Status = reader.IsDBNull(29) ? null : reader.GetString(29),
                        Approver = reader.IsDBNull(30) ? null : reader.GetString(30),
                        InvEntry = reader.IsDBNull(31) ? null : reader.GetString(31),
                        DestType = reader.IsDBNull(32) ? null : reader.GetString(32),
                        DestEntry = reader.IsDBNull(33) ? null : reader.GetString(33),
                        DestLine = reader.IsDBNull(34) ? null : (int)reader.GetInt32(34),
                        BaseType = reader.IsDBNull(35) ? null : reader.GetString(35),
                        BaseEntry = reader.IsDBNull(36) ? null : reader.GetString(36),
                        BaseLine = reader.IsDBNull(37) ? null : (int)reader.GetInt32(37),
                        DescExtOri = reader.IsDBNull(38) ? null : reader.GetString(38),
                        TimeStartOri = reader.IsDBNull(39) ? null : TimeSpan.Parse(reader.GetString(39)),
                        TimeEndOri = reader.IsDBNull(40) ? null : TimeSpan.Parse(reader.GetString(40)),
                        TimePaOri = reader.IsDBNull(41) ? null : TimeSpan.Parse(reader.GetString(41)),
                        TimeNFOri = reader.IsDBNull(42) ? null : TimeSpan.Parse(reader.GetString(42)),
                        TimeNrPaOri = reader.IsDBNull(43) ? null : reader.GetDecimal(43),
                        TimeNrNFOri = reader.IsDBNull(44) ? null : reader.GetDecimal(44),
                        TimeNrTotOri = reader.IsDBNull(45) ? null : reader.GetDecimal(45),
                        TimeNrNetOri = reader.IsDBNull(46) ? null : reader.GetDecimal(46),
                        CreatedDate = reader.IsDBNull(47) ? null : reader.GetDateTime(47),
                        CreatedBy = reader.IsDBNull(48) ? null : reader.GetString(48),
                        ModifiedDate = reader.IsDBNull(49) ? null : reader.GetDateTime(49),
                        ModifiedBy = reader.IsDBNull(50) ? null : reader.GetString(50),
                        // Campi di compatibilità
                        EmployeeId = reader.IsDBNull(3) ? string.Empty : reader.GetString(3), // Mappato su ResId
                        ProjectId = reader.IsDBNull(8) ? string.Empty : reader.GetString(8), // Mappato su Project
                        Hours = reader.IsDBNull(23) ? 0 : reader.GetDecimal(23), // Mappato su TimeNrNet
                        Description = reader.IsDBNull(26) ? null : reader.GetString(26) // Mappato su DescExt
                    };
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheet with DocEntry {DocEntry} from SAP HANA", docEntry);
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
                        DocEntry,
                        DocNum,
                        Code,
                        U_ResId as ResId,
                        U_CardCode as CardCode,
                        U_CardName as CardName,
                        U_RefId as RefId,
                        U_RefData as RefData,
                        U_Project as Project,
                        U_ProjectName as ProjectName,
                        U_SubProject as SubProject,
                        U_Activity as Activity,
                        U_ActivityId as ActivityId,
                        U_SubActivity as SubActivity,
                        U_ActivityName as ActivityName,
                        U_Date as Date,
                        U_TimeStart as TimeStart,
                        U_TimeEnd as TimeEnd,
                        U_TimePa as TimePa,
                        U_TimeNF as TimeNF,
                        U_TimeNrPa as TimeNrPa,
                        U_TimeNrNF as TimeNrNF,
                        U_TimeNrTot as TimeNrTot,
                        U_TimeNrNet as TimeNrNet,
                        U_Price as Price,
                        U_LineTotal as LineTotal,
                        U_DescExt as DescExt,
                        U_DescInt as DescInt,
                        U_ItemCode as ItemCode,
                        U_Status as Status,
                        U_Approver as Approver,
                        U_InvEntry as InvEntry,
                        U_DestType as DestType,
                        U_DestEntry as DestEntry,
                        U_DestLine as DestLine,
                        U_BaseType as BaseType,
                        U_BaseEntry as BaseEntry,
                        U_BaseLine as BaseLine,
                        U_DescExtOri as DescExtOri,
                        U_TimeStartOri as TimeStartOri,
                        U_TimeEndOri as TimeEndOri,
                        U_TimePaOri as TimePaOri,
                        U_TimeNFOri as TimeNFOri,
                        U_TimeNrPaOri as TimeNrPaOri,
                        U_TimeNrNFOri as TimeNrNFOri,
                        U_TimeNrTotOri as TimeNrTotOri,
                        U_TimeNrNetOri as TimeNrNetOri,
                        CreateDate as CreatedDate,
                        CreateTS as CreatedBy,
                        UpdateDate as ModifiedDate,
                        UpdateTS as ModifiedBy
                    FROM [@TIMESHEET]
                    WHERE U_ResId = ?
                    ORDER BY U_Date DESC";
                
                using var command = new OdbcCommand(query, connection);
                command.Parameters.AddWithValue("@ResId", employeeId);
                
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    timesheets.Add(new Timesheet
                    {
                        DocEntry = reader.IsDBNull(0) ? null : (int)reader.GetInt32(0),
                        DocNum = reader.GetString(1),
                        Code = reader.IsDBNull(2) ? null : reader.GetString(2),
                        ResId = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CardCode = reader.IsDBNull(4) ? null : reader.GetString(4),
                        CardName = reader.IsDBNull(5) ? null : reader.GetString(5),
                        RefId = reader.IsDBNull(6) ? null : reader.GetString(6),
                        RefData = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Project = reader.IsDBNull(8) ? null : reader.GetString(8),
                        ProjectName = reader.IsDBNull(9) ? null : reader.GetString(9),
                        SubProject = reader.IsDBNull(10) ? null : reader.GetString(10),
                        Activity = reader.IsDBNull(11) ? null : reader.GetString(11),
                        ActivityId = reader.IsDBNull(12) ? null : reader.GetString(12),
                        SubActivity = reader.IsDBNull(13) ? null : reader.GetString(13),
                        ActivityName = reader.IsDBNull(14) ? null : reader.GetString(14),
                        Date = reader.GetDateTime(15),
                        TimeStart = reader.IsDBNull(16) ? null : TimeSpan.Parse(reader.GetString(16)),
                        TimeEnd = reader.IsDBNull(17) ? null : TimeSpan.Parse(reader.GetString(17)),
                        TimePa = reader.IsDBNull(18) ? null : TimeSpan.Parse(reader.GetString(18)),
                        TimeNF = reader.IsDBNull(19) ? null : TimeSpan.Parse(reader.GetString(19)),
                        TimeNrPa = reader.IsDBNull(20) ? null : reader.GetDecimal(20),
                        TimeNrNF = reader.IsDBNull(21) ? null : reader.GetDecimal(21),
                        TimeNrTot = reader.IsDBNull(22) ? null : reader.GetDecimal(22),
                        TimeNrNet = reader.IsDBNull(23) ? null : reader.GetDecimal(23),
                        Price = reader.IsDBNull(24) ? null : reader.GetDecimal(24),
                        LineTotal = reader.IsDBNull(25) ? null : reader.GetDecimal(25),
                        DescExt = reader.IsDBNull(26) ? null : reader.GetString(26),
                        DescInt = reader.IsDBNull(27) ? null : reader.GetString(27),
                        ItemCode = reader.IsDBNull(28) ? null : reader.GetString(28),
                        Status = reader.IsDBNull(29) ? null : reader.GetString(29),
                        Approver = reader.IsDBNull(30) ? null : reader.GetString(30),
                        InvEntry = reader.IsDBNull(31) ? null : reader.GetString(31),
                        DestType = reader.IsDBNull(32) ? null : reader.GetString(32),
                        DestEntry = reader.IsDBNull(33) ? null : reader.GetString(33),
                        DestLine = reader.IsDBNull(34) ? null : (int)reader.GetInt32(34),
                        BaseType = reader.IsDBNull(35) ? null : reader.GetString(35),
                        BaseEntry = reader.IsDBNull(36) ? null : reader.GetString(36),
                        BaseLine = reader.IsDBNull(37) ? null : (int)reader.GetInt32(37),
                        DescExtOri = reader.IsDBNull(38) ? null : reader.GetString(38),
                        TimeStartOri = reader.IsDBNull(39) ? null : TimeSpan.Parse(reader.GetString(39)),
                        TimeEndOri = reader.IsDBNull(40) ? null : TimeSpan.Parse(reader.GetString(40)),
                        TimePaOri = reader.IsDBNull(41) ? null : TimeSpan.Parse(reader.GetString(41)),
                        TimeNFOri = reader.IsDBNull(42) ? null : TimeSpan.Parse(reader.GetString(42)),
                        TimeNrPaOri = reader.IsDBNull(43) ? null : reader.GetDecimal(43),
                        TimeNrNFOri = reader.IsDBNull(44) ? null : reader.GetDecimal(44),
                        TimeNrTotOri = reader.IsDBNull(45) ? null : reader.GetDecimal(45),
                        TimeNrNetOri = reader.IsDBNull(46) ? null : reader.GetDecimal(46),
                        CreatedDate = reader.IsDBNull(47) ? null : reader.GetDateTime(47),
                        CreatedBy = reader.IsDBNull(48) ? null : reader.GetString(48),
                        ModifiedDate = reader.IsDBNull(49) ? null : reader.GetDateTime(49),
                        ModifiedBy = reader.IsDBNull(50) ? null : reader.GetString(50),
                        // Campi di compatibilità
                        EmployeeId = reader.IsDBNull(3) ? string.Empty : reader.GetString(3), // Mappato su ResId
                        ProjectId = reader.IsDBNull(8) ? string.Empty : reader.GetString(8), // Mappato su Project
                        Hours = reader.IsDBNull(23) ? 0 : reader.GetDecimal(23), // Mappato su TimeNrNet
                        Description = reader.IsDBNull(26) ? null : reader.GetString(26) // Mappato su DescExt
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheets for employee {EmployeeId} from SAP HANA", employeeId);
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
                        DocEntry,
                        DocNum,
                        Code,
                        U_ResId as ResId,
                        U_CardCode as CardCode,
                        U_CardName as CardName,
                        U_RefId as RefId,
                        U_RefData as RefData,
                        U_Project as Project,
                        U_ProjectName as ProjectName,
                        U_SubProject as SubProject,
                        U_Activity as Activity,
                        U_ActivityId as ActivityId,
                        U_SubActivity as SubActivity,
                        U_ActivityName as ActivityName,
                        U_Date as Date,
                        U_TimeStart as TimeStart,
                        U_TimeEnd as TimeEnd,
                        U_TimePa as TimePa,
                        U_TimeNF as TimeNF,
                        U_TimeNrPa as TimeNrPa,
                        U_TimeNrNF as TimeNrNF,
                        U_TimeNrTot as TimeNrTot,
                        U_TimeNrNet as TimeNrNet,
                        U_Price as Price,
                        U_LineTotal as LineTotal,
                        U_DescExt as DescExt,
                        U_DescInt as DescInt,
                        U_ItemCode as ItemCode,
                        U_Status as Status,
                        U_Approver as Approver,
                        U_InvEntry as InvEntry,
                        U_DestType as DestType,
                        U_DestEntry as DestEntry,
                        U_DestLine as DestLine,
                        U_BaseType as BaseType,
                        U_BaseEntry as BaseEntry,
                        U_BaseLine as BaseLine,
                        U_DescExtOri as DescExtOri,
                        U_TimeStartOri as TimeStartOri,
                        U_TimeEndOri as TimeEndOri,
                        U_TimePaOri as TimePaOri,
                        U_TimeNFOri as TimeNFOri,
                        U_TimeNrPaOri as TimeNrPaOri,
                        U_TimeNrNFOri as TimeNrNFOri,
                        U_TimeNrTotOri as TimeNrTotOri,
                        U_TimeNrNetOri as TimeNrNetOri,
                        CreateDate as CreatedDate,
                        CreateTS as CreatedBy,
                        UpdateDate as ModifiedDate,
                        UpdateTS as ModifiedBy
                    FROM [@TIMESHEET]
                    WHERE U_Project = ?
                    ORDER BY U_Date DESC";
                
                using var command = new OdbcCommand(query, connection);
                command.Parameters.AddWithValue("@Project", projectId);
                
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    timesheets.Add(new Timesheet
                    {
                        DocEntry = reader.IsDBNull(0) ? null : (int)reader.GetInt32(0),
                        DocNum = reader.GetString(1),
                        Code = reader.IsDBNull(2) ? null : reader.GetString(2),
                        ResId = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CardCode = reader.IsDBNull(4) ? null : reader.GetString(4),
                        CardName = reader.IsDBNull(5) ? null : reader.GetString(5),
                        RefId = reader.IsDBNull(6) ? null : reader.GetString(6),
                        RefData = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Project = reader.IsDBNull(8) ? null : reader.GetString(8),
                        ProjectName = reader.IsDBNull(9) ? null : reader.GetString(9),
                        SubProject = reader.IsDBNull(10) ? null : reader.GetString(10),
                        Activity = reader.IsDBNull(11) ? null : reader.GetString(11),
                        ActivityId = reader.IsDBNull(12) ? null : reader.GetString(12),
                        SubActivity = reader.IsDBNull(13) ? null : reader.GetString(13),
                        ActivityName = reader.IsDBNull(14) ? null : reader.GetString(14),
                        Date = reader.GetDateTime(15),
                        TimeStart = reader.IsDBNull(16) ? null : TimeSpan.Parse(reader.GetString(16)),
                        TimeEnd = reader.IsDBNull(17) ? null : TimeSpan.Parse(reader.GetString(17)),
                        TimePa = reader.IsDBNull(18) ? null : TimeSpan.Parse(reader.GetString(18)),
                        TimeNF = reader.IsDBNull(19) ? null : TimeSpan.Parse(reader.GetString(19)),
                        TimeNrPa = reader.IsDBNull(20) ? null : reader.GetDecimal(20),
                        TimeNrNF = reader.IsDBNull(21) ? null : reader.GetDecimal(21),
                        TimeNrTot = reader.IsDBNull(22) ? null : reader.GetDecimal(22),
                        TimeNrNet = reader.IsDBNull(23) ? null : reader.GetDecimal(23),
                        Price = reader.IsDBNull(24) ? null : reader.GetDecimal(24),
                        LineTotal = reader.IsDBNull(25) ? null : reader.GetDecimal(25),
                        DescExt = reader.IsDBNull(26) ? null : reader.GetString(26),
                        DescInt = reader.IsDBNull(27) ? null : reader.GetString(27),
                        ItemCode = reader.IsDBNull(28) ? null : reader.GetString(28),
                        Status = reader.IsDBNull(29) ? null : reader.GetString(29),
                        Approver = reader.IsDBNull(30) ? null : reader.GetString(30),
                        InvEntry = reader.IsDBNull(31) ? null : reader.GetString(31),
                        DestType = reader.IsDBNull(32) ? null : reader.GetString(32),
                        DestEntry = reader.IsDBNull(33) ? null : reader.GetString(33),
                        DestLine = reader.IsDBNull(34) ? null : (int)reader.GetInt32(34),
                        BaseType = reader.IsDBNull(35) ? null : reader.GetString(35),
                        BaseEntry = reader.IsDBNull(36) ? null : reader.GetString(36),
                        BaseLine = reader.IsDBNull(37) ? null : (int)reader.GetInt32(37),
                        DescExtOri = reader.IsDBNull(38) ? null : reader.GetString(38),
                        TimeStartOri = reader.IsDBNull(39) ? null : TimeSpan.Parse(reader.GetString(39)),
                        TimeEndOri = reader.IsDBNull(40) ? null : TimeSpan.Parse(reader.GetString(40)),
                        TimePaOri = reader.IsDBNull(41) ? null : TimeSpan.Parse(reader.GetString(41)),
                        TimeNFOri = reader.IsDBNull(42) ? null : TimeSpan.Parse(reader.GetString(42)),
                        TimeNrPaOri = reader.IsDBNull(43) ? null : reader.GetDecimal(43),
                        TimeNrNFOri = reader.IsDBNull(44) ? null : reader.GetDecimal(44),
                        TimeNrTotOri = reader.IsDBNull(45) ? null : reader.GetDecimal(45),
                        TimeNrNetOri = reader.IsDBNull(46) ? null : reader.GetDecimal(46),
                        CreatedDate = reader.IsDBNull(47) ? null : reader.GetDateTime(47),
                        CreatedBy = reader.IsDBNull(48) ? null : reader.GetString(48),
                        ModifiedDate = reader.IsDBNull(49) ? null : reader.GetDateTime(49),
                        ModifiedBy = reader.IsDBNull(50) ? null : reader.GetString(50),
                        // Campi di compatibilità
                        EmployeeId = reader.IsDBNull(3) ? string.Empty : reader.GetString(3), // Mappato su ResId
                        ProjectId = reader.IsDBNull(8) ? string.Empty : reader.GetString(8), // Mappato su Project
                        Hours = reader.IsDBNull(23) ? 0 : reader.GetDecimal(23), // Mappato su TimeNrNet
                        Description = reader.IsDBNull(26) ? null : reader.GetString(26) // Mappato su DescExt
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheets for project {ProjectId} from SAP HANA", projectId);
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
                        DocEntry,
                        DocNum,
                        Code,
                        U_ResId as ResId,
                        U_CardCode as CardCode,
                        U_CardName as CardName,
                        U_RefId as RefId,
                        U_RefData as RefData,
                        U_Project as Project,
                        U_ProjectName as ProjectName,
                        U_SubProject as SubProject,
                        U_Activity as Activity,
                        U_ActivityId as ActivityId,
                        U_SubActivity as SubActivity,
                        U_ActivityName as ActivityName,
                        U_Date as Date,
                        U_TimeStart as TimeStart,
                        U_TimeEnd as TimeEnd,
                        U_TimePa as TimePa,
                        U_TimeNF as TimeNF,
                        U_TimeNrPa as TimeNrPa,
                        U_TimeNrNF as TimeNrNF,
                        U_TimeNrTot as TimeNrTot,
                        U_TimeNrNet as TimeNrNet,
                        U_Price as Price,
                        U_LineTotal as LineTotal,
                        U_DescExt as DescExt,
                        U_DescInt as DescInt,
                        U_ItemCode as ItemCode,
                        U_Status as Status,
                        U_Approver as Approver,
                        U_InvEntry as InvEntry,
                        U_DestType as DestType,
                        U_DestEntry as DestEntry,
                        U_DestLine as DestLine,
                        U_BaseType as BaseType,
                        U_BaseEntry as BaseEntry,
                        U_BaseLine as BaseLine,
                        U_DescExtOri as DescExtOri,
                        U_TimeStartOri as TimeStartOri,
                        U_TimeEndOri as TimeEndOri,
                        U_TimePaOri as TimePaOri,
                        U_TimeNFOri as TimeNFOri,
                        U_TimeNrPaOri as TimeNrPaOri,
                        U_TimeNrNFOri as TimeNrNFOri,
                        U_TimeNrTotOri as TimeNrTotOri,
                        U_TimeNrNetOri as TimeNrNetOri,
                        CreateDate as CreatedDate,
                        CreateTS as CreatedBy,
                        UpdateDate as ModifiedDate,
                        UpdateTS as ModifiedBy
                    FROM [@TIMESHEET]
                    WHERE U_Date BETWEEN ? AND ?
                    ORDER BY U_Date DESC";
                
                using var command = new OdbcCommand(query, connection);
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);
                
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    timesheets.Add(new Timesheet
                    {
                        DocEntry = reader.IsDBNull(0) ? null : (int)reader.GetInt32(0),
                        DocNum = reader.GetString(1),
                        Code = reader.IsDBNull(2) ? null : reader.GetString(2),
                        ResId = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CardCode = reader.IsDBNull(4) ? null : reader.GetString(4),
                        CardName = reader.IsDBNull(5) ? null : reader.GetString(5),
                        RefId = reader.IsDBNull(6) ? null : reader.GetString(6),
                        RefData = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Project = reader.IsDBNull(8) ? null : reader.GetString(8),
                        ProjectName = reader.IsDBNull(9) ? null : reader.GetString(9),
                        SubProject = reader.IsDBNull(10) ? null : reader.GetString(10),
                        Activity = reader.IsDBNull(11) ? null : reader.GetString(11),
                        ActivityId = reader.IsDBNull(12) ? null : reader.GetString(12),
                        SubActivity = reader.IsDBNull(13) ? null : reader.GetString(13),
                        ActivityName = reader.IsDBNull(14) ? null : reader.GetString(14),
                        Date = reader.GetDateTime(15),
                        TimeStart = reader.IsDBNull(16) ? null : TimeSpan.Parse(reader.GetString(16)),
                        TimeEnd = reader.IsDBNull(17) ? null : TimeSpan.Parse(reader.GetString(17)),
                        TimePa = reader.IsDBNull(18) ? null : TimeSpan.Parse(reader.GetString(18)),
                        TimeNF = reader.IsDBNull(19) ? null : TimeSpan.Parse(reader.GetString(19)),
                        TimeNrPa = reader.IsDBNull(20) ? null : reader.GetDecimal(20),
                        TimeNrNF = reader.IsDBNull(21) ? null : reader.GetDecimal(21),
                        TimeNrTot = reader.IsDBNull(22) ? null : reader.GetDecimal(22),
                        TimeNrNet = reader.IsDBNull(23) ? null : reader.GetDecimal(23),
                        Price = reader.IsDBNull(24) ? null : reader.GetDecimal(24),
                        LineTotal = reader.IsDBNull(25) ? null : reader.GetDecimal(25),
                        DescExt = reader.IsDBNull(26) ? null : reader.GetString(26),
                        DescInt = reader.IsDBNull(27) ? null : reader.GetString(27),
                        ItemCode = reader.IsDBNull(28) ? null : reader.GetString(28),
                        Status = reader.IsDBNull(29) ? null : reader.GetString(29),
                        Approver = reader.IsDBNull(30) ? null : reader.GetString(30),
                        InvEntry = reader.IsDBNull(31) ? null : reader.GetString(31),
                        DestType = reader.IsDBNull(32) ? null : reader.GetString(32),
                        DestEntry = reader.IsDBNull(33) ? null : reader.GetString(33),
                        DestLine = reader.IsDBNull(34) ? null : (int)reader.GetInt32(34),
                        BaseType = reader.IsDBNull(35) ? null : reader.GetString(35),
                        BaseEntry = reader.IsDBNull(36) ? null : reader.GetString(36),
                        BaseLine = reader.IsDBNull(37) ? null : (int)reader.GetInt32(37),
                        DescExtOri = reader.IsDBNull(38) ? null : reader.GetString(38),
                        TimeStartOri = reader.IsDBNull(39) ? null : TimeSpan.Parse(reader.GetString(39)),
                        TimeEndOri = reader.IsDBNull(40) ? null : TimeSpan.Parse(reader.GetString(40)),
                        TimePaOri = reader.IsDBNull(41) ? null : TimeSpan.Parse(reader.GetString(41)),
                        TimeNFOri = reader.IsDBNull(42) ? null : TimeSpan.Parse(reader.GetString(42)),
                        TimeNrPaOri = reader.IsDBNull(43) ? null : reader.GetDecimal(43),
                        TimeNrNFOri = reader.IsDBNull(44) ? null : reader.GetDecimal(44),
                        TimeNrTotOri = reader.IsDBNull(45) ? null : reader.GetDecimal(45),
                        TimeNrNetOri = reader.IsDBNull(46) ? null : reader.GetDecimal(46),
                        CreatedDate = reader.IsDBNull(47) ? null : reader.GetDateTime(47),
                        CreatedBy = reader.IsDBNull(48) ? null : reader.GetString(48),
                        ModifiedDate = reader.IsDBNull(49) ? null : reader.GetDateTime(49),
                        ModifiedBy = reader.IsDBNull(50) ? null : reader.GetString(50),
                        // Campi di compatibilità
                        EmployeeId = reader.IsDBNull(3) ? string.Empty : reader.GetString(3), // Mappato su ResId
                        ProjectId = reader.IsDBNull(8) ? string.Empty : reader.GetString(8), // Mappato su Project
                        Hours = reader.IsDBNull(23) ? 0 : reader.GetDecimal(23), // Mappato su TimeNrNet
                        Description = reader.IsDBNull(26) ? null : reader.GetString(26) // Mappato su DescExt
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheets for date range {StartDate} to {EndDate} from SAP HANA", startDate, endDate);
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
                        DocEntry,
                        DocNum,
                        Code,
                        U_ResId as ResId,
                        U_CardCode as CardCode,
                        U_CardName as CardName,
                        U_RefId as RefId,
                        U_RefData as RefData,
                        U_Project as Project,
                        U_ProjectName as ProjectName,
                        U_SubProject as SubProject,
                        U_Activity as Activity,
                        U_ActivityId as ActivityId,
                        U_SubActivity as SubActivity,
                        U_ActivityName as ActivityName,
                        U_Date as Date,
                        U_TimeStart as TimeStart,
                        U_TimeEnd as TimeEnd,
                        U_TimePa as TimePa,
                        U_TimeNF as TimeNF,
                        U_TimeNrPa as TimeNrPa,
                        U_TimeNrNF as TimeNrNF,
                        U_TimeNrTot as TimeNrTot,
                        U_TimeNrNet as TimeNrNet,
                        U_Price as Price,
                        U_LineTotal as LineTotal,
                        U_DescExt as DescExt,
                        U_DescInt as DescInt,
                        U_ItemCode as ItemCode,
                        U_Status as Status,
                        U_Approver as Approver,
                        U_InvEntry as InvEntry,
                        U_DestType as DestType,
                        U_DestEntry as DestEntry,
                        U_DestLine as DestLine,
                        U_BaseType as BaseType,
                        U_BaseEntry as BaseEntry,
                        U_BaseLine as BaseLine,
                        U_DescExtOri as DescExtOri,
                        U_TimeStartOri as TimeStartOri,
                        U_TimeEndOri as TimeEndOri,
                        U_TimePaOri as TimePaOri,
                        U_TimeNFOri as TimeNFOri,
                        U_TimeNrPaOri as TimeNrPaOri,
                        U_TimeNrNFOri as TimeNrNFOri,
                        U_TimeNrTotOri as TimeNrTotOri,
                        U_TimeNrNetOri as TimeNrNetOri,
                        CreateDate as CreatedDate,
                        CreateTS as CreatedBy,
                        UpdateDate as ModifiedDate,
                        UpdateTS as ModifiedBy
                    FROM [@TIMESHEET]
                    WHERE U_ResId = ? AND U_Date BETWEEN ? AND ?
                    ORDER BY U_Date DESC";
                
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
                        DocNum = reader.GetString(1),
                        Code = reader.IsDBNull(2) ? null : reader.GetString(2),
                        ResId = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CardCode = reader.IsDBNull(4) ? null : reader.GetString(4),
                        CardName = reader.IsDBNull(5) ? null : reader.GetString(5),
                        RefId = reader.IsDBNull(6) ? null : reader.GetString(6),
                        RefData = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Project = reader.IsDBNull(8) ? null : reader.GetString(8),
                        ProjectName = reader.IsDBNull(9) ? null : reader.GetString(9),
                        SubProject = reader.IsDBNull(10) ? null : reader.GetString(10),
                        Activity = reader.IsDBNull(11) ? null : reader.GetString(11),
                        ActivityId = reader.IsDBNull(12) ? null : reader.GetString(12),
                        SubActivity = reader.IsDBNull(13) ? null : reader.GetString(13),
                        ActivityName = reader.IsDBNull(14) ? null : reader.GetString(14),
                        Date = reader.GetDateTime(15),
                        TimeStart = reader.IsDBNull(16) ? null : TimeSpan.Parse(reader.GetString(16)),
                        TimeEnd = reader.IsDBNull(17) ? null : TimeSpan.Parse(reader.GetString(17)),
                        TimePa = reader.IsDBNull(18) ? null : TimeSpan.Parse(reader.GetString(18)),
                        TimeNF = reader.IsDBNull(19) ? null : TimeSpan.Parse(reader.GetString(19)),
                        TimeNrPa = reader.IsDBNull(20) ? null : reader.GetDecimal(20),
                        TimeNrNF = reader.IsDBNull(21) ? null : reader.GetDecimal(21),
                        TimeNrTot = reader.IsDBNull(22) ? null : reader.GetDecimal(22),
                        TimeNrNet = reader.IsDBNull(23) ? null : reader.GetDecimal(23),
                        Price = reader.IsDBNull(24) ? null : reader.GetDecimal(24),
                        LineTotal = reader.IsDBNull(25) ? null : reader.GetDecimal(25),
                        DescExt = reader.IsDBNull(26) ? null : reader.GetString(26),
                        DescInt = reader.IsDBNull(27) ? null : reader.GetString(27),
                        ItemCode = reader.IsDBNull(28) ? null : reader.GetString(28),
                        Status = reader.IsDBNull(29) ? null : reader.GetString(29),
                        Approver = reader.IsDBNull(30) ? null : reader.GetString(30),
                        InvEntry = reader.IsDBNull(31) ? null : reader.GetString(31),
                        DestType = reader.IsDBNull(32) ? null : reader.GetString(32),
                        DestEntry = reader.IsDBNull(33) ? null : reader.GetString(33),
                        DestLine = reader.IsDBNull(34) ? null : (int)reader.GetInt32(34),
                        BaseType = reader.IsDBNull(35) ? null : reader.GetString(35),
                        BaseEntry = reader.IsDBNull(36) ? null : reader.GetString(36),
                        BaseLine = reader.IsDBNull(37) ? null : (int)reader.GetInt32(37),
                        DescExtOri = reader.IsDBNull(38) ? null : reader.GetString(38),
                        TimeStartOri = reader.IsDBNull(39) ? null : TimeSpan.Parse(reader.GetString(39)),
                        TimeEndOri = reader.IsDBNull(40) ? null : TimeSpan.Parse(reader.GetString(40)),
                        TimePaOri = reader.IsDBNull(41) ? null : TimeSpan.Parse(reader.GetString(41)),
                        TimeNFOri = reader.IsDBNull(42) ? null : TimeSpan.Parse(reader.GetString(42)),
                        TimeNrPaOri = reader.IsDBNull(43) ? null : reader.GetDecimal(43),
                        TimeNrNFOri = reader.IsDBNull(44) ? null : reader.GetDecimal(44),
                        TimeNrTotOri = reader.IsDBNull(45) ? null : reader.GetDecimal(45),
                        TimeNrNetOri = reader.IsDBNull(46) ? null : reader.GetDecimal(46),
                        CreatedDate = reader.IsDBNull(47) ? null : reader.GetDateTime(47),
                        CreatedBy = reader.IsDBNull(48) ? null : reader.GetString(48),
                        ModifiedDate = reader.IsDBNull(49) ? null : reader.GetDateTime(49),
                        ModifiedBy = reader.IsDBNull(50) ? null : reader.GetString(50),
                        // Campi di compatibilità
                        EmployeeId = reader.IsDBNull(3) ? string.Empty : reader.GetString(3), // Mappato su ResId
                        ProjectId = reader.IsDBNull(8) ? string.Empty : reader.GetString(8), // Mappato su Project
                        Hours = reader.IsDBNull(23) ? 0 : reader.GetDecimal(23), // Mappato su TimeNrNet
                        Description = reader.IsDBNull(26) ? null : reader.GetString(26) // Mappato su DescExt
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheets for employee {EmployeeId} in date range {StartDate} to {EndDate} from SAP HANA", employeeId, startDate, endDate);
                throw;
            }
            
            return timesheets;
        }
    }
}
