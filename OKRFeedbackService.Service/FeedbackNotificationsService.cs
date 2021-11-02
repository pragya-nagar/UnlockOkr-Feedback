using OKRFeedbackService.Common;
using OKRFeedbackService.EF;
using OKRFeedbackService.Service.Contracts;
using OKRFeedbackService.ViewModel.Request;
using OKRFeedbackService.ViewModel.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace OKRFeedbackService.Service
{
    [ExcludeFromCodeCoverage]
    public class FeedbackNotificationsService : BaseService, IFeedbackNotificationsService
    {
        public FeedbackNotificationsService(IServicesAggregator servicesAggregateService) : base(servicesAggregateService)
        {
        }

        public async Task InsertAskRequestNotificationsAndEmails(List<FeedbackRequest> feedbackRequestList, AskFeedbackRequest askFeedbackRequest, string jwtToken)
        {
            //Mail to Feedback Provider
            var keyVault = await KeyVaultService.GetAzureBlobKeysAsync();
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";
            var loginUrl = settings.FrontEndUrl;
            var feedbackUrl = loginUrl;
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            foreach (var user in feedbackRequestList)
            {
                var feedbackProvider = await GetUser(jwtToken, user.RaisedForId);

                var feedbackRequester = await GetUser(jwtToken, askFeedbackRequest.FeedbackById);
                if (!string.IsNullOrEmpty(feedbackUrl))
                {
                    feedbackUrl = feedbackUrl + "?redirectUrl=unlock-me" + "&empId=" + feedbackProvider.EmployeeId;
                }

                if (feedbackProvider != null && feedbackRequester != null && askFeedbackRequest.FeedbackOnTypeId != 4)
                {
                    var okr = await MyGoalFeedBackResponse(askFeedbackRequest.FeedbackOnTypeId, askFeedbackRequest.FeedbackOnId, jwtToken);
                    if (okr != null)
                    {
                        var message = string.Empty;
                        var template = await GetMailerTemplateAsync(TemplateCodes.AF.ToString(), jwtToken);
                        string body = template.Body;
                        string subject = template.Subject;
                        subject = subject.Replace("<requestor>", feedbackRequester.FirstName);
                        body = body.Replace("user", feedbackProvider.FirstName).Replace("topBar", blobCdnUrl + Constants.TopBar).Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("login", blobCdnUrl + Constants.LoginButtonImage)
                            .Replace("<supportEmailId>", Constants.SupportEmailId).Replace("tick", blobCdnUrl + Constants.TickImages).Replace("shareFeedback", blobCdnUrl + Constants.ShareFeedbackImage).Replace("<URL>", feedbackUrl).Replace("<remark>", user.RequestRemark)
                            .Replace("feedbackAsk", blobCdnUrl + Constants.FeedbackProviderImage).Replace("Requestor", feedbackRequester.FirstName).Replace("<provideFeedbackUrl>", loginUrl + "?redirectUrl=" + "Feedback/" + (int)FeedbackType.AskFeedback + "/" + user.FeedbackRequestId + "/" + askFeedbackRequest.FeedbackById + "&empId=" + feedbackProvider.EmployeeId)
                            .Replace("feedbackprovider", blobCdnUrl + Constants.FeedbackProvider).Replace("year", Convert.ToString(DateTime.Now.Year))
                            .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                            .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                            .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);

                        if (user.FeedbackOnTypeId == 1)
                        {
                            subject = subject.Replace("OKR", okr.ObjectiveName);
                            body = body.Replace("OKRfocus", okr.ObjectiveName).Replace("<distinguisher>", "OKR");
                        }
                        else
                        {
                            var keyId = okr.KeyDetails.FirstOrDefault(x => x.GoalKeyId == user.FeedbackOnId);
                            subject = subject.Replace("OKR", keyId.KeyDescription);
                            body = body.Replace("OKRfocus", keyId.KeyDescription).Replace("<distinguisher>", "KR");
                        }

                        MailRequest mailRequest = new MailRequest();
                        if (feedbackProvider.EmailId != null && template.Subject != "")
                        {
                            mailRequest.MailTo = feedbackProvider.EmailId;
                            mailRequest.Subject = subject;
                            mailRequest.Body = body;
                            await SentMailAsync(mailRequest, jwtToken);
                        }

                        ////Notification to feedback provider 
                        if (user.FeedbackOnTypeId == 1)
                        {
                            message = Constants.FeedbackProviderMessage.Replace("<Requestor>", feedbackRequester.FirstName).Replace("<OKR Name>", okr.ObjectiveName);
                        }
                        else
                        {
                            var keyId = okr.KeyDetails.FirstOrDefault(x => x.GoalKeyId == user.FeedbackOnId);
                            message = Constants.FeedbackProviderMessage.Replace("<Requestor>", feedbackRequester.FirstName).Replace("<OKR Name>", keyId.KeyDescription);
                        }

                        NotificationDetails notificationDetails = new NotificationDetails
                        {
                            JwtToken = jwtToken,
                            To = user.RaisedForId,
                            By = askFeedbackRequest.FeedbackById,
                            AppId = Constants.AppId,
                            MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                            NotificationType = (int)NotificationType.AskFeedback,
                            Url = "Feedback / " + (int)FeedbackType.AskFeedback + " / " + user.FeedbackRequestId,
                            NotificationText = message,
                            NotificationOnTypeId = askFeedbackRequest.FeedbackOnTypeId,
                            NotificationOnId = askFeedbackRequest.FeedbackOnId
                        };

                        await NotificationsAsync(notificationDetails);
                    }
                }

                else if (feedbackProvider != null && feedbackRequester != null && askFeedbackRequest.FeedbackOnTypeId == 4)
                {
                    var teamDetails = GetTeamEmployeeByTeamId(askFeedbackRequest.FeedbackOnId, jwtToken);
                    var message = Constants.FeedbackProviderMessage.Replace("<Requestor>", feedbackRequester.FirstName).Replace("<OKR Name>", teamDetails.OrganisationName);
                    var template = await GetMailerTemplateAsync(TemplateCodes.AF.ToString(), jwtToken);
                    string body = template.Body;
                    string subject = template.Subject;
                    subject = subject.Replace("<requestor>", feedbackRequester.FirstName).Replace("OKR", teamDetails.OrganisationName);
                    body = body.Replace("user", feedbackProvider.FirstName).Replace("topBar", blobCdnUrl + Constants.TopBar)
                           .Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("<supportEmailId>", Constants.SupportEmailId)
                           .Replace("tick", blobCdnUrl + Constants.TickImages).Replace("shareFeedback", blobCdnUrl + Constants.ShareFeedbackImage)
                           .Replace("<URL>", feedbackUrl).Replace("<remark>", user.RequestRemark).Replace("feedbackAsk", blobCdnUrl + Constants.FeedbackProviderImage)
                           .Replace("Requestor", feedbackRequester.FirstName)
                           .Replace("<provideFeedbackUrl>", loginUrl + "?redirectUrl=" + "Feedback/" + (int)FeedbackType.AskFeedback + "/" + user.FeedbackRequestId + "/" + askFeedbackRequest.FeedbackById + "&empId=" + feedbackProvider.EmployeeId).Replace("feedbackprovider", blobCdnUrl + Constants.FeedbackProvider)
                           .Replace("year", Convert.ToString(DateTime.Now.Year)).Replace("OKRfocus", teamDetails.OrganisationName).Replace("<distinguisher>", "Team")
                           .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                           .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                           .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);



                    MailRequest mailRequest = new MailRequest();
                    if (feedbackProvider.EmailId != null && template.Subject != "")
                    {
                        mailRequest.MailTo = feedbackProvider.EmailId;
                        mailRequest.Subject = subject;
                        mailRequest.Body = body;
                        await SentMailAsync(mailRequest, jwtToken);
                    }

                    ////Notification to feedback provider 

                    NotificationDetails notificationDetails = new NotificationDetails
                    {
                        JwtToken = jwtToken,
                        To = user.RaisedForId,
                        By = askFeedbackRequest.FeedbackById,
                        AppId = Constants.AppId,
                        MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                        NotificationType = (int)NotificationType.AskFeedback,
                        Url = "Feedback / " + (int)FeedbackType.AskFeedback + " / " + user.FeedbackRequestId,
                        NotificationText = message,
                        NotificationOnTypeId = askFeedbackRequest.FeedbackOnTypeId,
                        NotificationOnId = askFeedbackRequest.FeedbackOnId
                    };

                    await NotificationsAsync(notificationDetails);
                }
            }
        }

        public async Task ProvideFeedbackNotificationsAndEmails(FeedbackRequest feedbackRequest, FeedbackDetail feedbackDetail, ProvideFeedbackRequest request, long loginUserId, string jwtToken)
        {
            var feedbackRequestBy = feedbackRequest.FeedbackById;
            var raisedForId = feedbackRequest.RaisedForId;
            var keyVault = await KeyVaultService.GetAzureBlobKeysAsync();
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";
            var loginUrl = settings?.FrontEndUrl;
            var feedbackUrl = loginUrl;
            var feedbackRequesterUrl = loginUrl;
            var feedbackRequester = await GetUser(jwtToken, feedbackRequestBy); //the one who has requested
            var feedbackProvider = await GetUser(jwtToken, raisedForId);// with whom feedback is requested or to whom feedback is provided
            var loggedInUser = await GetUser(jwtToken, loginUserId);
            var okrName = await MyGoalFeedBackResponse(feedbackRequest.FeedbackOnTypeId, feedbackRequest.FeedbackOnId, jwtToken);
            var template = await GetMailerTemplateAsync(TemplateCodes.PF.ToString(), jwtToken);
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            if (!string.IsNullOrEmpty(feedbackUrl))
            {
                feedbackUrl = feedbackUrl + "?redirectUrl=unlock-me" + "&empId=" + feedbackProvider.EmployeeId;
            }
            if (!string.IsNullOrEmpty(feedbackRequesterUrl))
            {
                feedbackRequesterUrl = feedbackRequesterUrl + "?redirectUrl=unlock-me" + "&empId=" + feedbackRequester.EmployeeId;
            }
            //Mail to Feedback Requestor
            if (feedbackRequester != null && feedbackProvider != null && okrName != null && loggedInUser != null)
            {
                if (request.FeedbackRequestId == 0)
                {
                    var message = string.Empty;
                    string body = template.Body;
                    string subject = template.Subject;
                    subject = subject.Replace("<provider>", loggedInUser.FirstName);
                    ////body = body.Replace("<requestor>", feedbackProvider.FirstName).Replace("<provider>", loggedInUser.FirstName + " " + loggedInUser.LastName);
                    body = body.Replace("Requestor", feedbackProvider.FirstName).Replace("Name", loggedInUser.FirstName)
                               .Replace("topBar", blobCdnUrl + Constants.TopBar)
                               .Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("login", blobCdnUrl + Constants.LoginButtonImage)
                               .Replace("tick", blobCdnUrl + Constants.TickImages).Replace("<URL>", feedbackUrl).Replace("replyButton", blobCdnUrl + Constants.ReplyButtonImage).Replace("<supportEmailId>", Constants.SupportEmailId)
                               .Replace("feedbackprovider", blobCdnUrl + Constants.FeedbackProvider).Replace("<remark>", feedbackDetail.SharedRemark.TrimEnd()).Replace("feedbackResponse", blobCdnUrl + Constants.FeedbackCommentImage)
                                .Replace("<commentUrl>", loginUrl + "?redirectUrl=" + "Comments/" + (int)FeedbackType.ProvideFeedback + "/" + feedbackRequest.FeedbackRequestId + "/" + feedbackDetail.FeedbackDetailId + "&empId=" + feedbackProvider.EmployeeId).Replace("feedbackprovider", blobCdnUrl + Constants.FeedbackProvider)
                                .Replace("year", Convert.ToString(DateTime.Now.Year))
                                .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                                .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                                .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);
                    if (feedbackRequest.FeedbackOnTypeId == 1)
                    {
                        body = body.Replace("OKR", okrName.ObjectiveName);
                        subject = subject.Replace("<feedback source>", okrName.ObjectiveName);
                        message = Constants.FeedbackRequestorMessageForUser.Replace("<Provider>", loggedInUser.FirstName).Replace("<OKR Name>", okrName.ObjectiveName);
                    }
                    else
                    {
                        var keyId = okrName.KeyDetails.FirstOrDefault(x => x.GoalKeyId == feedbackRequest.FeedbackOnId);
                        body = body.Replace("OKR", keyId.KeyDescription);
                        subject = subject.Replace("<feedback source>", keyId.KeyDescription);
                        message = Constants.FeedbackRequestorMessageForUser.Replace("<Provider>", loggedInUser.FirstName).Replace("<OKR Name>", keyId.KeyDescription);
                    }

                    MailRequest mailRequest = new MailRequest();
                    if (feedbackProvider.EmailId != null && template.Subject != "")
                    {
                        mailRequest.MailTo = feedbackProvider.EmailId;
                        mailRequest.Subject = subject;
                        mailRequest.Body = body;
                        await SentMailAsync(mailRequest, jwtToken);
                    }

                    NotificationDetails notificationDetails = new NotificationDetails
                    {
                        JwtToken = jwtToken,
                        To = raisedForId,
                        By = loginUserId,
                        NotificationText = message,
                        AppId = Constants.AppId,
                        MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                        NotificationType = (int)NotificationType.ProvideFeedback,
                        Url = "Feedback / " + (int)FeedbackType.ProvideFeedback + " / " + feedbackDetail.FeedbackRequestId,
                        NotificationOnTypeId = feedbackRequest.FeedbackOnTypeId,
                        NotificationOnId = feedbackRequest.FeedbackOnId
                    };
                    await NotificationsAsync(notificationDetails);
                }

                else
                {
                    var message = string.Empty;
                    string body = template.Body;
                    string subject = template.Subject;
                    subject = subject.Replace("<provider>", feedbackProvider.FirstName);
                    body = body.Replace("Requestor", feedbackRequester.FirstName).Replace("Name", feedbackProvider.FirstName)
                               .Replace("topBar", blobCdnUrl + Constants.TopBar)
                               .Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("<supportEmailId>", Constants.SupportEmailId)
                               .Replace("tick", blobCdnUrl + Constants.TickImages).Replace("feedbackResponse", blobCdnUrl + Constants.FeedbackCommentImage)
                               .Replace("<URL>", feedbackRequesterUrl).Replace("<remark>", feedbackDetail.SharedRemark).Replace("replyButton", blobCdnUrl + Constants.ReplyButtonImage)
                               .Replace("feedbackprovider", blobCdnUrl + Constants.FeedbackProvider)
                                                               .Replace("<commentUrl>", loginUrl + "?redirectUrl=" + "Comments/" + (int)FeedbackType.ProvideFeedback + "/" + feedbackRequest.FeedbackRequestId + "/" + feedbackDetail.FeedbackDetailId + "&empId=" + feedbackRequester.EmployeeId).Replace("feedbackprovider", blobCdnUrl + Constants.FeedbackProvider)
                                                               .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                                .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                                .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);
                    if (feedbackRequest.FeedbackOnTypeId == 1)
                    {
                        body = body.Replace("OKR", okrName.ObjectiveName);
                        subject = subject.Replace("<feedback source>", okrName.ObjectiveName);
                        message = Constants.FeedbackRequestorMessage.Replace("<Provider>", feedbackProvider.FirstName).Replace("<OKR Name>", okrName.ObjectiveName);
                    }
                    else
                    {
                        var keyId = okrName.KeyDetails.FirstOrDefault(x => x.GoalKeyId == feedbackRequest.FeedbackOnId);
                        body = body.Replace("OKR", keyId.KeyDescription);
                        subject = subject.Replace("<feedback source>", keyId.KeyDescription);
                        message = Constants.FeedbackRequestorMessage.Replace("<Provider>", feedbackProvider.FirstName).Replace("<OKR Name>", keyId.KeyDescription);
                    }

                    MailRequest mailRequest = new MailRequest();
                    if (feedbackRequester.EmailId != null && template.Subject != "")
                    {
                        mailRequest.MailTo = feedbackRequester.EmailId;
                        mailRequest.Subject = subject;
                        mailRequest.Body = body;
                        await SentMailAsync(mailRequest, jwtToken);
                    }

                    ///Notification to feedback requestor

                    NotificationDetails notificationDetails = new NotificationDetails
                    {
                        JwtToken = jwtToken,
                        By = raisedForId,
                        To = feedbackRequestBy,
                        AppId = Constants.AppId,
                        MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                        NotificationType = (int)NotificationType.ProvideFeedback,
                        NotificationText = message,
                        Url = "Feedback / " + (int)FeedbackType.ProvideFeedback + " / " + feedbackDetail.FeedbackRequestId,
                        NotificationOnTypeId = feedbackRequest.FeedbackOnTypeId,
                        NotificationOnId = feedbackRequest.FeedbackOnId
                    };
                    await NotificationsAsync(notificationDetails);
                }
            }
        }

        public async Task InsertCommentNotificationsAndEmails(FeedbackRequest feedbackRequest, FeedbackDetail result, string getLastComment, long receiver, EmployeeResult userList, Comment comment, string jwtToken, UserIdentity loginUser)
        {
            var keyVault = await KeyVaultService.GetAzureBlobKeysAsync();
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";
            var feedbackUrl = settings?.FrontEndUrl;
            var loginUrl = feedbackUrl;
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;

            if (feedbackRequest.FeedbackOnTypeId == Constants.TeamTypeId)
            {
                var teamDetails = GetTeamEmployeeByTeamId(feedbackRequest.FeedbackOnId, jwtToken);
                if (teamDetails != null)
                {
                    //// Mail to team leader
                    var createdUser = userList.Results.FirstOrDefault(x => x.EmployeeId == Convert.ToInt64(teamDetails.OrganisationHead));
                    if (createdUser != null && receiver != Convert.ToInt64(teamDetails.OrganisationHead))
                    {
                        var provider = userList.Results.FirstOrDefault(x => x.EmployeeId == receiver);
                        if (!string.IsNullOrEmpty(feedbackUrl))
                        {
                            feedbackUrl = feedbackUrl + "?redirectUrl=unlock-me" + "&empId=" + teamDetails.OrganisationHead;
                        }
                        var createdEmployeeName = createdUser.FirstName;
                        var createdEmailId = createdUser.EmailId;
                        var template = await GetMailerTemplateAsync(TemplateCodes.FRC.ToString(), jwtToken);
                        var body = template.Body;
                        body = body.Replace("requestor", createdEmployeeName);
                        body = body.Replace("provider", provider.FirstName).Replace("topBar", blobCdnUrl + Constants.TopBar).Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("feedbackComment", blobCdnUrl + Constants.FeedbackCommentImage)
                        .Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("<URL>", feedbackUrl).Replace("<feedbackBody>", getLastComment).Replace("feedbackProvider", blobCdnUrl + Constants.FeedbackProvider).Replace("<supportEmailId>", Constants.SupportEmailId)
                        .Replace("<commentUrl>", loginUrl + "?redirectUrl=" + "Comments/" + (int)FeedbackType.ProvideFeedback + "/" + result.FeedbackRequestId + "/" + comment.FeedbackDetailId + "&empId=" + createdUser.EmployeeId)
                        .Replace("replyButton", blobCdnUrl + Constants.ReplyButtonImage)
                        .Replace("year", Convert.ToString(DateTime.Now.Year))
                        .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                        .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                        .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);
                        var subject = template.Subject;
                        subject = subject.Replace("<okrName>", teamDetails.OrganisationName ?? "");
                        body = body.Replace("<okrName>", teamDetails.OrganisationName ?? "");
                        var message = Constants.FeedbackCommentsMessage.Replace("<feedback source>", teamDetails.OrganisationName ?? "").Replace("<username>", provider.FirstName);


                        NotificationDetails notificationDetails = new NotificationDetails
                        {
                            JwtToken = jwtToken,
                            By = loginUser.EmployeeId,
                            To = createdUser.EmployeeId,
                            AppId = Constants.AppId,
                            MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                            NotificationType = (int)NotificationType.Comments,
                            NotificationText = message,
                            Url = "Comments/" + (int)FeedbackType.ProvideFeedback + "/" + result.FeedbackRequestId + "/" + comment.FeedbackDetailId,
                            NotificationOnTypeId = result.FeedbackOnTypeId,
                            NotificationOnId = result.FeedbackOnId
                        };

                        await NotificationsAsync(notificationDetails);

                        MailRequest mailRequest = new MailRequest
                        {
                            MailTo = createdEmailId,
                            Subject = subject,
                            Body = body
                        };
                        await SentMailAsync(mailRequest, jwtToken);
                    }

                    //// Mail to team members
                    foreach (var employees in teamDetails.TeamEmployees)
                    {
                        if (employees.EmployeeId != receiver && employees.EmployeeId != Convert.ToInt64(teamDetails.OrganisationHead))
                        {
                            var createdUsers = userList.Results.FirstOrDefault(x => x.EmployeeId == employees.EmployeeId);
                            var provider = userList.Results.FirstOrDefault(x => x.EmployeeId == receiver);
                            if (!string.IsNullOrEmpty(feedbackUrl))
                            {
                                feedbackUrl = feedbackUrl + "?redirectUrl=unlock-me" + "&empId=" + employees.EmployeeId;
                            }
                            var createdEmployeeName = createdUsers.FirstName;
                            var createdEmailId = createdUsers.EmailId;
                            var template = await GetMailerTemplateAsync(TemplateCodes.FRC.ToString(), jwtToken);
                            var body = template.Body;
                            body = body.Replace("requestor", createdEmployeeName);
                            body = body.Replace("provider", provider.FirstName).Replace("topBar", blobCdnUrl + Constants.TopBar).Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("feedbackComment", blobCdnUrl + Constants.FeedbackCommentImage)
                            .Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("<URL>", feedbackUrl).Replace("<feedbackBody>", getLastComment).Replace("feedbackProvider", blobCdnUrl + Constants.FeedbackProvider).Replace("<supportEmailId>", Constants.SupportEmailId)
                            .Replace("<commentUrl>", loginUrl + "?redirectUrl=" + "Comments/" + (int)FeedbackType.ProvideFeedback + "/" + result.FeedbackRequestId + "/" + comment.FeedbackDetailId + "&empId=" + createdUsers.EmployeeId)
                            .Replace("replyButton", blobCdnUrl + Constants.ReplyButtonImage)
                            .Replace("year", Convert.ToString(DateTime.Now.Year))
                            .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                            .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                            .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);
                            var subject = template.Subject;
                            subject = subject.Replace("<okrName>", teamDetails.OrganisationName ?? "");
                            body = body.Replace("<okrName>", teamDetails.OrganisationName ?? "");
                            var message = Constants.FeedbackCommentsMessage.Replace("<feedback source>", teamDetails.OrganisationName ?? "").Replace("<username>", provider.FirstName);


                            NotificationDetails notificationDetails = new NotificationDetails();

                            notificationDetails.JwtToken = jwtToken;
                            notificationDetails.By = loginUser.EmployeeId;
                            notificationDetails.To = createdUsers.EmployeeId;
                            notificationDetails.AppId = Constants.AppId;
                            notificationDetails.MessageType = (int)MessageTypeForNotifications.NotificationsMessages;
                            notificationDetails.NotificationType = (int)NotificationType.Comments;
                            notificationDetails.NotificationText = message;
                            notificationDetails.Url = "Comments/" + (int)FeedbackType.ProvideFeedback + "/" + result.FeedbackRequestId + "/" + comment.FeedbackDetailId;
                            notificationDetails.NotificationOnTypeId = result.FeedbackOnTypeId;
                            notificationDetails.NotificationOnId = result.FeedbackOnId;

                            await NotificationsAsync(notificationDetails);

                            MailRequest mailRequest = new MailRequest
                            {
                                MailTo = createdEmailId,
                                Subject = subject,
                                Body = body
                            };
                            await SentMailAsync(mailRequest, jwtToken);
                        }
                    }
                }
            }

            else
            {
                var feedbackRequester = feedbackRequest.FeedbackById;
                var feedbackProvider = feedbackRequest.RaisedForId;
                var provider = userList.Results.FirstOrDefault(x => x.EmployeeId == feedbackProvider);
                var requester = userList.Results.FirstOrDefault(x => x.EmployeeId == feedbackRequester);

                if (receiver == feedbackProvider && receiver > 0 && (provider != null))
                {
                    var createdUser = userList.Results.FirstOrDefault(x => x.EmployeeId == feedbackRequester);
                    if (createdUser != null)
                    {
                        if (!string.IsNullOrEmpty(feedbackUrl))
                        {
                            feedbackUrl = feedbackUrl + "?redirectUrl=unlock-me" + "&empId=" + createdUser.EmployeeId;
                        }
                        var createdEmployeeName = createdUser.FirstName;
                        var createdEmailId = createdUser.EmailId;

                        var template = await GetMailerTemplateAsync(TemplateCodes.FRC.ToString(), jwtToken);
                        var body = template.Body;
                        body = body.Replace("requestor", createdEmployeeName);
                        body = body.Replace("provider", provider.FirstName).Replace("topBar", blobCdnUrl + Constants.TopBar).Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("feedbackComment", blobCdnUrl + Constants.FeedbackCommentImage)
                            .Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("<URL>", feedbackUrl).Replace("<feedbackBody>", getLastComment).Replace("feedbackProvider", blobCdnUrl + Constants.FeedbackProvider).Replace("<supportEmailId>", Constants.SupportEmailId)
                             .Replace("<commentUrl>", loginUrl + "?redirectUrl=" + "Comments/" + (int)FeedbackType.ProvideFeedback + "/" + result.FeedbackRequestId + "/" + comment.FeedbackDetailId + "&empId=" + createdUser.EmployeeId)
                            .Replace("replyButton", blobCdnUrl + Constants.ReplyButtonImage)
                            .Replace("year", Convert.ToString(DateTime.Now.Year))
                            .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                            .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                            .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);
                        var subject = template.Subject;

                        var okrName = await MyGoalFeedBackResponse(feedbackRequest.FeedbackOnTypeId, feedbackRequest.FeedbackOnId, jwtToken);

                        var message = string.Empty;
                        if (feedbackRequest.FeedbackOnTypeId == 1)
                        {
                            subject = subject.Replace("<okrName>", okrName.ObjectiveName);
                            body = body.Replace("<okrName>", okrName.ObjectiveName);
                            message = Constants.FeedbackCommentsMessage.Replace("<feedback source>", okrName.ObjectiveName).Replace("<username>", provider.FirstName);
                        }
                        else if (feedbackRequest.FeedbackOnTypeId == 2)
                        {
                            var keyId = okrName.KeyDetails.FirstOrDefault(x => x.GoalKeyId == feedbackRequest.FeedbackOnId);
                            subject = subject.Replace("<okrName>", keyId.KeyDescription);
                            body = body.Replace("<okrName>", keyId.KeyDescription);
                            message = Constants.FeedbackCommentsMessage.Replace("<feedback source>", keyId.KeyDescription).Replace("<username>", provider.FirstName);
                        }

                        NotificationDetails notificationDetails = new NotificationDetails();

                        notificationDetails.JwtToken = jwtToken;
                        notificationDetails.By = loginUser.EmployeeId;
                        notificationDetails.To = createdUser.EmployeeId;
                        notificationDetails.AppId = Constants.AppId;
                        notificationDetails.MessageType = (int)MessageTypeForNotifications.NotificationsMessages;
                        notificationDetails.NotificationType = (int)NotificationType.Comments;
                        notificationDetails.NotificationText = message;
                        notificationDetails.Url = "Comments/" + (int)FeedbackType.ProvideFeedback + "/" + result.FeedbackRequestId + "/" + comment.FeedbackDetailId;
                        notificationDetails.NotificationOnTypeId = result.FeedbackOnTypeId;
                        notificationDetails.NotificationOnId = result.FeedbackOnId;

                        await NotificationsAsync(notificationDetails);

                        MailRequest mailRequest = new MailRequest
                        {
                            MailTo = createdEmailId,
                            Subject = subject,
                            Body = body
                        };
                        await SentMailAsync(mailRequest, jwtToken);
                    }
                }
                else
                {
                    var createdUser = userList.Results.FirstOrDefault(x => x.EmployeeId == feedbackProvider);
                    if (createdUser != null && requester != null)
                    {
                        if (!string.IsNullOrEmpty(feedbackUrl))
                        {
                            feedbackUrl = feedbackUrl + "?redirectUrl=unlock-me" + "&empId=" + createdUser.EmployeeId;
                        }
                        var createdEmployeeName = createdUser.FirstName;
                        var createdEmailId = createdUser.EmailId;

                        var template = await GetMailerTemplateAsync(TemplateCodes.FRC.ToString(), jwtToken);
                        var body = template.Body;
                        body = body.Replace("requestor", createdEmployeeName);
                        body = body.Replace("provider", requester.FirstName).Replace("topBar", blobCdnUrl + Constants.TopBar).Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("feedbackComment", blobCdnUrl + Constants.FeedbackCommentImage)
                            .Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("<URL>", feedbackUrl).Replace("<feedbackBody>", getLastComment).Replace("feedbackProvider", blobCdnUrl + Constants.FeedbackProvider).Replace("<supportEmailId>", Constants.SupportEmailId)
                             .Replace("<commentUrl>", loginUrl + "?redirectUrl=" + "Comments/" + (int)FeedbackType.ProvideFeedback + "/" + result.FeedbackRequestId + "/" + comment.FeedbackDetailId + "&empId=" + createdUser.EmployeeId)
                            .Replace("replyButton", blobCdnUrl + Constants.ReplyButtonImage)
                            .Replace("year", Convert.ToString(DateTime.Now.Year))
                            .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                            .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                            .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);
                        var subject = template.Subject;

                        var okrName = await MyGoalFeedBackResponse(feedbackRequest.FeedbackOnTypeId, feedbackRequest.FeedbackOnId, jwtToken);
                        var message = string.Empty;
                        if (feedbackRequest.FeedbackOnTypeId == 1)
                        {
                            subject = subject.Replace("<okrName>", okrName.ObjectiveName);
                            body = body.Replace("<okrName>", okrName.ObjectiveName);
                            message = Constants.FeedbackCommentsMessage.Replace("<feedback source>", okrName.ObjectiveName).Replace("<username>", requester.FirstName);
                        }
                        else if (feedbackRequest.FeedbackOnTypeId == 2)
                        {
                            var keyId = okrName.KeyDetails.FirstOrDefault(x => x.GoalKeyId == feedbackRequest.FeedbackOnId);
                            subject = subject.Replace("<okrName>", keyId.KeyDescription);
                            body = body.Replace("<okrName>", keyId.KeyDescription);
                            message = Constants.FeedbackCommentsMessage.Replace("<feedback source>", keyId.KeyDescription).Replace("<username>", requester.FirstName);
                        }

                        NotificationDetails notificationDetails = new NotificationDetails();

                        notificationDetails.By = loginUser.EmployeeId;
                        notificationDetails.To = feedbackProvider;
                        notificationDetails.AppId = Constants.AppId;
                        notificationDetails.MessageType = (int)MessageTypeForNotifications.NotificationsMessages;
                        notificationDetails.NotificationType = (int)NotificationType.Comments;
                        notificationDetails.JwtToken = jwtToken;
                        notificationDetails.NotificationText = message;
                        notificationDetails.Url = "Comments/" + (int)FeedbackType.ProvideFeedback + "/" + result.FeedbackRequestId + "/" + comment.FeedbackDetailId;
                        notificationDetails.NotificationOnTypeId = result.FeedbackOnTypeId;
                        notificationDetails.NotificationOnId = result.FeedbackOnId;

                        await NotificationsAsync(notificationDetails);

                        MailRequest mailRequest = new MailRequest
                        {
                            MailTo = createdEmailId,
                            Subject = subject,
                            Body = body
                        };
                        await SentMailAsync(mailRequest, jwtToken);
                    }
                }
            }
        }

        public async Task CreateOneToOneRequestNotificationsAndEmailsWhenRequestedForOkr(long OneToOneDetailId, OneToOneRequest request, long userId, UserResponse requestTo, UserResponse requestFrom, string emailId, long loginUserId, string jwtToken)
        {
            var okrName = await MyGoalFeedBackResponse(request.RequestType, request.RequestId, jwtToken);
            MailRequest mailRequest = new MailRequest();
            var keyVault = await KeyVaultService.GetAzureBlobKeysAsync();
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";
            var loginUrl = settings.FrontEndUrl;
            var loginUrlForRedirection = loginUrl;
            var notificationsUrl = string.Empty;
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            if (!string.IsNullOrEmpty(loginUrl))
            {
                loginUrl = loginUrl + "?redirectUrl=unlock-me" + "&empId=" + requestTo.EmployeeId;
            }

            var template = await GetMailerTemplateAsync(TemplateCodes.RF.ToString(), jwtToken);
            var body = template.Body.Replace("attendee", requestTo.FirstName).Replace("requestor", requestFrom.FirstName + " " + requestFrom.LastName)
                .Replace("topBar", blobCdnUrl + Constants.TopBar).Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("connect", blobCdnUrl + Constants.ConnectImage)
                        .Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("<URL>", loginUrl).Replace("<supportEmailId>", Constants.SupportEmailId)
                        .Replace("year", Convert.ToString(DateTime.Now.Year))
                        .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                        .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                        .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);

            var subject = template.Subject.Replace("<organizer>", requestFrom.FirstName).Replace("<attendee>", requestTo.FirstName);

            var message = string.Empty;
            if (okrName != null)
            {
                if (request.RequestType == 1)
                {
                    body = body.Replace("<OKR>", okrName.ObjectiveName).Replace("<distinguisher>", "OKR");
                    if (okrName.EmployeeId == loginUserId)
                    {
                        body = body.Replace("<goToGoalsUrl>", loginUrlForRedirection + "?redirectUrl=" + "Feedback/" + (int)FeedbackType.OneOnOne + "/" + okrName.GoalObjectiveId + "/" + 1 + "/" + requestFrom.EmployeeId + "&empId=" + requestTo.EmployeeId);
                        notificationsUrl = "Feedback/" + (int)FeedbackType.OneOnOne + "/" + okrName.GoalObjectiveId + "/" + 1 + "/" + requestFrom.EmployeeId;
                    }
                    else
                    {
                        body = body.Replace("<goToGoalsUrl>", loginUrlForRedirection + "?redirectUrl=" + "Feedback/" + (int)FeedbackType.OneOnOne + "/" + okrName.GoalObjectiveId + "/" + 1 + "/" + requestTo.EmployeeId + "&empId=" + requestTo.EmployeeId);
                        notificationsUrl = "Feedback/" + (int)FeedbackType.OneOnOne + "/" + okrName.GoalObjectiveId + "/" + 1 + "/" + requestTo.EmployeeId;
                    }
                    message = Constants.FeedbackOneOnOneMessage.Replace("<user>", requestFrom.FirstName).Replace("<OKR>", okrName.ObjectiveName).Replace("<goal>", "goal");
                }
                else
                {
                    var keyId = okrName.KeyDetails.FirstOrDefault(x => x.GoalKeyId == request.RequestId);
                    body = body.Replace("<OKR>", keyId.KeyDescription).Replace("<distinguisher>", "KR");
                    if (keyId.EmployeeId == loginUserId)
                    {
                        var url = okrName.GoalObjectiveId == 0 ? loginUrlForRedirection + "?redirectUrl=" + "Feedback/" + (int)FeedbackType.OneOnOne + "/" + keyId.GoalKeyId + "/" + 2 + "/" + requestFrom.EmployeeId + "&empId=" + requestTo.EmployeeId : loginUrlForRedirection + "?redirectUrl=" + "Feedback/" + (int)FeedbackType.OneOnOne + "/" + okrName.GoalObjectiveId + "/" + 2 + "/" + requestFrom.EmployeeId + "&empId=" + requestTo.EmployeeId;
                        body = body.Replace("<goToGoalsUrl>", url);
                        notificationsUrl = okrName.GoalObjectiveId == 0 ? "Feedback/" + (int)FeedbackType.OneOnOne + "/" + keyId.GoalKeyId + "/" + 2 + "/" + requestFrom.EmployeeId : "Feedback/" + (int)FeedbackType.OneOnOne + "/" + okrName.GoalObjectiveId + "/" + 2 + "/" + requestFrom.EmployeeId;
                    }
                    else
                    {
                        var url = okrName.GoalObjectiveId == 0 ? loginUrlForRedirection + "?redirectUrl=" + "Feedback/" + (int)FeedbackType.OneOnOne + "/" + keyId.GoalKeyId + "/" + 2 + "/" + requestTo.EmployeeId + "&empId=" + requestTo.EmployeeId : loginUrlForRedirection + "?redirectUrl=" + "Feedback/" + (int)FeedbackType.OneOnOne + "/" + okrName.GoalObjectiveId + "/" + 2 + "/" + requestTo.EmployeeId + "&empId=" + requestTo.EmployeeId;
                        body = body.Replace("<goToGoalsUrl>", url);
                        notificationsUrl = okrName.GoalObjectiveId == 0 ? "Feedback/" + (int)FeedbackType.OneOnOne + "/" + keyId.GoalKeyId + "/" + 2 + "/" + requestTo.EmployeeId : "Feedback/" + (int)FeedbackType.OneOnOne + "/" + okrName.GoalObjectiveId + "/" + 2 + "/" + requestTo.EmployeeId;
                    }
                    message = Constants.FeedbackOneOnOneMessage.Replace("<user>", requestFrom.FirstName).Replace("<OKR>", keyId.KeyDescription).Replace("<goal>", "key result");
                }
            }


            NotificationDetails notificationDetails = new NotificationDetails
            {
                By = request.RequestedFrom,
                AppId = Constants.AppId,
                To = userId,
                NotificationType = (int)NotificationType.RequestOnetoOne,
                MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                JwtToken = jwtToken,
                NotificationText = message,
                Url = notificationsUrl
            };

            await NotificationsAsync(notificationDetails);


            if (template.Subject != null && emailId != null && requestFrom.EmailId != null)
            {
                mailRequest.MailTo = emailId;
                mailRequest.Subject = subject;
                mailRequest.Body = body;
                mailRequest.CC = requestFrom.EmailId; //organizer email id
                await SentMailAsync(mailRequest, jwtToken);
            }
        }

        public async Task CreateOneToOneRequestNotificationsAndEmailsWhenRequestedForFeedback(long OneToOneDetailId, OneToOneRequest request, UserResponse requestTo, UserResponse requestFrom, string emailId, FeedbackRequest feedbackRequest, string jwtToken)
        {
            MailRequest mailRequest = new MailRequest();
            var keyVault = await KeyVaultService.GetAzureBlobKeysAsync();
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";
            var loginUrl = settings?.FrontEndUrl;
            var loginUrlForRedirection = loginUrl;
            var notificationsUrl = string.Empty;
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            if (!string.IsNullOrEmpty(loginUrl))
            {
                loginUrl = loginUrl + "?redirectUrl=unlock-me" + "&empId=" + requestTo.EmployeeId;
            }

            var template = await GetMailerTemplateAsync(TemplateCodes.RF.ToString(), jwtToken);
            var body = template.Body.Replace("attendee", requestTo.FirstName).Replace("requestor", requestFrom.FirstName + " " + requestFrom.LastName)
              .Replace("topBar", blobCdnUrl + Constants.TopBar).Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("connect", blobCdnUrl + Constants.ConnectImage)
              .Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("<URL>", loginUrl).Replace("<supportEmailId>", Constants.SupportEmailId)
              .Replace("year", Convert.ToString(DateTime.Now.Year))
              .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
              .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
              .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);

            var subject = template.Subject.Replace("<organizer>", requestFrom.FirstName).Replace("<attendee>", requestTo.FirstName);

            var okrName = await MyGoalFeedBackResponse(feedbackRequest.FeedbackOnTypeId, feedbackRequest.FeedbackOnId, jwtToken);
            var message = string.Empty;
            if (okrName != null)
            {
                if (feedbackRequest.FeedbackOnTypeId == 1)
                {
                    body = body.Replace("<OKR>", okrName.ObjectiveName).Replace("<distinguisher>", "OKR").Replace("<goToGoalsUrl>", loginUrlForRedirection + "?redirectUrl=" + "Feedback/" + (int)FeedbackType.OneOnOne + "/" + okrName.GoalObjectiveId + "/" + 1 + "/" + requestFrom.EmployeeId + "&empId=" + requestTo.EmployeeId);
                    message = Constants.FeedbackOneOnOneMessage.Replace("<user>", requestFrom.FirstName).Replace("<OKR>", okrName.ObjectiveName).Replace("<goal>", "goal");
                    notificationsUrl = "Feedback/" + (int)FeedbackType.OneOnOne + "/" + okrName.GoalObjectiveId + "/" + 1 + "/" + requestFrom.EmployeeId;
                }
                else
                {
                    var keyId = okrName.KeyDetails.FirstOrDefault(x => x.GoalKeyId == feedbackRequest.FeedbackOnId);
                    body = body.Replace("<OKR>", keyId.KeyDescription).Replace("<distinguisher>", "KR");
                    message = Constants.FeedbackOneOnOneMessage.Replace("<user>", requestFrom.FirstName).Replace("<OKR>", keyId.KeyDescription).Replace("<goal>", "key result");
                    var url = okrName.GoalObjectiveId == 0 ? loginUrlForRedirection + "?redirectUrl=" + "Feedback/" + (int)FeedbackType.OneOnOne + "/" + keyId.GoalKeyId + "/" + 2 + "/" + requestFrom.EmployeeId + "&empId=" + requestTo.EmployeeId : loginUrlForRedirection + "?redirectUrl=" + "Feedback/" + (int)FeedbackType.OneOnOne + "/" + okrName.GoalObjectiveId + "/" + 2 + "/" + requestFrom.EmployeeId + "&empId=" + requestTo.EmployeeId;
                    body = body.Replace("<goToGoalsUrl>", url);
                    notificationsUrl = okrName.GoalObjectiveId == 0 ? "Feedback/" + (int)FeedbackType.OneOnOne + "/" + keyId.GoalKeyId + "/" + 2 + "/" + requestFrom.EmployeeId : "Feedback/" + (int)FeedbackType.OneOnOne + "/" + okrName.GoalObjectiveId + "/" + 2 + "/" + requestFrom.EmployeeId;
                }
            }

            foreach (var item in request.RequestedTo)
            {
                NotificationDetails notificationDetails = new NotificationDetails
                {
                    By = request.RequestedFrom,
                    To = item,
                    AppId = Constants.AppId,
                    NotificationType = (int)NotificationType.RequestOnetoOne,
                    MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                    JwtToken = jwtToken,
                    NotificationText = message,
                    Url = notificationsUrl
                };
                await NotificationsAsync(notificationDetails);
            }

            if (template.Subject != null && emailId != null && requestFrom.EmailId != null)
            {
                mailRequest.MailTo = emailId;
                mailRequest.Subject = subject;
                mailRequest.Body = body;
                mailRequest.CC = requestFrom.EmailId; ///organizer email id
                await SentMailAsync(mailRequest, jwtToken);
            }
        }

        public async Task InsertAskPersonalNotificationsAndEmails(List<FeedbackRequest> feedbackRequestPersonal, AskPersonalFeedbackRequest askFeedbackRequest, string jwtToken)
        {
            var keyVault = await KeyVaultService.GetAzureBlobKeysAsync();
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";
            var loginUrl = settings?.FrontEndUrl;
            var feedbackUrl = loginUrl;
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            foreach (var user in feedbackRequestPersonal)
            {
                var feedbackProvider = await GetUser(jwtToken, user.RaisedForId);

                var feedbackRequester = await GetUser(jwtToken, askFeedbackRequest.FeedbackById);
                if (!string.IsNullOrEmpty(feedbackUrl))
                {
                    feedbackUrl = feedbackUrl + "?redirectUrl=unlock-me" + "&empId=" + feedbackProvider.EmployeeId;
                }


                if (feedbackProvider != null && feedbackRequester != null)
                {
                    var message = string.Empty;
                    var template = await GetMailerTemplateAsync(TemplateCodes.PAF.ToString(), jwtToken);
                    string body = template.Body;
                    string subject = template.Subject;
                    subject = subject.Replace("<requestor>", feedbackRequester.FirstName).Replace("on OKR", "");
                    body = body.Replace("user", feedbackProvider.FirstName).Replace("topBar", blobCdnUrl + Constants.TopBar)
                           .Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("<supportEmailId>", Constants.SupportEmailId)
                           .Replace("tick", blobCdnUrl + Constants.TickImages).Replace("shareFeedback", blobCdnUrl + Constants.ShareFeedbackImage)
                           .Replace("<URL>", feedbackUrl).Replace("<remark>", user.RequestRemark).Replace("feedbackAsk", blobCdnUrl + Constants.FeedbackProviderImage)
                           .Replace("Requestor", feedbackRequester.FirstName).Replace("OKRfocus", " ")
                           .Replace("<provideFeedbackUrl>", loginUrl + "?redirectUrl=" + "PersonalFeedback/" + (int)FeedbackType.AskFeedback + "/" + user.FeedbackRequestId + "&empId=" + feedbackProvider.EmployeeId).Replace("feedbackprovider", blobCdnUrl + Constants.FeedbackProvider)
                           .Replace("year", Convert.ToString(DateTime.Now.Year))
                            .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                            .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                            .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);

                    MailRequest mailRequest = new MailRequest();
                    if (feedbackProvider.EmailId != null && template.Subject != "")
                    {
                        mailRequest.MailTo = feedbackProvider.EmailId;
                        mailRequest.Subject = subject;
                        mailRequest.Body = body;
                        await SentMailAsync(mailRequest, jwtToken);
                    }

                    //Notification to feedback provider 
                    if (user.FeedbackOnTypeId == 3)
                    {
                        message = Constants.PersonalFeedbackProviderMessage.Replace("<Requestor>", feedbackRequester.FirstName);


                        NotificationDetails notificationDetails = new NotificationDetails
                        {
                            JwtToken = jwtToken,
                            To = user.RaisedForId,
                            By = askFeedbackRequest.FeedbackById,
                            AppId = Constants.AppId,
                            MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                            NotificationType = (int)NotificationType.AskFeedback,
                            Url = "PersonalFeedback / " + (int)FeedbackType.AskFeedback + " / " + user.FeedbackRequestId,
                            NotificationText = message
                        };

                        await NotificationsAsync(notificationDetails);
                    }
                }
            }
        }

        public async Task CreatePersonalizeOneOnOneRequestNotificationsAndEmails(List<OneToOneDetail> oneToOneDetails, PersonalFeedbackOneOnOneRequest request, string jwtToken)
        {

            MailRequest mailRequest = new MailRequest();
            var keyVault = await KeyVaultService.GetAzureBlobKeysAsync();
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";
            var loginUrl = settings?.FrontEndUrl;
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            foreach (var user in oneToOneDetails)
            {
                var requestTo = await GetUser(jwtToken, user.RequestedTo);
                var requestFrom = await GetUser(jwtToken, user.RequestedFrom);

                if (!string.IsNullOrEmpty(loginUrl))
                {
                    loginUrl = loginUrl + "?redirectUrl=unlock-me" + "&empId=" + requestTo.EmployeeId;
                }

                var template = await GetMailerTemplateAsync(TemplateCodes.PRF.ToString(), jwtToken);
                var body = template.Body.Replace("attendee", requestTo.FirstName).Replace("requestor", requestFrom.FirstName + " " + requestFrom.LastName).Replace("replyButton", blobCdnUrl + Constants.ReplyButtonImage)
                    .Replace("topBar", blobCdnUrl + Constants.TopBar).Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("connect", blobCdnUrl + Constants.ConnectImage)
                            .Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("<URL>", loginUrl).Replace("<supportEmailId>", Constants.SupportEmailId)
                            .Replace("<commentUrl>", loginUrl + "?redirectUrl=" + "PersonalFeedback/" + (int)FeedbackType.OneOnOne + "/" + "/" + 1 + "/" + requestFrom.EmployeeId + "&empId=" + requestTo.EmployeeId)
                            .Replace("year", Convert.ToString(DateTime.Now.Year))
                            .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                            .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                            .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);

                var subject = template.Subject.Replace("<organizer>", requestFrom.FirstName).Replace("<attendee>", requestTo.FirstName);

                var message = string.Empty;
                message = Constants.PersonalFeedbackOneOnOneMessage.Replace("<user>", requestFrom.FirstName);

                NotificationDetails notificationDetails = new NotificationDetails
                {
                    By = requestFrom.EmployeeId,
                    AppId = Constants.AppId,
                    To = requestTo.EmployeeId,
                    NotificationType = (int)NotificationType.RequestOnetoOne,
                    MessageType = (int)MessageTypeForNotifications.AlertMessages,
                    JwtToken = jwtToken,
                    NotificationText = message,
                    Url = "PersonalFeedback/" + (int)FeedbackType.OneOnOne + "/" + user.OneToOneDetailId
                };

                await NotificationsAsync(notificationDetails);


                if (template.Subject != null && requestTo.EmailId != null && requestFrom.EmailId != null)
                {
                    mailRequest.MailTo = requestTo.EmailId;
                    mailRequest.Subject = subject;
                    mailRequest.Body = body;
                    mailRequest.CC = requestFrom.EmailId; //organizer email id
                    await SentMailAsync(mailRequest, jwtToken);
                }
            }
        }

        public async Task ProvidePersonalizeFeedbackNotificationsAndEmails(FeedbackRequest feedbackRequest, FeedbackDetail feedbackDetail, PersonalFeedbackRequest request, long loginUserId, string jwtToken)
        {
            var feedbackRequestBy = feedbackRequest.FeedbackById;
            var raisedForId = feedbackRequest.RaisedForId;
            var keyVault = await KeyVaultService.GetAzureBlobKeysAsync();
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";
            var loginUrl = settings?.FrontEndUrl;
            var feedbackUrl = loginUrl;
            var feedbackRequesterUrl = loginUrl;
            var feedbackRequester = await GetUser(jwtToken, feedbackRequestBy); //the one who has requested
            var feedbackProvider = await GetUser(jwtToken, raisedForId);// with whom feedback is requested or to whom feedback is provided
            var loggedInUser = await GetUser(jwtToken, loginUserId);
            var okrName = await MyGoalFeedBackResponse(feedbackRequest.FeedbackOnTypeId, feedbackRequest.FeedbackOnId, jwtToken);
            var template = await GetMailerTemplateAsync(TemplateCodes.PPF.ToString(), jwtToken);
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            if (!string.IsNullOrEmpty(feedbackUrl))
            {
                feedbackUrl = feedbackUrl + "?redirectUrl=unlock-me" + "&empId=" + feedbackProvider.EmployeeId;
            }
            if (!string.IsNullOrEmpty(feedbackRequesterUrl))
            {
                feedbackRequesterUrl = feedbackRequesterUrl + "?redirectUrl=unlock-me" + "&empId=" + feedbackRequester.EmployeeId;
            }
            //Mail to Feedback Requestor
            if (feedbackRequester != null && feedbackProvider != null && okrName != null && loggedInUser != null)
            {
                var giveFeedbackDirectlyTemplate = await GetMailerTemplateAsync(TemplateCodes.PGF.ToString(), jwtToken);
                if (request.FeedbackRequestId == 0)
                {
                    var message = string.Empty;
                    string body = giveFeedbackDirectlyTemplate.Body;
                    string subject = giveFeedbackDirectlyTemplate.Subject;
                    subject = subject.Replace("<provider>", loggedInUser.FirstName).Replace("on <feedback source>", " ");
                    ////body = body.Replace("<requestor>", feedbackProvider.FirstName).Replace("<provider>", loggedInUser.FirstName + " " + loggedInUser.LastName);
                    body = body.Replace("Requestor", feedbackProvider.FirstName).Replace("Name", loggedInUser.FirstName)
                               .Replace("topBar", blobCdnUrl + Constants.TopBar)
                               .Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("OKR", " ")
                               .Replace("tick", blobCdnUrl + Constants.TickImages).Replace("<URL>", feedbackUrl).Replace("replyButton", blobCdnUrl + Constants.ReplyButtonImage).Replace("<supportEmailId>", Constants.SupportEmailId)
                               .Replace("feedbackprovider", blobCdnUrl + Constants.FeedbackProvider).Replace("<remark>", feedbackDetail.SharedRemark.TrimEnd()).Replace("feedbackResponse", blobCdnUrl + Constants.FeedbackCommentImage)
                                .Replace("<commentUrl>", loginUrl + "?redirectUrl=" + "PersonalFeedback/" + (int)FeedbackType.ProvideFeedback + "/" + feedbackDetail.FeedbackDetailId + "&empId=" + feedbackProvider.EmployeeId).Replace("feedbackprovider", blobCdnUrl + Constants.FeedbackProvider)
                                .Replace("year", Convert.ToString(DateTime.Now.Year))
                                .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                                .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                                .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);

                    message = Constants.PersonalizeFeedbackRequestorMessage.Replace("<Provider>", loggedInUser.FirstName);

                    MailRequest mailRequest = new MailRequest();
                    if (feedbackProvider.EmailId != null && giveFeedbackDirectlyTemplate.Subject != "")
                    {
                        mailRequest.MailTo = feedbackProvider.EmailId;
                        mailRequest.Subject = subject;
                        mailRequest.Body = body;
                        await SentMailAsync(mailRequest, jwtToken);
                    }

                    NotificationDetails notificationDetails = new NotificationDetails
                    {
                        JwtToken = jwtToken,
                        To = raisedForId,
                        By = loginUserId,
                        NotificationText = message,
                        AppId = Constants.AppId,
                        MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                        NotificationType = (int)NotificationType.AskFeedback,
                        Url = "PersonalFeedback / " + (int)FeedbackType.ProvideFeedback + " / " + feedbackDetail.FeedbackDetailId
                    };
                    await NotificationsAsync(notificationDetails);
                }

                else
                {
                    var message = string.Empty;
                    string body = template.Body;
                    string subject = template.Subject;
                    subject = subject.Replace("<provider>", loggedInUser.FirstName).Replace("on <feedback source>", " ");
                    body = body.Replace("Requestor", feedbackRequester.FirstName).Replace("Name", feedbackProvider.FirstName)
                               .Replace("topBar", blobCdnUrl + Constants.TopBar)
                               .Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("<supportEmailId>", Constants.SupportEmailId)
                               .Replace("tick", blobCdnUrl + Constants.TickImages).Replace("feedbackResponse", blobCdnUrl + Constants.FeedbackCommentImage)
                               .Replace("<URL>", feedbackRequesterUrl).Replace("<remark>", feedbackDetail.SharedRemark).Replace("replyButton", blobCdnUrl + Constants.ReplyButtonImage)
                               .Replace("feedbackprovider", blobCdnUrl + Constants.FeedbackProvider)
                                                               .Replace("<commentUrl>", loginUrl + "?redirectUrl=" + "PersonalFeedback/" + (int)FeedbackType.ProvideFeedback + "/" + feedbackDetail.FeedbackDetailId + "&empId=" + feedbackRequester.EmployeeId).Replace("feedbackprovider", blobCdnUrl + Constants.FeedbackProvider)
                                                               .Replace("year", Convert.ToString(DateTime.Now.Year))
                                                               .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                                .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                                .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);

                    message = Constants.PersonalizeFeedbackRequestorMessage.Replace("<Provider>", feedbackProvider.FirstName);

                    MailRequest mailRequest = new MailRequest();
                    if (feedbackRequester.EmailId != null && template.Subject != "")
                    {
                        mailRequest.MailTo = feedbackRequester.EmailId;
                        mailRequest.Subject = subject;
                        mailRequest.Body = body;
                        await SentMailAsync(mailRequest, jwtToken);
                    }

                    //Notification to feedback requester

                    NotificationDetails notificationDetails = new NotificationDetails
                    {
                        JwtToken = jwtToken,
                        By = raisedForId,
                        To = feedbackRequestBy,
                        AppId = Constants.AppId,
                        MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                        NotificationType = (int)NotificationType.AskFeedback,
                        NotificationText = message,
                        Url = "PersonalFeedback / " + (int)FeedbackType.ProvideFeedback + " / " + feedbackDetail.FeedbackDetailId
                    };
                    await NotificationsAsync(notificationDetails);
                }
            }
        }

        /// <summary>
        /// When requestor requested again for feedback
        /// </summary>
        /// <param name="feedbackRequestId"></param>
        /// <param name="jwtToken"></param>
        /// <param name="raisedForId"></param>
        /// <param name="feedbackById"></param>
        /// <param name="requestRemark"></param>
        /// <returns></returns>
        public async Task RequestAgainNotifications(long feedbackRequestId, string jwtToken, long raisedForId, long feedbackById, string requestRemark)
        {
            var keyVault = await KeyVaultService.GetAzureBlobKeysAsync();
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";
            var feedbackUrl = settings?.FrontEndUrl;
            var feedbackProvider = await GetUser(jwtToken, raisedForId);
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;

            var feedbackRequester = await GetUser(jwtToken, feedbackById);
            if (!string.IsNullOrEmpty(feedbackUrl))
            {
                feedbackUrl = feedbackUrl + "?redirectUrl=unlock-me" + "&empId=" + feedbackProvider.EmployeeId;
            }

            if (feedbackProvider != null && feedbackRequester != null)
            {
                var template = await GetMailerTemplateAsync(TemplateCodes.PAF.ToString(), jwtToken);
                string body = template.Body;
                string subject = template.Subject;
                subject = subject.Replace("<requestor>", feedbackRequester.FirstName);
                subject = subject.Replace("<requestor>", feedbackRequester.FirstName).Replace("on OKR", "");
                body = body.Replace("user", feedbackProvider.FirstName).Replace("topBar", blobCdnUrl + Constants.TopBar)
                       .Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("<supportEmailId>", Constants.SupportEmailId)
                       .Replace("tick", blobCdnUrl + Constants.TickImages).Replace("shareFeedback", blobCdnUrl + Constants.ShareFeedbackImage)
                       .Replace("<URL>", feedbackUrl).Replace("<remark>", requestRemark).Replace("feedbackAsk", blobCdnUrl + Constants.FeedbackProviderImage)
                       .Replace("Requestor", feedbackRequester.FirstName).Replace("OKRfocus", " ")
                       .Replace("<provideFeedbackUrl>", feedbackUrl + "?redirectUrl=" + "Feedback/" + (int)FeedbackType.AskFeedback + "/" + feedbackRequestId + "/" + feedbackById + "&empId=" + feedbackProvider.EmployeeId).Replace("feedbackprovider", blobCdnUrl + Constants.FeedbackProvider)
                       .Replace("year", Convert.ToString(DateTime.Now.Year))
                       .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                        .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                        .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);


                MailRequest mailRequest = new MailRequest();
                if (feedbackProvider.EmailId != null && template.Subject != "")
                {
                    mailRequest.MailTo = feedbackProvider.EmailId;
                    mailRequest.Subject = subject;
                    mailRequest.Body = body;
                    await SentMailAsync(mailRequest, jwtToken);
                }

                //Notification to feedback provider 

                var message = Constants.AskedRequestAgain.Replace("<Requestor>", feedbackRequester.FirstName);


                NotificationDetails notificationDetails = new NotificationDetails
                {
                    JwtToken = jwtToken,
                    To = raisedForId,
                    By = feedbackById,
                    AppId = Constants.AppId,
                    MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                    NotificationType = (int)NotificationType.AskFeedback,
                    Url = "PersonalFeedback / " + (int)FeedbackType.AskFeedback + " / " + feedbackRequestId,
                    NotificationText = message
                };

                await NotificationsAsync(notificationDetails);

            }
        }

        public async Task ApproveAndRejectRequestOnetoOne(long detailId, int status, long notificationsDetailId, long requestedTo, long requestedFrom, string jwtToken)
        {
            var provider = await GetUser(jwtToken, requestedTo);
            var requestor = await GetUser(jwtToken, requestedFrom);
            if (status == 0 || status == 1)
            {
                string requesterMessage;
                long notificationTypeId = 0;
                string message;
                if (status == 0)
                {
                    message = Constants.RejectRequestOnetoOne.Replace("<Requestor>", requestor.FirstName);
                    requesterMessage = Constants.RejectRequestOnetoOneMessageForRequestor.Replace("<Provider>", provider.FirstName);
                    notificationTypeId = Constants.RejectTypeId;
                }
                else
                {
                    message = Constants.ApprovedRequestOnetoOne.Replace("<Requestor>", requestor.FirstName);
                    requesterMessage = Constants.ApprovedRequestOnetoOneMessageForRequestor.Replace("<Provider>", provider.FirstName);
                    notificationTypeId = Constants.ApproveTypeId;
                }
                UpdateNotificationTextRequest updateNotificationTextRequest = new UpdateNotificationTextRequest
                {
                    NotificationsDetailsId = notificationsDetailId,
                    Text = message,
                    NotificationTypeId = notificationTypeId
                };
                await UpdateNotificationText(updateNotificationTextRequest, jwtToken);
                NotificationDetails notificationDetails = new NotificationDetails
                {
                    JwtToken = jwtToken,
                    To = requestedFrom,
                    By = requestedTo,
                    AppId = Constants.AppId,
                    MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                    NotificationType = notificationTypeId,
                    Url = "PersonalFeedback/" + (int)FeedbackType.OneOnOne + "/" + detailId,
                    NotificationText = requesterMessage
                };
                await NotificationsAsync(notificationDetails);

            }

        }

        public async Task InsertPersonalizeCommentNotificationsAndEmails(FeedbackRequest feedbackRequest, FeedbackDetail result, string getLastComment, long receiver, EmployeeResult userList, Comment comment, string jwtToken, UserIdentity loginUser)
        {
            var feedbackRequester = feedbackRequest.FeedbackById;
            var feedbackProvider = feedbackRequest.RaisedForId;
            var keyVault = await KeyVaultService.GetAzureBlobKeysAsync();
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";
            var feedbackUrl = settings?.FrontEndUrl;
            var loginUrl = feedbackUrl;
            var provider = userList.Results.FirstOrDefault(x => x.EmployeeId == feedbackProvider);
            var requester = userList.Results.FirstOrDefault(x => x.EmployeeId == feedbackRequester);
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;

            if (receiver == feedbackProvider && receiver > 0 && (provider != null))
            {
                var createdUser = userList.Results.FirstOrDefault(x => x.EmployeeId == feedbackRequester);
                if (createdUser != null)
                {
                    if (!string.IsNullOrEmpty(feedbackUrl))
                    {
                        feedbackUrl = feedbackUrl + "?redirectUrl=unlock-me" + "&empId=" + createdUser.EmployeeId;
                    }
                    var createdEmployeeName = createdUser.FirstName;
                    var createdEmailId = createdUser.EmailId;
                    var emailUrl = result.CreatedBy != createdUser.EmployeeId ? loginUrl + "?redirectUrl=" + "PersonalFeedback/" + (int)FeedbackType.ProvideFeedback + "/" + comment.FeedbackDetailId + "&empId=" + createdUser.EmployeeId : loginUrl + "?redirectUrl=" + "PersonalFeedback/" + (int)FeedbackType.Comment + "/" + comment.FeedbackDetailId + "&empId=" + createdUser.EmployeeId;


                    var template = await GetMailerTemplateAsync(TemplateCodes.PFRC.ToString(), jwtToken);
                    var body = template.Body;
                    body = body.Replace("requestor", createdEmployeeName);
                    body = body.Replace("provider", provider.FirstName).Replace("topBar", blobCdnUrl + Constants.TopBar).Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("feedbackComment", blobCdnUrl + Constants.FeedbackCommentImage)
                        .Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("<URL>", feedbackUrl).Replace("<feedbackBody>", getLastComment).Replace("feedbackProvider", blobCdnUrl + Constants.FeedbackProvider).Replace("<supportEmailId>", Constants.SupportEmailId)
                         .Replace("<commentUrl>", emailUrl)
                        .Replace("replyButton", blobCdnUrl + Constants.ReplyButtonImage)
                        .Replace("year", Convert.ToString(DateTime.Now.Year))
                        .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                        .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                        .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);
                    var subject = template.Subject;

                    subject = subject.Replace("<okrName>", "Personal Feedback");
                    var message = Constants.PersonalizeFeedbackCommentsMessage.Replace("<username>", provider.FirstName);


                    NotificationDetails notificationDetails = new NotificationDetails();

                    notificationDetails.JwtToken = jwtToken;
                    notificationDetails.By = loginUser.EmployeeId;
                    notificationDetails.To = createdUser.EmployeeId;
                    notificationDetails.AppId = Constants.AppId;
                    notificationDetails.MessageType = (int)MessageTypeForNotifications.NotificationsMessages;
                    notificationDetails.NotificationType = (int)NotificationType.Comments;
                    notificationDetails.NotificationText = message;
                    notificationDetails.Url = result.CreatedBy != createdUser.EmployeeId ? "PersonalFeedback/" + (int)FeedbackType.ProvideFeedback + "/" + comment.FeedbackDetailId : "PersonalFeedback/" + (int)FeedbackType.Comment + "/" + comment.FeedbackDetailId;
                    notificationDetails.NotificationOnTypeId = result.FeedbackOnTypeId;
                    notificationDetails.NotificationOnId = result.FeedbackOnId;

                    await NotificationsAsync(notificationDetails);

                    MailRequest mailRequest = new MailRequest
                    {
                        MailTo = createdEmailId,
                        Subject = subject,
                        Body = body
                    };
                    await SentMailAsync(mailRequest, jwtToken);
                }
            }
            else
            {
                var createdUser = userList.Results.FirstOrDefault(x => x.EmployeeId == feedbackProvider);
                if (createdUser != null && requester != null)
                {
                    var commentEmailUrl = result.CreatedBy != feedbackProvider ? loginUrl + "?redirectUrl=" + "PersonalFeedback/" + (int)FeedbackType.ProvideFeedback + "/" + comment.FeedbackDetailId + "&empId=" + createdUser.EmployeeId : loginUrl + "?redirectUrl=" + "PersonalFeedback/" + (int)FeedbackType.Comment + "/" + comment.FeedbackDetailId + "&empId=" + createdUser.EmployeeId;
                    if (!string.IsNullOrEmpty(feedbackUrl))
                    {
                        feedbackUrl = feedbackUrl + "?redirectUrl=unlock-me" + "&empId=" + createdUser.EmployeeId;
                    }
                    var createdEmployeeName = createdUser.FirstName;
                    var createdEmailId = createdUser.EmailId;

                    var template = await GetMailerTemplateAsync(TemplateCodes.PFRC.ToString(), jwtToken);
                    var body = template.Body;
                    body = body.Replace("requestor", createdEmployeeName);
                    body = body.Replace("provider", requester.FirstName).Replace("topBar", blobCdnUrl + Constants.TopBar).Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("feedbackComment", blobCdnUrl + Constants.FeedbackCommentImage)
                        .Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("<URL>", feedbackUrl).Replace("<feedbackBody>", getLastComment).Replace("feedbackProvider", blobCdnUrl + Constants.FeedbackProvider).Replace("<supportEmailId>", Constants.SupportEmailId)
                         .Replace("<commentUrl>", commentEmailUrl)
                        .Replace("replyButton", blobCdnUrl + Constants.ReplyButtonImage)
                        .Replace("year", Convert.ToString(DateTime.Now.Year))
                        .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                        .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                        .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);
                    var subject = template.Subject;

                    var message = string.Empty;

                    subject = subject.Replace("<okrName>", "Personal Feedback");
                    message = Constants.PersonalizeFeedbackCommentsMessage.Replace("<username>", requester.FirstName);

                    NotificationDetails notificationDetails = new NotificationDetails();

                    notificationDetails.By = loginUser.EmployeeId;
                    notificationDetails.To = feedbackProvider;
                    notificationDetails.AppId = Constants.AppId;
                    notificationDetails.MessageType = (int)MessageTypeForNotifications.NotificationsMessages;
                    notificationDetails.NotificationType = (int)NotificationType.Comments;
                    notificationDetails.JwtToken = jwtToken;
                    notificationDetails.NotificationText = message;
                    notificationDetails.Url = result.CreatedBy != feedbackProvider ? "PersonalFeedback/" + (int)FeedbackType.ProvideFeedback + "/" + comment.FeedbackDetailId : "PersonalFeedback/" + (int)FeedbackType.Comment + "/" + comment.FeedbackDetailId;
                    notificationDetails.NotificationOnTypeId = result.FeedbackOnTypeId;
                    notificationDetails.NotificationOnId = result.FeedbackOnId;

                    await NotificationsAsync(notificationDetails);

                    MailRequest mailRequest = new MailRequest
                    {
                        MailTo = createdEmailId,
                        Subject = subject,
                        Body = body
                    };
                    await SentMailAsync(mailRequest, jwtToken);
                }
            }
        }

        public async Task ProvideTeamFeedbackNotificationsAndEmails(long teamId, FeedbackRequest feedbackRequest, FeedbackDetail feedbackDetail, ProvideFeedbackRequest request, long loginUserId, string jwtToken)
        {
            var template = await GetMailerTemplateAsync(TemplateCodes.PF.ToString(), jwtToken);
            var teamDetails = GetTeamEmployeeByTeamId(teamId, jwtToken);

            var loggedInUser = await GetUser(jwtToken, loginUserId);

            var keyVault = await KeyVaultService.GetAzureBlobKeysAsync();
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";
            var feedbackUrl = settings?.FrontEndUrl;
            var loginUrl = feedbackUrl;
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;


            if (!string.IsNullOrEmpty(feedbackUrl))
            {
                feedbackUrl = feedbackUrl + "?redirectUrl=unlock-me" + "&empId=" + loginUserId;
            }

            if (teamDetails != null && template != null && loggedInUser != null)
            {
                if (request.FeedbackRequestId != 0)
                {
                    var userDetails = await GetUser(jwtToken, request.RaisedForId);
                    var message = Constants.FeedbackRequestorMessageForUser.Replace("<Provider>", loggedInUser.FirstName).Replace("<OKR Name>", teamDetails.OrganisationName);
                    string body = template.Body;
                    string subject = template.Subject;
                    subject = subject.Replace("<provider>", loggedInUser.FirstName).Replace("<feedback source>", teamDetails.OrganisationName);

                    body = body.Replace("Requestor", userDetails.FirstName).Replace("Name", loggedInUser.FirstName)
                        .Replace("topBar", blobCdnUrl + Constants.TopBar)
                        .Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("login", blobCdnUrl + Constants.LoginButtonImage)
                        .Replace("tick", blobCdnUrl + Constants.TickImages).Replace("<URL>", feedbackUrl).Replace("replyButton", blobCdnUrl + Constants.ReplyButtonImage).Replace("<supportEmailId>", Constants.SupportEmailId)
                        .Replace("feedbackprovider", blobCdnUrl + Constants.FeedbackProvider).Replace("<remark>", feedbackDetail.SharedRemark.TrimEnd()).Replace("feedbackResponse", blobCdnUrl + Constants.FeedbackCommentImage)
                        .Replace("<commentUrl>", loginUrl + "?redirectUrl=" + "Comments/" + (int)FeedbackType.ProvideFeedback + "/" + feedbackRequest.FeedbackRequestId + "/" + feedbackDetail.FeedbackDetailId + "&empId=" + userDetails.EmployeeId).Replace("feedbackprovider", blobCdnUrl + Constants.FeedbackProvider)
                        .Replace("year", Convert.ToString(DateTime.Now.Year)).Replace("OKR", teamDetails.OrganisationName)
                        .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                        .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                        .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);

                    MailRequest mailRequest = new MailRequest();
                    if (userDetails.EmailId != null && template.Subject != "")
                    {
                        mailRequest.MailTo = userDetails.EmailId;
                        mailRequest.Subject = subject;
                        mailRequest.Body = body;
                        await SentMailAsync(mailRequest, jwtToken);
                    }

                    NotificationDetails notificationDetails = new NotificationDetails
                    {
                        JwtToken = jwtToken,
                        To = userDetails.EmployeeId,
                        By = loginUserId,
                        NotificationText = message,
                        AppId = Constants.AppId,
                        MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                        NotificationType = (int)NotificationType.ProvideFeedback,
                        Url = "Feedback / " + (int)FeedbackType.ProvideFeedback + " / " + feedbackDetail.FeedbackRequestId,
                        NotificationOnTypeId = feedbackRequest.FeedbackOnTypeId,
                        NotificationOnId = feedbackRequest.FeedbackOnId
                    };

                    await NotificationsAsync(notificationDetails);
                }
                else
                {
                    ////Email and notification to team members
                    foreach (var item in teamDetails.TeamEmployees)
                    {
                        if (item.EmployeeId != loginUserId)
                        {
                            var userDetails = await GetUser(jwtToken, item.EmployeeId);
                            var message = Constants.FeedbackRequestorMessageForUser.Replace("<Provider>", loggedInUser.FirstName).Replace("<OKR Name>", teamDetails.OrganisationName);
                            string body = template.Body;
                            string subject = template.Subject;
                            subject = subject.Replace("<provider>", loggedInUser.FirstName).Replace("<feedback source>", teamDetails.OrganisationName);

                            body = body.Replace("Requestor", item.FirstName).Replace("Name", loggedInUser.FirstName)
                                .Replace("topBar", blobCdnUrl + Constants.TopBar)
                                .Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("login", blobCdnUrl + Constants.LoginButtonImage)
                                .Replace("tick", blobCdnUrl + Constants.TickImages).Replace("<URL>", feedbackUrl).Replace("replyButton", blobCdnUrl + Constants.ReplyButtonImage).Replace("<supportEmailId>", Constants.SupportEmailId)
                                .Replace("feedbackprovider", blobCdnUrl + Constants.FeedbackProvider).Replace("<remark>", feedbackDetail.SharedRemark.TrimEnd()).Replace("feedbackResponse", blobCdnUrl + Constants.FeedbackCommentImage)
                                .Replace("<commentUrl>", loginUrl + "?redirectUrl=" + "Comments/" + (int)FeedbackType.ProvideFeedback + "/" + feedbackRequest.FeedbackRequestId + "/" + feedbackDetail.FeedbackDetailId + "&empId=" + item.EmployeeId).Replace("feedbackprovider", blobCdnUrl + Constants.FeedbackProvider)
                                .Replace("year", Convert.ToString(DateTime.Now.Year)).Replace("OKR", teamDetails.OrganisationName)
                                .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                                .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                                .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);

                            MailRequest mailRequest = new MailRequest();
                            if (userDetails.EmailId != null && template.Subject != "")
                            {
                                mailRequest.MailTo = userDetails.EmailId;
                                mailRequest.Subject = subject;
                                mailRequest.Body = body;
                                await SentMailAsync(mailRequest, jwtToken);
                            }

                            NotificationDetails notificationDetails = new NotificationDetails
                            {
                                JwtToken = jwtToken,
                                To = item.EmployeeId,
                                By = loginUserId,
                                NotificationText = message,
                                AppId = Constants.AppId,
                                MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                                NotificationType = (int)NotificationType.ProvideFeedback,
                                Url = "Feedback / " + (int)FeedbackType.ProvideFeedback + " / " + feedbackDetail.FeedbackRequestId,
                                NotificationOnTypeId = feedbackRequest.FeedbackOnTypeId,
                                NotificationOnId = feedbackRequest.FeedbackOnId
                            };

                            await NotificationsAsync(notificationDetails);
                        }
                    }
                }
            }
        }
        public async Task ProvideTeamOkrFeedbackNotificationsAndEmails(long teamId, FeedbackRequest feedbackRequest, FeedbackDetail feedbackDetail, ProvideFeedbackRequest request, long loginUserId, string jwtToken)
        {
            var template = await GetMailerTemplateAsync(TemplateCodes.PF.ToString(), jwtToken);
            var loggedInUser = await GetUser(jwtToken, loginUserId);
            var keyVault = await KeyVaultService.GetAzureBlobKeysAsync();
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";
            var feedbackUrl = settings?.FrontEndUrl;
            var loginUrl = feedbackUrl;
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;

            var okrName = await MyGoalFeedBackResponse(feedbackRequest.FeedbackOnTypeId, feedbackRequest.FeedbackOnId, jwtToken);
            var userDetails = await GetUser(jwtToken, request.RaisedForId);

            if (!string.IsNullOrEmpty(feedbackUrl))
            {
                feedbackUrl = feedbackUrl + "?redirectUrl=unlock-me" + "&empId=" + userDetails.EmployeeId;
            }
            if (userDetails != null && template != null && loggedInUser != null && okrName != null)
            {
                var message = string.Empty;
                string body = template.Body;
                string subject = template.Subject;
                subject = subject.Replace("<provider>", loggedInUser.FirstName);

                body = body.Replace("Requestor", userDetails.FirstName).Replace("Name", loggedInUser.FirstName).Replace("topBar", blobCdnUrl + Constants.TopBar).Replace("logo", blobCdnUrl + Constants.LogoImages)
                    .Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("tick", blobCdnUrl + Constants.TickImages).Replace("<URL>", feedbackUrl).Replace("replyButton", blobCdnUrl + Constants.ReplyButtonImage)
                    .Replace("<supportEmailId>", Constants.SupportEmailId).Replace("feedbackprovider", blobCdnUrl + Constants.FeedbackProvider).Replace("<remark>", feedbackDetail.SharedRemark.TrimEnd()).Replace("feedbackResponse", blobCdnUrl + Constants.FeedbackCommentImage)
                    .Replace("<commentUrl>", loginUrl + "?redirectUrl=" + "Feedback/" + (int)FeedbackType.ProvideFeedback + "/" + feedbackRequest.FeedbackRequestId + "&empId=" + userDetails.EmployeeId).Replace("feedbackprovider", blobCdnUrl + Constants.FeedbackProvider)
                    .Replace("year", Convert.ToString(DateTime.Now.Year))
                    .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                    .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                    .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);

                if (feedbackRequest.FeedbackOnTypeId == 1)
                {
                    body = body.Replace("OKR", okrName.ObjectiveName);
                    subject = subject.Replace("<feedback source>", okrName.ObjectiveName);
                    message = Constants.FeedbackRequestorMessageForUser.Replace("<Provider>", loggedInUser.FirstName).Replace("<OKR Name>", okrName.ObjectiveName);
                }
                else
                {
                    var keyId = okrName.KeyDetails.FirstOrDefault(x => x.GoalKeyId == feedbackRequest.FeedbackOnId);
                    body = body.Replace("OKR", keyId.KeyDescription);
                    subject = subject.Replace("<feedback source>", keyId.KeyDescription);
                    message = Constants.FeedbackRequestorMessageForUser.Replace("<Provider>", loggedInUser.FirstName).Replace("<OKR Name>", keyId.KeyDescription);
                }

                MailRequest mailRequest = new MailRequest();
                if (userDetails.EmailId != null && template.Subject != "")
                {
                    mailRequest.MailTo = userDetails.EmailId;
                    mailRequest.Subject = subject;
                    mailRequest.Body = body;
                    await SentMailAsync(mailRequest, jwtToken);
                }

                NotificationDetails notificationDetails = new NotificationDetails
                {
                    JwtToken = jwtToken,
                    To = userDetails.EmployeeId,
                    By = loginUserId,
                    NotificationText = message,
                    AppId = Constants.AppId,
                    MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                    NotificationType = (int)NotificationType.ProvideFeedback,
                    Url = "Feedback / " + (int)FeedbackType.ProvideFeedback + " / " + feedbackDetail.FeedbackRequestId,
                    NotificationOnTypeId = feedbackRequest.FeedbackOnTypeId,
                    NotificationOnId = feedbackRequest.FeedbackOnId
                };

                await NotificationsAsync(notificationDetails);
            }
        }
    }
}












