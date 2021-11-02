using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OKRFeedbackService.Common;
using OKRFeedbackService.EF;
using OKRFeedbackService.Service.Contracts;
using OKRFeedbackService.ViewModel.Request;
using OKRFeedbackService.ViewModel.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OKRFeedbackService.Service
{
    [ExcludeFromCodeCoverage]
    public class PersonalizeFeedbackService : BaseService, IPersonalizeFeedbackService
    {
        private readonly IRepositoryAsync<FeedbackRequest> feedbackRequestRepo;
        private readonly IRepositoryAsync<FeedbackDetail> feedbackDetailRepo;
        private readonly IRepositoryAsync<ErrorLog> errorLogRepo;
        private readonly IRepositoryAsync<Comment> commentRepo;
        private readonly IRepositoryAsync<OneToOneDetail> oneToOneDetailRepo;
        private readonly IFeedbackNotificationsService notificationsService;
        private readonly IRepositoryAsync<CriteriaFeedbackMapping> criteriaFeedbackMappingRepo;
        private readonly IRepositoryAsync<CriteriaMaster> criteriaMasterRepo;
        private readonly IRepositoryAsync<CriteriaTypeMaster> criteriaTypeRepo;
        public PersonalizeFeedbackService(IServicesAggregator servicesAggregateService, IFeedbackNotificationsService notificationsServices) : base(servicesAggregateService)
        {
            feedbackRequestRepo = UnitOfWorkAsync.RepositoryAsync<FeedbackRequest>();
            feedbackDetailRepo = UnitOfWorkAsync.RepositoryAsync<FeedbackDetail>();
            errorLogRepo = UnitOfWorkAsync.RepositoryAsync<ErrorLog>();
            commentRepo = UnitOfWorkAsync.RepositoryAsync<Comment>();
            oneToOneDetailRepo = UnitOfWorkAsync.RepositoryAsync<OneToOneDetail>();
            criteriaFeedbackMappingRepo = UnitOfWorkAsync.RepositoryAsync<CriteriaFeedbackMapping>();
            criteriaMasterRepo = UnitOfWorkAsync.RepositoryAsync<CriteriaMaster>();
            criteriaTypeRepo = UnitOfWorkAsync.RepositoryAsync<CriteriaTypeMaster>();
            notificationsService = notificationsServices;

        }

        public async Task<IOperationStatus> AddPersonalFeedbackAsync(AskPersonalFeedbackRequest askFeedbackRequest, UserIdentity loginUser, string jwtToken)
        {
            List<FeedbackRequest> feedbackRequestPersonal = new List<FeedbackRequest>();
            foreach (var user in askFeedbackRequest.RaisedForId)
            {
                FeedbackRequest feedbackRequest = new FeedbackRequest();
                feedbackRequest.RaisedTypeId = askFeedbackRequest.RaisedTypeId;
                feedbackRequest.RaisedForId = user;
                feedbackRequest.FeedbackById = askFeedbackRequest.FeedbackById;
                feedbackRequest.RequestRemark = askFeedbackRequest.RequestRemark;
                feedbackRequest.FeedbackOnTypeId = askFeedbackRequest.FeedbackOnTypeId;
                feedbackRequest.FeedbackOnId = user;
                feedbackRequest.CreatedBy = loginUser.EmployeeId;
                feedbackRequestRepo.Add(feedbackRequest);
                await UnitOfWorkAsync.SaveChangesAsync();
                feedbackRequestPersonal.Add(feedbackRequest);
            }

            await Task.Run(async () =>
            {
                await notificationsService.InsertAskPersonalNotificationsAndEmails(feedbackRequestPersonal, askFeedbackRequest, jwtToken).ConfigureAwait(false);
            }).ConfigureAwait(false);

            return OperationStatus;
        }

        public async Task<List<AskPersonalFeedbackResponse>> GetAskedFeedbackUserDetailAsync(long employeeId, bool sortOrder, string jwtToken, string searchName)
        {
            List<AskPersonalFeedbackResponse> askFeedbackResponses = new List<AskPersonalFeedbackResponse>();
            var result = await GetFeedbackDetailRequest(employeeId);

            foreach (var item in result)
            {
                var userDetails = await GetUser(jwtToken, item.RaisedForId);
                if (string.IsNullOrEmpty(searchName))
                {
                    AskPersonalFeedbackResponse askPersonalFeedbackResponse = new AskPersonalFeedbackResponse
                    {
                        FeedbackRequestId = item.FeedbackRequestId,
                        EmployeeId = item.RaisedForId,
                        FirstName = userDetails.FirstName,
                        LastName = userDetails.LastName,
                        Designation = userDetails.Designation,
                        Status = item.Status,
                        ImagePath = userDetails.ImagePath,
                        Date = item.CreatedOn.ToString(),
                        RequestRemark = item.RequestRemark

                    };
                    askFeedbackResponses.Add(askPersonalFeedbackResponse);
                }
                else
                {
                    if (userDetails.FirstName.ToLower().Contains(searchName.ToLower()) || (userDetails.FirstName.ToLower() + " " + userDetails.LastName.ToLower()).Contains(searchName.ToLower()))
                    {
                        AskPersonalFeedbackResponse askPersonalFeedbackResponse = new AskPersonalFeedbackResponse
                        {
                            FeedbackRequestId = item.FeedbackRequestId,
                            EmployeeId = item.RaisedForId,
                            FirstName = userDetails.FirstName,
                            LastName = userDetails.LastName,
                            Designation = userDetails.Designation,
                            Status = item.Status,
                            ImagePath = userDetails.ImagePath,
                            Date = item.CreatedOn.ToString(),
                            RequestRemark = item.RequestRemark
                        };
                        askFeedbackResponses.Add(askPersonalFeedbackResponse);
                    }
                }

            }
            if (sortOrder)
            {
                return askFeedbackResponses.OrderByDescending(x => Convert.ToDateTime(x.Date).Date).ThenByDescending(x => Convert.ToDateTime(x.Date).TimeOfDay).ToList();
            }
            return askFeedbackResponses.OrderBy(x => Convert.ToDateTime(x.Date).Date).ThenBy(x => Convert.ToDateTime(x.Date).TimeOfDay).ToList();
        }


        public async Task<List<ProvidePersonalFeedbackResponse>> GetProvideFeedbackUserDetailAsync(long employeeId, bool sortOrder, string jwtToken, string searchName)
        {
            List<ProvidePersonalFeedbackResponse> providePersonalFeedbackResponses = new List<ProvidePersonalFeedbackResponse>();
            var userAllDetails = GetAllUserFromUsers(jwtToken);
            var result = await GetFeedbackDetailsAsync(employeeId);
            foreach (var item in result)
            {
                var userDetails = await GetUser(jwtToken, item.FeedbackOnId);
                var feedbackRequest = await GetFeedbackRequestById(item.FeedbackRequestId);
                if (feedbackRequest != null)
                {
                    var requestorDetails = await GetUser(jwtToken, feedbackRequest.FeedbackById);
                    var criteriaFeedbackMappings = await CriteriaFeedbackMappings(item.FeedbackDetailId);
                    if (string.IsNullOrEmpty(searchName))
                    {
                        ProvidePersonalFeedbackResponse providePersonalFeedback = new ProvidePersonalFeedbackResponse
                        {
                            FeedbackDetailId = item.FeedbackDetailId,
                            CriteriaTypeId = (int)item.CriteriaTypeId,
                            CriteriaName = await GetCriteriaTypeMasters((int)item.CriteriaTypeId),
                            EmployeeId = userDetails.EmployeeId,
                            FirstName = userDetails.FirstName,
                            LastName = userDetails.LastName,
                            SharedRemark = item.SharedRemark,
                            ImagePath = userDetails.ImagePath,
                            AskByFirstName = requestorDetails.FirstName,
                            AskByLastName = requestorDetails.LastName,
                            AskByCreatedOn = feedbackRequest.CreatedOn,
                            AskByRequestRemark = feedbackRequest.RequestRemark,
                            AskByImagePath = requestorDetails.ImagePath,
                            Date = item.CreatedOn
                        };
                        List<CriteriaFeedbackMappingResponse> criteriaData = criteriaFeedbackMappings.Select(a => new CriteriaFeedbackMappingResponse()
                        {
                            CriteriaMasterId = a.CriteriaMasterId,
                            Score = a.Score,
                            CriteriaName = GetCriteriaMasterById(a.CriteriaMasterId)
                        }).ToList();
                        providePersonalFeedback.criteriaFeedbackMappingResponses = criteriaData;
                        List<Comment> comment = await GetCommentByFeedBackId(item.FeedbackDetailId);
                        providePersonalFeedback.FeedbackComments = FeedbackResponeLoopSeparation(comment,userAllDetails);
                        providePersonalFeedbackResponses.Add(providePersonalFeedback);
                    }

                    else
                    {
                        if (userDetails.FirstName.ToLower().Contains(searchName.ToLower()) || (userDetails.FirstName.ToLower() + " " + userDetails.LastName.ToLower()).Contains(searchName.ToLower()))
                        {
                            ProvidePersonalFeedbackResponse providePersonalFeedback = new ProvidePersonalFeedbackResponse
                            {
                                FeedbackDetailId = item.FeedbackDetailId,
                                CriteriaTypeId = (int)item.CriteriaTypeId,
                                CriteriaName = await GetCriteriaTypeMasters((int)item.CriteriaTypeId),
                                EmployeeId = userDetails.EmployeeId,
                                FirstName = userDetails.FirstName,
                                LastName = userDetails.LastName,
                                SharedRemark = item.SharedRemark,
                                ImagePath = userDetails.ImagePath,
                                AskByFirstName = requestorDetails.FirstName,
                                AskByLastName = requestorDetails.LastName,
                                AskByCreatedOn = feedbackRequest.CreatedOn,
                                AskByRequestRemark = feedbackRequest.RequestRemark,
                                AskByImagePath = requestorDetails.ImagePath,
                                Date = item.CreatedOn
                            };
                            List<CriteriaFeedbackMappingResponse> criteriaData = criteriaFeedbackMappings.Select(a => new CriteriaFeedbackMappingResponse()
                            {
                                CriteriaMasterId = a.CriteriaMasterId,
                                Score = a.Score,
                                CriteriaName = GetCriteriaMasterById(a.CriteriaMasterId)
                            }).ToList();
                            providePersonalFeedback.criteriaFeedbackMappingResponses = criteriaData;
                            List<Comment> comment = await GetCommentByFeedBackId(item.FeedbackDetailId);
                            providePersonalFeedback.FeedbackComments = FeedbackResponeLoopSeparation(comment, userAllDetails);
                            providePersonalFeedbackResponses.Add(providePersonalFeedback);
                        }
                    }
                }

            }
            if (sortOrder)
            {
                return providePersonalFeedbackResponses.OrderByDescending(x => x.Date).ToList();
            }
            return providePersonalFeedbackResponses.OrderBy(x => x.Date).ToList();
        }



        public async Task<List<FeedbackRequest>> GetFeedbackDetailRequest(long employeeId)
        {
            return await feedbackRequestRepo.GetQueryable().Where(x => x.FeedbackById == employeeId && x.FeedbackOnTypeId == 3 && x.FeedbackRequestType == 1 && x.IsActive).ToListAsync();
        }

        public async Task<List<FeedbackDetail>> GetFeedbackDetailsAsync(long empId)
        {
            return await feedbackDetailRepo.GetQueryable().Where(x => x.CreatedBy == empId && x.FeedbackOnTypeId == 3 && x.IsActive).ToListAsync();
        }

        public async Task<FeedbackDetail> InsertFeedbackDetail(FeedbackDetail feedbackDetail)
        {
            feedbackDetailRepo.Add(feedbackDetail);
            await UnitOfWorkAsync.SaveChangesAsync();
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
                feedbackRequestRepo.Update(requestDetails);
                await UnitOfWorkAsync.SaveChangesAsync();

            }
        }

        public async Task<FeedbackRequest> UpdateAskRequest(FeedbackRequest feedbackRequest)
        {
            feedbackRequestRepo.Add(feedbackRequest);
            await UnitOfWorkAsync.SaveChangesAsync();
            return feedbackRequest;
        }

        public async Task<FeedbackRequest> GetFeedbackRequestById(long feedbackRequestId)
        {
            return await feedbackRequestRepo.GetQueryable().FirstOrDefaultAsync(x => x.FeedbackRequestId == feedbackRequestId && x.IsActive);
        }

        public async Task AddOneOnOneRequestAsync(PersonalFeedbackOneOnOneRequest request, UserIdentity currentUser, string jwtToken)
        {
            List<OneToOneDetail> oneToOneDetails = new List<OneToOneDetail>();
            foreach (var item in request.RequestedTo)
            {
                OneToOneDetail detailDb = new OneToOneDetail()
                {
                    CreatedBy = currentUser.EmployeeId,
                    CreatedOn = DateTime.UtcNow,
                    RequestId = item,
                    RequestType = request.RequestType,
                    IsActive = true,
                    OneToOneRemark = request.OnetoOneRemark,
                    RequestedFrom = request.RequestedFrom,
                    RequestedTo = item,
                    UpdatedBy = currentUser.EmployeeId,
                    UpdatedOn = DateTime.UtcNow
                };

                oneToOneDetailRepo.Add(detailDb);
                oneToOneDetails.Add(detailDb);
                await UnitOfWorkAsync.SaveChangesAsync();
            }

            await Task.Run(async () =>
            {
                await notificationsService.CreatePersonalizeOneOnOneRequestNotificationsAndEmails(oneToOneDetails, request, jwtToken).ConfigureAwait(false);
            }).ConfigureAwait(false);

        }

        public async Task<FeedbackDetail> GetFeedback(long feedbackDetailId)
        {
            return await feedbackDetailRepo.GetQueryable().FirstOrDefaultAsync(x => x.FeedbackDetailId == feedbackDetailId && x.IsActive);
        }

        public async Task<FeedbackDetail> ProvideFeedback(PersonalFeedbackRequest request, UserIdentity loginUser, string jwtToken)
        {
            FeedbackDetail feedbackDetail = new FeedbackDetail();

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

                await InsertFeedbackRequest(feedbackRequest);
                var UserFeedbackRequest = await GetFeedbackRequestById(feedbackRequest.FeedbackRequestId);
                feedbackDetail.FeedbackRequestId = feedbackRequest.FeedbackRequestId;
                feedbackDetail.FeedbackOnTypeId = request.FeedbackOnTypeId;
                feedbackDetail.FeedbackOnId = request.FeedbackOnId;
                feedbackDetail.SharedRemark = request.SharedRemark;
                feedbackDetail.CreatedBy = loginUser.EmployeeId;
                feedbackDetail.CriteriaTypeId = request.CriteriaTypeId;
                await InsertFeedbackDetail(feedbackDetail);
                if (request.criteriaFeedbackMappingRequests.Count > 0)
                {
                    await ScoreCriteriaFeedbackMapping(request.criteriaFeedbackMappingRequests, loginUser.EmployeeId, feedbackDetail.FeedbackDetailId);
                }

                await Task.Run(async () =>
                {
                    await notificationsService.ProvidePersonalizeFeedbackNotificationsAndEmails(UserFeedbackRequest, feedbackDetail, request, loginUser.EmployeeId, jwtToken).ConfigureAwait(false);
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
                feedbackDetail.CriteriaTypeId = request.CriteriaTypeId;
                await InsertFeedbackDetail(feedbackDetail);
                if (request.criteriaFeedbackMappingRequests.Count > 0)
                {
                    await ScoreCriteriaFeedbackMapping(request.criteriaFeedbackMappingRequests, loginUser.EmployeeId, feedbackDetail.FeedbackDetailId);
                }
                var UserFeedbackRequest = await GetFeedbackRequestById(request.FeedbackRequestId);
                await Task.Run(async () =>
                {
                    await notificationsService.ProvidePersonalizeFeedbackNotificationsAndEmails(UserFeedbackRequest, feedbackDetail, request, loginUser.EmployeeId, jwtToken).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            return feedbackDetail;
        }

        public async Task ScoreCriteriaFeedbackMapping(List<CriteriaFeedbackMappingRequest> criteriaFeedbackMappingRequests, long employeeId, long feedbackDetailId)
        {
            foreach (var item in criteriaFeedbackMappingRequests)
            {
                if (item.CriteriaMasterId != 0)
                {
                    CriteriaFeedbackMapping criteriaFeedbackMapping = new CriteriaFeedbackMapping();
                    criteriaFeedbackMapping.FeedbackDetailId = feedbackDetailId;
                    criteriaFeedbackMapping.Score = item.Score;
                    criteriaFeedbackMapping.CriteriaMasterId = item.CriteriaMasterId;
                    criteriaFeedbackMapping.CreatedBy = employeeId;
                    await InsertCriteriaFeedbackMapping(criteriaFeedbackMapping);

                }

            }
        }

        public async Task<CriteriaFeedbackMapping> InsertCriteriaFeedbackMapping(CriteriaFeedbackMapping criteriaFeedbackMapping)
        {
            criteriaFeedbackMappingRepo.Add(criteriaFeedbackMapping);
            await UnitOfWorkAsync.SaveChangesAsync();
            return criteriaFeedbackMapping;
        }

        public async Task<string> GetCriteriaTypeMasters(int typeId)
        {
            string result = string.Empty;
            var criteria = await criteriaTypeRepo.GetQueryable().FirstOrDefaultAsync(x => x.CriteriaTypeId == typeId);
            if (criteria != null)
            {
                result = criteria.CriteriaTypeName;
            }

            return result;
        }

        public async Task<List<CriteriaMaster>> GetCriteriaMastersWithId(int typeId)
        {
            return await criteriaMasterRepo.GetQueryable().Where(x => x.IsActive && x.CriteriaTypeId == typeId).ToListAsync();
        }


        public async Task<CriteriaMasterResponse> GetCriteriaDetails(int typeId)
        {
            CriteriaMasterResponse detailsResponse = new CriteriaMasterResponse();
            List<Criteria> criteriaDetailsdetails = new List<Criteria>();
            var criteriaDetails = await GetCriteriaMastersWithId(typeId);
            foreach (var data in criteriaDetails)
            {
                Criteria details = new Criteria();
                details.CriteriaName = data.CriteriaName;
                details.CriteriaMasterId = data.CriteriaMasterId;
                criteriaDetailsdetails.Add(details);
            }
            detailsResponse.CriteriaList = criteriaDetailsdetails;
            detailsResponse.CriteriaName = await GetCriteriaTypeMasters(typeId);
            detailsResponse.CriteriaTypeId = typeId;

            return detailsResponse;
        }

        public async Task<bool> CancelFeedbackRequest(long feedbackRequestId, UserIdentity loginUser)
        {
            bool result = false;
            var requestDetails = await GetFeedbackRequestById(feedbackRequestId);

            if (requestDetails != null)
            {
                requestDetails.Status = (int)CancelRequest.CR;
                requestDetails.UpdatedBy = loginUser.EmployeeId;
                requestDetails.UpdatedOn = DateTime.UtcNow;
                feedbackRequestRepo.Update(requestDetails);
                await UnitOfWorkAsync.SaveChangesAsync();
                result = true;

            }
            return result;
        }

        public async Task<bool> FeedbackRequestAgain(long feedbackRequestId, string jwtToken)
        {
            bool result = false;
            var requestDetails = await GetFeedbackRequestById(feedbackRequestId);
            if (requestDetails != null)
            {
                await Task.Run(async () =>
                {
                    await notificationsService.RequestAgainNotifications(feedbackRequestId, jwtToken, requestDetails.RaisedForId, requestDetails.FeedbackById, requestDetails.RequestRemark).ConfigureAwait(false);
                }).ConfigureAwait(false);
                result = true;
            }
            return result;
        }

        public async Task<FeedbackRequest> InsertFeedbackRequest(FeedbackRequest feedbackRequest)
        {
            feedbackRequestRepo.Add(feedbackRequest);
            await UnitOfWorkAsync.SaveChangesAsync();
            return feedbackRequest;
        }


        public async Task<List<ViewPersonalFeedbackResponse>> ViewPersonalFeedback(long employeeId, bool sortOrder, string searchName, string jwtToken)
        {
            List<ViewPersonalFeedbackResponse> viewPersonalFeedbackResponses = new List<ViewPersonalFeedbackResponse>();
            var getAllPersonalFeedback = await GetAllPersonalFeedbackAsync(employeeId);
            var userAllDetails = GetAllUserFromUsers(jwtToken);

            foreach (var item in getAllPersonalFeedback)
            {
                ViewPersonalFeedbackResponse viewFeedbackDetail = new ViewPersonalFeedbackResponse();
                var feedbackRequest = await GetFeedbackRequestById(item.FeedbackRequestId);
                if (feedbackRequest != null)
                {
                    var userDetails = await GetUser(jwtToken, feedbackRequest.FeedbackById);
                    var providedUser = userAllDetails.Results.FirstOrDefault(x => x.EmployeeId == item.CreatedBy);
                    var criteriaFeedbackMappings = await CriteriaFeedbackMappings(item.FeedbackDetailId);
                    if (!string.IsNullOrEmpty(searchName))
                    {
                        if (providedUser.FirstName.ToLower().Contains(searchName.ToLower()) || (providedUser.FirstName.ToLower() + " " + providedUser.LastName.ToLower()).Contains(searchName.ToLower()))
                        {
                            viewFeedbackDetail.FeedbackDetailId = item.FeedbackDetailId;
                            viewFeedbackDetail.EmployeeId = providedUser.EmployeeId;
                            viewFeedbackDetail.CriteriaTypeId = item.CriteriaTypeId;
                            viewFeedbackDetail.FeedbackCriteriaName = GetCriteriaTypeMasters((int)item.CriteriaTypeId).Result;
                            viewFeedbackDetail.FirstName = providedUser.FirstName;
                            viewFeedbackDetail.LastName = providedUser.LastName;
                            viewFeedbackDetail.SharedRemark = item.SharedRemark;
                            viewFeedbackDetail.ImagePath = providedUser.ImagePath;
                            viewFeedbackDetail.Date = item.CreatedOn;
                            viewFeedbackDetail.AskByRequestRemark = feedbackRequest.FeedbackRequestType == 2 ? "" : feedbackRequest.RequestRemark;
                            viewFeedbackDetail.AskByFirstName = feedbackRequest.FeedbackRequestType == 2 ? "" : userDetails.FirstName ?? "";
                            viewFeedbackDetail.AskByCreatedOn = feedbackRequest.FeedbackRequestType == 2 ? (DateTime?)null : feedbackRequest.CreatedOn;
                            viewFeedbackDetail.AskByImagePath = feedbackRequest.FeedbackRequestType == 2 ? "" : userDetails.ImagePath ?? "";
                            viewFeedbackDetail.AskByLastName = feedbackRequest.FeedbackRequestType == 2 ? "" : userDetails.LastName ?? "";
                            List<CriteriaFeedbackMappingResponse> criteriaData = criteriaFeedbackMappings.Select(a => new CriteriaFeedbackMappingResponse()
                            {
                                CriteriaMasterId = a.CriteriaMasterId,
                                Score = a.Score,
                                CriteriaName = GetCriteriaMasterById(a.CriteriaMasterId)
                            }).ToList();
                            viewFeedbackDetail.CriteriaFeedbackMappingResponses = criteriaData;
                            List<Comment> comment = await GetCommentByFeedBackId(item.FeedbackDetailId);
                            viewFeedbackDetail.FeedbackComments = FeedbackResponeLoopSeparation(comment, userAllDetails);
                            viewPersonalFeedbackResponses.Add(viewFeedbackDetail);
                        }
                    }
                    else
                    {
                        viewFeedbackDetail.FeedbackDetailId = item.FeedbackDetailId;
                        viewFeedbackDetail.EmployeeId = providedUser.EmployeeId;
                        viewFeedbackDetail.CriteriaTypeId = item.CriteriaTypeId;
                        viewFeedbackDetail.FeedbackCriteriaName = GetCriteriaTypeMasters((int)item.CriteriaTypeId).Result;
                        viewFeedbackDetail.FirstName = providedUser.FirstName;
                        viewFeedbackDetail.LastName = providedUser.LastName;
                        viewFeedbackDetail.SharedRemark = item.SharedRemark;
                        viewFeedbackDetail.ImagePath = providedUser.ImagePath;
                        viewFeedbackDetail.Date = item.CreatedOn;
                        viewFeedbackDetail.AskByRequestRemark = feedbackRequest.FeedbackRequestType == 2 ? "" : feedbackRequest.RequestRemark;
                        viewFeedbackDetail.AskByFirstName = feedbackRequest.FeedbackRequestType == 2 ? "" : userDetails.FirstName ?? "";
                        viewFeedbackDetail.AskByCreatedOn = feedbackRequest.FeedbackRequestType == 2 ? (DateTime?)null : feedbackRequest.CreatedOn;
                        viewFeedbackDetail.AskByImagePath = feedbackRequest.FeedbackRequestType == 2 ? "" : userDetails.ImagePath ?? "";
                        viewFeedbackDetail.AskByLastName = feedbackRequest.FeedbackRequestType == 2 ? "" : userDetails.LastName ?? "";
                        List<CriteriaFeedbackMappingResponse> criteriaData = criteriaFeedbackMappings.Select(a => new CriteriaFeedbackMappingResponse()
                        {
                            CriteriaMasterId = a.CriteriaMasterId,
                            Score = a.Score,
                            CriteriaName = GetCriteriaMasterById(a.CriteriaMasterId)

                        }).ToList();
                        viewFeedbackDetail.CriteriaFeedbackMappingResponses = criteriaData;
                        List<Comment> comment = await GetCommentByFeedBackId(item.FeedbackDetailId);
                        viewFeedbackDetail.FeedbackComments = FeedbackResponeLoopSeparation(comment, userAllDetails);
                        viewPersonalFeedbackResponses.Add(viewFeedbackDetail);
                    }
                }

            }
            if (sortOrder)
            {
                return viewPersonalFeedbackResponses.OrderByDescending(x => x.Date).ToList();
            }
            return viewPersonalFeedbackResponses.OrderBy(x => x.Date).ToList();
        }

        public async Task<List<CriteriaFeedbackMapping>> CriteriaFeedbackMappings(long FeedbackDetailId)
        {
            return await criteriaFeedbackMappingRepo.GetQueryable().Where(x => x.IsActive && x.FeedbackDetailId == FeedbackDetailId).OrderBy(x => x.CriteriaMasterId).ToListAsync();
        }

        public async Task<List<FeedbackDetail>> GetAllPersonalFeedbackAsync(long empId)
        {
            return await feedbackDetailRepo.GetQueryable().Where(x => x.FeedbackOnId == empId && x.IsActive && x.FeedbackOnTypeId == 3).ToListAsync();
        }
        public async Task<List<Comment>> GetCommentByFeedBackId(long feedbackDetailId)
        {
            return await commentRepo.GetQueryable().Where(x => x.FeedbackDetailId == feedbackDetailId && x.IsActive).ToListAsync();
        }

        public string GetCriteriaMasterById(long criteriaMasterId)
        {
            var value = criteriaMasterRepo.FindOne(x => x.CriteriaMasterId == criteriaMasterId);
            string criteriaName = string.Empty;
            if (value != null)
            {
                criteriaName = value.CriteriaName;
            }
            return criteriaName;
        }

        private List<FeedbackComment> FeedbackResponeLoopSeparation(List<Comment> comment, EmployeeResult userList)
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
        
        public async Task<List<FeedbackDetail>> GetFeedbackDetailsById(long feedbackRequestId, long loginuser, int feedbackonTypeId, long feedbackOnId)
        {
            return await feedbackDetailRepo.GetQueryable().Where(x => x.FeedbackRequestId == feedbackRequestId && x.IsActive && x.CreatedBy == loginuser && x.FeedbackOnTypeId == feedbackonTypeId && x.FeedbackOnId == feedbackOnId).OrderByDescending(x => x.FeedbackDetailId).ToListAsync();
        }

        public async Task<PersonalFeedbackResponse> PersonalFeedbackResponse(long feedbackRequestId, string jwtToken, UserIdentity userIdentity)
        {
            var personalFeedbackResponse = new PersonalFeedbackResponse();
            var feedbackRequest = await GetFeedbackRequestByIdAsync(feedbackRequestId);
            if (feedbackRequest != null)
            {
                List<FeedbackProvideDetails> feedbackProvideDetails = new List<FeedbackProvideDetails>();
                var userDetails = await GetUser(jwtToken, feedbackRequest.FeedbackById);

                var feedbackdetails = await GetFeedbackDetailsById(feedbackRequestId, userIdentity.EmployeeId, feedbackRequest.FeedbackOnTypeId, feedbackRequest.FeedbackById);
                var sharedUserDetails = await GetUser(jwtToken, userIdentity.EmployeeId);
                if (feedbackdetails.Count > 0)
                {
                    foreach (var item in feedbackdetails)
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
                personalFeedbackResponse.FeedbackRequestId = feedbackRequest.FeedbackRequestId;
                personalFeedbackResponse.FeedbackOnTypeId = feedbackRequest.FeedbackOnTypeId;
                personalFeedbackResponse.FeedbackOnId = feedbackRequest.FeedbackOnId;
                personalFeedbackResponse.AskByRequestRemark = feedbackRequest.RequestRemark;
                personalFeedbackResponse.FeedbackById = feedbackRequest.FeedbackById;
                personalFeedbackResponse.AskByFirstName = userDetails.FirstName ?? "";
                personalFeedbackResponse.AskByLastName = userDetails.LastName ?? "";
                personalFeedbackResponse.AskByImagePath = userDetails.ImagePath ?? "";
                personalFeedbackResponse.AskByCreatedOn = feedbackRequest.CreatedOn;
                personalFeedbackResponse.Status = feedbackRequest.Status;
                personalFeedbackResponse.feedbackProvideDetails = feedbackProvideDetails;

            }
            return personalFeedbackResponse;
        }

        public async Task<FeedbackRequest> GetFeedbackRequestByIdAsync(long feedbackRequestId)
        {
            return await feedbackRequestRepo.GetQueryable().FirstOrDefaultAsync(x => x.FeedbackRequestId == feedbackRequestId);
        }

        public async Task<bool> ApproveRejectRequestOnetoOne(AcceptRejectRequest acceptRejectRequest, UserIdentity loginUser, string jwtToken)
        {
            bool result = false;
            var request = await GetRequestOnetoOneByIdAsync(acceptRejectRequest.OneToOneDetailId);
            if (request != null)
            {
                request.Status = acceptRejectRequest.Status;
                request.UpdatedOn = DateTime.UtcNow;
                request.UpdatedBy = loginUser.EmployeeId;
                oneToOneDetailRepo.Update(request);
                await UnitOfWorkAsync.SaveChangesAsync();

                await Task.Run(async () =>
                {
                    await notificationsService.ApproveAndRejectRequestOnetoOne(acceptRejectRequest.OneToOneDetailId, acceptRejectRequest.Status, acceptRejectRequest.NotificationsDetailId, request.RequestedTo, request.RequestedFrom, jwtToken).ConfigureAwait(false);
                }).ConfigureAwait(false);

                result = true;
            }

            return result;
        }

        public async Task<OneToOneDetail> GetRequestOnetoOneByIdAsync(long detailId)
        {
            return await oneToOneDetailRepo.GetQueryable().FirstOrDefaultAsync(x => x.OneToOneDetailId == detailId && x.IsActive == true);
        }
    }
}
