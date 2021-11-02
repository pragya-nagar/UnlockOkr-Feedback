﻿using System.Collections.Generic;

namespace OKRFeedbackService.ViewModel.Response
{
    public class EmployeeResult
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int HeaderCode { get; set; }
        public List<UserResponse> Results { get; set; }
    }
}
