using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PMMS.Infra;
using PMMS.Models;
using System.Data;

namespace PMMS.Areas.Admin.Projects
{
    public class ProjectServices
    {
        public static List<Project> GET(long id = 0)
        {
            DateTime? nullDateTime = null;
            var listObj = new List<Project>();
            string actionName = "GET";
            string controllerName = "Project";
            string clientIp = AppHttpContextAccessor.AppHttpContext?.Connection?.RemoteIpAddress?.ToString();

            try
            {
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = id, Direction = ParameterDirection.Input, IsNullable = true });

                var dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Project_Get", parameters.ToList(), false, actionName, controllerName);

                if (dt != null && dt.Rows.Count > 0)
                    foreach (DataRow dr in dt.Rows)
                        listObj.Add(new Project()
                        {
                            Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
                            ClientId = dr["ClientId"] != DBNull.Value ? Convert.ToInt64(dr["ClientId"]) : 0,
                            ProjectTitle = dr["ProjectTitle"] != DBNull.Value ? Convert.ToString(dr["ProjectTitle"]) : "",
                            Service_Ids = dr["Service_Ids"] != DBNull.Value ? Convert.ToString(dr["Service_Ids"]) : "",
                            Service_Names = dr["Service_Names"] != DBNull.Value ? Convert.ToString(dr["Service_Names"]) : "",
                            Client_Name = dr["Client_Name"] != DBNull.Value ? Convert.ToString(dr["Client_Name"]) : "",
                            Budget = dr["Budget"] != DBNull.Value ? Convert.ToDecimal(dr["Budget"]) : 0,
                            Description = dr["Description"] != DBNull.Value ? Convert.ToString(dr["Description"]) : "",
                            StartDate = dr["StartDate"] != DBNull.Value ? Convert.ToDateTime(dr["StartDate"]) : nullDateTime,
                            EndDate = dr["EndDate"] != DBNull.Value ? Convert.ToDateTime(dr["EndDate"]) : nullDateTime,
                            Status = dr["Status"] != DBNull.Value ? Convert.ToString(dr["Status"]) : "",
                            Status_TEXT = dr["Status_TEXT"] != DBNull.Value ? Convert.ToString(dr["Status_TEXT"]) : "",
                            Priority = dr["Priority"] != DBNull.Value ? Convert.ToString(dr["Priority"]) : "",
                            Priority_TEXT = dr["Priority_TEXT"] != DBNull.Value ? Convert.ToString(dr["Priority_TEXT"]) : "",
                         });
            }
            catch (Exception ex)
            {
                LogEntry.InsertLogEntryFromException(ex, controllerName, actionName, clientIp);
            }

            return listObj;
        }



        public static List<ProjectFeatureList> GET_ProjectFeatureList(long id = 0 , string Service_Ids = "")
        {
            var listObj = new List<ProjectFeatureList>();
            string actionName = "GET_ProjectFeatureList";
            string controllerName = "Project";
            string clientIp = AppHttpContextAccessor.AppHttpContext?.Connection?.RemoteIpAddress?.ToString();

            try
            {
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = id, Direction = ParameterDirection.Input, IsNullable = true });
                parameters.Add(new SqlParameter("Service_Ids", SqlDbType.VarChar) { Value = Service_Ids, Direction = ParameterDirection.Input, IsNullable = true });

                var dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Project_Feature_List_Get", parameters.ToList(), false, actionName, controllerName);

                if (dt != null && dt.Rows.Count > 0)
                    foreach (DataRow dr in dt.Rows)
                        listObj.Add(new ProjectFeatureList()
                        {
                            Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
                            ProjectId = dr["ProjectId"] != DBNull.Value ? Convert.ToInt64(dr["ProjectId"]) : 0,
                            ChecklistId = dr["ChecklistId"] != DBNull.Value ? Convert.ToInt64(dr["ChecklistId"]) : 0,
                            ServiceId = dr["ServiceId"] != DBNull.Value ? Convert.ToInt64(dr["ServiceId"]) : 0,
                            Service_Name = dr["Service_Name"] != DBNull.Value ? Convert.ToString(dr["Service_Name"]) : "",
                            Project_Feature_Name = dr["Project_Feature_Name"] != DBNull.Value ? Convert.ToString(dr["Project_Feature_Name"]) : "",
                            IsIncluded = dr["IsIncluded"] != DBNull.Value ? Convert.ToBoolean(dr["IsIncluded"]) : false,
                            Notes = dr["Notes"] != DBNull.Value ? Convert.ToString(dr["Notes"]) : "",
                        });
            }
            catch (Exception ex)
            {
                LogEntry.InsertLogEntryFromException(ex, controllerName, actionName, clientIp);
            }

            return listObj;
        }

        public static (bool, string, long) Save(Project obj = null)
        {
            string actionName = "Save";
            string controllerName = "Project";
            string clientIp = AppHttpContextAccessor.AppHttpContext?
                .Connection?
                .RemoteIpAddress?
                .ToString();

            if (obj != null)
                try
                {
                    DataTable ProjectFeaturelist_table = new DataTable();
                    ProjectFeaturelist_table.Columns.Add("ID", typeof(long));
                    ProjectFeaturelist_table.Columns.Add("ChecklistId", typeof(long));
                    ProjectFeaturelist_table.Columns.Add("IsIncluded", typeof(bool));
                    ProjectFeaturelist_table.Columns.Add("Notes", typeof(string));
                    

                    if (obj != null && obj.ProjectFeatureList.Count > 0)
                    {
                        foreach (var projectfeaturelist in obj.ProjectFeatureList)
                        {
                            ProjectFeaturelist_table.Rows.Add(projectfeaturelist.Id, projectfeaturelist.ChecklistId, projectfeaturelist.IsIncluded,projectfeaturelist.Notes);
                        }
                    }
                    var parameters = new List<SqlParameter>();

                    parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = obj.Id, Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("ClientId", SqlDbType.BigInt) { Value = obj.ClientId, Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("ProjectTitle", SqlDbType.NVarChar) { Value = obj.ProjectTitle ?? "", Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Description", SqlDbType.NVarChar) { Value = obj.Description ?? "", Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Service_Ids", SqlDbType.NVarChar) { Value = obj.Service_Ids ?? "", Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Budget", SqlDbType.Decimal) { Value = obj.Budget ?? 0, Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Timeline", SqlDbType.NVarChar) { Value = obj.Timeline ?? "", Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("StartDate", SqlDbType.DateTime) { Value = obj.StartDate, Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("EndDate", SqlDbType.DateTime) { Value = obj.EndDate, Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Status", SqlDbType.NVarChar) { Value = obj.Status ?? "", Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Priority", SqlDbType.NVarChar) { Value = obj.Priority ?? "", Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Type_ProjectFeaturelist", SqlDbType.Structured) { Value = ProjectFeaturelist_table, Direction = ParameterDirection.Input, IsNullable = true });
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
                    var response = DataContext_Command.ExecuteStoredProcedure("SP_Project_Save", parameters.ToArray(), actionName, controllerName);

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
            string controllerName = "Project";
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

                    var response = DataContext_Command.ExecuteStoredProcedure("SP_Project_Delete", parameters.ToArray(), actionName, controllerName);

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