﻿namespace NetCoreIdentity.EmailServices
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email,string subject,string htmlMessage);
    }
}
