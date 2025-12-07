using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anamel.Core.Entities
{
    public class OtpData
    {
        public string Otp { get; set; }
        public DateTime ExpiryTime { get; set; }
        public int Attempts { get; set; }
        public bool IsVerified { get; set; }
    }
}
