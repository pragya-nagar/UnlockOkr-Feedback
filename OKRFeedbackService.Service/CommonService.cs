using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OKRFeedbackService.Common;
using OKRFeedbackService.EF;
using OKRFeedbackService.Service.Contracts;
using OKRFeedbackService.ViewModel.Response;
using Serilog;


namespace OKRFeedbackService.Service
{
    [ExcludeFromCodeCoverage]
    public class CommonService : BaseService, ICommonService
    {
        public CommonService(IServicesAggregator servicesAggregateService) : base(servicesAggregateService)
        {
            UnitOfWorkAsync.RepositoryAsync<FeedbackRequest>();
            UnitOfWorkAsync.RepositoryAsync<FeedbackDetail>();
            UnitOfWorkAsync.RepositoryAsync<ErrorLog>();
            UnitOfWorkAsync.RepositoryAsync<Comment>();
            UnitOfWorkAsync.RepositoryAsync<OneToOneDetail>();
        }

        public async Task<UserIdentity> GetUserIdentity()
        {
            Logger.Information("GetUserIdentity called");
            var hasIdentity = HttpContext.Request.Headers.TryGetValue("UserIdentity", out var userIdentity);
            Logger.Information("is found the user identity in  header-" + hasIdentity);
            if (!hasIdentity) return await GetUserIdentity(UserToken);
            Logger.Information("Value found the user identity in  header-" + userIdentity);
            var decryptVal = Encryption.DecryptStringAes(userIdentity, Configuration.GetValue<string>("Encryption:SecretKey"),
                Configuration.GetValue<string>("Encryption:SecretIVKey"));
            var identity = JsonConvert.DeserializeObject<UserIdentity>(decryptVal);
            Logger.Information("User information is received for employee id" + identity.EmployeeId);
            return await Task.FromResult(identity).ConfigureAwait(false);
        }
        public async Task<UserIdentity> GetUserIdentity(string jwtToken)
        {
            UserIdentity loginUserDetail = new UserIdentity();
            if (jwtToken != "")
            {
                using var httpClient = GetHttpClient(jwtToken);
                httpClient.BaseAddress = new Uri(Configuration.GetSection("User:BaseUrl").Value);
                using var response = await httpClient.PostAsync("api/User/Identity", new StringContent(""));
                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<PayloadCustom<UserIdentity>>(apiResponse);
                    loginUserDetail = user.Entity;
                }
            }
            return loginUserDetail;
        }
    }
}
