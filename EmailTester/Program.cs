using System;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace EmailTester
{
    class Program
    {
        private static MailAddress _recipient;
        private static MailAddress _sender;
        private static EmailOptions _options;

        static void Main(string[] args)
        {
            Console.WriteLine();

            if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                Console.WriteLine("Usage: EmailTester [recipient]");
                return;
            }

            try
            {
                _recipient = new MailAddress(args[0]);
            }
            catch (FormatException ex)
            {
                Console.WriteLine("Recipient email is invalid:");
                PrintException(ex);
                return;
            }

            _options = new EmailOptions();

            try
            {
                var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                config.Bind("EmailOptions", _options);
            }
            catch (Exception ex)
            {
                Console.WriteLine("There is a configuration error.");
                PrintException(ex);
                return;
            }

            if (_options == null || string.IsNullOrEmpty(_options.SenderEmail) ||
                string.IsNullOrEmpty(_options.SmtpHost) || _options.SmtpPort == 0)
            {
                Console.WriteLine("There is a configuration error.");
                return;
            }

            try
            {
                _sender = new MailAddress(_options.SenderEmail);
            }
            catch (FormatException ex)
            {
                Console.WriteLine("The configured sender email is invalid:");
                PrintException(ex);
                return;
            }

            Console.WriteLine("Attempting to send email...");

            if (SendEmail(false))
            {
                Console.WriteLine();
                Console.WriteLine("Email successfully sent.");
            }

            Console.WriteLine();
            Console.WriteLine("Attempting to send SSL email...");

            if (SendEmail(true))
            {
                Console.WriteLine();
                Console.WriteLine("SSL email successfully sent.");
            }
        }

        private static bool SendEmail(bool enableSsl)
        {
            var subject = $"Email test from {Environment.MachineName}";
            var body = $"This is a test email sent from {Environment.MachineName}{(enableSsl ? " (SSL enabled)" : null)}";

            try
            {
                using var message = new MailMessage(_sender, _recipient)
                {
                    Subject = subject,
                    Body = body,
                    SubjectEncoding = Encoding.UTF8,
                    BodyEncoding = Encoding.UTF8
                };

                using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort) { EnableSsl = enableSsl };
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error sending email:");
                PrintException(ex);
                return false;
            }

            return true;
        }

        private static void PrintException(Exception ex)
        {
            Console.WriteLine(ex.Message);
            if (ex.InnerException != null) PrintException(ex.InnerException);
        }
    }

    internal class EmailOptions
    {
        public string SenderEmail { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
    }
}
