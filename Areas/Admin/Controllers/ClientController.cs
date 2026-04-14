using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
    public class ClientController : BaseController<ResponseModel<Client>>
    {
        public ClientController(IRepositoryWrapper repository) : base(repository) { }
        public IActionResult Index()
        {
            CommonViewModel.ObjList = new List<Client>();
            try
            {
               CommonViewModel.ObjList = ClientServices.GET(0).ToList();
                
            }
            catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }


            return View(CommonViewModel);
        }

        [HttpGet]
        public IActionResult Partial_AddEditForm(int Id)
        {
           
            try
            {
                CommonViewModel.Obj = new Client();


                if (Id> 0)
                {
                    CommonViewModel.Obj = ClientServices.GET(Id).FirstOrDefault();
                }               
               
            }
            catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }

           
            return PartialView("_Partial_AddEditForm", CommonViewModel);
        }

        [HttpPost]
        public JsonResult Save(Client viewModel)
        {

            try
            {
               

                if (string.IsNullOrEmpty(viewModel.Name))
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please Enter Client Name.";

                    return Json(CommonViewModel);
                }
                if (string.IsNullOrEmpty(viewModel.Phone))
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please Enter Phone No.";

                    return Json(CommonViewModel);
                }
                if (!string.IsNullOrEmpty(viewModel.Phone) &&
                        !ValidateField.IsValidMobileNo_D10(viewModel.Phone))
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please Enter Valid 10 Digit Phone Number.";
                    return Json(CommonViewModel);
                }
                if (string.IsNullOrEmpty(viewModel.Email))
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please Enter Email.";

                    return Json(CommonViewModel);
                }
                if (!string.IsNullOrEmpty(viewModel.Email) &&
                       !ValidateField.IsValidEmail(viewModel.Email))
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please Enter Valid Email.";
                    return Json(CommonViewModel);
                }
               var (IsSuccess, response, Id) = ClientServices.Save(viewModel);

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

            var (IsSuccess, response) = ClientServices.Delete(Id);
            
           
            CommonViewModel.IsConfirm = IsSuccess;
            CommonViewModel.IsSuccess = IsSuccess;
            CommonViewModel.StatusCode = IsSuccess ? ResponseStatusCode.Success : ResponseStatusCode.Error;
            CommonViewModel.Message = response;
            CommonViewModel.RedirectURL = Url.Action("Index", "Client", new { area = "Admin" });


            return Json(CommonViewModel);
        }
    }
}
