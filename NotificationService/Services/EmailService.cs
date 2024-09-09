using FluentEmail.Core;

namespace NotificationService.Services
{
    public class EmailService : IEmailService
    {
        private readonly IFluentEmail _fluentEmail;

        public EmailService(IFluentEmail fluentEmail)
        {
            _fluentEmail = fluentEmail ?? throw new ArgumentNullException(nameof(fluentEmail));
        }

        public async Task Send(EmailMetadata emailMetadata)
        {
            var email = _fluentEmail
                .To(emailMetadata.ToAddress)
                .Subject(emailMetadata.Subject)
                .Body(emailMetadata.Body);

            await email.SendAsync();
        }
    }
}
