using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PMMS.Areas.Admin.Projects;
using PMMS.Controllers;
using PMMS.Infra;
using PMMS.Infra.Services;
using PMMS.Models;
using System.Data;
using System.Net.Mail;

namespace PMMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProjectController : BaseController<ResponseModel<Project>>
    {
        public ProjectController(IRepositoryWrapper repository) : base(repository) { }
        public IActionResult Index()
            {
            CommonViewModel.ObjList = new List<Project>();
            try
            {
               CommonViewModel.ObjList = ProjectServices.GET(0).ToList();
                
            }
            catch (Exception ex)
            {
                string actionName = ControllerContext.RouteData.Values["action"]?.ToString();
                string controllerName = ControllerContext.RouteData.Values["controller"]?.ToString();
                string clientIp = HttpContext.Connection?.RemoteIpAddress?.ToString();
                LogEntry.InsertLogEntryFromException(ex, controllerName, actionName, clientIp);
                return null;
            }


            return View(CommonViewModel);
        }

        [HttpGet]
        public IActionResult Partial_AddEditForm(int Id)
        {
           
            try
            {
                CommonViewModel.Obj = new Project() { StartDate = DateTime.Now , Status = "1" , Priority = "0"};
                CommonViewModel.Obj.ProjectFeatureList = new List<ProjectFeatureList>();
                var list = new List<SelectListItem_Custom>();
                var dt = new DataTable();
                
                if (Id> 0)
                {
                    CommonViewModel.Obj = ProjectServices.GET(Id).FirstOrDefault();
                    CommonViewModel.Obj.ProjectFeatureList = ProjectServices.GET_ProjectFeatureList(Id , CommonViewModel.Obj.Service_Ids).ToList();
                }
                // ================= CLIENT DROPDOWN =================
                dt = new DataTable();
                dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Client_Get",
                        new List<SqlParameter> {
                    new SqlParameter("@Id", SqlDbType.BigInt){ Value = 0 }
                        }, true);

                if (dt != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        list.Add(new SelectListItem_Custom(
                            dr["Id"]?.ToString(),
                            dr["Client_Name"]?.ToString(),
                            "Client")
                        {
                            Value = dr["Id"]?.ToString(),
                            Text = dr["Client_Name"]?.ToString()
                        });
                    }
                }
                // ================= SERVICE DROPDOWN =================
                dt = new DataTable();
                dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Services_Get",
                        new List<SqlParameter> {
                    new SqlParameter("@Id", SqlDbType.BigInt){ Value = 0 }
                        }, true);

                if (dt != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        list.Add(new SelectListItem_Custom(
                            dr["Id"]?.ToString(),
                            dr["ServiceName"]?.ToString(),
                            "Service")
                        {
                            Value = dr["Id"]?.ToString(),
                            Text = dr["ServiceName"]?.ToString()
                        });
                    }
                }
                dt = new DataTable();
                dt = DataContext_Command.ExecuteStoredProcedure_DataTable(
                    "SP_Multiple_Lov_Combo",
                    new List<SqlParameter>
                    {
                new SqlParameter("@Lov_Column", SqlDbType.VarChar){ Value = "PROJECTSTATUS,PRIORITY" }
                    }, true);

                if (dt != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        list.Add(new SelectListItem_Custom(
                            dr["LOV_Code"]?.ToString(),
                            dr["LOV_Desc"]?.ToString(),
                            dr["LOV_Column"]?.ToString())
                        {
                            Value = dr["LOV_Code"]?.ToString(),
                            Text = dr["LOV_Desc"]?.ToString(),
                            Group = dr["LOV_Column"]?.ToString()
                        });
                    }
                }
                CommonViewModel.SelectListItems = list;
                return PartialView("_Partial_AddEditForm", CommonViewModel);
            }
            catch (Exception ex)
            {
                string actionName = ControllerContext.RouteData.Values["action"]?.ToString();
                string controllerName = ControllerContext.RouteData.Values["controller"]?.ToString();
                string clientIp = HttpContext.Connection?.RemoteIpAddress?.ToString();
                LogEntry.InsertLogEntryFromException(ex, controllerName, actionName, clientIp);
                return null;
            }


            
        }

        [HttpPost]
        public JsonResult Save(Project viewModel)
        {

            try
            {
               

                if (string.IsNullOrEmpty(viewModel.ProjectTitle))
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please Enter Project Title.";

                    return Json(CommonViewModel);
                }
                if (viewModel.ClientId == 0)
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please Select Client.";

                    return Json(CommonViewModel);
                }
                if (viewModel.StartDate == null)
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please Select Start Date.";

                    return Json(CommonViewModel);
                }
                if ((viewModel.StartDate != null && viewModel.EndDate != null) && viewModel.EndDate < viewModel.StartDate)
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "End Date Must be Greater Than Start Date.";

                    return Json(CommonViewModel);
                }
                
                if (string.IsNullOrEmpty(viewModel.Description))
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please Enter Description.";

                    return Json(CommonViewModel);
                }
                if (string.IsNullOrEmpty(viewModel.Service_Ids))
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please Select Services.";

                    return Json(CommonViewModel);
                }
                var (IsSuccess, response, Id) = ProjectServices.Save(viewModel);

                CommonViewModel.IsConfirm = IsSuccess;
                CommonViewModel.IsSuccess = IsSuccess;
                CommonViewModel.StatusCode = IsSuccess ? ResponseStatusCode.Success : ResponseStatusCode.Error;
                CommonViewModel.Message = response;
                CommonViewModel.RedirectURL = IsSuccess ? Url.Action("Index", "Project", new { area = "Admin" }) : "";

                return Json(CommonViewModel);
            }
            catch (Exception ex)
            {
                string actionName = ControllerContext.RouteData.Values["action"]?.ToString();
                string controllerName = ControllerContext.RouteData.Values["controller"]?.ToString();
                string clientIp = HttpContext.Connection?.RemoteIpAddress?.ToString();
                LogEntry.InsertLogEntryFromException(ex, controllerName, actionName, clientIp);
                return null;
            }
            return Json(CommonViewModel);
        }

        public ActionResult DeleteConfirmed(long Id = 0)
        {
            

            var (IsSuccess, response) = ProjectServices.Delete(Id);
            
           
            CommonViewModel.IsConfirm = IsSuccess;
            CommonViewModel.IsSuccess = IsSuccess;
            CommonViewModel.StatusCode = IsSuccess ? ResponseStatusCode.Success : ResponseStatusCode.Error;
            CommonViewModel.Message = response;
            CommonViewModel.RedirectURL = Url.Action("Index", "Project", new { area = "Admin" });


            return Json(CommonViewModel);
        }
        [HttpGet]
        public JsonResult GetProjectFeatureList(string Service_Ids = "" , long Id = 0)
        {

            var list = new List<ProjectFeatureList>();

            list = ProjectServices.GET_ProjectFeatureList(Id, Service_Ids);

            return Json(list);

        }
    }
}
