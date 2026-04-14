using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using PMMS.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMMS
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter user name.")] public string UserName { get; set; }
        [Required(ErrorMessage = "Please enter password.")] public string Password { get; set; }
        public string User_Type { get; set; }
        public bool RememberMe { get; set; }
        public int Punch_In_Count { get; set; }
        public TimeOnly? PunchInTime { get; set; }
        public TimeOnly? PunchOutTime { get; set; }
        public decimal Worked_Hours_Decimal { get; set; }
        public string   Worked_Hours { get; set; }
        public int TotalMinutes { get; set; }
        public LoginViewModel PuchOutData { get; set; }
        public int MyProperty { get; set; }
       
    }
    public class RegisterViewModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
        public string Role { get; set; }
    }
    public class ForgotPassword
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string OTP { get; set; }

        // Add these properties to match your table
        public DateTime CreatedAt { get; set; }
        public bool IsUsed { get; set; }
    }

    public class DashboardCountViewModel
    {
        public int PendingLeave60Days { get; set; }
        public int ClientFollowup5Days { get; set; }
        public int BNIFollowup5Days { get; set; }
        public int BNIMeeting5Days { get; set; }
    }
}
