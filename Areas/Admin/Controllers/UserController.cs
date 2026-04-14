using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PMMS.Controllers;
using PMMS.Infra;
using PMMS.Infra.Services;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PMMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : BaseController<ResponseModel<User>>
    {

        public UserController(IRepositoryWrapper repository) : base(repository) { }

        // GET: Admin/User
        //[CustomAuthorizeAttribute(AccessType_Enum.Read)]
        public ActionResult Index()
        {
            try
            {
                CommonViewModel.ObjList = new List<User>();

                

                CommonViewModel.ObjList = (
                    from x in _context.Using<User>().GetAll().ToList()

                    join y in _context.Using<UserRoleMapping>().GetAll().ToList()
                        on x.Id equals y.UserId into yx
                    from y in yx.DefaultIfEmpty()

                    join z in _context.Using<Role>().GetAll().ToList()
                        on (y != null ? y.RoleId : 0) equals z.Id into zx
                    from z in zx.DefaultIfEmpty()

                    where (y != null ? y.RoleId : 1) > 0
                          && x.Id > 1
                          && (y != null ? y.UserId : 0) != Common.LoggedUser_Id()

                    select new User()
                    {
                        Id = x.Id,
                        UserName = x.UserName,
                        Email = x.Email,
                        User_Role_Id = (z != null ? z.Id : 0),
                        User_Role = (z != null ? z.Name : ""),
                        CreatedBy = x.CreatedBy
                    }).ToList();
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

        public ActionResult Partial_AddEditForm(long Id = 0)
        {
            try
            {
                CommonViewModel.Obj = new User() { };

                if (Id > 0)
                {
                    CommonViewModel.Obj = (from x in _context.Using<User>().GetAll().ToList()
                                           join y in _context.Using<UserRoleMapping>().GetAll().ToList() on x.Id equals y.UserId into yx
                                           from y in yx.DefaultIfEmpty()
                                           join z in _context.Using<Role>().GetAll().ToList() on (y != null ? y.RoleId : 0) equals z.Id into zx
                                           from z in zx.DefaultIfEmpty()
                                           where x.Id == Id && x.Id > 1 && (y != null ? y.UserId : 0) != Common.LoggedUser_Id()
                                           select new User() { Id = x.Id, UserName = x.UserName, Email = x.Email,User_Role_Id = (z != null ? z.Id : 0), User_Role = (z != null ? z.Name : ""), IsActive = x.IsActive }).FirstOrDefault();
                }

                //if (!string.IsNullOrEmpty(CommonViewModel.Obj.Password))
                //    //CommonViewModel.Obj.Password = Common.Decrypt(CommonViewModel.Obj.Password);
                //    CommonViewModel.Obj.Password = "";

                var list = new List<SelectListItem_Custom>();

                var listRole = _context.Using<Role>().GetByCondition(x => x.Id > 1).Select(x => new SelectListItem_Custom(x.Id.ToString(), x.Name, "R")).Distinct().ToList();

                if (listRole != null && listRole.Count() > 0) list.AddRange(listRole);

                if (CommonViewModel.Obj != null)
                {
                    CommonViewModel.Obj.User_Id_Str = CommonViewModel.Obj != null && CommonViewModel.Obj.Id > 0 ? Common.Encrypt(CommonViewModel.Obj.Id.ToString()) : null;
                    CommonViewModel.Obj.Role_Id_Str = CommonViewModel.Obj != null && CommonViewModel.Obj.User_Role_Id > 0 ? Common.Encrypt(CommonViewModel.Obj.User_Role_Id.ToString()) : null;
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
        public ActionResult Save(User viewModel)
        {
            try
            {
                if (viewModel != null)
                {


                    #region Validation

                    if (string.IsNullOrEmpty(viewModel.UserName))
                    {

                        CommonViewModel.Message = "Please enter Username.";
                        CommonViewModel.IsSuccess = false;
                        CommonViewModel.StatusCode = ResponseStatusCode.Error;

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
                    bool isUserNameOrEmailExists = _context.Using<User>().Any(x =>
                     (
                         x.UserName.ToLower() == viewModel.UserName.ToLower()
                         || x.Email.ToLower() == viewModel.Email.ToLower()
                     )
                     && x.Id != viewModel.Id
                    );

                    if (isUserNameOrEmailExists)
                    {
                        CommonViewModel.Message = "Username or Email already exists. Please try another.";
                        CommonViewModel.IsSuccess = false;
                        CommonViewModel.StatusCode = ResponseStatusCode.Error;
                        return Json(CommonViewModel);
                    }
                    if (string.IsNullOrEmpty(viewModel.Password) && viewModel.Id == 0)
                    {

                        CommonViewModel.Message = "Please enter Password.";
                        CommonViewModel.IsSuccess = false;
                        CommonViewModel.StatusCode = ResponseStatusCode.Error;

                        return Json(CommonViewModel);
                    }

                    if (viewModel.User_Role_Id == 0)
                    {

                        CommonViewModel.Message = "Please select Role.";
                        CommonViewModel.IsSuccess = false;
                        CommonViewModel.StatusCode = ResponseStatusCode.Error;

                        return Json(CommonViewModel);
                    }
                    

                    var objAvailable = (from x in _context.Using<User>().GetAll().ToList()
                                        join y in _context.Using<UserRoleMapping>().GetAll().ToList() on x.Id equals y.UserId into yx
                                        from y in yx.DefaultIfEmpty()
                                        join z in _context.Using<Role>().GetAll().ToList() on (y != null ? y.RoleId : 0) equals z.Id into zx
                                        from z in zx.DefaultIfEmpty()
                                        where x.UserName.ToLower().Trim().Replace(" ", "") == viewModel.UserName.ToLower().Trim().Replace(" ", "")
                                        && x.Id != viewModel.Id
                                        select new User() { Id = x.Id, UserName = x.UserName, User_Role_Id = (z != null ? z.Id : 1), User_Role = (z != null ? z.Name : "") }).FirstOrDefault();

                    if (objAvailable != null || viewModel.User_Role_Id == 0)
                    {
                        CommonViewModel.Message = "Username already exist. Please try another Username.";
                        CommonViewModel.IsSuccess = false;
                        CommonViewModel.StatusCode = ResponseStatusCode.Error;

                        return Json(CommonViewModel);
                    }

                    #endregion


                    #region Database-Transaction

                    using (var transaction = _context.BeginTransaction())
                    {
                        try
                        {
                            if (viewModel.IsPassword_Reset == true) viewModel.Password = "12345";

                            if (!string.IsNullOrEmpty(viewModel.Password)) viewModel.Password = Common.Encrypt(viewModel.Password);

                            //User obj = _context.Using<User>().Where(x => x.UserName.ToLower().Replace(" ", "") == viewModel.UserName.ToLower().Replace(" ", "")).FirstOrDefault();
                            User obj = _context.Using<User>().GetByCondition(x => x.Id > 0 && x.Id == viewModel.Id).FirstOrDefault();

                            if (obj != null && Common.IsAdmin())
                            {
                                obj.UserName = viewModel.UserName;
                                obj.Email = viewModel.Email;

                                if (viewModel.IsPassword_Reset == true) obj.Password = viewModel.Password;


                                obj.IsActive = viewModel.IsActive;

                                _context.Using<User>().Update(obj);
                                //                        _context.Entry(obj).State = EntityState.Modified;
                                //_context.SaveChanges();

                            }
                            else if (Common.IsAdmin())
                            {
                                var _user = _context.Using<User>().Add(viewModel);
                                viewModel.Id = _user.Id;
                                //_context.SaveChanges();
                                //_context.Entry(viewModel).Reload();

                            }



                            var role = _context.Using<Role>().GetByCondition(x => x.Id == viewModel.User_Role_Id).FirstOrDefault();



                            if (viewModel.Id > 0 && role != null)
                            {
                                try
                                {
                                    UserRoleMapping UserRole = _context.Using<UserRoleMapping>().GetByCondition(x => x.UserId == viewModel.Id && x.RoleId == viewModel.RoleId).FirstOrDefault();

                                    if (UserRole != null)
                                    {
                                        UserRole.RoleId = viewModel.User_Role_Id;

                                        _context.Using<UserRoleMapping>().Update(UserRole);
                                        //_context.Entry(UserRole).State = EntityState.Modified;
                                        //_context.SaveChanges();
                                    }
                                    else
                                    {
                                        _context.Using<UserRoleMapping>().Add(new UserRoleMapping() { UserId = viewModel.Id, RoleId = viewModel.User_Role_Id });
                                        //_context.SaveChanges();
                                    }

                                    var listUserMenuAccess = _context.Using<UserMenuAccess>().GetByCondition(x => x.UserId == viewModel.Id && x.RoleId == viewModel.RoleId).ToList();

                                    if (listUserMenuAccess != null && listUserMenuAccess.Count() > 0)
                                    {
                                        foreach (var access in listUserMenuAccess)
                                        {
                                            _context.Using<UserMenuAccess>().Delete(access);
                                            //_context.Entry(access).State = EntityState.Deleted;
                                            //_context.SaveChanges();
                                        }
                                    }

                                    foreach (var item in _context.Using<RoleMenuAccess>().GetByCondition(x => x.RoleId == viewModel.User_Role_Id).ToList())
                                    {
                                        var userMenuAccess = new UserMenuAccess()
                                        {
                                            MenuId = item.MenuId,
                                            UserId = viewModel.Id,
                                            RoleId = viewModel.User_Role_Id,
                                            IsCreate = item.IsCreate,
                                            IsUpdate = item.IsUpdate,
                                            IsRead = item.IsRead,
                                            IsDelete = item.IsDelete,
                                            IsActive = item.IsActive,
                                            IsDeleted = item.IsDeleted,
                                            IsSetDefault = true,

                                        };

                                        _context.Using<UserMenuAccess>().Add(userMenuAccess);
                                        //_context.SaveChanges();
                                    }

                                }
                                catch (Exception ex) { }
                            }


                            CommonViewModel.IsConfirm = true;
                            CommonViewModel.IsSuccess = true;
                            CommonViewModel.StatusCode = ResponseStatusCode.Success;
                            CommonViewModel.Message = "Record saved successfully ! ";
                            CommonViewModel.RedirectURL = Url.Action("Index", "User", new { area = "Admin" });

                            transaction.Commit();

                            return Json(CommonViewModel);
                        }
                        catch (Exception ex) { transaction.Rollback(); }
                    }

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
        public ActionResult DeleteConfirmed(long Id)
        {
            try
            {
                //if (_context.Using<User>().GetAll().ToList().Any(x => x.Id == Id))
                if (_context.Using<User>().GetAll().ToList().Any(x => x.Id == Id))
                {
                    var UserRole = _context.Using<UserRoleMapping>().GetByCondition(x => x.UserId == Id).ToList();

                    if (UserRole != null)
                        foreach (var obj in UserRole)
                        {
                            _context.Using<UserRoleMapping>().Delete(obj);
                            //_context.Entry(obj).State = EntityState.Deleted;
                            //_context.SaveChanges();
                        }

                    var UserMenu = _context.Using<UserMenuAccess>().GetByCondition(x => x.UserId == Id).ToList();

                    if (UserMenu != null)
                        foreach (var obj in UserMenu)
                        {
                            _context.Using<UserMenuAccess>().Delete(obj);
                            //_context.Entry(obj).State = EntityState.Deleted;
                            //_context.SaveChanges();
                        }

                    var user = _context.Using<User>().GetByCondition(x => x.Id == Id).FirstOrDefault();

                    if (user != null)
                    {
                        _context.Using<User>().Delete(user);
                        //_context.Entry(user).State = EntityState.Deleted;
                        //_context.SaveChanges();
                    }



                    CommonViewModel.IsConfirm = true;
                    CommonViewModel.IsSuccess = true;
                    CommonViewModel.StatusCode = ResponseStatusCode.Success;
                    CommonViewModel.Message = "Data deleted successfully ! ";
                    CommonViewModel.RedirectURL = Url.Action("Index", "User", new { area = "Admin" });

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


            CommonViewModel.Message = "Unable to delete User.";
            CommonViewModel.IsSuccess = false;
            CommonViewModel.StatusCode = ResponseStatusCode.Error;

            return Json(CommonViewModel);
        }

    }

}