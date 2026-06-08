using System;
using System.Net;
using MailKit.Security;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace Rsdo.Concordancer.Api.Framework;

public static class SerilogExtensions
{
    public static LoggerConfiguration RsdoEmail(
        this LoggerSinkConfiguration sinkConfiguration,
        string fromEmail,
        string toEmail,
        string mailServer,
        int port,
        bool enableSsl,
        string userName,
        string password,
        string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}",
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Warning,
        int batchPostingLimit = 100,
        string mailSubject = "Log Email")
    {
        NetworkCredential credentials = null;
        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
        {
            credentials = new NetworkCredential()
            {
                UserName = userName,
                Password = password,
            };
        }

        return sinkConfiguration.Email(
            from: fromEmail,
            to: toEmail,
            host: mailServer,
            port: port,
            connectionSecurity: enableSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None,
            credentials: credentials,
            subject: mailSubject,
            body: outputTemplate,
            restrictedToMinimumLevel: restrictedToMinimumLevel);
    }
}