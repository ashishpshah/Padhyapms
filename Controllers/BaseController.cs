using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using NuGet.Protocol.Core.Types;
using PMMS.Infra;
using System.Data;

namespace PMMS.Controllers
{


	public class BaseController : Controller
	{

	}

    public class BaseController<T> : BaseController where T : class
    {
        public T CommonViewModel { get; set; } = default(T);

        public bool IsLogActive = false;

        public readonly DateTime? nullDateTime = null;
        public string ControllerName = "";
        public string ActionName = "";
        public string AreaName = "";

        public bool IsEmployee { get; set; }

        public long Logged_In_UserId { get; set; }
        public long Logged_In_EmployeeId { get; set; }
        public long CompanyId { get; set; }

        public IRepositoryWrapper _context;

        public BaseController()
        {
            try { IsEmployee = Common.Get_Session_Int(SessionKey.KEY_IS_EMPLOYEE) == 1; } catch { }

            try { Logged_In_UserId = Common.LoggedUser_Id(); } catch { }
            try { Logged_In_EmployeeId = Common.Get_Session_Int(SessionKey.KEY_EMPLOYEE_ID); } catch { }
          

        }

        public BaseController(IRepositoryWrapper repository)
        {
            try { IsEmployee = Common.Get_Session_Int(SessionKey.KEY_IS_EMPLOYEE) == 1; } catch { }

            try { Logged_In_UserId = Common.LoggedUser_Id(); } catch { }
            try { Logged_In_EmployeeId = Common.Get_Session_Int(SessionKey.KEY_EMPLOYEE_ID); } catch { }
          

            _context = repository;
            CommonViewModel = (dynamic)Activator.CreateInstance(typeof(T));
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                ControllerName = Convert.ToString(context.RouteData.Values["controller"]);
                ActionName = Convert.ToString(context.RouteData.Values["action"]);

                if (context.RouteData.DataTokens != null) AreaName = Convert.ToString(context.RouteData.DataTokens["area"]);

                if (string.IsNullOrEmpty(AreaName)) AreaName = Convert.ToString(context.RouteData.Values["area"]);

                List<UserMenuAccess> listMenuAccess = JsonConvert.DeserializeObject<List<UserMenuAccess>>(Common.Get_Session(SessionKey.KEY_USER_MENUPERMISSION));

                if (listMenuAccess != null && listMenuAccess.Count > 0)
                {
                    if (listMenuAccess.FindIndex(x => x.Controller == ControllerName) > -1)
                    {
                        //CommonViewModel.IsCreate = listMenuAccess[listMenuAccess.FindIndex(x => x.Controller == ControllerName)].IsCreate;
                        //CommonViewModel.IsRead = listMenuAccess[listMenuAccess.FindIndex(x => x.Controller == ControllerName)].IsRead;
                        //CommonViewModel.IsUpdate = listMenuAccess[listMenuAccess.FindIndex(x => x.Controller == ControllerName)].IsUpdate;
                        //CommonViewModel.IsDelete = listMenuAccess[listMenuAccess.FindIndex(x => x.Controller == ControllerName)].IsDelete;

                        try { Common.Set_Session_Int(SessionKey.CURRENT_MENU_ID, listMenuAccess[listMenuAccess.FindIndex(x => x.Controller == ControllerName)].MenuId); }
                        catch { Common.Set_Session_Int(SessionKey.CURRENT_MENU_ID, 0); }


                    }
                }

                if (!Common.IsUserLogged() && !string.IsNullOrEmpty(AreaName) && !(Convert.ToString(ControllerName).ToLower() == "home" && (new string[] { "account", "login", "forgotpassword", "forgotpassword_sendotp", "forgotpassword_verifyotp", "forgotpassword_resetpassword" }).Any(x => x == Convert.ToString(ActionName).ToLower())))
                {
                    context.Result = new RedirectResult(Url.Content("~/") + (string.IsNullOrEmpty(AreaName) ? "" : AreaName + "/") + "Home/Account");
                    return;
                }
                else if (!Common.IsUserLogged() && !Common.IsAdmin() && !string.IsNullOrEmpty(AreaName) && !(Convert.ToString(ControllerName).ToLower() == "home" && (new string[] { "account", "login", "forgotpassword", "forgotpassword_sendotp", "forgotpassword_verifyotp", "forgotpassword_resetpassword" }).Any(x => x == Convert.ToString(ActionName).ToLower())))
                {
                    context.Result = new RedirectResult(Url.Content("~/") + "Home/Login");
                    return;
                }

            }
            catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }
        }


        public string GetCurrentAction() => (string.IsNullOrEmpty(AreaName) ? "" : AreaName + " - ") + ControllerName + " - " + ActionName;
        public string GetCurrentControllerUrl() => (string.IsNullOrEmpty(AreaName) ? "" : AreaName + "/") + ControllerName;


        public static T GetValue<T>(object val)
        {
            if (val != null) return default;

            Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            return (T)Convert.ChangeType(val, targetType);
        }


        public static T GetValue<T>(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return default;

            Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            return (T)Convert.ChangeType(row[columnName], targetType);
        }
    }
}
