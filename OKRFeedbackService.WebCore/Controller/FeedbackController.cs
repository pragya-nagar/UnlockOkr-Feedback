using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OKRFeedbackService.EF;
using OKRFeedbackService.Service.Contracts;
using OKRFeedbackService.Common;
using OKRFeedbackService.ViewModel.Request;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using OKRFeedbackService.ViewModel.Response;
using System.Linq;

namespace OKRFeedbackService.WebCore.Controller
{
    [Route("api/[controller]")]
    [ApiController]

    public class FeedbackController : ApiControllerBase
    {
        private readonly IFeedbackService feedbackService;
        private readonly ICommonService commonService;

        public FeedbackController(IFeedbackService feedbackServices , ICommonService commonServices)
        {
            feedbackService = feedbackServices;
            commonService = commonServices;
        }

        [HttpPost]
        [Route("Ask")]
        public async Task<IActionResult> InsertAskRequest(AskFeedbackRequest requestFeedback)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            if (loginUser.EmployeeId != requestFeedback.FeedbackById || loginUser.RolePermissions.Any(x => x.Permissions.Any(e => e.PermissionName == Constants.AskFeedback && !e.Status)))
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var payloadSave = new PayloadCustom<FeedbackRequest>();

            if (requestFeedback.RaisedTypeId == 0)
                ModelState.AddModelError("RaisedTypeId", "RaisedTypeId cant be 0.");

            if (requestFeedback.RaisedForId.Count <= 0)
                ModelState.AddModelError("RaisedForId", "RaisedForId cant be 0.");

            if (requestFeedback.FeedbackOnTypeId == 0)
                ModelState.AddModelError("FeedbackOnTypeId", "FeedbackOnTypeId cant be 0.");

            if (requestFeedback.FeedbackOnId == 0)
                ModelState.AddModelError("FeedbackOnId", "FeedbackOnId cant be 0.");

            if (requestFeedback.FeedbackById == 0)
                ModelState.AddModelError("FeedbackById", "FeedbackById cant be 0.");

            if (ModelState.IsValid)
            {
                IOperationStatus operation = await feedbackService.InsertAskRequest(requestFeedback, loginUser, UserToken);
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

        [HttpPost]
        [Route("Provide")]
        public async Task<IActionResult> ProvideFeedback(ProvideFeedbackRequest requestFeedback)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            var feedbackRequestDetails = await feedbackService.GetFeedbackRequest(requestFeedback.FeedbackRequestId);

            if (requestFeedback.FeedbackRequestId != 0)
            {
                if (feedbackRequestDetails == null || loginUser.EmployeeId != feedbackRequestDetails.RaisedForId || loginUser.RolePermissions.Any(x => x.Permissions.Any(e => e.PermissionName == Constants.GiveFeedback && !e.Status)))
                {
                    return StatusCode((int)HttpStatusCode.Forbidden);
                }
            }


            var payloadSave = new PayloadCustom<FeedbackDetail>();

            ModelErrorProvideFeedback(requestFeedback, loginUser.EmployeeId);

            if (ModelState.IsValid)
            {
                payloadSave.Entity = await feedbackService.ProvideFeedback(requestFeedback, loginUser, UserToken);
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
        [Route("Feedback/{feedbackOnTypeId}/{feedbackOnId}")]
        public async Task<IActionResult> GetFeedback(int feedbackOnTypeId, long feedbackOnId)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            if (loginUser.RolePermissions.Any(x => x.Permissions.Any(e => e.PermissionName == Constants.ViewFeedback && !e.Status)))
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var payloadSave = new PayloadCustom<OkrFeedbackResponse>();

            if (feedbackOnTypeId == 0)
                ModelState.AddModelError("feedbackOnTypeId", "feedbackOnTypeId cant be 0.");

            if (feedbackOnId == 0)
                ModelState.AddModelError("feedbackOnId", "feedbackOnId cant be 0.");

            if (ModelState.IsValid)
            {
                payloadSave.Entity = await feedbackService.GetFeedbackResponse(feedbackOnTypeId, feedbackOnId, UserToken, loginUser);
                if (payloadSave.Entity != null)
                {
                    payloadSave.MessageType = MessageType.Success.ToString();
                    payloadSave.IsSuccess = true;
                    payloadSave.Status = (int)HttpStatusCode.OK;
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Forbidden);
                }
            }
            else
            {
                payloadSave = GetPayloadStatus(payloadSave);
            }

            return Ok(payloadSave);
        }

        [HttpPost]
        [Route("Comment")]
        public async Task<IActionResult> InsertComment(CommentRequest commentRequest)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            if (loginUser.RolePermissions.Any(x => x.Permissions.Any(e => e.PermissionName == Constants.CommentOnFeedback && !e.Status)))
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var payloadSave = new PayloadCustom<Comment>();

