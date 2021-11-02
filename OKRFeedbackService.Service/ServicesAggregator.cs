using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using OKRFeedbackService.EF;
using OKRFeedbackService.Service.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace OKRFeedbackService.Service
{
    [ExcludeFromCodeCoverage]
    public class ServicesAggregator : IServicesAggregator
    {
        public IUnitOfWorkAsync UnitOfWorkAsync { get; set; }
        public IOperationStatus OperationStatus { get; set; }
        public IConfiguration Configuration { get; set; }
        public IHostingEnvironment HostingEnvironment { get; set; }
        public IMapper Mapper { get; set; }

        public ServicesAggregator(IUnitOfWorkAsync unitOfWorkAsync, IOperationStatus operationStatus, IConfiguration configuration, IHostingEnvironment environment)
        {
            UnitOfWorkAsync = unitOfWorkAsync;
            OperationStatus = operationStatus;
            Configuration = configuration;
            HostingEnvironment = environment;
        }
    }
}
