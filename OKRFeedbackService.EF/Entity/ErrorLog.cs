using System;
using System.Diagnostics.CodeAnalysis;

namespace OKRFeedbackService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class ErrorLog
    {
        public long LogId { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string PageName { get; set; }
        public string FunctionName { get; set; }
        public string ErrorDetail { get; set; }
    }
}
