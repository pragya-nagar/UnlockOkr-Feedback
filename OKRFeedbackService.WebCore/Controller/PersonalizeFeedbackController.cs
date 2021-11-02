using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OKRFeedbackService.Common;
using OKRFeedbackService.EF;
using OKRFeedbackService.Service.Contracts;
using OKRFeedbackService.ViewModel.Request;
using OKRFeedbackService.ViewModel.Response;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OKRFeedbackService.WebCore.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonalizeFeedbackController : ApiControllerBase
    {
        private readonly IPersonalizeFeedbackService feedbackService;
        private readonly ICommonService commonService;

        public PersonalizeFeedbackController(IPersonalizeFeedbackService feedbackServices , ICommonService commonServices)
        {
            feedbackService = feedbackServices;
            commonService = commonServices;
        }

        [HttpPost]
        [Route("AskPersonalFeedback")]
        public async Task<IActionResult> AddPersonalFeedback(AskPersonalFeedbackRequest requestFeeback)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            if (loginUser.EmployeeId != requestFeeback.FeedbackById || loginUser.RolePermissions.Any(x => x.Permissions.Any(e => e.PermissionName == Constants.AskFeedback && !e.Status)))
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var payloadSave = new PayloadCustom<FeedbackRequest>();

            if (requestFeeback.RaisedTypeId == 0)
                ModelState.AddModelError("RaisedTypeId", "RaisedTypeId cant be 0.");

            if (requestFeeback.RaisedForId.Count <= 0)
                ModelState.AddModelError("RaisedForId", "RaisedForId cant be 0.");

            if (requestFeeback.FeedbackOnTypeId == 0)
                ModelState.AddModelError("FeedbackOnTypeId", "FeedbackOnTypeId cant be 0.");

            if (requestFeeback.FeedbackById == 0)
                ModelState.AddModelError("FeedbackById", "FeedbackById cant be 0.");

            if (ModelState.IsValid)
            {
                IOperationStatus operation = await feedbackService.AddPersonalFeedbackAsync(requestFeeback, loginUser, UserToken);
                if (operation != null)
                {
                    payloadSave.MessageType = MessageType.Success.ToString();
                    payloadSave.IsSuccess = true;
                    payloadSave.Status = (int)HttpStatusCode.OK;
                }
            }
            else
            {
                payloadSave = GetPayloadStatus(payloadSave);
            }

            return Ok(payloadSave);
        }

        [HttpGet]
        [Route("AskedPersonalFeedbackDetails")]
        public async Task<IActionResult> AskFeedbackDetail(long employeeId, bool sortOrder, string searchName)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (loginUser.RolePermissions.Any(x => x.Permissions.Any(e => e.PermissionName == Constants.AskFeedback && !e.Status)))
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var payloadSave = new PayloadCustom<AskPersonalFeedbackResponse>();


            if (ModelState.IsValid)
            {
                payloadSave.EntityList = await feedbackService.GetAskedFeedbackUserDetailAsync(employeeId, sortOrder, UserToken, searchName);
                if (payloadSave.EntityList != null)
                {
                    payloadSave.MessageType = MessageType.Success.ToString();
                    payloadSave.IsSuccess = true;
                    payloadSave.Status = (int)HttpStatusCode.OK;
                }
            }
            else
            {
                payloadSave = GetPayloadStatus(payloadSave);
            }

            return Ok(payloadSave);
        }

        [HttpPost]
        [Route("PersonalFeedbackOneOnOne")]
        public async Task<IActionResult> RequestOneOne(PersonalFeedbackOneOnOneRequest oneToOneRequest)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (loginUser.RolePermissions.Any(x => x.Permissions.Any(e => e.PermissionName == Constants.FeedbackRequestOneOne && !e.Status)))
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var payloadSave = new PayloadCustom<bool>();

            if (oneToOneRequest.RequestType == 0)
                ModelState.AddModelError("RequestType", "RequestType cant be 0.");

            if (string.IsNullOrWhiteSpace(oneToOneRequest.OnetoOneRemark))
                ModelState.AddModelError("OnetoOneRemark", "OnetoOneRemark cant be blank.");

            if (oneToOneRequest.RequestedFrom == 0)
                ModelState.AddModelError("RequestedFrom", "RequestedFrom cant be 0.");

            if (oneToOneRequest.RequestedTo.Count <= 0)
                ModelState.AddModelError("RequestedTo", "RequstedTo cant be 0.");
            foreach (var item in oneToOneRequest.RequestedTo)
            {
                if (loginUser.EmployeeId == item)
                    ModelState.AddModelError("Result", "login employee cant be equal to requested to.");
            }

            if (ModelState.IsValid)
            {
                await feedbackService.AddOneOnOneRequestAsync(oneToOneRequest, loginUser, UserToken);
                payloadSave.MessageType = MessageType.Success.ToString();
                payloadSave.IsSuccess = true;
                payloadSave.Status = (int)HttpStatusCode.OK;
            }
            else
            {
                payloadSave = GetPayloadStatus(payloadSave);
            }

            return Ok(payloadSave);
        }

        [HttpGet]
        [Route("ProvidePersonalFeedback")]
        public async Task<IActionResult> ProvideFeedbackDetail(bool sortOrder, string searchName)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (loginUser.RolePermissions.Any(x => x.Permissions.Any(e => e.PermissionName == Constants.GiveFeedback && !e.Status)))
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var payloadSave = new PayloadCustom<ProvidePersonalFeedbackResponse>();


            if (ModelState.IsValid)
            {
                payloadSave.EntityList = await feedbackService.GetProvideFeedbackUserDetailAsync(loginUser.EmployeeId, sortOrder, UserToken, searchName);
                if (payloadSave.EntityList.Count > 0)
                {
                    payloadSave.MessageType = MessageType.Success.ToString();
                    payloadSave.IsSuccess = true;
                    payloadSave.Status = (int)HttpStatusCode.OK;
                }
            }
            else
            {
                payloadSave = GetPayloadStatus(payloadSave);
            }

            return Ok(payloadSave);
        }

        [HttpPost]
        [Route("Provide")]
        public async Task<IActionResult> ProviedFeedback(PersonalFeedbackRequest requestFeeback)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            var payloadSave = new PayloadCustom<FeedbackDetail>();

            if (requestFeeback.CriteriaTypeId == 1)
            {
                var praiseDetails = await feedbackService.GetCriteriaMastersWithId(requestFeeback.CriteriaTypeId);
                if (requestFeeback.criteriaFeedbackMappingRequests.Count < praiseDetails.Count)
                {
                    ModelState.AddModelError("criteriaFeedbackMappingRequests", "Not all parameters are scored");
                }
            }
            else
            {
                var growthOpportunityDetails = await feedbackService.GetCriteriaMastersWithId(requestFeeback.CriteriaTypeId);
                if (requestFeeback.criteriaFeedbackMappingRequests.Count < growthOpportunityDetails.Count)
                {
                    ModelState.AddModelError("criteriaFeedbackMappingRequests", "Not all parameters are scored");
                }
            }

            ModelErrorProviedFeedback(requestFeeback, loginUser.EmployeeId);

            if (ModelState.IsValid)
            {
                payloadSave.Entity = await feedbackService.ProvideFeedback(requestFeeback, loginUser, UserToken);
                if (payloadSave.Entity != null)
                {
                    payloadSave.MessageType = MessageType.Success.ToString();
                    payloadSave.IsSuccess = true;
                    payloadSave.Status = (int)HttpStatusCode.OK;
                }
            }
            else
            {
                payloadSave = GetPayloadStatus(payloadSave);
            }

            return Ok(payloadSave);
        }

        [HttpGet]
        [Route("CriteriaMaster")]
        public async Task<IActionResult> GetCriteriaMaster(int typeId)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            var payloadSave = new PayloadCustom<CriteriaMasterResponse>();

            payloadSave.Entity = await feedbackService.GetCriteriaDetails(typeId);
            if (payloadSave.Entity != null)
            {
                payloadSave.MessageType = MessageType.Success.ToString();
                payloadSave.IsSuccess = true;
                payloadSave.Status = (int)HttpStatusCode.OK;
            }
            return Ok(payloadSave);
        }

        [HttpPut]
        [Route("CancelFeedback")]
        public async Task<IActionResult> CancelFeedbackRequest(long feedbackRequestId)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var payloadSave = new PayloadCustom<bool>();

            if (feedbackRequestId == 0)
                ModelState.AddModelError("feedbackRequestId", "FeedbackRequestId cant be 0.");

            if (ModelState.IsValid)
            {
                payloadSave.Entity = await feedbackService.CancelFeedbackRequest(feedbackRequestId, loginUser);
                if (payloadSave.Entity)
                {
                    payloadSave.MessageType = MessageType.Success.ToString();
                    payloadSave.IsSuccess = true;
                    payloadSave.Status = (int)HttpStatusCode.OK;
                }
            }
            else
            {
                payloadSave = GetPayloadStatus(payloadSave);
            }

            return Ok(payloadSave);
        }


        [HttpGet]
        [Route("RequestFeedbackAgain")]
        public async Task<IActionResult> RequestFeedbackAgain(long feedbackRequestId)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var payloadSave = new PayloadCustom<bool>();

            if (feedbackRequestId == 0)
                ModelState.AddModelError("feedbackRequestId", "FeedbackRequestId cant be 0.");

            if (ModelState.IsValid)
            {
                payloadSave.Entity = await feedbackService.FeedbackRequestAgain(feedbackRequestId, UserToken);
                if (payloadSave.Entity)
                {
                    payloadSave.MessageType = MessageType.Success.ToString();
                    payloadSave.IsSuccess = true;
                    payloadSave.Status = (int)HttpStatusCode.OK;
                }
            }
            else
            {
                payloadSave = GetPayloadStatus(payloadSave);
            }

            return Ok(payloadSave);
        }

        [HttpGet]
        [Route("PersonalFeedback")]
        public async Task<IActionResult> PersonalFeedback(bool sortOrder, string searchName)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var payloadSave = new PayloadCustom<ViewPersonalFeedbackResponse>();


            payloadSave.EntityList = await feedbackService.ViewPersonalFeedback(loginUser.EmployeeId, sortOrder, searchName, UserToken);
            if (payloadSave.EntityList.Count > 0)
            {
                payloadSave.MessageType = MessageType.Success.ToString();
                payloadSave.IsSuccess = true;
                payloadSave.Status = (int)HttpStatusCode.OK;
            }
            payloadSave.MessageType = MessageType.Success.ToString();
            payloadSave.IsSuccess = true;
            payloadSave.Status = (int)HttpStatusCode.OK;

            return Ok(payloadSave);
        }


        [HttpGet]
        [Route("PersonalFeedbackResponse")]
        public async Task<IActionResult> PersonalFeedbackResponse(long feedbackRequestId)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var payloadSave = new PayloadCustom<PersonalFeedbackResponse>();

            if (feedbackRequestId == 0)
                ModelState.AddModelError("FeedbackRequest", "Feedback Request can not be 0.");

            if (ModelState.IsValid)
            {
                payloadSave.Entity = await feedbackService.PersonalFeedbackResponse(feedbackRequestId, UserToken, loginUser);
                if (payloadSave.Entity.Status == 4)
                {
                    return StatusCode((int)HttpStatusCode.Forbidden);
                }
                else
                {
                    payloadSave.MessageType = MessageType.Success.ToString();
                    payloadSave.IsSuccess = true;
                    payloadSave.Status = (int)HttpStatusCode.OK;
                }
            }
            else
            {
                payloadSave = GetPayloadStatus(payloadSave);
            }

            return Ok(payloadSave);
        }

        /// <summary>
        /// status = 0 reject and 1 = approve
        /// </summary>
        /// <param name="detailId"></param>
        /// <param name="status"></param>
        /// <returns></returns>

        [HttpPut]
        [Route("ApproveRejectRequest")]
        public async Task<IActionResult> ApproveRejectRequestOnetoOne(AcceptRejectRequest acceptRejectRequest)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var payloadSave = new PayloadCustom<bool>();

            if (acceptRejectRequest.OneToOneDetailId == 0)
                ModelState.AddModelError("detailId", "DetailId cant be 0.");

            if (acceptRejectRequest.NotificationsDetailId == 0)
                ModelState.AddModelError("NotificationsDetailId", "NotificationsDetailId cant be 0.");

            var isExistOneToOneDetailsId = await feedbackService.GetRequestOnetoOneByIdAsync(acceptRejectRequest.OneToOneDetailId);
            if (isExistOneToOneDetailsId == null)
            {
                ModelState.AddModelError("OneToOneDetailId", "OntoOneDetailId does not exist");
            }

            else if (isExistOneToOneDetailsId!=null)
            {
                if (isExistOneToOneDetailsId.Status == 0 || isExistOneToOneDetailsId.Status == 1)
                {
                    ModelState.AddModelError("OneToOneDetailId", "Already accepted/rejected the request");
                }
            }

            if (ModelState.IsValid)
            {
                payloadSave.Entity = await feedbackService.ApproveRejectRequestOnetoOne(acceptRejectRequest, loginUser, UserToken);
                if (payloadSave.Entity)
                {
                    payloadSave.MessageType = MessageType.Success.ToString();
                    payloadSave.IsSuccess = true;
                    payloadSave.Status = (int)HttpStatusCode.OK;
                }
            }
            else
            {
                payloadSave = GetPayloadStatus(payloadSave);
            }

            return Ok(payloadSave);
        }
        
        private void ModelErrorProviedFeedback(PersonalFeedbackRequest requestFeeback, long loginEmpId)
        {
            if (string.IsNullOrWhiteSpace(requestFeeback.SharedRemark))
                ModelState.AddModelError("SharedRemark", "SharedRemark cant be blank.");



            if (requestFeeback.RaisedTypeId == 0)
                ModelState.AddModelError("RaisedTypeId", "RaisedTypeId cant be 0.");



            if (requestFeeback.RaisedForId == 0)
                ModelState.AddModelError("RaisedForId", "RaisedForId cant be 0.");



            if (requestFeeback.FeedbackOnTypeId == 0)
                ModelState.AddModelError("FeedbackOnTypeId", "FeedbackOnTypeId cant be 0.");



            if (requestFeeback.FeedbackOnId == 0)
                ModelState.AddModelError("FeedbackOnId", "FeedbackOnId cant be 0.");



            if (requestFeeback.Status == 0)
                ModelState.AddModelError("Status", "Status cant be 0.");



            if (loginEmpId == requestFeeback.RaisedForId)
                ModelState.AddModelError("RaisedForId", "You cannot provide feedback to yourself");
        }



    }
}