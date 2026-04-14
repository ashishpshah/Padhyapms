using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using Newtonsoft.Json;

using System.Data;
using System.Text;

namespace PMMS.Infra
{
    public partial class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }


        public virtual DbSet<Menu> Menus { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<ForgotPassword> ForgotPassword { get; set; } = null!;
        public virtual DbSet<UserLog> UserLogs { get; set; }
        public virtual DbSet<UserMenuAccess> UserMenuAccesses { get; set; }
        public virtual DbSet<UserRoleMapping> UserRoleMappings { get; set; }
        public virtual DbSet<RoleMenuAccess> RoleMenuAccesses { get; set; }






        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.HasDefaultSchema("padhyaso_Leoz");



            modelBuilder.Entity<LOV>(entity =>
            {
                entity.ToTable("LOV", "dbo");
            });



            modelBuilder.Entity<UserRoleMapping>().ToTable("UserRoleMapping");
            modelBuilder.Entity<Menu>().ToTable("Menu");
            modelBuilder.Entity<UserMenuAccess>().ToTable("UserMenuAccess");
            modelBuilder.Entity<RoleMenuAccess>().ToTable("RoleMenuAccess");
            modelBuilder.Entity<User>().HasKey(e => new { e.Id });
            modelBuilder.Entity<Role>().HasKey(e => new { e.Id });
            modelBuilder.Entity<UserRoleMapping>().HasKey(e => new { e.Id });
            modelBuilder.Entity<Menu>().HasKey(e => new { e.Id });
            modelBuilder.Entity<UserMenuAccess>().HasKey(e => new { e.UserId, e.RoleId, e.MenuId, e.IsCreate, e.IsUpdate, e.IsRead, e.IsDelete });
            modelBuilder.Entity<RoleMenuAccess>().HasKey(e => new { e.RoleId, e.MenuId, e.IsCreate, e.IsUpdate, e.IsRead, e.IsDelete });
            modelBuilder.Entity<LOV>().HasNoKey();

            base.OnModelCreating(modelBuilder);
        }

        public int SaveChanges(CancellationToken cancellationToken = default)
        {
            var entities = ChangeTracker.Entries()
            .Where(x =>
                (x.State == EntityState.Added || x.State == EntityState.Modified)
                && x.Entity is EntitiesBase
            ).ToList();


            if (entities.Any())
            {
                var user = Common.LoggedUser_Id();


                if (user == null || user <= 0)
                    //throw new InvalidOperationException("Opps...! An unexpected error occurred while saving.");
                    user = 1;

                else
                {
                    foreach (var entity in entities)
                    {
                        if (entity.State == EntityState.Added)
                        {
                            ((EntitiesBase)entity.Entity).IsActive = true;
                            ((EntitiesBase)entity.Entity).IsDeleted = false;
                            ((EntitiesBase)entity.Entity).CreatedDate = DateTime.Now;
                            ((EntitiesBase)entity.Entity).CreatedBy = ((EntitiesBase)entity.Entity).CreatedBy == 0 ? user : ((EntitiesBase)entity.Entity).CreatedBy;
                            ((EntitiesBase)entity.Entity).LastModifiedDate = DateTime.Now;
                            ((EntitiesBase)entity.Entity).LastModifiedBy = ((EntitiesBase)entity.Entity).CreatedBy == 0 ? user : ((EntitiesBase)entity.Entity).CreatedBy;
                        }

                        if (entity.State == EntityState.Modified)
                        {
                            ((EntitiesBase)entity.Entity).LastModifiedDate = DateTime.Now;
                            ((EntitiesBase)entity.Entity).LastModifiedBy = user;
                        }

                        if (entity.State == EntityState.Deleted)
                        {
                            ((EntitiesBase)entity.Entity).IsActive = false;
                            ((EntitiesBase)entity.Entity).IsDeleted = true;
                            ((EntitiesBase)entity.Entity).LastModifiedDate = DateTime.Now;
                            ((EntitiesBase)entity.Entity).LastModifiedBy = user;
                        }

                    }
                }
            }

            return base.SaveChanges();
        }
    }


    public static class DataContext_Command
    {
        public static string _connectionString = AppHttpContextAccessor.AppConfiguration.GetSection("ConnectionStrings").GetSection("DataConnection").Value;

        public static string Get_DbSchemaName()
        {
            string keyValue = "database=";
            int startIndex = _connectionString.IndexOf(keyValue) + keyValue.Length;
            int endIndex = _connectionString.IndexOf(';', startIndex);
            return _connectionString.Substring(startIndex, endIndex - startIndex);
        }

        public static DataTable ExecuteQuery(string query, string ActionName = "", string controllername = "")
        {
            try
            {
                DataTable dt = new DataTable();

                SqlConnection connection = new SqlConnection(_connectionString);

                SqlDataAdapter oraAdapter = new SqlDataAdapter(query, connection);

                oraAdapter.Fill(dt);

                return dt;
            }
            catch (Exception ex)
            {
                string clientIp = AppHttpContextAccessor.AppHttpContext?
.Connection?
.RemoteIpAddress?
.ToString();
                LogEntry.InsertLogEntryFromException(ex, controllername, ActionName, clientIp);
                return null;
            }

        }

        public static DataSet ExecuteQuery_DataSet(string sqlquerys, string ActionName = "", string controllername = "")
        {
            DataSet ds = new DataSet();

            try
            {
                DataTable dt = new DataTable();

                SqlConnection connection = new SqlConnection(_connectionString);

                foreach (var sqlquery in sqlquerys.Split(";"))
                {
                    dt = new DataTable();

                    SqlDataAdapter oraAdapter = new SqlDataAdapter(sqlquery, connection);

                    SqlCommandBuilder oraBuilder = new SqlCommandBuilder(oraAdapter);

                    oraAdapter.Fill(dt);

                    if (dt != null)
                        ds.Tables.Add(dt);
                }

            }
            catch (Exception ex)
            {
                string clientIp = AppHttpContextAccessor.AppHttpContext?
   .Connection?
   .RemoteIpAddress?
   .ToString();
                LogEntry.InsertLogEntryFromException(ex, controllername, ActionName, clientIp);
                return null;
            }

            return ds;
        }

        public static DataTable ExecuteStoredProcedure_DataTable(string query, List<SqlParameter> parameters = null, bool returnParameter = false, string ActionName = "", string controllername = "")
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (parameters != null)
                            foreach (SqlParameter param in parameters)
                            {
                                if (param.Direction == ParameterDirection.Input) param.IsNullable = true;
                                cmd.Parameters.Add(param);
                            }

                        SqlDataAdapter da = new SqlDataAdapter(cmd);

                        da.Fill(dt);

                        parameters = null;
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                string clientIp = AppHttpContextAccessor.AppHttpContext?
   .Connection?
   .RemoteIpAddress?
   .ToString();
                LogEntry.InsertLogEntryFromException(ex, controllername, ActionName, clientIp);
                return null;
            }

            return dt;
        }

        public static DataSet ExecuteStoredProcedure_DataSet(string sp, List<SqlParameter> spCol = null, string ActionName = "", string controllername = "")
        {
            DataSet ds = new DataSet();

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(sp, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (spCol != null && spCol.Count > 0)
                            cmd.Parameters.AddRange(spCol.ToArray());

                        using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                        {
                            adp.Fill(ds);
                        }
                    }

                    con.Close();
                }
            }
            catch (Exception ex)
            {
                string clientIp = AppHttpContextAccessor.AppHttpContext?
    .Connection?
    .RemoteIpAddress?
    .ToString();
                LogEntry.InsertLogEntryFromException(ex, controllername, ActionName, clientIp);
            }

            return ds;
        }

        public static bool ExecuteNonQuery(string query, List<SqlParameter> parameters = null)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    SqlCommand cmd = con.CreateCommand();

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;

                    if (parameters != null)
                        foreach (SqlParameter param in parameters)
                            cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                LogService.LogInsert("ExecuteNonQuery - DataContext", "", ex);
                return false;
            }
        }

        public static (bool IsSuccess, string Message, long Id) ExecuteStoredProcedure(string query, List<SqlParameter> parameters, bool returnParameter = false, string ActionName = "", string controllername = "")
        {
            var response = string.Empty;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    try
                    {
                        con.Open();

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = query;
                        //cmd.DeriveParameters();

                        if (parameters != null && parameters.Count > 0)
                            cmd.Parameters.AddRange(parameters.ToArray());

                        if (returnParameter)
                            cmd.Parameters.Add(new SqlParameter("@response", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });

                        cmd.CommandTimeout = 86400;
                        cmd.ExecuteNonQuery();

                        //RETURN VALUE
                        //response = cmd.Parameters["P_Response"].Value.ToString();

                        response = "S|Success";

                        if (cmd.Parameters.Contains("@response"))
                        {
                            response = cmd.Parameters["@response"].Value.ToString();
                        }

                        con.Close();
                        cmd.Parameters.Clear();
                        cmd.Dispose();

                    }
                    catch (Exception ex)
                    {
                        con.Close();
                        cmd.Parameters.Clear();
                        cmd.Dispose();
                        string clientIp = AppHttpContextAccessor.AppHttpContext?
                    .Connection?
                    .RemoteIpAddress?
                    .ToString();
                        LogEntry.InsertLogEntryFromException(ex, controllername, ActionName, clientIp);
                        response = "E|Opps!... Something went wrong. " + JsonConvert.SerializeObject(ex) + "|0";
                    }
                }
            }

            if (!string.IsNullOrEmpty(response) && response.Contains("|"))
            {
                var msgtype = response.Split('|').Length > 0 ? Convert.ToString(response.Split('|')[0]) : "";
                var message = response.Split('|').Length > 1 ? Convert.ToString(response.Split('|')[1]).Replace("\"", "") : "";

                Int64 strid = 0;
                if (Int64.TryParse(response.Split('|').Length > 2 ? Convert.ToString(response.Split('|')[2]).Replace("\"", "") : "0", out strid)) { }
                //string paths = response.Split('|').Length > 3 ? response.Split('|')[3].Replace("\"", "") : "0";


                return (msgtype.Contains("S"), message, strid);
            }
            else
                return (false, ResponseStatusMessage.Error, 0);
        }


        public static (bool, string, long, string) ExecuteStoredProcedure_SQLwithpath(string query, List<SqlParameter> parameters, bool returnParameter = false, string ActionName = "", string controllername = "")
        {
            var response = string.Empty;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    try
                    {
                        con.Open();

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = query;
                        //cmd.DeriveParameters();

                        if (parameters != null && parameters.Count > 0)
                            cmd.Parameters.AddRange(parameters.ToArray());

                        if (returnParameter)
                            cmd.Parameters.Add(new SqlParameter("@response", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });

                        cmd.CommandTimeout = 86400;
                        cmd.ExecuteNonQuery();

                        //RETURN VALUE
                        //response = cmd.Parameters["P_Response"].Value.ToString();

                        response = "S|Success";

                        if (cmd.Parameters.Contains("@response"))
                        {
                            response = cmd.Parameters["@response"].Value.ToString();
                        }

                        con.Close();
                        cmd.Parameters.Clear();
                        cmd.Dispose();

                    }
                    catch (Exception ex)
                    {
                        con.Close();
                        cmd.Parameters.Clear();
                        cmd.Dispose();
                        string clientIp = AppHttpContextAccessor.AppHttpContext?
                    .Connection?
                    .RemoteIpAddress?
                    .ToString();
                        LogEntry.InsertLogEntryFromException(ex, controllername, ActionName, clientIp);
                        response = "E|Opps!... Something went wrong. " + JsonConvert.SerializeObject(ex) + "|0";
                    }
                }
            }

            if (!string.IsNullOrEmpty(response) && response.Contains("|"))
            {
                var msgtype = response.Split('|').Length > 0 ? Convert.ToString(response.Split('|')[0]) : "";
                var message = response.Split('|').Length > 1 ? Convert.ToString(response.Split('|')[1]).Replace("\"", "") : "";

                Int64 strid = 0;
                if (Int64.TryParse(response.Split('|').Length > 2 ? Convert.ToString(response.Split('|')[2]).Replace("\"", "") : "0", out strid)) { }
                string paths = response.Split('|').Length > 3 ? response.Split('|')[3].Replace("\"", "") : "0";


                return (msgtype.Contains("S"), message, strid, paths);
            }
            else
                return (false, ResponseStatusMessage.Error, 0, "0");
        }

        public static string ExecuteStoredProcedure(string sp, SqlParameter[] spCol, string ActionName, string controllername)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sp, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter returnParameter = new SqlParameter("@response", SqlDbType.NVarChar, 2000);

                        returnParameter.Direction = ParameterDirection.Output;

                        if (spCol != null && spCol.Length > 0)
                            cmd.Parameters.AddRange(spCol);


                        cmd.Parameters.Add(returnParameter);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();

                        return returnParameter.Value.ToString();
                    }
                }

            }
            catch (SqlException ex)
            {
                StringBuilder errorMessages = new StringBuilder();
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages.Append("Index #......" + i.ToString() + Environment.NewLine +
                                         "Message:....." + ex.Errors[i].Message + Environment.NewLine +
                                         "LineNumber:.." + ex.Errors[i].LineNumber + Environment.NewLine);
                }
                //Activity_Log.SendToDB("Database Oparation", "Error: " + "StoredProcedure: " + sp, ex);
                string clientIp = AppHttpContextAccessor.AppHttpContext?
                    .Connection?
                    .RemoteIpAddress?
                    .ToString();
                LogEntry.InsertLogEntryFromException(ex, controllername, ActionName, clientIp);
                return "E|" + errorMessages.ToString();
            }
            catch (Exception ex)
            {
                //Activity_Log.SendToDB("Database Oparation", "Error: " + "StoredProcedure: " + sp, ex);
                return "E|" + ex.Message.ToString();
            }
        }

        public static bool ExecuteNonQuery_Delete(string query, List<SqlParameter> parameters = null, string ActionName = "", string controllername = "")
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    SqlCommand cmd = con.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;

                    if (parameters != null)
                        foreach (SqlParameter param in parameters)
                            cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                string clientIp = AppHttpContextAccessor.AppHttpContext?
                    .Connection?
                    .RemoteIpAddress?
                    .ToString();
                LogEntry.InsertLogEntryFromException(ex, controllername, ActionName, clientIp);
                return false;
            }
        }








        public static List<LOV> LOV_Get(string Lov_Column = "", string Flag = "")
        {
            DateTime? nullDateTime = null;
            var listObj = new List<LOV>();
            string actionName = "LOV_Get";
            string controllerName = "LOV";
            string clientIp = AppHttpContextAccessor.AppHttpContext?.Connection?.RemoteIpAddress?.ToString();
            try
            {
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("Lov_Column", SqlDbType.VarChar) { Value = Lov_Column, Direction = ParameterDirection.Input, IsNullable = true });
                parameters.Add(new SqlParameter("Lov_Code", SqlDbType.VarChar) { Value = "", Direction = ParameterDirection.Input, IsNullable = true });
                parameters.Add(new SqlParameter("Flag", SqlDbType.VarChar) { Value = Flag, Direction = ParameterDirection.Input, IsNullable = true });

                var dt = ExecuteStoredProcedure_DataTable("SP_LOV_Get", parameters.ToList(), false, actionName, controllerName);

                if (dt != null && dt.Rows.Count > 0)
                    foreach (DataRow dr in dt.Rows)
                        listObj.Add(new LOV()
                        {

                            LOV_Column = dr["LOV_Column"] != DBNull.Value ? Convert.ToString(dr["LOV_Column"]) : "",
                            Display_Text = dr["Display_Text"] != DBNull.Value ? Convert.ToString(dr["Display_Text"]) : "",
                        });
            }
            catch (Exception ex) { /*LogService.LogInsert(GetCurrentAction(), "", ex);*/ }

            return listObj;
        }
        public static List<LOV> LOV_Detail_Get(string Lov_Column = "", string Lov_Code = "", string Flag = "")
        {
            DateTime? nullDateTime = null;
            var listObj = new List<LOV>();
            string actionName = "LOV_Detail_Get";
            string controllerName = "LOV";
            string clientIp = AppHttpContextAccessor.AppHttpContext?.Connection?.RemoteIpAddress?.ToString();
            try
            {
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("Lov_Column", SqlDbType.VarChar) { Value = Lov_Column, Direction = ParameterDirection.Input, IsNullable = true });
                parameters.Add(new SqlParameter("Lov_Code", SqlDbType.VarChar) { Value = Lov_Code, Direction = ParameterDirection.Input, IsNullable = true });
                parameters.Add(new SqlParameter("Flag", SqlDbType.VarChar) { Value = Flag, Direction = ParameterDirection.Input, IsNullable = true });

                var dt = ExecuteStoredProcedure_DataTable("SP_LOV_Get", parameters.ToList(), false, actionName, controllerName);

                if (dt != null && dt.Rows.Count > 0)
                    foreach (DataRow dr in dt.Rows)
                        listObj.Add(new LOV()
                        {

                            LOV_Column = dr["LOV_Column"] != DBNull.Value ? Convert.ToString(dr["LOV_Column"]) : "",
                            Display_Text = dr["Display_Text"] != DBNull.Value ? Convert.ToString(dr["Display_Text"]) : "",
                            LOV_Code = dr["LOV_Code"] != DBNull.Value ? Convert.ToString(dr["LOV_Code"]) : "",
                            LOV_Desc = dr["LOV_Desc"] != DBNull.Value ? Convert.ToString(dr["LOV_Desc"]) : "",
                            DisplayOrder = dr["DisplayOrder"] != DBNull.Value ? Convert.ToInt64(dr["DisplayOrder"]) : 0,
                            MaxDisplay_Seq_No = dr["MaxDisplay_Seq_No"] != DBNull.Value ? Convert.ToInt64(dr["MaxDisplay_Seq_No"]) : 0,
                        });
            }
            catch (Exception ex) { LogEntry.InsertLogEntryFromException(ex, controllerName, actionName, clientIp); }

            return listObj;
        }
        public static (bool, string, long) Lov_Save(LOV obj = null)
        {
            string actionName = "Lov_Save";
            string controllerName = "LOV";
            string clientIp = AppHttpContextAccessor.AppHttpContext?.Connection?.RemoteIpAddress?.ToString();
            if (obj != null)
                try
                {
                    var parameters = new List<SqlParameter>();

                    parameters.Add(new SqlParameter("Lov_Column", SqlDbType.NVarChar) { Value = obj.LOV_Column, Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Display_Text", SqlDbType.NVarChar) { Value = obj.Display_Text, Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Operated_By", SqlDbType.BigInt) { Value = Common.LoggedUser_Id(), Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Action", SqlDbType.NVarChar) { Value = obj.Action, Direction = ParameterDirection.Input, IsNullable = true });

                    var response = ExecuteStoredProcedure("SP_Lov_Save", parameters.ToArray(), actionName, controllerName);

                    var msgtype = response.Split('|').Length > 0 ? response.Split('|')[0] : "";
                    var message = response.Split('|').Length > 1 ? response.Split('|')[1].Replace("\"", "") : "";
                    var strid = response.Split('|').Length > 2 ? response.Split('|')[2].Replace("\"", "") ?? "0" : "0";

                    return (msgtype.Contains("S"), message, Convert.ToInt64(strid));

                }
                catch (Exception ex) { LogEntry.InsertLogEntryFromException(ex, controllerName, actionName, clientIp); }

            return (false, ResponseStatusMessage.Error, 0);
        }
        public static (bool, string, long) Lov_Detail_Save(LOV obj = null)
        {
            string actionName = "Lov_Detail_Save";
            string controllerName = "LOV";
            string clientIp = AppHttpContextAccessor.AppHttpContext?.Connection?.RemoteIpAddress?.ToString();
            if (obj != null)
                try
                {
                    var parameters = new List<SqlParameter>();
                    string display_text = obj.Display_Text.Replace("%20", " ");
                    string lov_col = obj.LOV_Column.Replace("%20", " ");
                    parameters.Add(new SqlParameter("Lov_Column", SqlDbType.NVarChar) { Value = obj.LOV_Column, Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Display_Text", SqlDbType.NVarChar) { Value = obj.Display_Text, Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Lov_Code", SqlDbType.NVarChar) { Value = obj.LOV_Code, Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Lov_Desc", SqlDbType.NVarChar) { Value = obj.LOV_Desc, Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("DisplayOrder", SqlDbType.BigInt) { Value = obj.DisplayOrder, Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Operated_By", SqlDbType.BigInt) { Value = Common.LoggedUser_Id(), Direction = ParameterDirection.Input, IsNullable = true });
                    parameters.Add(new SqlParameter("Action", SqlDbType.NVarChar) { Value = obj.Action, Direction = ParameterDirection.Input, IsNullable = true });

                    var response = ExecuteStoredProcedure("SP_LovDtl_Save", parameters.ToArray(), actionName, controllerName);

                    var msgtype = response.Split('|').Length > 0 ? response.Split('|')[0] : "";
                    var message = response.Split('|').Length > 1 ? response.Split('|')[1].Replace("\"", "") : "";
                    var strid = response.Split('|').Length > 2 ? response.Split('|')[2].Replace("\"", "") ?? "0" : "0";

                    return (msgtype.Contains("S"), message, Convert.ToInt64(strid));

                }
                catch (Exception ex) { LogEntry.InsertLogEntryFromException(ex, controllerName, actionName, clientIp); }

            return (false, ResponseStatusMessage.Error, 0);
        }
        public static (bool, string) LOV_Dtl_Delete(string Lov_Column = "", string Lov_Code = "")
        {
            string actionName = "LOV_Dtl_Delete";
            string controllerName = "LOV";
            string clientIp = AppHttpContextAccessor.AppHttpContext?.Connection?.RemoteIpAddress?.ToString();
            try
            {
                var parameters = new List<SqlParameter>();

                parameters.Add(new SqlParameter("Lov_Column", SqlDbType.VarChar) { Value = Lov_Column, Direction = ParameterDirection.Input, IsNullable = true });
                parameters.Add(new SqlParameter("Lov_Code", SqlDbType.VarChar) { Value = Lov_Code, Direction = ParameterDirection.Input, IsNullable = true });
                parameters.Add(new SqlParameter("Operated_By", SqlDbType.BigInt) { Value = Common.LoggedUser_Id(), Direction = ParameterDirection.Input, IsNullable = true });

                var response = ExecuteStoredProcedure("SP_LovDtl_Delete", parameters.ToArray(), actionName, controllerName);

                var msgtype = response.Split('|').Length > 0 ? response.Split('|')[0] : "";
                var message = response.Split('|').Length > 1 ? response.Split('|')[1].Replace("\"", "") : "";
                var strid = response.Split('|').Length > 2 ? response.Split('|')[2].Replace("\"", "") ?? "0" : "0";

                return (msgtype.Contains("S"), message);

            }
            catch (Exception ex) { LogEntry.InsertLogEntryFromException(ex, controllerName, actionName, clientIp); }

            return (false, ResponseStatusMessage.Error);
        }

        //public static (bool, string) Employee_Status(long Id = 0, long Logged_In_VendorId = 0, bool IsActive = false, bool IsDelete = false)
        //{
        //	if (Id > 0)
        //		try
        //		{
        //			var parameters = new List<SqlParameter>();

        //			parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = Id, Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("VendorId", SqlDbType.BigInt) { Value = Logged_In_VendorId, Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("IsActive", SqlDbType.NVarChar) { Value = IsActive, Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("Operated_By", SqlDbType.BigInt) { Value = Common.LoggedUser_Id(), Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("Operated_RoleId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.KEY_USER_ROLE_ID), Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("Operated_MenuId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.CURRENT_MENU_ID), Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("Action", SqlDbType.NVarChar) { Value = IsDelete ? "DELETE" : "STATUS", Direction = ParameterDirection.Input, IsNullable = true });

        //			var response = ExecuteStoredProcedure("SP_Employee_Status", parameters.ToArray());

        //			var msgtype = response.Split('|').Length > 0 ? response.Split('|')[0] : "";
        //			var message = response.Split('|').Length > Common.LoggedCompany_Id() ? response.Split('|')[Common.LoggedCompany_Id()].Replace("\"", "") : "";
        //			var strid = response.Split('|').Length > 2 ? response.Split('|')[2].Replace("\"", "") ?? "0" : "0";

        //			return (msgtype.Contains("S"), message);

        //		}
        //		catch (Exception ex) { /*LogService.LogInsert(GetCurrentAction(), "", ex);*/ }

        //	return (false, ResponseStatusMessage.Error);
        //}

        //public static List<Vendor> Vendor_Get(long id = 0)
        //{
        //	DateTime? nullDateTime = null;
        //	var listObj = new List<Vendor>();

        //	try
        //	{
        //		var parameters = new List<SqlParameter>();
        //		parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = id, Direction = ParameterDirection.Input, IsNullable = true });

        //		parameters.Add(new SqlParameter("Operated_By", SqlDbType.BigInt) { Value = Common.LoggedUser_Id(), Direction = ParameterDirection.Input, IsNullable = true });
        //		parameters.Add(new SqlParameter("Operated_RoleId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.KEY_USER_ROLE_ID), Direction = ParameterDirection.Input, IsNullable = true });
        //		parameters.Add(new SqlParameter("Operated_MenuId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.CURRENT_MENU_ID), Direction = ParameterDirection.Input, IsNullable = true });

        //		var dt = ExecuteStoredProcedure_DataTable("SP_Vendor_GET", parameters.ToList());

        //		if (dt != null && dt.Rows.Count > 0)
        //			foreach (DataRow dr in dt.Rows)
        //				listObj.Add(new Vendor()
        //				{
        //					Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
        //					UserId = dr["UserId"] != DBNull.Value ? Convert.ToInt64(dr["UserId"]) : 0,
        //					UserName = dr["UserName"] != DBNull.Value ? Convert.ToString(dr["UserName"]) : "",
        //					FirstName = dr["FirstName"] != DBNull.Value ? Convert.ToString(dr["FirstName"]) : "",
        //					MiddleName = dr["MiddleName"] != DBNull.Value ? Convert.ToString(dr["MiddleName"]) : "",
        //					LastName = dr["LastName"] != DBNull.Value ? Convert.ToString(dr["LastName"]) : "",
        //					Email = dr["Email"] != DBNull.Value ? Convert.ToString(dr["Email"]) : "",
        //					ContactNo = dr["ContactNo"] != DBNull.Value ? Convert.ToString(dr["ContactNo"]) : "",
        //					ContactNo_Alternate = dr["ContactNo_Alternate"] != DBNull.Value ? Convert.ToString(dr["ContactNo_Alternate"]) : "",
        //					IsActive = dr["IsActive"] != DBNull.Value ? Convert.ToBoolean(dr["IsActive"]) : false,
        //					CreatedBy = dr["CreatedBy"] != DBNull.Value ? Convert.ToInt64(dr["CreatedBy"]) : 0,
        //					Logo = dr["Logo"] != DBNull.Value && dr["Logo"] != null ? (byte[])dr["Logo"] : null
        //				});
        //	}
        //	catch (Exception ex) { /*LogService.LogInsert(GetCurrentAction(), "", ex);*/ }

        //	return listObj;
        //}

        //public static (bool, string, long) Vendor_Save(Vendor obj = null)
        //{
        //	if (obj != null)
        //		try
        //		{
        //			var parameters = new List<SqlParameter>();

        //			parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = obj.Id, Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("UserId", SqlDbType.BigInt) { Value = obj.UserId, Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("UserName", SqlDbType.NVarChar) { Value = obj.UserName, Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("Password", SqlDbType.NVarChar) { Value = obj.Password, Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("FirstName", SqlDbType.NVarChar) { Value = obj.FirstName, Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("MiddleName", SqlDbType.NVarChar) { Value = obj.MiddleName, Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("LastName", SqlDbType.NVarChar) { Value = obj.LastName, Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("Email", SqlDbType.NVarChar) { Value = obj.Email, Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("ContactNo", SqlDbType.NVarChar) { Value = obj.ContactNo, Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("ContactNo_Alternate", SqlDbType.NVarChar) { Value = obj.ContactNo_Alternate, Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("IsActive", SqlDbType.NVarChar) { Value = obj.IsActive, Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("Operated_By", SqlDbType.BigInt) { Value = Common.LoggedUser_Id(), Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("Operated_RoleId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.KEY_USER_ROLE_ID), Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("Operated_MenuId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.CURRENT_MENU_ID), Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("Action", SqlDbType.NVarChar) { Value = obj.Id > 0 ? "UPDATE" : "INSERT", Direction = ParameterDirection.Input, IsNullable = true });

        //			var response = ExecuteStoredProcedure("SP_Vendor_Save", parameters.ToArray());

        //			var msgtype = response.Split('|').Length > 0 ? response.Split('|')[0] : "";
        //			var message = response.Split('|').Length > Common.LoggedCompany_Id() ? response.Split('|')[Common.LoggedCompany_Id()].Replace("\"", "") : "";
        //			var strid = response.Split('|').Length > 2 ? response.Split('|')[2].Replace("\"", "") ?? "0" : "0";

        //			return (msgtype.Contains("S"), message, Convert.ToInt64(strid));

        //		}
        //		catch (Exception ex) { /*LogService.LogInsert(GetCurrentAction(), "", ex);*/ }

        //	return (false, ResponseStatusMessage.Error, 0);
        //}

        //public static (bool, string) Vendor_Status(long Id = 0, bool IsActive = false, bool IsDelete = false)
        //{
        //	if (Id > 0)
        //		try
        //		{
        //			var parameters = new List<SqlParameter>();

        //			parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = Id, Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("IsActive", SqlDbType.NVarChar) { Value = IsActive, Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("Operated_By", SqlDbType.BigInt) { Value = Common.LoggedUser_Id(), Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("Operated_RoleId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.KEY_USER_ROLE_ID), Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("Operated_MenuId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.CURRENT_MENU_ID), Direction = ParameterDirection.Input, IsNullable = true });
        //			parameters.Add(new SqlParameter("Action", SqlDbType.NVarChar) { Value = IsDelete ? "DELETE" : "STATUS", Direction = ParameterDirection.Input, IsNullable = true });

        //			var response = ExecuteStoredProcedure("SP_Vendor_Status", parameters.ToArray());

        //			var msgtype = response.Split('|').Length > 0 ? response.Split('|')[0] : "";
        //			var message = response.Split('|').Length > Common.LoggedCompany_Id() ? response.Split('|')[Common.LoggedCompany_Id()].Replace("\"", "") : "";
        //			var strid = response.Split('|').Length > 2 ? response.Split('|')[2].Replace("\"", "") ?? "0" : "0";

        //			return (msgtype.Contains("S"), message);

        //		}
        //		catch (Exception ex) { /*LogService.LogInsert(GetCurrentAction(), "", ex);*/ }

        //	return (false, ResponseStatusMessage.Error);
        //}

        //public static List<Warehouses> Warehouses_Get(long id = 0)
        //{
        //	DateTime? nullDateTime = null;
        //	var listObj = new List<Warehouses>();

        //	try
        //	{
        //		var parameters = new List<SqlParameter>();
        //		parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = id, Direction = ParameterDirection.Input, IsNullable = true });

        //		parameters.Add(new SqlParameter("Operated_By", SqlDbType.BigInt) { Value = Common.LoggedUser_Id(), Direction = ParameterDirection.Input, IsNullable = true });
        //		parameters.Add(new SqlParameter("Operated_RoleId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.KEY_USER_ROLE_ID), Direction = ParameterDirection.Input, IsNullable = true });
        //		parameters.Add(new SqlParameter("Operated_MenuId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.CURRENT_MENU_ID), Direction = ParameterDirection.Input, IsNullable = true });

        //		var dt = ExecuteStoredProcedure_DataTable("SP_Warehouses_GET", parameters.ToList());

        //              if (dt != null && dt.Rows.Count > 0)
        //                  foreach (DataRow dr in dt.Rows)
        //                      listObj.Add(new Warehouses()
        //                      {
        //                          Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
        //                          //WarehouseName = dr["WarehouseName"] != DBNull.Value ? Convert.ToString(dr["WarehousesName"]) : "",
        //                          //ContactPerson = dr["ContactPerson"] != DBNull.Value ? Convert.ToString(dr["ContactPerson"]) : "",
        //                          //Phone = dr["Phone"] != DBNull.Value ? Convert.ToString(dr["Phone"]) : "",
        //                          //Email = dr["Email"] != DBNull.Value ? Convert.ToString(dr["Email"]) : "",
        //                          //Address = dr["Address"] != DBNull.Value ? Convert.ToString(dr["Address"]) : "",
        //                          //City = dr["City"] != DBNull.Value ? Convert.ToString(dr["City"]) : "",
        //                          //State = dr["State"] != DBNull.Value ? Convert.ToString(dr["State"]) : "",
        //                          //Pincode = dr["Pincode"] != DBNull.Value ? Convert.ToString(dr["Pincode"]) : "",
        //                          //GSTNumber = dr["GSTNumber"] != DBNull.Value ? Convert.ToString(dr["GSTNumber"]) : "",
        //                          //Capacity = dr["Capacity"] != DBNull.Value ? Convert.ToString(dr["Capacity"]) : "",
        //                          //Status = dr["Status"] != DBNull.Value ? Convert.ToString(dr["Status"]) : "",

        //				});
        //	}
        //	catch (Exception ex) { /*LogService.LogInsert(GetCurrentAction(), "", ex);*/ }

        //	return listObj;
        //}

        //public static (bool, string, long) Warehouses_Save(Warehouses obj = null)
        //{
        //	if (obj != null)
        //		try
        //		{
        //			var parameters = new List<SqlParameter>();

        //                  parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = obj.Id, Direction = ParameterDirection.Input, IsNullable = true });
        //                  parameters.Add(new SqlParameter("WarehouseName", SqlDbType.NVarChar) { Value = obj.WarehouseName, Direction = ParameterDirection.Input, IsNullable = true });
        //                  parameters.Add(new SqlParameter("ContactPerson", SqlDbType.NVarChar) { Value = obj.ContactPerson, Direction = ParameterDirection.Input, IsNullable = true });
        //                  parameters.Add(new SqlParameter("Phone", SqlDbType.NVarChar) { Value = obj.Phone, Direction = ParameterDirection.Input, IsNullable = true });
        //                  parameters.Add(new SqlParameter("Email", SqlDbType.NVarChar) { Value = obj.Email, Direction = ParameterDirection.Input, IsNullable = true });
        //                  parameters.Add(new SqlParameter("Address", SqlDbType.NVarChar) { Value = obj.Address, Direction = ParameterDirection.Input, IsNullable = true });
        //                  parameters.Add(new SqlParameter("City_ID", SqlDbType.Int) { Value = obj.City_Id, Direction = ParameterDirection.Input, IsNullable = true });
        //                  parameters.Add(new SqlParameter("StateId", SqlDbType.Int) { Value = obj.StateId, Direction = ParameterDirection.Input, IsNullable = true });
        //                  parameters.Add(new SqlParameter("Pincode", SqlDbType.NVarChar) { Value = obj.Pincode, Direction = ParameterDirection.Input, IsNullable = true });
        //                  parameters.Add(new SqlParameter("GSTNumber", SqlDbType.NVarChar) { Value = obj.GSTNumber, Direction = ParameterDirection.Input, IsNullable = true });
        //                  parameters.Add(new SqlParameter("Capacity", SqlDbType.NVarChar) { Value = obj.Capacity, Direction = ParameterDirection.Input, IsNullable = true });
        //                  parameters.Add(new SqlParameter("Status", SqlDbType.NVarChar) { Value = obj.Status, Direction = ParameterDirection.Input, IsNullable = true });
        //                  //parameters.Add(new SqlParameter("Operated_By", SqlDbType.BigInt) { Value = Common.LoggedUser_Id(), Direction = ParameterDirection.Input, IsNullable = true });
        //                  //parameters.Add(new SqlParameter("Operated_RoleId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.KEY_USER_ROLE_ID), Direction = ParameterDirection.Input, IsNullable = true });
        //                  //parameters.Add(new SqlParameter("Operated_MenuId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.CURRENT_MENU_ID), Direction = ParameterDirection.Input, IsNullable = true });
        //                  parameters.Add(new SqlParameter("Action", SqlDbType.NVarChar) { Value = obj.Id > 0 ? "UPDATE" : "INSERT", Direction = ParameterDirection.Input, IsNullable = true });

        //			var response = ExecuteStoredProcedure("SP_Warehouses_Save", parameters.ToArray());

        //			var msgtype = response.Split('|').Length > 0 ? response.Split('|')[0] : "";
        //			var message = response.Split('|').Length > Common.LoggedCompany_Id() ? response.Split('|')[Common.LoggedCompany_Id()].Replace("\"", "") : "";
        //			var strid = response.Split('|').Length > 2 ? response.Split('|')[2].Replace("\"", "") ?? "0" : "0";

        //			return (msgtype.Contains("S"), message, Convert.ToInt64(strid));

        //		}
        //		catch (Exception ex) { /*LogService.LogInsert(GetCurrentAction(), "", ex);*/ }

        //	return (false, ResponseStatusMessage.Error, 0);
        //}
    }

}
