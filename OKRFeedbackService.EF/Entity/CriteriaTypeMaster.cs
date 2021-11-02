using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OKRFeedbackService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class CriteriaTypeMaster
    {
        public CriteriaTypeMaster()
        {
            CriteriaMaster = new HashSet<CriteriaMaster>();
        }

        public int CriteriaTypeId { get; set; }
        public string CriteriaTypeName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public long? UpdatedBy { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<CriteriaMaster> CriteriaMaster { get; set; }
    }
}
