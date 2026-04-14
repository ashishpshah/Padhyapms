using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PMMS.Controllers;
using PMMS.Infra;
using PMMS.Models;
using System.Data;
using System.Net.Mail;

namespace PMMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ClientController : BaseController<ResponseModel<Client>>
    {
        public ClientController(IRepositoryWrapper repository) : base(repository) { }
        public IActionResult Index()
        {
            CommonViewModel.ObjList = new List<Client>();
            try
            {
               CommonViewModel.ObjList = new List<Client>();
            }
            catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }


            return View(CommonViewModel);
        }

        [HttpGet]
        public IActionResult Partial_AddEditForm(int Id)
        {
            var obj = new Client();

            var dt = new DataTable();

            var list = new List<SelectListItem_Custom>();
            List<SqlParameter> oParams = new List<SqlParameter>();
            try
            {



                oParams.Add(new SqlParameter("@ClientId", SqlDbType.BigInt) { Value = Id == 0 ? null : Id });

                dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_ClientMaster_Get", oParams, true);

                if (dt != null && dt.Rows.Count > 0)
                {
                    obj = new Client()
                    {
                        ClientId = dt.Rows[0]["ClientId"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["ClientId"]) : 0,
                        ClientName = dt.Rows[0]["ClientName"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["ClientName"]) : null,
                        IsActive = dt.Rows[0]["IsActive"] != DBNull.Value ? Convert.ToBoolean(dt.Rows[0]["IsActive"]) : true
                    };
                }
               
            }
            catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }

            CommonViewModel.Obj = obj;
            return PartialView("_Partial_AddEditForm", CommonViewModel);
        }

        [HttpPost]
        public JsonResult Save(Client viewModel)
        {

            try
            {
               

                if (string.IsNullOrEmpty(viewModel.ClientName))
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please Enter Client Name.";

                    return Json(CommonViewModel);
                }

                List<SqlParameter> oParams = new List<SqlParameter>();

                oParams.Add(new SqlParameter("@ClientId", SqlDbType.BigInt) { Value = viewModel.ClientId });
                oParams.Add(new SqlParameter("@ClientName", SqlDbType.VarChar) { Value = viewModel.ClientName });
                oParams.Add(new SqlParameter("@Operated_By", SqlDbType.BigInt) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID) });
                oParams.Add(new SqlParameter("@Action", SqlDbType.VarChar) { Value = viewModel.ClientId == 0 ? "INSERT" : "UPDATE" });

                var (IsSuccess, response, Id) = DataContext_Command.ExecuteStoredProcedure("SP_ClientMaster_Save", oParams, true);

                CommonViewModel.IsConfirm = IsSuccess;
                CommonViewModel.IsSuccess = IsSuccess;
                CommonViewModel.StatusCode = IsSuccess ? ResponseStatusCode.Success : ResponseStatusCode.Error;
                CommonViewModel.Message = response;
                CommonViewModel.RedirectURL = IsSuccess ? Url.Action("Index") : "";

                return Json(CommonViewModel);
            }
            catch (Exception ex)
            {
                LogService.LogInsert(GetCurrentAction(), "", ex);

                CommonViewModel.IsSuccess = false;
                CommonViewModel.StatusCode = ResponseStatusCode.Error;
                CommonViewModel.Message = ResponseStatusMessage.Error + " | " + ex.Message;
            }
            return Json(CommonViewModel);
        }

        public ActionResult DeleteConfirmed(long Id = 0)
        {
            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@ClientId", SqlDbType.Int) { Value = Id, Direction = ParameterDirection.Input });
            parameters.Add(new SqlParameter("@Operated_By", SqlDbType.Int) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID), Direction = ParameterDirection.Input });

            var response = DataContext_Command.ExecuteStoredProcedure("SP_ClientMaster_Delete", parameters.ToArray());

            var msgtype = response.Split('|').Length > 0 ? response.Split('|')[0] : "";
            var message = response.Split('|').Length > 1 ? response.Split('|')[1].Replace("\"", "") : "";
            var strid = response.Split('|').Length > 2 ? response.Split('|')[2].Replace("\"", "") ?? "0" : "0";

            if (msgtype.Contains("S"))
            {
                //Common.Set_Session(SessionKey.Truck_NAME, Convert.ToString(strid));
                CommonViewModel.IsConfirm = true;
                CommonViewModel.IsSuccess = true;
                CommonViewModel.StatusCode = ResponseStatusCode.Success;
                CommonViewModel.Message = message;
                CommonViewModel.RedirectURL = Url.Action("Index", "Client");
                return Json(CommonViewModel);
            }
            CommonViewModel.IsConfirm = true;
            CommonViewModel.IsSuccess = false;
            CommonViewModel.Status = ResponseStatusMessage.Error;
            CommonViewModel.Message = message;

            return Json(CommonViewModel);
        }
    }
}
