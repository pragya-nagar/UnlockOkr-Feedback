using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace OKRFeedbackService.EF
{
    [ExcludeFromCodeCoverage]
    public class EntityBase : IObjectState
    {
        [NotMapped]
        public ObjectState ObjectStateEnum { get; set; }
    }
}
