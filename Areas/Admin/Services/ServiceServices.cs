using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PMMS.Infra;
using PMMS.Models;
using System.Data;

namespace PMMS.Areas.Admin.Services
{
    public class ServiceServices 
    {
        public static List<Service> GET(long id = 0)
        {
            var listObj = new List<Service>();
            string actionName = "GET";
            string controllerName = "Service";
            string clientIp = AppHttpContextAccessor.AppHttpContext?.Connection?.RemoteIpAddress?.ToString();

            try
            {
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = id, Direction = ParameterDirection.Input, IsNullable = true });

                var dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Services_Get", parameters.ToList(), false, actionName, controllerName);

                if (dt != null && dt.Rows.Count > 0)
                    foreach (DataRow dr in dt.Rows)
                        listObj.Add(new Service()
                        {
                            Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
                            ServiceName = dr["ServiceName"] != DBNull.Value ? Convert.ToString(dr["ServiceName"]) : "",
                            Slug = dr["Slug"] != DBNull.Value ? Convert.ToString(dr["Slug"]) : "",
                            ShortDescription = dr["ShortDescription"] != DBNull.Value ? Convert.ToString(dr["ShortDescription"]) : "",
                            DisplayOrder = dr["DisplayOrder"] != DBNull.Value ? Convert.ToInt32(dr["DisplayOrder"]) : 0,
                            Title = dr["Title"] != DBNull.Value ? Convert.ToString(dr["Title"]) : "",
                            Description = dr["Description"] != DBNull.Value ? Convert.ToString(dr["Description"]) : "",
                            BestFor = dr["BestFor"] != DBNull.Value ? Convert.ToString(dr["BestFor"]) : "",
                            Technologies = dr["Technologies"] != DBNull.Value ? Convert.ToString(dr["Technologies"]) : "",
                         });
            }
            catch (Exception ex)
            {
                LogEntry.InsertLogEntryFromException(ex, controllerName, actionName, clientIp);
            }

            return listObj;
        }



        public static List<ServiceChecklist> GET_ServiceChecklist(long id = 0)
        {
            var listObj = new List<ServiceChecklist>();
            string actionName = "GET";
            string controllerName = "Service";
            string clientIp = AppHttpContextAccessor.AppHttpContext?.Connection?.RemoteIpAddress?.ToString();

            try
            {
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = id, Direction = ParameterDirection.Input, IsNullable = true });

                var dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Services_Checklist_Get", parameters.ToList(), false, actionName, controllerName);

                if (dt != null && dt.Rows.Count > 0)
                    foreach (DataRow dr in dt.Rows)
                        listObj.Add(new ServiceChecklist()
                        {
                            Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
                            ServiceId = dr["ServiceId"] != DBNull.Value ? Convert.ToInt64(dr["ServiceId"]) : 0,
                            ItemName = dr["ItemName"] != DBNull.Value ? Convert.ToString(dr["ItemName"]) : "",
                            DisplayOrder = dr["DisplayOrder"] != DBNull.Value ? Convert.ToInt32(dr["DisplayOrder"]) : 0,
                        });
            }
            catch (Exception ex)
            {
                LogEntry.InsertLogEntryFromException(ex, controllerName, actionName, clientIp);
            }

            return listObj;
        }

        public static (bool, string, long) Save(Service obj = null)
        {
            string actionName = "Save";
            string controllerName = "Service";
            string clientIp = AppHttpContextAccessor.AppHttpContext?
                .Connection?
                .RemoteIpAddress?
                .ToString();

            if (obj != null)
                try
                {
                    DataTable ServiceChecklist_table = new DataTable();
                    ServiceChecklist_table.Columns.Add("ID", typeof(long));                    
                    ServiceChecklist_table.Columns.Add("ItemName", typeof(string));
                    ServiceChecklist_table.Columns.Add("DisplayOrder", typeof(int));
                    

                    if (obj != null && obj.ServiceChecklist.Count > 0)
                    {
                        foreach (var servicechecklist in obj.ServiceChecklist)
                        {
                            ServiceChecklist_table.Rows.Add(servicechecklist.Id, servicechecklist.ItemName, servicechecklist.DisplayOrder);
                        }
                    }
                    var parameters = new List<SqlParameter>();

                    parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = obj.Id, Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("ServiceName", SqlDbType.NVarChar) { Value = obj.ServiceName ?? "", Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Slug", SqlDbType.NVarChar) { Value = obj.Slug ?? "", Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("ShortDescription", SqlDbType.NVarChar) { Value = obj.ShortDescription ?? "", Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Title", SqlDbType.NVarChar) { Value = obj.Title ?? "", Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Description", SqlDbType.NVarChar) { Value = obj.Description ?? "", Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("BestFor", SqlDbType.NVarChar) { Value = obj.BestFor ?? "", Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Technologies", SqlDbType.NVarChar) { Value = obj.Technologies ?? "", Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Type_ServiceChecklist", SqlDbType.Structured) { Value = ServiceChecklist_table, Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("DisplayOrder", SqlDbType.Int) { Value = obj.DisplayOrder, Direction = ParameterDirection.Input, IsNullable = true });
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
                    var response = DataContext_Command.ExecuteStoredProcedure("SP_Services_Save", parameters.ToArray(), actionName, controllerName);

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
            string controllerName = "Service";
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

                    var response = DataContext_Command.ExecuteStoredProcedure("SP_Services_Delete", parameters.ToArray(), actionName, controllerName);

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


        public static (bool, string) Delete_ServiceCheckList(long Id = 0)
        {
            string actionName = "Delete";
            string controllerName = "Service";
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

                    var response = DataContext_Command.ExecuteStoredProcedure("SP_Services_Checklist_Delete", parameters.ToArray(), actionName, controllerName);

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