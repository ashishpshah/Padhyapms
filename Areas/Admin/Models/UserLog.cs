using System;
using System.Collections.Generic;

namespace PMMS
{
    public partial class UserLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? MenuId { get; set; }
        public DateTime? EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public string ActionPerformed { get; set; }
        public string IpAddress { get; set; }
    }
}
