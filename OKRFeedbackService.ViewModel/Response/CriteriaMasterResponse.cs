

using System.Collections.Generic;

namespace OKRFeedbackService.ViewModel.Response
{
    public class CriteriaMasterResponse
    {
       
        public int? CriteriaTypeId { get; set; }
      public string CriteriaName { get; set; }
        public List<Criteria> CriteriaList { get; set; }
       
    }

    public class Criteria
    {
        public long CriteriaMasterId { get; set; }
        public string CriteriaName { get; set; }
    }
}
