using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;

namespace LittleArkFoundation.Data
{
    public class EmailService
    {
        public async Task<bool> SendEmailAsync(string recipientEmail, string subject, string body)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(recipientEmail))
                {
                    throw new ArgumentException("Recipient email is required.");
                }

                string senderEmail = "stuofficialschool@gmail.com";
                string senderPassword = "hibz zouo afaw nwzl";  // TODO: Store this in secrets/vault. Never store this in code.

                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtpClient.EnableSsl = true;

                    using (MailMessage mail = new MailMessage(senderEmail, recipientEmail, subject, body))
                    {
                        mail.IsBodyHtml = false;
                        await smtpClient.SendMailAsync(mail);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }

        public static string GetSecureNumericVerificationCode(int length)
        {
            if (length <= 0) throw new ArgumentException("Length must be greater than 0");

            const string digits = "0123456789"; // Digits only (0-9)
            var codeBuilder = new StringBuilder(length);
            var randomBytes = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            for (int i = 0; i < length; i++)
            {
                int index = randomBytes[i] % digits.Length; // Select a random digit
                codeBuilder.Append(digits[index]);
            }

            return codeBuilder.ToString(); // Return as a string
        }

        public static string GetSecureAlphanumericCode(int length)
        {
            char[] Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

            if (length <= 0) throw new ArgumentException("Length must be greater than 0");

            var codeBuilder = new StringBuilder(length);
            var randomBytes = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            for (int i = 0; i < length; i++)
            {
                int index = randomBytes[i] % Base32Chars.Length; // Get a random character
                codeBuilder.Append(Base32Chars[index]);
            }

            return codeBuilder.ToString();
        }
    }
}
