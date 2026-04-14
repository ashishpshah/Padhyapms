using Microsoft.AspNetCore.Mvc;
using PMMS.Areas.Admin.Services;
using PMMS.Controllers;
using PMMS.Infra;
using PMMS.Infra.Services;
using PMMS.Models;
using System.Data;
using System.Net.Mail;

namespace PMMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ServiceController : BaseController<ResponseModel<Service>>
    {
        public ServiceController(IRepositoryWrapper repository) : base(repository) { }
        public IActionResult Index()
        {
            CommonViewModel.ObjList = new List<Service>();
            try
            {
               CommonViewModel.ObjList = ServiceServices.GET(0).ToList();
                
            }
            catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }


            return View(CommonViewModel);
        }

        [HttpGet]
        public IActionResult Partial_AddEditForm(int Id)
        {
           
            try
            {
                CommonViewModel.Obj = new Service();
                CommonViewModel.Obj.ServiceChecklist = new List<ServiceChecklist>();

                if (Id> 0)
                {
                    CommonViewModel.Obj = ServiceServices.GET(Id).FirstOrDefault();
                    CommonViewModel.Obj.ServiceChecklist = ServiceServices.GET_ServiceChecklist(Id).ToList();
                }               
               
            }
            catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }

           
            return PartialView("_Partial_AddEditForm", CommonViewModel);
        }

        [HttpPost]
        public JsonResult Save(Service viewModel)
        {

            try
            {
               

                if (string.IsNullOrEmpty(viewModel.ServiceName))
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please Enter Service Name.";

                    return Json(CommonViewModel);
                }
                if (string.IsNullOrEmpty(viewModel.Slug))
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please Enter Slug.";

                    return Json(CommonViewModel);
                }
                if (string.IsNullOrEmpty(viewModel.ShortDescription))
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please Enter Short Description.";

                    return Json(CommonViewModel);
                }
                if (string.IsNullOrEmpty(viewModel.Title))
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please Enter Title.";

                    return Json(CommonViewModel);
                }                
                if (string.IsNullOrEmpty(viewModel.BestFor))
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please Enter Best For.";

                    return Json(CommonViewModel);
                }
                if (string.IsNullOrEmpty(viewModel.Technologies))
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please Enter Technologies.";

                    return Json(CommonViewModel);
                }
                if (string.IsNullOrEmpty(viewModel.Description))
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please Enter Description.";

                    return Json(CommonViewModel);
                }
                if (viewModel.ServiceChecklist != null && viewModel.ServiceChecklist.Count > 0)
                {
                    for (int i = 0; i < viewModel.ServiceChecklist.Count; i++)
                    {
                        if (string.IsNullOrEmpty(viewModel.ServiceChecklist[i].ItemName))
                        {
                            CommonViewModel.IsSuccess = false;
                            CommonViewModel.StatusCode = ResponseStatusCode.Error;
                            CommonViewModel.Message = "Please Enter Item Name at Line No " + (i + 1);
                            return Json(CommonViewModel);
                        }                      
                    }
                }
                var (IsSuccess, response, Id) = ServiceServices.Save(viewModel);

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
            

            var (IsSuccess, response) = ServiceServices.Delete(Id);
            
           
            CommonViewModel.IsConfirm = IsSuccess;
            CommonViewModel.IsSuccess = IsSuccess;
            CommonViewModel.StatusCode = IsSuccess ? ResponseStatusCode.Success : ResponseStatusCode.Error;
            CommonViewModel.Message = response;
            CommonViewModel.RedirectURL = Url.Action("Index", "Service", new { area = "Admin" });


            return Json(CommonViewModel);
        }
    }
}
