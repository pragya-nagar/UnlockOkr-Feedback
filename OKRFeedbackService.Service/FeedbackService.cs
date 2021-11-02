using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OKRFeedbackService.Common;
using OKRFeedbackService.EF;
using OKRFeedbackService.Service.Contracts;
using OKRFeedbackService.ViewModel.Request;
using OKRFeedbackService.ViewModel.Response;

namespace OKRFeedbackService.Service
{
    [ExcludeFromCodeCoverage]
    public class FeedbackService : BaseService, IFeedbackService
    {
        private readonly IRepositoryAsync<FeedbackRequest> feedbackRequestRepo;
        private readonly IRepositoryAsync<FeedbackDetail> feedbackDetailRepo;
        private readonly IRepositoryAsync<ErrorLog> errorLogRepo;
        private readonly IRepositoryAsync<Comment> commentRepo;
        private readonly IRepositoryAsync<OneToOneDetail> oneToOneDetailRepo;
        private readonly IFeedbackNotificationsService notificationsService;

        public FeedbackService(IServicesAggregator servicesAggregateService, IFeedbackNotificationsService notificationsServices) : base(servicesAggregateService)
        {
            feedbackRequestRepo = UnitOfWorkAsync.RepositoryAsync<FeedbackRequest>();
            feedbackDetailRepo = UnitOfWorkAsync.RepositoryAsync<FeedbackDetail>();
            errorLogRepo = UnitOfWorkAsync.RepositoryAsync<ErrorLog>();
            commentRepo = UnitOfWorkAsync.RepositoryAsync<Comment>();
            oneToOneDetailRepo = UnitOfWorkAsync.RepositoryAsync<OneToOneDetail>();
            notificationsService = notificationsServices;
        }

        public async Task<IOperationStatus> InsertAskRequest(AskFeedbackRequest askFeedbackRequest, UserIdentity loginUser, string jwtToken)
        {
            List<FeedbackRequest> feedbackRequestList = new List<FeedbackRequest>();
            IOperationStatus operationStatus = new OperationStatus();
            foreach (var user in askFeedbackRequest.RaisedForId)
            {
                FeedbackRequest feedbackRequest = new FeedbackRequest
                {
                    RaisedTypeId = askFeedbackRequest.RaisedTypeId,
                    RaisedForId = user,
                    FeedbackById = askFeedbackRequest.FeedbackById,
                    RequestRemark = askFeedbackRequest.RequestRemark,
                    FeedbackOnTypeId = askFeedbackRequest.FeedbackOnTypeId,
                    FeedbackOnId = askFeedbackRequest.FeedbackOnId,
                    CreatedBy = loginUser.EmployeeId
                };
                feedbackRequestRepo.Add(feedbackRequest);
                operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                feedbackRequestList.Add(feedbackRequest);

            }
            await Task.Run(async () =>
            {
                await notificationsService.InsertAskRequestNotificationsAndEmails(feedbackRequestList, askFeedbackRequest, jwtToken).ConfigureAwait(false);
            }).ConfigureAwait(false);
            return operationStatus;
        }

        public void SaveLog(string pageName, string functionName, string errorDetail)
        {
            ErrorLog errorLog = new ErrorLog
            {
                PageName = pageName,
                FunctionName = functionName,
                ErrorDetail = errorDetail
            };
            errorLogRepo.Add(errorLog);
            UnitOfWorkAsync.SaveChanges();
        }

