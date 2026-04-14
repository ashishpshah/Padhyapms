using Microsoft.Data.SqlClient;
using System.Data;

namespace PMMS.Infra
{

    public class LogEntry
    {
        
        public static void InsertLogEntry(ErrorLog log)





        {
            if (log == null) throw new ArgumentNullException(nameof(log));

            SqlParameter[] spParams = new SqlParameter[]
            {
                new SqlParameter("@ControllerName", SqlDbType.VarChar, 200) { Value = (object)log.ControllerName ?? DBNull.Value },
                new SqlParameter("@ActionName", SqlDbType.VarChar, 200) { Value = (object)log.ActionName ?? DBNull.Value },
                new SqlParameter("@ErrorMessage", SqlDbType.VarChar, -1) { Value = (object)log.ErrorMessage ?? DBNull.Value },
                new SqlParameter("@ErrorType", SqlDbType.VarChar, 200) { Value = (object)log.ErrorType ?? DBNull.Value },            
                new SqlParameter("@UserId", SqlDbType.BigInt) { Value = Common.LoggedUser_Id() },
                new SqlParameter("@ClientIP", SqlDbType.VarChar, 50) { Value = (object)log.ClientIP ?? DBNull.Value },
            };

            ExecuteSPForLogEntry("usp_ErrorLog_Insert", spParams);
        }

        
        public static void InsertLogEntryFromException(
            Exception ex,
             string? controllerName = null,
            string? actionName = null,          
            string? clientIP = null
            )
        {
            if (ex == null) return;

            var log = new ErrorLog
            {
                ActionName = actionName,
                ControllerName = controllerName,
                ErrorMessage = ex.Message,
                ErrorType = ex.GetType().FullName,
                ClientIP = clientIP
            };

            InsertLogEntry(log);
        }

        public static void ExecuteSPForLogEntry(string sp, SqlParameter[] spCol)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(AppHttpContextAccessor.DataConnectionString))
                using (SqlCommand cmd = new SqlCommand(sp, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (spCol != null && spCol.Length > 0)
                    {
                        cmd.Parameters.AddRange(spCol);
                    }

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
              
                try
                {
                    System.Diagnostics.Trace.TraceError($"LogEntry.ExecuteSPForLogEntry failed for SP '{sp}': {ex}");
                }
                catch
                {
                    
                }
            }
        }
    }
 }
