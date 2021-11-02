
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OKRFeedbackService.EF;
using OKRFeedbackService.Service.Contracts;
using OKRFeedbackService.ViewModel.Request;
using OKRFeedbackService.ViewModel.Response;
using OKRFeedbackService.WebCore.Controller;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace OKRFeedbackService.UnitTest.Controller
{
    public class PersonalizeFeedbackControllerUnitTest
    {
        private readonly Mock<IPersonalizeFeedbackService> _feedbackService;
        private readonly Mock<IOperationStatus> _operationStatus;
        private readonly Mock<ICommonService> _commonService;

        private readonly PersonalizeFeedbackController _feedbackController;


        public PersonalizeFeedbackControllerUnitTest()
        {
            _feedbackService = new Mock<IPersonalizeFeedbackService>();
            _operationStatus = new Mock<IOperationStatus>();
            _commonService = new Mock<ICommonService>();
            _feedbackController = new PersonalizeFeedbackController(_feedbackService.Object, _commonService.Object);
            SetUserClaimsAndRequest();

        }

        [Fact]
        public async Task InsertAskRequest_IsSuccessful()
        {
            AskPersonalFeedbackRequest askFeedbackRequest = new AskPersonalFeedbackRequest
            {
                RaisedTypeId = 1,
                RaisedForId = new List<long> { 795, 188 },
                RequestRemark = "",
                FeedbackOnTypeId = 1,
                FeedbackById = 14254
            };

            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback", Status = true } } } }
            };

            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.AddPersonalFeedbackAsync(It.IsAny<AskPersonalFeedbackRequest>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(_operationStatus.Object);

            var result = await _feedbackController.AddPersonalFeedback(askFeedbackRequest);

            PayloadCustom<FeedbackRequest> requData = ((PayloadCustom<FeedbackRequest>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task InsertAskRequest_Error()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback", Status = true } } } }
            };

            AskPersonalFeedbackRequest request = new AskPersonalFeedbackRequest
            {
                FeedbackById = 14254,
                RaisedForId = new List<long> { 795, 188 }

            };

            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.AddPersonalFeedback(request);
            PayloadCustom<FeedbackRequest> requData = ((PayloadCustom<FeedbackRequest>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);
        }

       
        [Fact]
        public async Task InsertAskRequest_InvalidToken()
        {
            _commonService.Setup(e => e.GetUserIdentity());

            var result = await _feedbackController.AddPersonalFeedback(null) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task InsertAskRequest_Forbidden()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback" } } } }
            };

            AskPersonalFeedbackRequest request = new AskPersonalFeedbackRequest
            {
                FeedbackById = 14254
            };

            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.AddPersonalFeedback(request) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task RequestOneOne_InvalidToken()
        {
            _commonService.Setup(e => e.GetUserIdentity());

            var result = await _feedbackController.RequestOneOne(null) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task RequestOneOne_Error()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Feedback Request 1:1", Status = true } } } }
            };

            PersonalFeedbackOneOnOneRequest req = new PersonalFeedbackOneOnOneRequest() { RequestedTo = new List<long> { 188, 795 } };

            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.RequestOneOne(req);
            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task RequestOneOne_ModelError()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Feedback Request 1:1", Status = true } } } }
            };

            PersonalFeedbackOneOnOneRequest req = new PersonalFeedbackOneOnOneRequest() { RequestedTo = new List<long> { 188, 795 },OnetoOneRemark = "" };

            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.RequestOneOne(req);
            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task RequestOneOne_ModelStateError()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Feedback Request 1:1", Status = true } } } }
            };

            PersonalFeedbackOneOnOneRequest req = new PersonalFeedbackOneOnOneRequest() { RequestedTo = new List<long> { 188, 795 }, OnetoOneRemark = "abc",RequestType = 0 };

            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.RequestOneOne(req);
            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);
        }


        [Fact]
        public async Task RequestOneOne_NotSuccess()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Feedback Request 1:1", Status = true } } } }
            };

            PersonalFeedbackOneOnOneRequest req = new PersonalFeedbackOneOnOneRequest() { RequestedTo = new List<long> { 188, 795 } };

            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.RequestOneOne(req);
            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task RequestOneOne_Success()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Feedback Request 1:1", Status = true } } } }
            };

            PersonalFeedbackOneOnOneRequest req = new PersonalFeedbackOneOnOneRequest() { RequestType = 1, OnetoOneRemark = "test", RequestedFrom = 1, RequestedTo = new List<long> { 188, 795 } };

            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.AddOneOnOneRequestAsync(It.IsAny<PersonalFeedbackOneOnOneRequest>(), It.IsAny<UserIdentity>(), It.IsAny<string>()));

            var result = await _feedbackController.RequestOneOne(req);
            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);

            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task RequestOneOne_Forbidden()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Feedback Request 1:1" } } } }
            };

            PersonalFeedbackOneOnOneRequest req = new PersonalFeedbackOneOnOneRequest() { };

            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.RequestOneOne(req) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task AskFeedbackDetail_IsSuccessful()
        {
            List<AskPersonalFeedbackResponse> askPersonalFeedbackResponse = new List<AskPersonalFeedbackResponse>();
            long employeeId = 238;
            bool value = true;
            string searchname = "";
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback", Status = true } } } }
            };

            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.GetAskedFeedbackUserDetailAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(askPersonalFeedbackResponse);

            var result = await _feedbackController.AskFeedbackDetail(employeeId, value, searchname);

            PayloadCustom<AskPersonalFeedbackResponse> requData = ((PayloadCustom<AskPersonalFeedbackResponse>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task AskFeedbackDetail_Error()
        {
            long employeeId = 238;
            bool value = true;
            string searchname = "";
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback", Status = true } } } }
            };

            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            var result = await _feedbackController.AskFeedbackDetail(employeeId, value, searchname);
            PayloadCustom<AskPersonalFeedbackResponse> requData = ((PayloadCustom<AskPersonalFeedbackResponse>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task AskFeedbackDetail_InvalidToken()
        {
            long employeeId = 238;
            bool value = true;
            string searchname = "";
            _commonService.Setup(e => e.GetUserIdentity());

            var result = await _feedbackController.AskFeedbackDetail(employeeId, value, searchname) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task AskFeedbackDetail_Forbidden()
        {
            long employeeId = 238;
            bool value = true;
            string searchname = "";
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback" } } } }
            };

            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.AskFeedbackDetail(employeeId, value, searchname) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        }


        [Fact]
        public async Task ProvideFeedbackDetail_InvalidToken()
        {

            bool value = true;
            string searchname = "";
            _commonService.Setup(e => e.GetUserIdentity());

            var result = await _feedbackController.ProvideFeedbackDetail(value, searchname) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }


        [Fact]
        public async Task ProvideFeedbackDetail_IsSuccessful()
        {

            bool value = true;
            string searchname = "";
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback" } } } }
            };

            var providePersonalFeedbackResponse = new List<ProvidePersonalFeedbackResponse>() { new ProvidePersonalFeedbackResponse()
            {
                FirstName = "abc",
                LastName = "abcd"
            }};

            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.GetProvideFeedbackUserDetailAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(providePersonalFeedbackResponse);
            var result = await _feedbackController.ProvideFeedbackDetail(value, searchname);

            PayloadCustom<ProvidePersonalFeedbackResponse> requData = ((PayloadCustom<ProvidePersonalFeedbackResponse>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }


        [Fact]
        public async Task ProvideFeedback_InvalidToken()
        {

            _commonService.Setup(e => e.GetUserIdentity());

            PersonalFeedbackRequest personalFeedbackRequest = new PersonalFeedbackRequest();

            var result = await _feedbackController.ProviedFeedback(personalFeedbackRequest) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task ProvideFeedback_Error()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback", Status = true } } } }
            };

            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            List<CriteriaMaster> criteriaMasters = new List<CriteriaMaster>()
            {
                new CriteriaMaster()
                {

                }
            };



            _feedbackService.Setup(e => e.GetCriteriaMastersWithId(It.IsAny<int>())).ReturnsAsync(criteriaMasters);

            PersonalFeedbackRequest personalFeedbackRequest = new PersonalFeedbackRequest();
            personalFeedbackRequest.SharedRemark = "";
            personalFeedbackRequest.criteriaFeedbackMappingRequests = new List<CriteriaFeedbackMappingRequest>()
            {
                new CriteriaFeedbackMappingRequest()
                {

                }
            };

            FeedbackDetail feedbackDetail = new FeedbackDetail();

            var result = await _feedbackController.ProviedFeedback(personalFeedbackRequest);

            PayloadCustom<FeedbackDetail> requData = ((PayloadCustom<FeedbackDetail>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);
        }


        [Fact]
        public async Task ProvideFeedback_Successfull()
        {
            _commonService.Setup(e => e.GetUserIdentity());

            PersonalFeedbackRequest personalFeedbackRequest = new PersonalFeedbackRequest();

            FeedbackDetail feedbackDetail = new FeedbackDetail();

            _feedbackService.Setup(e => e.ProvideFeedback(It.IsAny<PersonalFeedbackRequest>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(feedbackDetail);
            var result = await _feedbackController.ProviedFeedback(personalFeedbackRequest);
            Assert.NotNull(result);
        }




        [Fact]
        public async Task CriteriaMaster_Successfull()
        {
            int typeId = 1;
            _commonService.Setup(e => e.GetUserIdentity());
            CriteriaMasterResponse criteriaMasterResponse = new CriteriaMasterResponse();

            _feedbackService.Setup(e => e.GetCriteriaDetails(It.IsAny<int>())).ReturnsAsync(criteriaMasterResponse);
            var result = await _feedbackController.GetCriteriaMaster(typeId);
            Assert.NotNull(result);
        }


        [Fact]
        public async Task CriteriaMaster_InvalidToken()
        {
            
            int typeId = 1;
            _commonService.Setup(e => e.GetUserIdentity());
            CriteriaMasterResponse criteriaMasterResponse = new CriteriaMasterResponse();

           
            var result = await _feedbackController.GetCriteriaMaster(typeId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

      

        [Fact]
        public async Task CancelFeedbackRequest_Successfull()
        {
            long feedback = 1;
            _commonService.Setup(e => e.GetUserIdentity());

            _feedbackService.Setup(e => e.CancelFeedbackRequest(It.IsAny<long>(), It.IsAny<UserIdentity>())).ReturnsAsync(true);
            var result = await _feedbackController.CancelFeedbackRequest(feedback);
            Assert.NotNull(result);
        }


        [Fact]
        public async Task CancelFeedbackRequest_InvalidToken()
        {
            long feedback = 1;
            _commonService.Setup(e => e.GetUserIdentity());

          
            var result = await _feedbackController.CancelFeedbackRequest(feedback) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }


        [Fact]
        public async Task CancelFeedbackRequest_Error()
        {
            long feedback = 0;
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback", Status = true } } } }
            };
           
            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.CancelFeedbackRequest(feedback) ;

            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);
           
        }


        [Fact]
        public async Task RequestFeedbackAgain_Successfull()
        {
            long feedback = 1;
            _commonService.Setup(e => e.GetUserIdentity());
            _feedbackService.Setup(e => e.FeedbackRequestAgain(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(true);
            var result = await _feedbackController.RequestFeedbackAgain(feedback);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task RequestFeedbackAgain_InvalidToken()
        {
            long feedback = 1;
            _commonService.Setup(e => e.GetUserIdentity());
          
            var result = await _feedbackController.RequestFeedbackAgain(feedback) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }


        [Fact]
        public async Task RequestFeedbackAgain_Error()
        {
            long feedback = 0;
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback", Status = true } } } }
            };

            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.RequestFeedbackAgain(feedback);

            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);
        }


        [Fact]
        public async Task PersonalFeedback_Successfull()
        {
            bool value = true;
            string name = "";

            var providePersonalFeedbackResponse = new List<ViewPersonalFeedbackResponse>() { new ViewPersonalFeedbackResponse()
            {
                FirstName = "abc",
                LastName = "abcd"
            }};
            _commonService.Setup(e => e.GetUserIdentity());
            _feedbackService.Setup(e => e.ViewPersonalFeedback(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(providePersonalFeedbackResponse);
            var result = await _feedbackController.PersonalFeedback(value, name);

            Assert.NotNull(result);
        }


        [Fact]
        public async Task PersonalFeedback_InvalidToken()
        {
            bool value = true;
            string name = "";

            var providePersonalFeedbackResponse = new List<ViewPersonalFeedbackResponse>() { new ViewPersonalFeedbackResponse()
            {
                FirstName = "abc",
                LastName = "abcd"
            }};
            _commonService.Setup(e => e.GetUserIdentity());
            
            var result = await _feedbackController.PersonalFeedback(value, name) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task PersonalFeedbackResponse_IsSuccessful()
        {
            long feedbackRequestId = 1238;

            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback", Status = true } } } }
            };
            PersonalFeedbackResponse personalFeedbackResponse = new PersonalFeedbackResponse();

            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.PersonalFeedbackResponse(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(personalFeedbackResponse);

            var result = await _feedbackController.PersonalFeedbackResponse(feedbackRequestId);

            PayloadCustom<PersonalFeedbackResponse> requData = ((PayloadCustom<PersonalFeedbackResponse>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task PersonalFeedbackResponse_Error()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback", Status = true } } } }
            };
            long feedbackRequestId = 0;

            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.PersonalFeedbackResponse(feedbackRequestId);
            PayloadCustom<PersonalFeedbackResponse> requData = ((PayloadCustom<PersonalFeedbackResponse>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task PersonalFeedbackResponse_InvalidToken()
        {
            _commonService.Setup(e => e.GetUserIdentity());
            long feedbackRequestId = 1238;
            var result = await _feedbackController.PersonalFeedbackResponse(feedbackRequestId) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task PersonalFeedbackResponse_Forbidden()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback" } } } }
            };
            PersonalFeedbackResponse personalFeedbackResponse = new PersonalFeedbackResponse
            {
                Status = 4
            };
            long feedbackRequestId = 1238;

            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.PersonalFeedbackResponse(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(personalFeedbackResponse);

            var result = await _feedbackController.PersonalFeedbackResponse(feedbackRequestId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task ApproveRejectRequestOneToOne_InvalidToken()
        {
            AcceptRejectRequest acceptRejectRequest = new AcceptRejectRequest();
            _commonService.Setup(e => e.GetUserIdentity());
            var result = await _feedbackController.ApproveRejectRequestOnetoOne(acceptRejectRequest) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task ApproveRejectRequestOneToOne_Error()
        {
            AcceptRejectRequest acceptRejectRequest = new AcceptRejectRequest();
            acceptRejectRequest.OneToOneDetailId = 0;
            acceptRejectRequest.NotificationsDetailId = 1;

            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback", Status = true } } } }
            };


            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            var result = await _feedbackController.ApproveRejectRequestOnetoOne(acceptRejectRequest);

            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);

        }



        [Fact]
        public async Task ApproveRejectRequestOneToOne_NotificationsError()
        {
            AcceptRejectRequest acceptRejectRequest = new AcceptRejectRequest();
            acceptRejectRequest.OneToOneDetailId = 1;
            acceptRejectRequest.NotificationsDetailId = 0;

            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback", Status = true } } } }
            };


            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            var result = await _feedbackController.ApproveRejectRequestOnetoOne(acceptRejectRequest);

            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);

        }


        [Fact]
        public async Task ApproveRejectRequestOneToOne_detailsError()
        {
            AcceptRejectRequest acceptRejectRequest = new AcceptRejectRequest();
            acceptRejectRequest.OneToOneDetailId = 1;
            acceptRejectRequest.NotificationsDetailId = 1;

            OneToOneDetail oneToOneDetail = new OneToOneDetail();
            

            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback", Status = true } } } }
            };


            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.GetRequestOnetoOneByIdAsync(It.IsAny<long>())).ReturnsAsync(oneToOneDetail);
            var result = await _feedbackController.ApproveRejectRequestOnetoOne(acceptRejectRequest);

            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);

        }


        [Fact]
        public async Task ApproveRejectRequestOneToOne_detailsErrorExists()
        {
            AcceptRejectRequest acceptRejectRequest = new AcceptRejectRequest();
            acceptRejectRequest.OneToOneDetailId = 1;
            acceptRejectRequest.NotificationsDetailId = 1;

            OneToOneDetail oneToOneDetail = new OneToOneDetail();
            oneToOneDetail.OneToOneDetailId = 1;
            oneToOneDetail.Status = 0;

            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback", Status = true } } } }
            };


            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.GetRequestOnetoOneByIdAsync(It.IsAny<long>())).ReturnsAsync(oneToOneDetail);
            var result = await _feedbackController.ApproveRejectRequestOnetoOne(acceptRejectRequest);

            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);

        }


        [Fact]
        public async Task ApproveRejectRequestOneToOne_IsSuccessful()
        {
            OneToOneDetail oneToOneDetail = new OneToOneDetail()
            {
                OneToOneDetailId = 1234
        };
            AcceptRejectRequest acceptRejectRequest = new AcceptRejectRequest
            {
                OneToOneDetailId = 1235,
                Status = 2,
                NotificationsDetailId = 3
            };
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback", Status = true } } } }
            };


            _commonService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.GetRequestOnetoOneByIdAsync(It.IsAny<long>())).ReturnsAsync(oneToOneDetail);
            _feedbackService.Setup(e => e.ApproveRejectRequestOnetoOne(It.IsAny<AcceptRejectRequest>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(true);

            var result = await _feedbackController.ApproveRejectRequestOnetoOne(acceptRejectRequest);

            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }




        private void SetUserClaimsAndRequest()
        {
            _feedbackController.ControllerContext = new ControllerContext();

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "108"),
                new Claim(ClaimTypes.NameIdentifier, "108")
            };

            var identity = new ClaimsIdentity(claims, "108");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _feedbackController.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                User = claimsPrincipal
            };
            string sampleAuthToken = Guid.NewGuid().ToString();
           // _feedbackController.ControllerHeader = sampleAuthToken;

        }

    }
}
