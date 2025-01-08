using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace IdeaApp.Services;

public class EmailSender : IEmailSender
{
    public readonly ILogger _logger;
    
    public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor, 
                        ILogger<EmailSender> logger)
    {
        Options = optionsAccessor.Value;
        _logger = logger;
    }

    //get options for message sender
    public AuthMessageSenderOptions Options {get;}

    //send email
    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        //if grid key is empty or invalid throw exception else send email
        if (string.IsNullOrEmpty(Options.SendGridKey))
        {
            _logger.LogError("SendGrid API key is missing");
            throw new Exception("Null SendGridKey");
        }
        await Execute(Options.SendGridKey, subject, message, toEmail);
    }

    //what the email is supposed to look like
    public async Task Execute(string apiKey, string subject, string message, string toEmail)
    {
        //retrieve apikey from environment variables
        apiKey = Environment.GetEnvironmentVariable("SendGridAPIKey");

        _logger.LogInformation($"SendGrid API Key: {apiKey}");

        //check if api key is missing
        if(string.IsNullOrEmpty(apiKey))
        {
            _logger.LogError("SendGrid API key is missing");
            throw new Exception("SendGrid API key is missing");
        }

        //sendgridclient allows you to communicate w/the sendgrid api
        var client = new SendGridClient(apiKey);

        //creating the email
        var msg = new SendGridMessage()
        {
            From = new EmailAddress("mark.mathenge@adept-techno.com", "IdeaHub Confirmation"),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };

        //recipient of email
        msg.AddTo(new EmailAddress(toEmail));

        //Disable click tracking
        msg.SetClickTracking(false, false);

        try
        {
            //send email
            var response = await client.SendEmailAsync(msg);

            //log whether sending was successful or not
            _logger.LogInformation(
                $"SendGridAPIKey: {apiKey}",
                response.IsSuccessStatusCode
                ? $"Email to {toEmail} queued successfully!"
                : $"Failure Email to {toEmail}. Status Code: {response.StatusCode}, Response Body: {await response.Body.ReadAsStringAsync()}"
            );
        }
        catch (Exception e)
        {
            _logger.LogError($"An error occurred while sending email to {toEmail}");
        }
        
    }
}
