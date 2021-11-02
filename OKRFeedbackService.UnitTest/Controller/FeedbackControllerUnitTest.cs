using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;
using OKRFeedbackService.Service.Contracts;
using OKRFeedbackService.EF;
using OKRFeedbackService.ViewModel.Response;
using OKRFeedbackService.ViewModel.Request;
using OKRFeedbackService.WebCore.Controller;

namespace OKRFeedbackService.UnitTest.Controller
{
    public class FeedbackControllerUnitTest
    {
        private readonly Mock<IFeedbackService> _feedbackService;
        private readonly Mock<IOperationStatus> _operationStatus;
        private readonly Mock<ICommonService> _baseService;
        private readonly FeedbackController _feedbackController;


        public FeedbackControllerUnitTest()
        {
            _feedbackService = new Mock<IFeedbackService>();
            _operationStatus = new Mock<IOperationStatus>();
            _baseService = new Mock<ICommonService>();
            _feedbackController = new FeedbackController(_feedbackService.Object, _baseService.Object);
            SetUserClaimsAndRequest();

        }

        [Fact]
        public async Task InsertAskRequest_IsSuccessful()
        {
            AskFeedbackRequest askFeedbackRequest = new AskFeedbackRequest
            {
                RaisedTypeId = 1,
                RaisedForId = new List<long> { 795, 188 },
                RequestRemark = "",
                FeedbackOnTypeId = 1,
                FeedbackOnId = 1,
                FeedbackById = 14254
            };

            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback", Status = true } } } }
            };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.InsertAskRequest(It.IsAny<AskFeedbackRequest>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(_operationStatus.Object);

            var result = await _feedbackController.InsertAskRequest(askFeedbackRequest);

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

            AskFeedbackRequest request = new AskFeedbackRequest
            {
                FeedbackById = 14254,
                RaisedForId = new List<long> { 795, 188 }

            };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.InsertAskRequest(request);
            PayloadCustom<FeedbackRequest> requData = ((PayloadCustom<FeedbackRequest>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task InsertAskRequest_InvalidToken()
        {
            _baseService.Setup(e => e.GetUserIdentity());

            var result = await _feedbackController.InsertAskRequest(null) as StatusCodeResult;

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

            AskFeedbackRequest request = new AskFeedbackRequest
            {
                FeedbackById = 14254
            };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.InsertAskRequest(request) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task ProviedFeedback_InvalidToken()
        {
            _baseService.Setup(e => e.GetUserIdentity());

            var result = await _feedbackController.ProvideFeedback(null) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task ProviedFeedback_Error()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 1,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Give Feedback", Status = true } } } }
            };

            ProvideFeedbackRequest request = new ProvideFeedbackRequest
            {
                RaisedForId = 14254,
                FeedbackRequestId = 1

            };

            FeedbackRequest feedbackRequest = new FeedbackRequest
            {
                RaisedForId = 1
            };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(x => x.GetFeedbackRequest(request.FeedbackRequestId)).ReturnsAsync(feedbackRequest);
            var result = await _feedbackController.ProvideFeedback(request);
            PayloadCustom<FeedbackDetail> requData = ((PayloadCustom<FeedbackDetail>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task ProviedFeedback_Success()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Give Feedback", Status = true } } } }
            };

            ProvideFeedbackRequest request = new ProvideFeedbackRequest()
            {
                FeedbackDetailId = 1,
                FeedbackOnId = 1,
                FeedbackOnTypeId = 1,
                FeedbackRequestId = 1,
                RaisedForId = 1,
                RaisedTypeId = 1,
                SharedRemark = "test",
                Status = 1
            };

            FeedbackRequest feedbackRequest = new FeedbackRequest
            {
                RaisedForId = 14254
            };


            FeedbackDetail response = new FeedbackDetail();

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.ProvideFeedback(It.IsAny<ProvideFeedbackRequest>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(response);
            _feedbackService.Setup(x => x.GetFeedbackRequest(request.FeedbackRequestId)).ReturnsAsync(feedbackRequest);
            var result = await _feedbackController.ProvideFeedback(request);
            PayloadCustom<FeedbackDetail> requData = ((PayloadCustom<FeedbackDetail>)((ObjectResult)result).Value);

            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task ProviedFeedback_Forbidden()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Give Feedback" } } } }
            };

            ProvideFeedbackRequest request = new ProvideFeedbackRequest()
            {
                FeedbackDetailId = 1,
                FeedbackOnId = 1,
                FeedbackOnTypeId = 1,
                FeedbackRequestId = 1,
                RaisedForId = 1,
                RaisedTypeId = 1,
                SharedRemark = "test",
                Status = 1
            };

            FeedbackDetail response = new FeedbackDetail();

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.ProvideFeedback(It.IsAny<ProvideFeedbackRequest>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(response);

            var result = await _feedbackController.ProvideFeedback(request) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task GetFeedback_InvalidToken()
        {
            _baseService.Setup(e => e.GetUserIdentity());

            var result = await _feedbackController.GetFeedback(0, 0) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetFeedback_Error()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "View Feedback", Status = true } } } }
            };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.GetFeedback(0, 0);
            PayloadCustom<OkrFeedbackResponse> requData = ((PayloadCustom<OkrFeedbackResponse>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task GetFeedback_Success()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "View Feedback", Status = true } } } }
            };

