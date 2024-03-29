using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace send_email_using_razor_engine
{
    public class EmailService : IEmailService
    {
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        public EmailService(
            IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider
        )
        {
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task SendMail(string toEmail, string subject, string body)
        {
            SmtpClient client = new SmtpClient("smtp-relay.brevo.com", 587);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("username", "password");

            // Create email message
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("fromMail@gmail.com");
            mailMessage.To.Add(toEmail);
            mailMessage.Subject = subject;
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = body;

            // Send email
            await client.SendMailAsync(mailMessage);
        }

        public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            Console.WriteLine($"View name {viewName}");
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            using var sw = new StringWriter();
            var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);

            if (viewResult.View == null)
            {
                var searchedLocations = string.Join(", ", viewResult.SearchedLocations);
                throw new ArgumentNullException($"{viewName} does not match any available view. Searched the following locations: {searchedLocations}");
            }

            var viewDictionary = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }

    public interface IEmailService
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
        Task SendMail(string toEmail, string subject, string body);
    }
}