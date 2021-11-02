using System;
using System.Collections.Generic;
using System.Text;

namespace OKRFeedbackService.ViewModel.Response
{
    public class DatabaseVaultResponse
    {
        public string ConnectionString { get; set; }
        public string CurrentSchema { get; set; }
    }
}
