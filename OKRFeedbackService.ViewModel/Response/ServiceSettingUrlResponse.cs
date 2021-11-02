using System;
using System.Collections.Generic;
using System.Text;

namespace OKRFeedbackService.ViewModel.Response
{
    public class ServiceSettingUrlResponse
    {
        public string UnlockLog { get; set; }
        public string OkrBaseAddress { get; set; }
        public string OkrUnlockTime { get; set; }
        public string FrontEndUrl { get; set; }
        public string ResetPassUrl { get; set; }
        public string NotificationBaseAddress { get; set; }
    }
}