            OkrFeedbackResponse okrFeedback = new OkrFeedbackResponse() { KeyDetails = new List<string>() { "xyz" } };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.GetFeedbackResponse(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(okrFeedback);

            var result = await _feedbackController.GetFeedback(1, 2);
            PayloadCustom<OkrFeedbackResponse> requData = ((PayloadCustom<OkrFeedbackResponse>)((ObjectResult)result).Value);

            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetFeedback_Forbidden()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "View Feedback" } } } }
            };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.GetFeedback(0, 0) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task GetFeedback_NotSuccess()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "View Feedback", Status = true } } } }
            };

            OkrFeedbackResponse okrFeedback = null;

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.GetFeedbackResponse(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(okrFeedback);

            var result = await _feedbackController.GetFeedback(1, 2) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task InsertComment_InvalidToken()
        {
            _baseService.Setup(e => e.GetUserIdentity());

            var result = await _feedbackController.InsertComment(null) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task InsertComment_Error()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Comment on Feedback", Status = true } } } }
            };

            CommentRequest comm = new CommentRequest() { Comments = string.Empty, FeedbackDetailId = 0, ParentCommentId = 1 };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.InsertComment(comm);
            PayloadCustom<Comment> requData = ((PayloadCustom<Comment>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task InsertComment_Success()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Comment on Feedback", Status = true } } } }
            };

            CommentRequest comm = new CommentRequest() { Comments = "comment", FeedbackDetailId = 1, ParentCommentId = 1 };
            Comment response = new Comment();
            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.InsertComment(It.IsAny<CommentRequest>(), It.IsAny<UserIdentity>(), It.IsAny<string>())).ReturnsAsync(response);

            var result = await _feedbackController.InsertComment(comm);
            PayloadCustom<Comment> requData = ((PayloadCustom<Comment>)((ObjectResult)result).Value);

            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task InsertComment_Forbidden()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Comment on Feedback" } } } }
            };

            CommentRequest comm = new CommentRequest() { Comments = String.Empty, FeedbackDetailId = 0, ParentCommentId = 1 };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.InsertComment(comm) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task RequestOneOne_InvalidToken()
        {
            _baseService.Setup(e => e.GetUserIdentity());

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

            OneToOneRequest req = new OneToOneRequest() { RequestedTo = new List<long> { 188, 795 } };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

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

            OneToOneRequest req = new OneToOneRequest() { RequestedTo = new List<long> { 188, 795 } };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

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

            OneToOneRequest req = new OneToOneRequest() { RequestType = 1, OnetoOneRemark = "test", RequestedFrom = 1, RequestId = 1 , RequestedTo = new List<long> { 188, 795 } };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.CreateOneToOneRequest(It.IsAny<OneToOneRequest>(), It.IsAny<UserIdentity>(), It.IsAny<string>()));

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

            OneToOneRequest req = new OneToOneRequest() { };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.RequestOneOne(req) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task AskFeedbackDetail_InvalidToken()
        {
            _baseService.Setup(e => e.GetUserIdentity());

            var result = await _feedbackController.AskFeedbackDetail(0) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task AskFeedbackDetail_Error()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback", Status = true } } } }
            };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.AskFeedbackDetail(0);
            PayloadCustom<AskFeedbackResponse> requData = ((PayloadCustom<AskFeedbackResponse>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task AskFeedbackDetail_Success()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback", Status = true } } } }
            };

            List<AskFeedbackResponse> response = new List<AskFeedbackResponse>();

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.GetFeedbackDetail(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(response);

            var result = await _feedbackController.AskFeedbackDetail(1);
            PayloadCustom<AskFeedbackResponse> requData = ((PayloadCustom<AskFeedbackResponse>)((ObjectResult)result).Value);

            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task AskFeedbackDetail_Forbidden()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Ask Feedback" } } } }
            };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.AskFeedbackDetail(0) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task MyGoalFeedbackResponse_InvalidToken()
        {
            _baseService.Setup(e => e.GetUserIdentity());

            var result = await _feedbackController.MyGoalFeedbackResponse(0) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task MyGoalFeedbackResponse_Forbidden()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Give Feedback" } } } }
            };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.MyGoalFeedbackResponse(0) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task MyGoalFeedbackResponse_Error()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Give Feedback", Status = true } } } }
            };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.MyGoalFeedbackResponse(0);
            PayloadCustom<MyGoalDetailFeedbackResponse> requData = ((PayloadCustom<MyGoalDetailFeedbackResponse>)((ObjectResult)result).Value);

            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task MyGoalFeedbackResponse_Success()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Give Feedback", Status = true } } } }
            };

            MyGoalDetailFeedbackResponse response = new MyGoalDetailFeedbackResponse() { KeyDetails = new List<GoalKeyDetails>() { new GoalKeyDetails() { GoalKeyId = 1 } } };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.MyGoalFeedbackResponse(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(response);

            var result = await _feedbackController.MyGoalFeedbackResponse(1);
            PayloadCustom<MyGoalDetailFeedbackResponse> requData = ((PayloadCustom<MyGoalDetailFeedbackResponse>)((ObjectResult)result).Value);

            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task MyGoalFeedbackResponse_NotSuccess()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "Give Feedback", Status = true } } } }
            };

            MyGoalDetailFeedbackResponse response = null;

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.MyGoalFeedbackResponse(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<UserIdentity>())).ReturnsAsync(response);

            var result = await _feedbackController.MyGoalFeedbackResponse(1) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task MostFeedbackReport_InvalidToken()
        {
            _baseService.Setup(e => e.GetUserIdentity());

            var result = await _feedbackController.MostFeedbackReport(1) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task MostFeedbackReport_Forbidden()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "View Feedback" } } } }
            };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);

            var result = await _feedbackController.MostFeedbackReport(1) as StatusCodeResult;

            Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task MostFeedbackReport_Success()
        {
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "View Feedback", Status = true } } } }
            };

            List<MostFeedbackReponse> response = new List<MostFeedbackReponse>() { new MostFeedbackReponse() { EmployeeId = 14256, FirstName = "xxx", LastName = "xxx", OneOnOneRequested = 1, Requested = 2, Shared = 3 } };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.MostFeedbackReponses(It.IsAny<long>(), It.IsAny<string>())).Returns(response);

            var result = await _feedbackController.MostFeedbackReport(1);
            PayloadCustom<MostFeedbackReponse> requData = ((PayloadCustom<MostFeedbackReponse>)((ObjectResult)result).Value);

            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetViewFeedbackResponse_Success()
        {
            long employeeId = 264;
            UserIdentity identity = new UserIdentity
            {
                EmployeeId = 14254,
                FirstName = "Meghdoot",
                RolePermissions = new List<UserRolePermission>() { new UserRolePermission() { Permissions = new List<PermissionDetailModel>() { new PermissionDetailModel() { PermissionName = "View Feedback", Status = true } } } }
            };

            List<ViewFeedbackResponse> response = new List<ViewFeedbackResponse>() { new ViewFeedbackResponse() { RaisedForId = 1 } };

            _baseService.Setup(e => e.GetUserIdentity()).ReturnsAsync(identity);
            _feedbackService.Setup(e => e.GetViewFeedbackResponse(It.IsAny<long>())).ReturnsAsync(response);

            var result = await _feedbackController.GetViewFeedbackResponse(employeeId);
            PayloadCustom<ViewFeedbackResponse> requData = ((PayloadCustom<ViewFeedbackResponse>)((ObjectResult)result).Value);

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
            //_feedbackController.ControllerHeader = sampleAuthToken;

        }
    }
}
