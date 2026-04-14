using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PMMS.Infra;
using PMMS.Models;
using System.Data;

namespace PMMS.Areas.Admin.Services
{
    public class ClientServices 
    {
        public static List<Client> GET(long id = 0)
        {
            var listObj = new List<Client>();
            string actionName = "GET";
            string controllerName = "Client";
            string clientIp = AppHttpContextAccessor.AppHttpContext?.Connection?.RemoteIpAddress?.ToString();

            try
            {
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = id, Direction = ParameterDirection.Input, IsNullable = true });

                var dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Client_Get", parameters.ToList(), false, actionName, controllerName);

                if (dt != null && dt.Rows.Count > 0)
                    foreach (DataRow dr in dt.Rows)
                        listObj.Add(new Client()
                        {
                            Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
                            Name = dr["Name"] != DBNull.Value ? Convert.ToString(dr["Name"]) : "",
                            Email = dr["Email"] != DBNull.Value ? Convert.ToString(dr["Email"]) : "",
                            Phone = dr["Phone"] != DBNull.Value ? Convert.ToString(dr["Phone"]) : "",
                            CompanyName = dr["CompanyName"] != DBNull.Value ? Convert.ToString(dr["CompanyName"]) : "",
                            Website = dr["Website"] != DBNull.Value ? Convert.ToString(dr["Website"]) : "",
                         });
            }
            catch (Exception ex)
            {
                LogEntry.InsertLogEntryFromException(ex, controllerName, actionName, clientIp);
            }

            return listObj;
        }

        public static (bool, string, long) Save(Client obj = null)
        {
            string actionName = "Save";
            string controllerName = "Client";
            string clientIp = AppHttpContextAccessor.AppHttpContext?
                .Connection?
                .RemoteIpAddress?
                .ToString();

            if (obj != null)
                try
                {
                    var parameters = new List<SqlParameter>();

                    parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = obj.Id, Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Name", SqlDbType.NVarChar) { Value = obj.Name ?? "", Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Email", SqlDbType.NVarChar) { Value = obj.Email ?? "", Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Phone", SqlDbType.NVarChar) { Value = obj.Phone ?? "", Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("CompanyName", SqlDbType.NVarChar) { Value = obj.CompanyName ?? "", Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Website", SqlDbType.NVarChar) { Value = obj.Website ?? "", Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Operated_By", SqlDbType.BigInt)
                    {
                        Value = Common.Get_Session_Int(SessionKey.KEY_USER_ID),
                        Direction = ParameterDirection.Input,
                        IsNullable = true
                    });

                    parameters.Add(new SqlParameter("Action", SqlDbType.NVarChar)
                    {
                        Value = obj.Id > 0 ? "UPDATE" : "INSERT",
                        Direction = ParameterDirection.Input,
                        IsNullable = true
                    });
                    var response = DataContext_Command.ExecuteStoredProcedure("SP_Client_Save", parameters.ToArray(), actionName, controllerName);

                    var msgtype = response.Split('|').Length > 0 ? response.Split('|')[0] : "";
                    var message = response.Split('|').Length > 1 ? response.Split('|')[1].Replace("\"", "") : "";
                    var strid = response.Split('|').Length > 2 ? response.Split('|')[2].Replace("\"", "") ?? "0" : "0";

                    
                    return (msgtype.Contains("S"), message, Convert.ToInt64(strid));
                }
                catch (Exception ex)
                {
                    LogEntry.InsertLogEntryFromException(ex, controllerName, actionName, clientIp);
                }

            return (false, ResponseStatusMessage.Error, 0);
        }

        public static (bool, string) Delete(long Id = 0)
        {
            string actionName = "Delete";
            string controllerName = "Client";
            string clientIp = AppHttpContextAccessor.AppHttpContext?
                .Connection?
                .RemoteIpAddress?
                .ToString();

            if (Id > 0)
                try
                {
                    var parameters = new List<SqlParameter>();

                    parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = Id, Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Operated_By", SqlDbType.BigInt)
                    {
                        Value = Common.Get_Session_Int(SessionKey.KEY_USER_ID),
                        Direction = ParameterDirection.Input,
                        IsNullable = true
                    });

                    var response = DataContext_Command.ExecuteStoredProcedure("SP_Client_Delete", parameters.ToArray(), actionName, controllerName);

                    var msgtype = response.Split('|').Length > 0 ? response.Split('|')[0] : "";
                    var message = response.Split('|').Length > 1 ? response.Split('|')[1].Replace("\"", "") : "";
                    var strid = response.Split('|').Length > 2 ? response.Split('|')[2].Replace("\"", "") ?? "0" : "0";

                    

                    return (msgtype.Contains("S"), message);
                }
                catch (Exception ex)
                {
                    LogEntry.InsertLogEntryFromException(ex, controllerName, actionName, clientIp);
                }

            return (false, ResponseStatusMessage.Error);
        }
    }
}