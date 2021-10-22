using Amazon;
using Amazon.Runtime.CredentialManagement;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace GemTD
{
    public class Helpers
    {
        public static string GetRDSConnectionString()
        {
            string hostname = "bold-data.cyxwx6wqtiak.us-east-1.rds.amazonaws.com";
            string dbname = "gem_td";
            string username = "thrasos";
            string password = "Pow3rt3ch";
            return $"Server={hostname};Port=9006;Database={dbname};User ID={username};Password={password};";
        }

        public static async Task<Boolean> SendFeedbackSTMP(string subject, string desc, string logfile, string username, string email, string name, string userId)
        {
            // The email body for recipients with non-HTML email clients.
            string textBody = $"{desc} \n\n <br>" +
                $"<br><br>" +
                $"LogFile: <br> " +
                $"{ logfile }";

            // The HTML body of the email.
            string htmlBody = $@"<html>
                <head></head>
                <body>
                  <h1>Game Feedback/Bug Report</h1>
                    <h3>{subject}</h3>
                    <p>Submitted by:<br>
                    UserId: {userId}<br>
                    Username: {username}<br>
                    Name: {name}<br>
                    Email: {email}<br>
                  <p>{textBody}</p>
                </body>
                </html>";
            // Replace sender@example.com with your "From" address. 
            // This address must be verified with Amazon SES.
            String FROM = "thrasosoftfeedback@gmail.com";
            String FROMNAME = "Feedback Agent";

            // Replace recipient@example.com with a "To" address. If your account 
            // is still in the sandbox, this address must be verified.
            String TO = "thrasosoftbugreports@gmail.com";

            // Replace smtp_username with your Amazon SES SMTP user name.
            String SMTP_USERNAME = "AKIAVDQDWGS7QS4AZAXI";

            // Replace smtp_password with your Amazon SES SMTP password.
            String SMTP_PASSWORD = "BA/8BeoWNelJIpAoA/tLRMTW2M1iWK7W6tr7a0EZeEly";

            // (Optional) the name of a configuration set to use for this message.
            // If you comment out this line, you also need to remove or comment out
            // the "X-SES-CONFIGURATION-SET" header below.
            String CONFIGSET = "ConfigSet";

            // If you're using Amazon SES in a region other than US West (Oregon), 
            // replace email-smtp.us-west-2.amazonaws.com with the Amazon SES SMTP  
            // endpoint in the appropriate AWS Region.
            String HOST = "email-smtp.us-east-1.amazonaws.com";

            // The port you will connect to on the Amazon SES SMTP endpoint. We
            // are choosing port 587 because we will use STARTTLS to encrypt
            // the connection.
            int PORT = 587;

            // The subject line of the email
            String SUBJECT = subject;

            // The body of the email
            String BODY = htmlBody;

            // Create and build a new MailMessage object
            MailMessage message = new MailMessage();
            message.IsBodyHtml = true;
            message.From = new MailAddress(FROM, FROMNAME);
            message.To.Add(new MailAddress(TO));
            message.Subject = SUBJECT;
            message.Body = BODY;
            // Comment or delete the next line if you are not using a configuration set
            message.Headers.Add("X-SES-CONFIGURATION-SET", CONFIGSET);

            using (var client = new System.Net.Mail.SmtpClient(HOST, PORT))
            {
                // Pass SMTP credentials
                client.Credentials =
                    new NetworkCredential(SMTP_USERNAME, SMTP_PASSWORD);

                // Enable SSL encryption
                client.EnableSsl = true;

                // Try to send the message. Show status in console.
                try
                {
                    Console.WriteLine("Attempting to send email...");
                    client.Send(message);
                    Console.WriteLine("Email sent!");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("The email was not sent.");
                    Console.WriteLine("Error message: " + ex.Message);
                    return false;
                }

            }
        }

        public static async Task<SendEmailResponse> SendBugReportEmail(string subject, string desc, string logfile, string username, string email, string name,string userId)
        {
      
            // Replace sender@example.com with your "From" address.
            // This address must be verified with Amazon SES.
            string senderAddress = "thrasosoftfeedback@gmail.com";

            // Replace recipient@example.com with a "To" address. If your account
            // is still in the sandbox, this address must be verified.
            string receiverAddress = "thrasosoftbugreports@gmail.com";

            // The configuration set to use for this email. If you do not want to use a
            // configuration set, comment out the following property and the
            // ConfigurationSetName = configSet argument below. 
            //string configSet = "ConfigSet";

            // The subject line for the email.
            //string subject = subject;

            // The email body for recipients with non-HTML email clients.
            string textBody = $"{desc} \n\n\n\n" +
                $"\n" +
                $"LogFile: \n" +
                $"{ logfile }";

            // The HTML body of the email.
            string htmlBody = $@"<html>
                <head></head>
                <body>
                  <h1>Game Feedback/Bug Report</h1>
                    <h3>${subject}</h3>
                    <p>Submitted by:</br>
                    UserId: ${userId}</br>
                    Username: ${username}</br>
                    Name: ${name}</br>
                    Email: ${email}</br>
                  <p>
                </body>
                </html>";

            // Replace USWest2 with the AWS Region you're using for Amazon SES.
            // Acceptable values are EUWest1, USEast1, and USWest2.
            using (var client = new AmazonSimpleEmailServiceClient(RegionEndpoint.USEast1))
            {
                var sendRequest = new SendEmailRequest
                {
                    Source = senderAddress,
                    Destination = new Destination
                    {
                        ToAddresses =
                        new List<string> { receiverAddress }
                    },
                    Message = new Message
                    {
                        Subject = new Content(subject),
                        Body = new Body
                        {
                            Html = new Content
                            {
                                Charset = "UTF-8",
                                Data = htmlBody
                            },
                            Text = new Content
                            {
                                Charset = "UTF-8",
                                Data = textBody
                            }
                        }
                    },
                    // If you are not using a configuration set, comment
                    // or remove the following line 
                    //ConfigurationSetName = configSet
                };
                try
                {
                    Console.WriteLine("Sending email using Amazon SES...");
                    var response = await client.SendEmailAsync(sendRequest);
                    Console.WriteLine("The email was sent successfully.");
                    return response;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("The email was not sent.");
                    Console.WriteLine("Error message: " + ex.Message);
                    throw ex;
                }
            }
        }
    }
}