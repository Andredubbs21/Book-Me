namespace NotificationService.Services
{
    public interface IEmailService
    {
        Task Send(EmailMetadata emailMetadata);
    }
}