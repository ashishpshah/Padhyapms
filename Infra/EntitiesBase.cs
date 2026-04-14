using Microsoft.AspNetCore.DataProtection;
using PMMS.Infra;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMMS
{
    public class EntitiesBase
    {
        //[Key, Column(Order = 1)]
        //[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public virtual long Id { get; set; }
        public virtual long CreatedBy { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public virtual Nullable<System.DateTime> CreatedDate { get; set; }
        public virtual long LastModifiedBy { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public virtual Nullable<System.DateTime> LastModifiedDate { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual bool IsDeleted { get; set; }
        [NotMapped] public virtual string CreatedDate_Text { get; set; }
        [NotMapped] public virtual string LastModifiedDate_Text { get; set; }
        [NotMapped] public virtual bool IsSetDefault { get; set; }
        [NotMapped] public virtual int SrNo { get; set; }

        [NotMapped] public string Id_Encrypted { get; set; }
        [NotMapped] public long Id_Decrypted { get; set; }

        public EntitiesBase()
        {
            Id_Encrypted = Id > 0 ? Id.ToEncryptedId(AppHttpContextAccessor.GetDataProtector()) : "";
            Id_Decrypted = !string.IsNullOrEmpty(Id_Encrypted) ? Id_Encrypted.ToDecryptedId(AppHttpContextAccessor.GetDataProtector()) : 0;

            CreatedDate_Text = CreatedDate != null ? CreatedDate?.ToString(Common.DateTimeFormat_ddMMyyyy).Replace("-", "/") : "";
            LastModifiedDate_Text = LastModifiedDate != null ? LastModifiedDate?.ToString(Common.DateTimeFormat_ddMMyyyy).Replace("-", "/") : "";
        }
    }

    public static class EncryptionExtensions
    {
        public static string ToEncryptedId(this long id, IDataProtector protector)
        {
            return protector.Protect(id.ToString());
        }

        public static long ToDecryptedId(this string value, IDataProtector protector)
        {
            return Convert.ToInt64(protector.Unprotect(value));
        }
    }
}
