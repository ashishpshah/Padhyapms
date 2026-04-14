using System.ComponentModel.DataAnnotations;

namespace PMMS.Infra
{
    public class ErrorLog
    {

        [Key]
        public long ErrorId { get; set; }

        public string? ActionName { get; set; }


        public string? ControllerName { get; set; }

        public string ErrorMessage { get; set; }

        public string? ErrorType { get; set; }

        public string? StackTrace { get; set; }

        public string? RequestUrl { get; set; }

        public string? RequestPayload { get; set; }


        public string? UserAgent { get; set; }

        public long? UserId { get; set; }
        public string? ClientIP { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }

    }

    public class ExceptionDto
    {
     
        public string Message { get; set; } = default!;

        public string? StackTrace { get; set; }

        public string? ControllerName { get; set; }
        public string? RequestUrl { get; set; }
        public string? RequestPayload { get; set; }
        public string? UserAgent { get; set; }
        public long? UserId { get; set; }
        public string? ClientIP { get; set; }
        public string? CreatedBy { get; set; }
    }
}