        public async Task<FeedbackDetail> ProvideFeedback(ProvideFeedbackRequest request, UserIdentity loginUser, string jwtToken)
        {
            FeedbackDetail feedbackDetail = new FeedbackDetail();

            ////var userList = _webDataProvider.GetAllUserFromUsers(jwtToken);
            ////var providerName = userList.Results.FirstOrDefault(x => x.EmployeeId == request.RaisedForId);
            ////var requester = userList.Results.FirstOrDefault(x => x.EmployeeId == loginUser.EmployeeId);

            if (request.RaisedTypeId == Constants.TeamRaisedTypeId)
            {
                if (request.FeedbackOnTypeId == Constants.TeamTypeId)
                {
                    if (request.FeedbackDetailId == 0 && request.FeedbackRequestId == 0)
                    {
                        FeedbackRequest feedbackRequest = new FeedbackRequest
                        {
                            RaisedTypeId = request.RaisedTypeId,
                            RaisedForId = request.RaisedForId,
                            FeedbackById = loginUser.EmployeeId,
                            RequestRemark = "",
                            FeedbackOnTypeId = request.FeedbackOnTypeId,
                            FeedbackOnId = request.TeamId,
                            CreatedBy = loginUser.EmployeeId,
                            Status = request.Status,
                            FeedbackRequestType = Constants.DirectFeedback
                        };

                        feedbackRequestRepo.Add(feedbackRequest);
                        await UnitOfWorkAsync.SaveChangesAsync();

                        var userFeedbackRequest = await GetFeedbackRequestById(feedbackRequest.FeedbackRequestId);
                        feedbackDetail.FeedbackRequestId = feedbackRequest.FeedbackRequestId;
                        feedbackDetail.FeedbackOnTypeId = request.FeedbackOnTypeId;
                        feedbackDetail.FeedbackOnId = request.TeamId;
                        feedbackDetail.SharedRemark = request.SharedRemark;
                        feedbackDetail.CreatedBy = loginUser.EmployeeId;
                        await InsertFeedbackDetail(feedbackDetail);

                        await Task.Run(async () =>
                        {
                            await notificationsService.ProvideTeamFeedbackNotificationsAndEmails(request.TeamId, userFeedbackRequest, feedbackDetail, request, loginUser.EmployeeId, jwtToken).ConfigureAwait(false);
                        }).ConfigureAwait(false);
                    }

                    else if (request.FeedbackDetailId == 0 && request.FeedbackRequestId != 0)
                    {
                        await UpdateFeedbackStatus(request.FeedbackRequestId, request.Status, loginUser.EmployeeId);
                        feedbackDetail.FeedbackRequestId = request.FeedbackRequestId;
                        feedbackDetail.FeedbackOnTypeId = request.FeedbackOnTypeId;
                        feedbackDetail.FeedbackOnId = request.FeedbackOnId;
                        feedbackDetail.SharedRemark = request.SharedRemark;
                        feedbackDetail.CreatedBy = loginUser.EmployeeId;
                        await InsertFeedbackDetail(feedbackDetail);
                        var userFeedbackRequest = await GetFeedbackRequestById(request.FeedbackRequestId);

                        await Task.Run(async () =>
                        {
                            await notificationsService.ProvideTeamFeedbackNotificationsAndEmails(request.TeamId, userFeedbackRequest, feedbackDetail, request, loginUser.EmployeeId, jwtToken).ConfigureAwait(false);
                        }).ConfigureAwait(false);
                    }
                }

                else if (request.FeedbackOnTypeId == Constants.ObjectiveTypeId || request.FeedbackOnTypeId == Constants.KeyTypeId)
                {
                    if (request.FeedbackDetailId == 0 && request.FeedbackRequestId == 0)
                    {
                        FeedbackRequest feedbackRequest = new FeedbackRequest
                        {
                            RaisedTypeId = request.RaisedTypeId,
                            RaisedForId = request.RaisedForId,
                            FeedbackById = loginUser.EmployeeId,
                            RequestRemark = "",
                            FeedbackOnTypeId = request.FeedbackOnTypeId,
                            FeedbackOnId = request.FeedbackOnId,
                            CreatedBy = loginUser.EmployeeId,
                            Status = request.Status,
                            FeedbackRequestType = Constants.DirectFeedback
                        };

                        feedbackRequestRepo.Add(feedbackRequest);
                        await UnitOfWorkAsync.SaveChangesAsync();

                        var userFeedbackRequest = await GetFeedbackRequestById(feedbackRequest.FeedbackRequestId);
                        feedbackDetail.FeedbackRequestId = feedbackRequest.FeedbackRequestId;
                        feedbackDetail.FeedbackOnTypeId = request.FeedbackOnTypeId;
                        feedbackDetail.FeedbackOnId = request.FeedbackOnId;
                        feedbackDetail.SharedRemark = request.SharedRemark;
                        feedbackDetail.CreatedBy = loginUser.EmployeeId;
                        await InsertFeedbackDetail(feedbackDetail);

                        await Task.Run(async () =>
                        {
                            await notificationsService.ProvideTeamOkrFeedbackNotificationsAndEmails(request.TeamId, userFeedbackRequest, feedbackDetail, request, loginUser.EmployeeId, jwtToken).ConfigureAwait(false);
                        }).ConfigureAwait(false);
                    }

                    else if (request.FeedbackDetailId == 0 && request.FeedbackRequestId != 0)
                    {
                        await UpdateFeedbackStatus(request.FeedbackRequestId, request.Status, loginUser.EmployeeId);
                        feedbackDetail.FeedbackRequestId = request.FeedbackRequestId;
                        feedbackDetail.FeedbackOnTypeId = request.FeedbackOnTypeId;
                        feedbackDetail.FeedbackOnId = request.FeedbackOnId;
                        feedbackDetail.SharedRemark = request.SharedRemark;
                        feedbackDetail.CreatedBy = loginUser.EmployeeId;
                        await InsertFeedbackDetail(feedbackDetail);
                        var userFeedbackRequest = await GetFeedbackRequestById(request.FeedbackRequestId);

                        await Task.Run(async () =>
                        {
                            await notificationsService.ProvideTeamOkrFeedbackNotificationsAndEmails(request.TeamId, userFeedbackRequest, feedbackDetail, request, loginUser.EmployeeId, jwtToken).ConfigureAwait(false);
                        }).ConfigureAwait(false);
                    }
                }
            }

            else if (request.FeedbackDetailId == 0 && request.FeedbackRequestId == 0)
            {
                FeedbackRequest feedbackRequest = new FeedbackRequest
                {
                    RaisedTypeId = request.RaisedTypeId,
                    RaisedForId = request.RaisedForId,
                    FeedbackById = loginUser.EmployeeId,
                    RequestRemark = "",
                    FeedbackOnTypeId = request.FeedbackOnTypeId,
                    FeedbackOnId = request.FeedbackOnId,
                    CreatedBy = loginUser.EmployeeId,
                    Status = request.Status,
                    FeedbackRequestType = Constants.DirectFeedback
                };

                feedbackRequestRepo.Add(feedbackRequest);
                await UnitOfWorkAsync.SaveChangesAsync();

                var userFeedbackRequest = await GetFeedbackRequestById(feedbackRequest.FeedbackRequestId);
                feedbackDetail.FeedbackRequestId = feedbackRequest.FeedbackRequestId;
                feedbackDetail.FeedbackOnTypeId = request.FeedbackOnTypeId;
                feedbackDetail.FeedbackOnId = request.FeedbackOnId;
                feedbackDetail.SharedRemark = request.SharedRemark;
                feedbackDetail.CreatedBy = loginUser.EmployeeId;
                await InsertFeedbackDetail(feedbackDetail);

                await Task.Run(async () =>
                {
                    await notificationsService.ProvideFeedbackNotificationsAndEmails(userFeedbackRequest, feedbackDetail, request, loginUser.EmployeeId, jwtToken).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }

            else if (request.FeedbackDetailId == 0 && request.FeedbackRequestId != 0)
            {
                await UpdateFeedbackStatus(request.FeedbackRequestId, request.Status, loginUser.EmployeeId);
                feedbackDetail.FeedbackRequestId = request.FeedbackRequestId;
                feedbackDetail.FeedbackOnTypeId = request.FeedbackOnTypeId;
                feedbackDetail.FeedbackOnId = request.FeedbackOnId;
                feedbackDetail.SharedRemark = request.SharedRemark;
                feedbackDetail.CreatedBy = loginUser.EmployeeId;
                await InsertFeedbackDetail(feedbackDetail);
                var userFeedbackRequest = await GetFeedbackRequestById(request.FeedbackRequestId);

                await Task.Run(async () =>
                {
                    await notificationsService.ProvideFeedbackNotificationsAndEmails(userFeedbackRequest, feedbackDetail, request, loginUser.EmployeeId, jwtToken).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }

            return feedbackDetail;
        }

        public async Task UpdateFeedbackStatus(long feedbackRequestId, int statusId, long loginUser)
        {
            var requestDetails = await GetFeedbackRequestById(feedbackRequestId);
            if (requestDetails != null)
            {
                requestDetails.Status = statusId;
                requestDetails.UpdatedBy = loginUser;
                requestDetails.UpdatedOn = DateTime.UtcNow;
                await UpdateFeedbackRequest(requestDetails);
            }
        }

        public async Task<Comment> InsertComment(CommentRequest commentRequest, UserIdentity loginUser, string jwtToken)
        {
            Comment comment = new Comment();
            var userList = GetAllUserFromUsers(jwtToken);
            comment.Comments = commentRequest.Comments;
            comment.FeedbackDetailId = commentRequest.FeedbackDetailId;
            comment.ParentCommentId = commentRequest.ParentCommentId;
            comment.CreatedBy = loginUser.EmployeeId;
            await InsertComment(comment);

            var result = await GetFeedback(comment.FeedbackDetailId);
            var getLastComment = await GetLastCommentDetails(comment.FeedbackDetailId);
            var feedbackRequest = await GetFeedbackRequestById(result.FeedbackRequestId);

            if (getLastComment != null && feedbackRequest.FeedbackOnTypeId != 3)
            {
                await Task.Run(async () =>
                {
                    await notificationsService.InsertCommentNotificationsAndEmails(feedbackRequest, result, getLastComment, comment.CreatedBy, userList, comment, jwtToken, loginUser).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            if (getLastComment != null && feedbackRequest.FeedbackOnTypeId == 3)
            {
                await Task.Run(async () =>
                {
                    await notificationsService.InsertPersonalizeCommentNotificationsAndEmails(feedbackRequest, result, getLastComment, comment.CreatedBy, userList, comment, jwtToken, loginUser).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }

            return comment;
        }

        public async Task<Comment> InsertComment(Comment comment)
        {
            commentRepo.Add(comment);
            await UnitOfWorkAsync.SaveChangesAsync();
            return comment;
        }

        public async Task<OkrFeedbackResponse> GetFeedbackResponse(int feedbackOnTypeId, long feedbackOnId, string jwtToken, UserIdentity loginUser)
        {
            OkrFeedbackResponse okrFeedbackResponse = new OkrFeedbackResponse();
            List<FeedbackResponse> feedbackResponses = new List<FeedbackResponse>();
            List<string> keyDesc = new List<string>();
            var userList = GetAllUserFromUsers(jwtToken);
            if (feedbackOnTypeId == Constants.TeamTypeId)
            {
                var teamDetails = GetTeamEmployeeByTeamId(feedbackOnId, jwtToken);
                if (teamDetails == null)
                {
                    return null;
                }

                okrFeedbackResponse.ObjectiveDescription = "";
                okrFeedbackResponse.ObjectiveName = teamDetails.OrganisationName;
                okrFeedbackResponse.KeyDetails = keyDesc;
                okrFeedbackResponse.FeedbackOnTypeId = feedbackOnTypeId;
                okrFeedbackResponse.FeedbackOnId = feedbackOnId;
                okrFeedbackResponse.TeamId = teamDetails.OrganisationId;
                okrFeedbackResponse.TeamName = teamDetails.OrganisationName;
            }
            else
            {
                var okrResult = await MyGoalFeedBackResponse(feedbackOnTypeId, feedbackOnId, jwtToken); //get key detail
                if (okrResult == null)
                {
                    return null;
                }
                foreach (var key in okrResult.KeyDetails)
                {
                    keyDesc.Add(key.KeyDescription);
                }

                okrFeedbackResponse.ObjectiveDescription = okrResult.ObjectiveDescription;
                okrFeedbackResponse.ObjectiveName = okrResult.ObjectiveName;
                okrFeedbackResponse.KeyDetails = keyDesc;
                okrFeedbackResponse.FeedbackOnTypeId = feedbackOnTypeId;
                okrFeedbackResponse.FeedbackOnId = feedbackOnId;
                okrFeedbackResponse.TeamId = okrResult.TeamId;
                okrFeedbackResponse.TeamName = okrResult.TeamName;
            }

            var feedbackDetail = await GetFeedbackDetail(feedbackOnTypeId, feedbackOnId);
            foreach (var data in feedbackDetail)
            {
                var feedbackRequest = await GetFeedbackRequestById(data.FeedbackRequestId);
                var list = userList.Results.FirstOrDefault(x => x.EmployeeId == data.CreatedBy);
                var userDetails = await GetUser(jwtToken, feedbackRequest.FeedbackById);

                if (loginUser.EmployeeId == data.CreatedBy)
                {
                    FeedbackResponse feedbackResponse = new FeedbackResponse
                    {
                        FeedbackDetailId = data.FeedbackDetailId,
                        FeedbackDetail = data.SharedRemark,
                        CreatedOn = data.CreatedOn,
                        ImagePath = list == null ? "" : list.ImagePath,
                        FirstName = list == null ? "" : list.FirstName,
                        LastName = list == null ? "" : list.LastName,
                        EmployeeId = list == null ? 0 : list.EmployeeId,
                        AskByRequestRemark = feedbackRequest.FeedbackRequestType == 2 ? "" : feedbackRequest.RequestRemark,
                        AskByFirstName = feedbackRequest.FeedbackRequestType == 2 ? "" : userDetails.FirstName ?? "",
                        AskByCreatedOn = feedbackRequest.FeedbackRequestType == 2 ? (DateTime?)null : feedbackRequest.CreatedOn,
                        AskByImagePath = feedbackRequest.FeedbackRequestType == 2 ? "" : userDetails.ImagePath ?? "",
                        AskByLastName = feedbackRequest.FeedbackRequestType == 2 ? "" : userDetails.LastName ?? ""
                    };
                    List<Comment> comment = await GetCommentByFeedBackId(data.FeedbackDetailId);

                    List<FeedbackComment> feedbackComments = FeedbackResponeLoopSepration(comment, userList);
                    feedbackResponse.FeedbackComments = feedbackComments;
                    feedbackResponses.Add(feedbackResponse);


                }

                if ((loginUser.EmployeeId == feedbackRequest.FeedbackById && feedbackRequest.FeedbackRequestType == 1) || (loginUser.EmployeeId != feedbackRequest.FeedbackById && feedbackRequest.FeedbackRequestType == 2 && feedbackRequest.FeedbackOnTypeId == Constants.TeamTypeId))
                {
                    FeedbackResponse feedbackResponse = new FeedbackResponse
                    {
                        FeedbackDetailId = data.FeedbackDetailId,
                        FeedbackDetail = data.SharedRemark,
                        CreatedOn = data.CreatedOn,
                        ImagePath = list == null ? "" : list.ImagePath,
                        FirstName = list == null ? "" : list.FirstName,
                        LastName = list == null ? "" : list.LastName,
                        EmployeeId = list == null ? 0 : list.EmployeeId,
                        AskByRequestRemark = feedbackRequest.FeedbackRequestType == 2 ? "" : feedbackRequest.RequestRemark,
                        AskByFirstName = feedbackRequest.FeedbackRequestType == 2 ? "" : userDetails.FirstName ?? "",
                        AskByCreatedOn = feedbackRequest.FeedbackRequestType == 2 ? (DateTime?)null : feedbackRequest.CreatedOn,
                        AskByImagePath = feedbackRequest.FeedbackRequestType == 2 ? "" : userDetails.ImagePath ?? "",
                        AskByLastName = feedbackRequest.FeedbackRequestType == 2 ? "" : userDetails.LastName ?? ""
                    };
                    List<Comment> comment = await GetCommentByFeedBackId(data.FeedbackDetailId);

                    List<FeedbackComment> feedbackComments = FeedbackResponeLoopSepration(comment, userList);
                    feedbackResponse.FeedbackComments = feedbackComments;
                    feedbackResponses.Add(feedbackResponse);
                }

                if (loginUser.EmployeeId == feedbackRequest.RaisedForId && feedbackRequest.FeedbackRequestType == 2 && feedbackRequest.FeedbackOnTypeId != Constants.TeamTypeId)
                {
                    FeedbackResponse feedbackResponse = new FeedbackResponse
                    {
                        FeedbackDetailId = data.FeedbackDetailId,
                        FeedbackDetail = data.SharedRemark,
                        CreatedOn = data.CreatedOn,
                        ImagePath = list == null ? "" : list.ImagePath,
                        FirstName = list == null ? "" : list.FirstName,
                        LastName = list == null ? "" : list.LastName,
                        EmployeeId = list == null ? 0 : list.EmployeeId,
                        AskByRequestRemark = feedbackRequest.FeedbackRequestType == 2 ? "" : feedbackRequest.RequestRemark,
                        AskByFirstName = feedbackRequest.FeedbackRequestType == 2 ? "" : userDetails.FirstName ?? "",
                        AskByCreatedOn = feedbackRequest.FeedbackRequestType == 2 ? (DateTime?)null : feedbackRequest.CreatedOn,
                        AskByImagePath = feedbackRequest.FeedbackRequestType == 2 ? "" : userDetails.ImagePath ?? "",
                        AskByLastName = feedbackRequest.FeedbackRequestType == 2 ? "" : userDetails.LastName ?? ""
                    };
                    List<Comment> comment = await GetCommentByFeedBackId(data.FeedbackDetailId);

                    List<FeedbackComment> feedbackComments = FeedbackResponeLoopSepration(comment, userList);
                    feedbackResponse.FeedbackComments = feedbackComments;
                    feedbackResponses.Add(feedbackResponse);
                }

            }

            var allNotifications = await GetAllNotifications(jwtToken);
            var updateNotifications = allNotifications.Where(x => x.IsDeleted == false && x.NotificationOnTypeId == feedbackOnTypeId && x.NotificationOnId == feedbackOnId && x.NotificationsBy != loginUser.EmployeeId).ToList();
            foreach (var item in updateNotifications)
            {
                await ReadNotificationsForFeedback(item.NotificationsDetailsId, jwtToken);
            }

            okrFeedbackResponse.FeedbackResponses = feedbackResponses;
            return okrFeedbackResponse;
        }

        public async Task CreateOneToOneRequest(OneToOneRequest request, UserIdentity currentUser, string jwtToken)
        {
            var userList = GetAllUserFromUsers(jwtToken);
            var requestFrom = userList.Results.FirstOrDefault(x => x.EmployeeId == request.RequestedFrom);
            ////var employee = requestFrom.FirstName + " " + requestFrom.LastName;
            foreach (var item in request.RequestedTo)
            {
                var requestTo = userList.Results.FirstOrDefault(x => x.EmployeeId == item);
                var emailId = requestTo.EmailId;
                OneToOneDetail detailDb = new OneToOneDetail()
                {
                    CreatedBy = currentUser.EmployeeId,
                    CreatedOn = DateTime.UtcNow,
                    RequestId = request.RequestId,
                    RequestType = request.RequestType,
                    IsActive = true,
                    OneToOneRemark = request.OnetoOneRemark,
                    RequestedFrom = request.RequestedFrom,
                    RequestedTo = item,
                    UpdatedBy = currentUser.EmployeeId,
                    UpdatedOn = DateTime.UtcNow
                };

                oneToOneDetailRepo.Add(detailDb);
                await UnitOfWorkAsync.SaveChangesAsync();

                if (request.RequestType == 1 || request.RequestType == 2)
                {
                    await Task.Run(async () =>
                    {
                        await notificationsService.CreateOneToOneRequestNotificationsAndEmailsWhenRequestedForOkr(detailDb.OneToOneDetailId, request, item, requestTo, requestFrom, emailId, currentUser.EmployeeId, jwtToken);
                    }).ConfigureAwait(false);
                }
                else
                {
                    var feedbackDetailId = await GetFeedback(request.RequestId);
                    var feedbackRequest = await GetFeedbackRequestById(feedbackDetailId.FeedbackRequestId);
                    if (feedbackRequest != null)
                    {
                        await Task.Run(async () =>
                        {
                            await notificationsService.CreateOneToOneRequestNotificationsAndEmailsWhenRequestedForFeedback(detailDb.OneToOneDetailId, request, requestTo, requestFrom, emailId, feedbackRequest, jwtToken).ConfigureAwait(false);
                        }).ConfigureAwait(false);
                    }
                }
            }


        }

        public async Task<OneToOneDetail> CreateOneToOneRequest(OneToOneDetail oneToOneDetail)
        {
            oneToOneDetailRepo.Add(oneToOneDetail);
            await UnitOfWorkAsync.SaveChangesAsync();
            return oneToOneDetail;
        }

        public async Task<List<AskFeedbackResponse>> GetFeedbackDetail(long employeeId, string jwtToken)
        {
            List<AskFeedbackResponse> askFeedbackResponses = new List<AskFeedbackResponse>();
            var result = await GetFeedbackDetail(employeeId);
            foreach (var item in result)
            {
                AskFeedbackResponse askFeedbackResponse = new AskFeedbackResponse
                {
                    FeedbackOnId = item.FeedbackOnId,
                    FeedbackOnTypeId = item.FeedbackOnTypeId,
                    FeedbackRequestId = item.FeedbackRequestId,
                    Status = item.Status,
                    RequestRemark = item.RequestRemark,
                };
                askFeedbackResponses.Add(askFeedbackResponse);
            }
            return askFeedbackResponses;
        }

        public async Task<List<FeedbackDetail>> GetFeedbackDetail(int feedbackOnTypeId, long feedbackOnId)
        {
            return await feedbackDetailRepo.GetQueryable().Where(x => x.FeedbackOnTypeId == feedbackOnTypeId && x.FeedbackOnId == feedbackOnId && x.IsActive).OrderByDescending(x => x.FeedbackDetailId).ToListAsync();
        }

        public async Task<List<FeedbackRequest>> GetFeedbackDetail(long employeeId)
        {
            return await feedbackRequestRepo.GetQueryable().Where(x => x.RaisedForId == employeeId && x.IsActive).ToListAsync();
        }
        public async Task<MyGoalDetailFeedbackResponse> MyGoalFeedbackResponse(long feedbackRequestId, string jwtToken, UserIdentity userIdentity)
        {
            var myGoalFeedbackResponse = new MyGoalDetailFeedbackResponse();
            var feedbackRequest = await GetFeedbackRequestByIdAsync(feedbackRequestId);
            if (feedbackRequest != null)
            {
                List<FeedbackProvideDetails> feedbackProvideDetails = new List<FeedbackProvideDetails>();
                List<GoalKeyDetails> keyDesc = new List<GoalKeyDetails>();

                if (feedbackRequest.FeedbackOnTypeId == Constants.TeamTypeId)
                {
                    var teamDetails = GetTeamEmployeeByTeamId(feedbackRequest.FeedbackOnId, jwtToken);
                    if (teamDetails == null)
                        return null;
                    var userDetails = await GetUser(jwtToken, feedbackRequest.FeedbackById);

                    var feedbackDetails = await GetFeedbackDetailsById(feedbackRequestId, userIdentity.EmployeeId, feedbackRequest.FeedbackOnTypeId, feedbackRequest.FeedbackOnId);
                    var sharedUserDetails = await GetUser(jwtToken, userIdentity.EmployeeId);
                    if (feedbackDetails.Count > 0)
                    {
                        foreach (var item in feedbackDetails)
                        {
                            feedbackProvideDetails.Add(new FeedbackProvideDetails
                            {
                                FeedbackDetailId = item.FeedbackDetailId,
                                SharedRemark = item.SharedRemark,
                                SharedByCreatedOn = item.CreatedOn,
                                SharedByFirstname = sharedUserDetails.FirstName ?? "",
                                SharedByLastname = sharedUserDetails.LastName ?? "",
                                SharedByImagePath = sharedUserDetails.ImagePath ?? ""
                            });

                        }
                    }

                    myGoalFeedbackResponse.ObjectiveDescription = "";
                    myGoalFeedbackResponse.ObjectiveName = teamDetails.OrganisationName;
                    myGoalFeedbackResponse.KeyDetails = keyDesc;
                    myGoalFeedbackResponse.FeedbackOnTypeId = feedbackRequest.FeedbackOnTypeId;
                    myGoalFeedbackResponse.FeedbackOnId = feedbackRequest.FeedbackOnId;
                    myGoalFeedbackResponse.AskByRequestRemark = feedbackRequest.RequestRemark;
                    myGoalFeedbackResponse.FeedbackById = feedbackRequest.FeedbackById;
                    myGoalFeedbackResponse.AskByFirstName = userDetails.FirstName ?? "";
                    myGoalFeedbackResponse.AskByLastName = userDetails.LastName ?? "";
                    myGoalFeedbackResponse.AskByImagePath = userDetails.ImagePath ?? "";
                    myGoalFeedbackResponse.AskByCreatedOn = feedbackRequest.CreatedOn;
                    myGoalFeedbackResponse.FeedbackProvideDetails = feedbackProvideDetails;
                    myGoalFeedbackResponse.TeamId = teamDetails.OrganisationId;
                    myGoalFeedbackResponse.TeamName = teamDetails.OrganisationName;

                }

                else
                {
                    var okrResult = MyGoalFeedBackResponse(feedbackRequest.FeedbackOnTypeId, feedbackRequest.FeedbackOnId, jwtToken);
                    if (okrResult == null)
                        return null;
                    var userDetails = await GetUser(jwtToken, feedbackRequest.FeedbackById);

                    foreach (var key in okrResult.Result.KeyDetails)
                    {
                        keyDesc.Add(new GoalKeyDetails { GoalKeyId = key.GoalKeyId, KeyDescription = key.KeyDescription });
                    }

                    var feedbackDetails = await GetFeedbackDetailsById(feedbackRequestId, userIdentity.EmployeeId, feedbackRequest.FeedbackOnTypeId, feedbackRequest.FeedbackOnId);
                    var sharedUserDetails = await GetUser(jwtToken, userIdentity.EmployeeId);
                    if (feedbackDetails.Count > 0)
                    {
                        foreach (var item in feedbackDetails)
                        {
                            feedbackProvideDetails.Add(new FeedbackProvideDetails
                            {
                                FeedbackDetailId = item.FeedbackDetailId,
                                SharedRemark = item.SharedRemark,
                                SharedByCreatedOn = item.CreatedOn,
                                SharedByFirstname = sharedUserDetails.FirstName ?? "",
                                SharedByLastname = sharedUserDetails.LastName ?? "",
                                SharedByImagePath = sharedUserDetails.ImagePath ?? ""
                            });
                        }
                    }

                    myGoalFeedbackResponse.ObjectiveDescription = okrResult.Result.ObjectiveDescription;
                    myGoalFeedbackResponse.ObjectiveName = okrResult.Result.ObjectiveName;
                    myGoalFeedbackResponse.KeyDetails = keyDesc;
                    myGoalFeedbackResponse.FeedbackOnTypeId = feedbackRequest.FeedbackOnTypeId;
                    myGoalFeedbackResponse.FeedbackOnId = feedbackRequest.FeedbackOnId;
                    myGoalFeedbackResponse.AskByRequestRemark = feedbackRequest.RequestRemark;
                    myGoalFeedbackResponse.FeedbackById = feedbackRequest.FeedbackById;
                    myGoalFeedbackResponse.AskByFirstName = userDetails.FirstName ?? "";
                    myGoalFeedbackResponse.AskByLastName = userDetails.LastName ?? "";
                    myGoalFeedbackResponse.AskByImagePath = userDetails.ImagePath ?? "";
                    myGoalFeedbackResponse.AskByCreatedOn = feedbackRequest.CreatedOn;
                    myGoalFeedbackResponse.FeedbackProvideDetails = feedbackProvideDetails;
                    myGoalFeedbackResponse.TeamId = okrResult.Result.TeamId;
                    myGoalFeedbackResponse.TeamName = okrResult.Result.TeamName;
                }

            }
            return myGoalFeedbackResponse;
        }


        public List<MostFeedbackReponse> MostFeedbackReponses(long empId, string token)
        {
            return MostFeedbackReponse(empId, token);
        }

        private List<FeedbackComment> FeedbackResponeLoopSepration(List<Comment> comment, EmployeeResult userList)
        {
            List<FeedbackComment> feedbackComments = new List<FeedbackComment>();

            foreach (var item in comment)
            {
                var commentname = userList.Results.FirstOrDefault(x => x.EmployeeId == item.CreatedBy);
                FeedbackComment feedbackComment = new FeedbackComment
                {
                    CommentId = item.CommentId,
                    Comment = item.Comments,
                    CreatedOn = item.CreatedOn,
                    ImagePath = commentname == null ? "" : commentname.ImagePath,
                    FirstName = commentname == null ? "" : commentname.FirstName,
                    LastName = commentname == null ? "" : commentname.LastName,
                    EmployeeId = commentname == null ? 0 : commentname.EmployeeId
                };
                feedbackComments.Add(feedbackComment);
            }
            return feedbackComments;
        }

        public async Task<FeedbackRequest> GetFeedbackRequest(long feedbackRequestId)
        {
            return await GetFeedbackRequestById(feedbackRequestId);
        }

        public async Task<List<ViewFeedbackResponse>> GetViewFeedbackResponse(long empId)
        {
            var viewFeedbackResponse = new List<ViewFeedbackResponse>();
            var feedback = await GetViewFeedbackForMyGoals(empId);
            if (feedback.Count > 0)
            {
                foreach (var item in feedback)
                {
                    ViewFeedbackResponse viewFeedback = new ViewFeedbackResponse
                    {
                        RaisedForId = item.RaisedForId,
                        FeedbackById = item.FeedbackById,
                        FeedbackOnId = item.FeedbackOnId,
                        FeedbackOnTypeId = item.FeedbackOnTypeId
                    };
                    viewFeedbackResponse.Add(viewFeedback);
                }
            }

            var directFeedback = await GetViewFeedbackForMyGoalsForDirectFeedback(empId);
            if (directFeedback.Count > 0)
            {
                foreach (var item in directFeedback)
                {
                    ViewFeedbackResponse viewFeedback = new ViewFeedbackResponse
                    {
                        RaisedForId = item.RaisedForId,
                        FeedbackById = item.FeedbackById,
                        FeedbackOnId = item.FeedbackOnId,
                        FeedbackOnTypeId = item.FeedbackOnTypeId
                    };
                    viewFeedbackResponse.Add(viewFeedback);
                }
            }
            return viewFeedbackResponse;
        }

        public async Task<FeedbackRequest> UpdateFeedbackRequest(FeedbackRequest feedbackRequest)
        {
            feedbackRequestRepo.Update(feedbackRequest);
            await UnitOfWorkAsync.SaveChangesAsync();
            return feedbackRequest;
        }

        public async Task<FeedbackRequest> GetFeedbackRequestById(long feedbackRequestId)
        {
            return await feedbackRequestRepo.GetQueryable().FirstOrDefaultAsync(x => x.FeedbackRequestId == feedbackRequestId);
        }

        public async Task<FeedbackDetail> InsertFeedbackDetail(FeedbackDetail feedbackDetail)
        {
            feedbackDetailRepo.Add(feedbackDetail);
            await UnitOfWorkAsync.SaveChangesAsync();
            return feedbackDetail;
        }



        public async Task<List<Comment>> GetCommentByFeedBackId(long feedbackDetailId)
        {
            return await commentRepo.GetQueryable().Where(x => x.FeedbackDetailId == feedbackDetailId).ToListAsync();
        }

        public async Task<FeedbackDetail> GetFeedback(long feedbackDetailId)
        {
            return await feedbackDetailRepo.GetQueryable().FirstOrDefaultAsync(x => x.FeedbackDetailId == feedbackDetailId && x.IsActive);
        }
        public async Task<string> GetLastCommentDetails(long feedbackDetailId)
        {
            var comment = await commentRepo.GetQueryable().OrderByDescending(x => x.CommentId).FirstOrDefaultAsync(x => x.FeedbackDetailId == feedbackDetailId && x.IsActive);
            return comment.Comments;
        }

        public async Task<FeedbackRequest> GetFeedbackRequestByIdAsync(long feedbackRequestId)
        {
            return await feedbackRequestRepo.GetQueryable().FirstOrDefaultAsync(x => x.FeedbackRequestId == feedbackRequestId);
        }

        public async Task<int> GetCommentsCount(long feedbackDetailId)
        {
            return await commentRepo.GetQueryable().CountAsync(x => x.FeedbackDetailId == feedbackDetailId && x.IsActive);
        }

        public async Task<long> GetReceiver(long feedbackDetailsId)
        {
            var receiver = await commentRepo.GetQueryable().LastAsync(x => x.FeedbackDetailId == feedbackDetailsId && x.IsActive);
            return receiver.CreatedBy;
        }

        public List<MostFeedbackReponse> MostFeedbackReponse(long empId, string token)
        {
            List<MostFeedbackReponse> result = new List<MostFeedbackReponse>();
            List<Data> datas = new List<Data>();
            var allEmployee = GetAllUserFromUsers(token).Results;
            using (var command = FeedbackDBContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "EXEC sp_GetAllFeedbackCount " + empId;
                command.CommandType = CommandType.Text;
                FeedbackDBContext.Database.OpenConnection();
                var dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    Data item2 = new Data
                    {
                        EmployeeId = Convert.ToInt64(dataReader["EmployeeId"].ToString()),
                        FeedbackType = Convert.ToString(dataReader["FeedbackType"].ToString()),
                        CountFeedback = Convert.ToInt32(dataReader["CountFeedbackType"].ToString())
                    };
                    datas.Add(item2);
                }

                FeedbackDBContext.Database.CloseConnection();

                var distinctEmployee = datas.Select(e => new { e.EmployeeId })
                                       .Distinct();
                foreach (var item3 in distinctEmployee)
                {
                    MostFeedbackReponse item = new MostFeedbackReponse();
                    var requestonetoone = datas.FirstOrDefault(x => x.EmployeeId == item3.EmployeeId && x.FeedbackType == Constants.OneOnOneRequested)?.CountFeedback == null ? 0 : datas.FirstOrDefault(x => x.EmployeeId == item3.EmployeeId && x.FeedbackType == Constants.OneOnOneRequested).CountFeedback;
                    var requested = datas.FirstOrDefault(x => x.EmployeeId == item3.EmployeeId && x.FeedbackType == Constants.Requested)?.CountFeedback == null ? 0 : datas.FirstOrDefault(x => x.EmployeeId == item3.EmployeeId && x.FeedbackType == Constants.Requested).CountFeedback;
                    var shared = datas.FirstOrDefault(x => x.EmployeeId == item3.EmployeeId && x.FeedbackType == Constants.Shared)?.CountFeedback == null ? 0 : datas.FirstOrDefault(x => x.EmployeeId == item3.EmployeeId && x.FeedbackType == Constants.Shared).CountFeedback;
                    item.EmployeeId = item3.EmployeeId;
                    item.FirstName = allEmployee.FirstOrDefault(x => x.EmployeeId == item3.EmployeeId)?.FirstName;
                    item.LastName = allEmployee.FirstOrDefault(x => x.EmployeeId == item3.EmployeeId)?.LastName;
                    item.OneOnOneRequested = requestonetoone;
                    item.Requested = requested;
                    item.Shared = shared;
                    result.Add(item);
                }
            }

            return result;
        }

        public async Task<List<FeedbackRequest>> GetViewFeedbackForMyGoals(long empId)
        {
            return await feedbackRequestRepo.GetQueryable().Where(x => x.FeedbackById == empId && x.IsActive && x.Status == 3).ToListAsync();
        }

        public async Task<List<FeedbackRequest>> GetViewFeedbackForMyGoalsForDirectFeedback(long empId)
        {
            return await feedbackRequestRepo.GetQueryable().Where(x => x.RaisedForId == empId && x.IsActive && x.Status == 3 && x.FeedbackRequestType == 2).ToListAsync();
        }


        public async Task<List<FeedbackDetail>> GetFeedbackDetailsById(long feedbackRequestId, long loginuser, int feedbackonTypeId, long feedbackOnId)
        {
            return await feedbackDetailRepo.GetQueryable().Where(x => x.FeedbackRequestId == feedbackRequestId && x.IsActive && x.CreatedBy == loginuser && x.FeedbackOnTypeId == feedbackonTypeId && x.FeedbackOnId == feedbackOnId).OrderByDescending(x => x.FeedbackDetailId).ToListAsync();
        }

        public async Task<bool> GetReadFeedbackResponse(long okrId, string jwtToken, UserIdentity userIdentity)
        {
            var notifications = await GetAllNotifications(jwtToken);
            var unreadNotifications = notifications.Where(x => x.IsRead == false && x.NotificationOnId == okrId && x.IsDeleted == false && x.NotificationsBy != userIdentity.EmployeeId).ToList();
            if (unreadNotifications.Any())
            {
                return true;
            }
            else
            {
                return false;

            }

        }

    }
}
