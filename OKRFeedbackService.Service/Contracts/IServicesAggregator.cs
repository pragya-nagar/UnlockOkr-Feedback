﻿using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using OKRFeedbackService.EF;

namespace OKRFeedbackService.Service.Contracts
{
    public interface IServicesAggregator
    {
        IUnitOfWorkAsync UnitOfWorkAsync { get; set; }
        IOperationStatus OperationStatus { get; set; }
        IConfiguration Configuration { get; set; }
        IHostingEnvironment HostingEnvironment { get; set; }
        IMapper Mapper { get; set; }
    }
}
