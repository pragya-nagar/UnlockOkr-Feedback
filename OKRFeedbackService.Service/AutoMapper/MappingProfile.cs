using AutoMapper;
using OKRFeedbackService.EF;
using OKRFeedbackService.ViewModel.Request;
using System.Diagnostics.CodeAnalysis;

namespace OKRFeedbackService.Service.AutoMapper
{
    [ExcludeFromCodeCoverage]
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AskFeedbackRequest, FeedbackRequest>()
              .ForMember(dest => dest.RaisedTypeId, opts => opts.MapFrom(src => src.RaisedTypeId))
              .ForMember(dest => dest.RaisedForId, opts => opts.MapFrom(src => src.RaisedForId))
              .ForMember(dest => dest.FeedbackById, opts => opts.MapFrom(src => src.FeedbackById))
              .ForMember(dest => dest.RequestRemark, opts => opts.MapFrom(src => src.RequestRemark))
              .ForMember(dest => dest.FeedbackOnTypeId, opts => opts.MapFrom(src => src.FeedbackOnTypeId))
                                          .ReverseMap();
        }
    }
}