            if (commentRequest.Comments == "")
            {
                ModelState.AddModelError("Comments", "Comments cant be blank.");
            }
            if (commentRequest.FeedbackDetailId == 0)
            {
                ModelState.AddModelError("FeedbackDetailId", "FeedbackDetailId cant be 0.");
            }
            if (ModelState.IsValid)
            {
                payloadSave.Entity = await feedbackService.InsertComment(commentRequest, loginUser, UserToken);
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

        [HttpPost]
        [Route("OneToOne/Request")]
        public async Task<IActionResult> RequestOneOne(OneToOneRequest oneToOneRequest)
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

            if (oneToOneRequest.RequestId == 0)
                ModelState.AddModelError("RequestId", "RequestId cant be 0.");

            if (string.IsNullOrWhiteSpace(oneToOneRequest.OnetoOneRemark))
                ModelState.AddModelError("OneToOneRemark", "OneToOneRemark cant be blank.");

            if (oneToOneRequest.RequestedFrom == 0)
                ModelState.AddModelError("RequestedFrom", "RequestedFrom cant be 0.");

            if (oneToOneRequest.RequestedTo.Count <= 0)
                ModelState.AddModelError("RequestedTo", "RequestedTo cant be 0.");
            foreach (var item in oneToOneRequest.RequestedTo)
            {
                if (loginUser.EmployeeId == item)
                    ModelState.AddModelError("Result", "login employee cant be equal to requested to.");
            }

            if (ModelState.IsValid)
            {
                await feedbackService.CreateOneToOneRequest(oneToOneRequest, loginUser, UserToken);
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
        [Route("AskFeedback/{employeeId}")]
        public async Task<IActionResult> AskFeedbackDetail(long employeeId)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (loginUser.RolePermissions.Any(x => x.Permissions.Any(e => e.PermissionName == Constants.AskFeedback && !e.Status)))
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var payloadSave = new PayloadCustom<AskFeedbackResponse>();

            if (employeeId == 0)
                ModelState.AddModelError("employeeId", "employeeId cant be 0.");

            if (ModelState.IsValid)
            {
                payloadSave.EntityList = await feedbackService.GetFeedbackDetail(employeeId, UserToken);
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
        [Route("MyGoalFeedbackResponse")]
        public async Task<IActionResult> MyGoalFeedbackResponse(long feedbckRequestId)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (loginUser.RolePermissions.Any(x => x.Permissions.Any(e => e.PermissionName == Constants.GiveFeedback && !e.Status)))
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }
            var feedbackRequestDetails = await feedbackService.GetFeedbackRequest(feedbckRequestId);
            if (feedbackRequestDetails != null && feedbackRequestDetails.FeedbackOnTypeId != Constants.TeamTypeId)
            {
                if (!(loginUser.EmployeeId == feedbackRequestDetails.RaisedForId || loginUser.EmployeeId == feedbackRequestDetails.FeedbackById))
                {
                    return StatusCode((int)HttpStatusCode.Forbidden);
                }

            }
            var payloadSave = new PayloadCustom<MyGoalDetailFeedbackResponse>();

            if (feedbckRequestId == 0)
                ModelState.AddModelError("FeedbackRequest", "Feedback Request can not be 0.");

            if (ModelState.IsValid)
            {
                payloadSave.Entity = await feedbackService.MyGoalFeedbackResponse(feedbckRequestId, UserToken, loginUser);
                if (payloadSave.Entity != null)
                {

                    payloadSave.MessageType = MessageType.Success.ToString();
                    payloadSave.IsSuccess = true;
                    payloadSave.Status = (int)HttpStatusCode.OK;
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Forbidden);
                }
            }
            else
            {
                payloadSave = GetPayloadStatus(payloadSave);
            }

            return Ok(payloadSave);
        }

        [HttpGet]
        [Route("MostFeedbackReport")]
        public async Task<IActionResult> MostFeedbackReport(long empId)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (loginUser.RolePermissions.Any(x => x.Permissions.Any(e => e.PermissionName == Constants.ViewFeedback && !e.Status)))
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var payloadSave = new PayloadCustom<MostFeedbackReponse>
            {
                EntityList = feedbackService.MostFeedbackReponses(empId, UserToken)
            };
            if (payloadSave.EntityList.Count > 0)
            {
                payloadSave.MessageType = MessageType.Success.ToString();
                payloadSave.IsSuccess = true;
                payloadSave.Status = (int)HttpStatusCode.OK;
            }

            return Ok(payloadSave);
        }

        [HttpGet]
        [Route("AllFeedbackByUser")]
        public async Task<IActionResult> GetViewFeedbackResponse(long employeeId)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser is null)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var payloadSave = new PayloadCustom<ViewFeedbackResponse>
            {
                EntityList = await feedbackService.GetViewFeedbackResponse(employeeId),
                MessageType = MessageType.Success.ToString(),
                IsSuccess = true,
                Status = (int)HttpStatusCode.OK
            };

            return Ok(payloadSave);
        }

        [HttpGet]
        [Route("ReadFeedbackResponse")]
        public async Task<IActionResult> ReadFeedback(long okrId)
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser is null)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var payloadSave = new PayloadCustom<bool>
            {
                Entity = await feedbackService.GetReadFeedbackResponse(okrId, UserToken, loginUser),
                MessageType = MessageType.Success.ToString(),
                IsSuccess = true,
                Status = (int)HttpStatusCode.OK
            };

            return Ok(payloadSave);
        }

        #region Private Methods

        private void ModelErrorProvideFeedback(ProvideFeedbackRequest requestFeedback, long loginEmpId)
        {
            if (string.IsNullOrWhiteSpace(requestFeedback.SharedRemark))
                ModelState.AddModelError("SharedRemark", "SharedRemark cant be blank.");

            if (requestFeedback.RaisedTypeId == 0)
                ModelState.AddModelError("RaisedTypeId", "RaisedTypeId cant be 0.");

            if (requestFeedback.RaisedForId == 0)
                ModelState.AddModelError("RaisedForId", "RaisedForId cant be 0.");

            if (requestFeedback.FeedbackOnTypeId == 0)
                ModelState.AddModelError("FeedbackOnTypeId", "FeedbackOnTypeId cant be 0.");

            if (requestFeedback.FeedbackOnId == 0)
                ModelState.AddModelError("FeedbackOnId", "FeedbackOnId cant be 0.");

            if (requestFeedback.Status == 0)
                ModelState.AddModelError("Status", "Status cant be 0.");

            if (loginEmpId == requestFeedback.RaisedForId)
                ModelState.AddModelError("RaisedForId", "You cannot provide feedback to yourself");
        }

        #endregion


    }
}

