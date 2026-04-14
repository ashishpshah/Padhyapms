using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PMMS.Controllers;
using PMMS.Infra;
using PMMS.Infra.Services;
using PMMS.Models;
using System.Data;
using System.Globalization;

namespace PMMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LOVController : BaseController<ResponseModel<LOV>>
    {
        public LOVController(IRepositoryWrapper repository) : base(repository) { }
        public ActionResult Index()
        {
            try
            {
                CommonViewModel.ObjList = new List<LOV>();
                CommonViewModel.ObjList = DataContext_Command.LOV_Get("", "LI").ToList();



                return View(CommonViewModel);
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


        //[CustomAuthorizeAttribute(AccessType_Enum.Read)]
        public ActionResult Partial_AddEditForm(string Lov_Column = "")
        {
            try
            {
                CommonViewModel.Obj = new LOV() { LOV_Column = "" };

                if (Lov_Column != "")
                {
                    CommonViewModel.Obj = DataContext_Command.LOV_Get(Lov_Column, "LE").FirstOrDefault();

                }

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


        public ActionResult IndexLovdtl(string Lov_Column = "", string Display_Text = "")
        {

            try
            {

                CommonViewModel.ObjList = new List<LOV>();
                CommonViewModel.Obj = new LOV() { LOV_Column = Lov_Column.Replace(" ", "%20"), Display_Text = Display_Text.Replace(" ", "%20") };
                CommonViewModel.ObjList = DataContext_Command.LOV_Detail_Get(Lov_Column, "", "LDI").ToList();
                if (CommonViewModel.ObjList.Count != 0)
                {
                    CommonViewModel.Obj.DisplayOrder = CommonViewModel.ObjList.Select(m => m.MaxDisplay_Seq_No).FirstOrDefault();
                }
                else
                {
                    CommonViewModel.Obj.DisplayOrder = 1;
                }

                return View(CommonViewModel);
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


        //[CustomAuthorizeAttribute(AccessType_Enum.Read)]
        public ActionResult Partial_AddEditForm_Lovdtl(string Lov_Column = "", string Display_Text = "", string DisplayOrder = "", string Lov_Code = "")
        {
            try
            {
                long displayOrderValue = 0; // default value

                if (!string.IsNullOrWhiteSpace(DisplayOrder))
                {
                    long.TryParse(DisplayOrder, out displayOrderValue);
                }
                CommonViewModel.Obj = new LOV() { LOV_Column = Lov_Column.Replace(" ", "%20"), Display_Text = Display_Text.Replace(" ", "%20"), DisplayOrder = displayOrderValue, LOV_Code = "" };

                if (Lov_Code != "")
                {
                    CommonViewModel.Obj = DataContext_Command.LOV_Detail_Get(Lov_Column, Lov_Code, "LDE").FirstOrDefault();

                }

                return PartialView("_Partial_AddEditForm_Lovdtl", CommonViewModel);
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
        //[CustomAuthorizeAttribute(AccessType_Enum.Write)]
        public ActionResult Save(LOV viewModel)
        {
            try
            {
                if (viewModel != null && viewModel != null)
                {
                    #region Validation

                    //if (!Common.IsAdmin())
                    //{
                    //	CommonViewModel.IsSuccess = false;
                    //	CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    //	CommonViewModel.Message = ResponseStatusMessage.UnAuthorize;

                    //	return Json(CommonViewModel);
                    //}

                    if (string.IsNullOrEmpty(viewModel.LOV_Column))
                    {
                        CommonViewModel.IsSuccess = false;
                        CommonViewModel.StatusCode = ResponseStatusCode.Error;
                        CommonViewModel.Message = "Please enter Column Name.";

                        return Json(CommonViewModel);
                    }
                    if (string.IsNullOrEmpty(viewModel.Display_Text))
                    {
                        CommonViewModel.IsSuccess = false;
                        CommonViewModel.StatusCode = ResponseStatusCode.Error;
                        CommonViewModel.Message = "Please enter Display Text.";

                        return Json(CommonViewModel);
                    }


                    #endregion

                    #region Database-Transaction

                    bool isNew = viewModel.Id == 0;

                    var (IsSuccess, response, Id) = DataContext_Command.Lov_Save(viewModel);
                    viewModel.Id = Id;

                    CommonViewModel.IsConfirm = IsSuccess;
                    CommonViewModel.IsSuccess = IsSuccess;
                    CommonViewModel.StatusCode = IsSuccess ? ResponseStatusCode.Success : ResponseStatusCode.Error;
                    CommonViewModel.Message = response;
                    CommonViewModel.RedirectURL = Url.Action("Index", "LOV", new { area = "Admin" });




                    return Json(CommonViewModel);



                    #endregion
                }
            }
            catch (Exception ex)
            {
                string actionName = ControllerContext.RouteData.Values["action"]?.ToString();
                string controllerName = ControllerContext.RouteData.Values["controller"]?.ToString();
                string clientIp = HttpContext.Connection?.RemoteIpAddress?.ToString();
                LogEntry.InsertLogEntryFromException(ex, controllerName, actionName, clientIp);
                return null;
            }

            CommonViewModel.Message = ResponseStatusMessage.Error;
            CommonViewModel.IsSuccess = false;
            CommonViewModel.StatusCode = ResponseStatusCode.Error;

            return Json(CommonViewModel);
        }

        [HttpPost]
        //[CustomAuthorizeAttribute(AccessType_Enum.Write)]
        public ActionResult SaveLovDetail(LOV viewModel)
        {
            try
            {
                if (viewModel != null && viewModel != null)
                {
                    #region Validation

                    //if (!Common.IsAdmin())
                    //{
                    //	CommonViewModel.IsSuccess = false;
                    //	CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    //	CommonViewModel.Message = ResponseStatusMessage.UnAuthorize;

                    //	return Json(CommonViewModel);
                    //}

                    if (string.IsNullOrEmpty(viewModel.LOV_Desc))
                    {
                        CommonViewModel.IsSuccess = false;
                        CommonViewModel.StatusCode = ResponseStatusCode.Error;
                        CommonViewModel.Message = "Please enter Description.";

                        return Json(CommonViewModel);
                    }



                    #endregion

                    #region Database-Transaction
                    bool isNew = viewModel.Id == 0;
                    var (IsSuccess, response, Id) = DataContext_Command.Lov_Detail_Save(viewModel);
                    viewModel.Id = Id;

                    CommonViewModel.IsConfirm = IsSuccess;
                    CommonViewModel.IsSuccess = IsSuccess;
                    CommonViewModel.StatusCode = IsSuccess ? ResponseStatusCode.Success : ResponseStatusCode.Error;
                    CommonViewModel.Message = response;
                    CommonViewModel.RedirectURL = Url.Content("~/Admin/") + this.ControllerContext.RouteData.Values["Controller"].ToString() + "/IndexLovdtl?Lov_Column=" + viewModel.LOV_Column + "&Display_Text=" + viewModel.Display_Text;



                    return Json(CommonViewModel);



                    #endregion
                }
            }
            catch (Exception ex)
            {
                string actionName = ControllerContext.RouteData.Values["action"]?.ToString();
                string controllerName = ControllerContext.RouteData.Values["controller"]?.ToString();
                string clientIp = HttpContext.Connection?.RemoteIpAddress?.ToString();
                LogEntry.InsertLogEntryFromException(ex, controllerName, actionName, clientIp);
                return null;
            }

            CommonViewModel.Message = ResponseStatusMessage.Error;
            CommonViewModel.IsSuccess = false;
            CommonViewModel.StatusCode = ResponseStatusCode.Error;

            return Json(CommonViewModel);
        }
        [HttpPost]
        //[CustomAuthorizeAttribute(AccessType_Enum.Delete)]
        public ActionResult DeleteConfirmed(string Lov_Column = "", string Lov_Code = "", string Display_Text = "")
        {
            try
            {
                //if (Common.IsAdmin() && !_context.Using<UserRoleMapping>().Any(x => x.EmployeeId == Id)
                //	&& _context.Employees.Any(x => x.Id > 1 && x.Id == Id))
                if (true)
                {
                    var (IsSuccess, response) = DataContext_Command.LOV_Dtl_Delete(Lov_Column, Lov_Code);

                    CommonViewModel.IsConfirm = IsSuccess;
                    CommonViewModel.IsSuccess = IsSuccess;
                    CommonViewModel.StatusCode = IsSuccess ? ResponseStatusCode.Success : ResponseStatusCode.Error;
                    CommonViewModel.Message = response;
                    CommonViewModel.RedirectURL = Url.Content("~/Admin/") + this.ControllerContext.RouteData.Values["Controller"].ToString() + "/IndexLovDtl?Lov_Column=" + Lov_Column + "&Display_Text=" + Display_Text; ;

                    //var obj = _context.Employees.GetByCondition(x => x.Id == Id).FirstOrDefault();

                    //_context.Entry(obj).State = EntityState.Deleted;
                    //_context.SaveChanges();

                    //CommonViewModel.IsConfirm = true;
                    //CommonViewModel.IsSuccess = true;
                    //CommonViewModel.StatusCode = ResponseStatusCode.Success;
                    //CommonViewModel.Message = ResponseStatusMessage.Delete;

                    //CommonViewModel.RedirectURL = Url.Action("Index", "Employee", new { area = "Admin" });


                    return Json(CommonViewModel);
                }
            }
            catch (Exception ex)
            {
                string actionName = ControllerContext.RouteData.Values["action"]?.ToString();
                string controllerName = ControllerContext.RouteData.Values["controller"]?.ToString();
                string clientIp = HttpContext.Connection?.RemoteIpAddress?.ToString();
                LogEntry.InsertLogEntryFromException(ex, controllerName, actionName, clientIp);
                return null;
            }

            CommonViewModel.IsSuccess = false;
            CommonViewModel.StatusCode = ResponseStatusCode.Error;
            CommonViewModel.Message = ResponseStatusMessage.Unable_Delete;

            return Json(CommonViewModel);
        }
    }
}
