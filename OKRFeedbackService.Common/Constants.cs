
using System.Diagnostics.CodeAnalysis;

namespace OKRFeedbackService.Common
{
    [ExcludeFromCodeCoverage]
    public static class Constants
    {
        public const int AppId = 5;
        public const string MSGsucess = "Success";
        public const long MailerTemplateToAskFeedback = 9;
        public const string FeedbackProviderMessage = "<Requestor> has requested for your feedback on <OKR Name>.";
        public const string PersonalFeedbackProviderMessage = "<Requestor> has requested for your personal feedback.";
        public const string FeedbackRequestorMessage = "<Provider> has responded to your feedback request on <OKR Name>";
        public const string FeedbackRequestorMessageForUser = "<Provider> has shared a feedback on <OKR Name>";
        public const string PersonalizeFeedbackRequestorMessage = "<Provider> has shared a personal feedback. Check Now!!";
        public const string FeedbackCommentsMessage = "A new comment has been added by <username> to your feedback on <feedback source>";
        public const string PersonalizeFeedbackCommentsMessage = "A new comment has been added by <username> to your personal feedback.";
        public const string FeedbackOneOnOneMessage = "<user> has requested 1:1 on <goal> <OKR>.";
        public const string PersonalFeedbackOneOnOneMessage = "<user> has requested 1:1.";
        public const string AskFeedback = "Ask Feedback";
        public const string GiveFeedback = "Give Feedback";
        public const string ViewFeedback = "View Feedback";
        public const string CommentOnFeedback = "Comment on Feedback";
        public const string FeedbackRequestOneOne = "Feedback Request 1:1";
        public const string POST = "POST";
        public const string GET = "GET";
        public const string ControlerHeader = "ControllerHeader";
        public const string Base64Regex = @"^[a-zA-Z0-9\+/]*={0,3}$";
        public const string ApplicationJson = "application/json";
        public const string controlerHeader = "controlerHeader";
        public const string GetKeyDetail = "api/MyGoals/GetKeyDetail?type=";
        public const string TypeId = "&typeId=";
        public const int DirectFeedback = 2;
        public const string OneOnOneRequested = "One On One Requested";
        public const string Requested = "Requested";
        public const string Shared = "Shared";
        public const string ArrowImage = "arrow.png";
        public const string TopBar = "topBar.png";
        public const string LogoImages = "logo.png";
        public const string TickImages = "tick.png";
        public const string FeedbackProvider = "feedback-provider.png";
        public const string ShareFeedbackImage = "share-feedback.png";
        public const string FeedbackProviderImage = "right-images1.png";
        public const string FeedbackCommentImage = "right-images1.png";
        public const string ReplyButtonImage = "reply-btn.png";
        public const string ConnectImage = "handshake.png";
        public const string LoginButtonImage = "login.png";
        public const string SupportEmailId = "adminsupport@unlockokr.com";
        public const string AskedRequestAgain = "<Requestor> has requested  for your personal feedback again.Check now!!";
        public const string RejectRequestOnetoOne = "Rejected  <Requestor>'s 1:1 request";
        public const string ApprovedRequestOnetoOne = "Accepted <Requestor>'s  1:1 request ";
        public const long RejectTypeId = 5;
        public const long ApproveTypeId = 6;
        public const string RejectRequestOnetoOneMessageForRequestor = "<Provider> has rejected your request for 1:1 ";
        public const string ApprovedRequestOnetoOneMessageForRequestor = "<Provider> has accepted your request for 1:1";
        public const int TeamTypeId = 4;
        public const int TeamRaisedTypeId = 2;
        public const int ObjectiveTypeId = 1;
        public const int KeyTypeId = 2;
        public const string Facebook = "facebook.png";
        public const string Linkedin = "linkedin.png";
        public const string Twitter = "twitter.png";       
        public const string Instagram = "instagram.png";

        public const string GetAllUsers = "GetAllUsers";
    }
}
