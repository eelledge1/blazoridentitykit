namespace APIs.Interfaces
{
    public interface IEmailTSender
    {
        Task SendEmailAsync(string to, string subject, string htmlMessage);
    }
}
