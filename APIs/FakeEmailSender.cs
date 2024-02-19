using APIs.Interfaces;

namespace APIs
{
    public class FakeEmailSender : IEmailTSender
    {
        public Task SendEmailAsync(string to, string subject, string htmlMessage)
        {
            // Log the email content to the console or a file instead of sending it
            Console.WriteLine($"Sending email to: {to}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: {htmlMessage}");

            // Simulate email sending delay
            return Task.CompletedTask;
        }
    }
}
