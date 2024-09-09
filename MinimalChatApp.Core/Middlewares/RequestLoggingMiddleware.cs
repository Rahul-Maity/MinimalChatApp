using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using MinimalChatApp.DomainModel.Data;
using MinimalChatApp.DomainModel.Models;

namespace MinimalChatApp.Core.Middlewares;
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _next = next;
        _serviceScopeFactory = serviceScopeFactory;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Request Body
            context.Request.EnableBuffering();
            string requestBody = null;
            if (context.Request.ContentLength > 0)
            {
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }
            }

            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var timeOfCall = DateTime.UtcNow;

            var userName = GetUsernameFromJwtToken(context);

            _logger.LogInformation("Request Info: IP={IP}, Time={Time}, Username={Username}, Body={Body}",
                ipAddress, timeOfCall, userName, requestBody);

            try
            {
                var logEntry = new ApiLog
                {
                    IP = ipAddress,
                    TimeOfCall = timeOfCall.ToUniversalTime(),
                    RequestBody = requestBody,
                    Username = userName
                };

                
                dbContext.ApiLogs.Add(logEntry);

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex) { _logger.LogError(ex, "An error occurred while saving request log to the database"); }
        }

        await _next(context);



      
    }

    private string GetUsernameFromJwtToken(HttpContext context)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
                return "Anonymous";

            var tokenString = authHeader.ToString().Replace("Bearer ", "");

            var principal = tokenHandler.ValidateToken(tokenString, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Hgc13iLjgc13iLpI2d2yCpY3rqXtRyIFn2jjuVg6pI2d2yCpY3rqXtRyIjuVg6Fn2")), // Replace with your actual secret key
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            var claimsPrincipal = (ClaimsPrincipal)principal;

            var name = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
            return name;


        }
        catch { return "Anonymous"; }
    }
}
