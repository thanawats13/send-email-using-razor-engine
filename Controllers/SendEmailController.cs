using Microsoft.AspNetCore.Mvc;
using send_email_using_razor_engine.DTO;

namespace send_email_using_razor_engine.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SendEmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public SendEmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            MyClass data = new MyClass
            {
                Name = "Thanawat",
                Title = "I'm from Thailand",
                Content = "I love my job. T^T"
            };

            string body = await _emailService.RenderViewToStringAsync("../EmailTemplates/Template-one", data);
            await _emailService.SendMail("toMail@mail.com", "Hello Razor", body);

            return Ok(body);
        }
    }
}