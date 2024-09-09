using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using MinimalChatApp.DomainModel.Data;
using MinimalChatApp.DomainModel.Dtos.Incoming;
using MinimalChatApp.Repository.ApiLogs;

namespace MinimalChatApp.Controllers;
[Route("api/")]
[ApiController]
public class LogsController : ControllerBase
{
    private readonly ILogRepository _logRepository;
    public LogsController(ILogRepository logRepository)
    {
       _logRepository = logRepository;
    }

    [HttpGet("log")]
    [Authorize] 
    public async Task<IActionResult> GetLogs([FromBody] GetLogsReqDto model)
    {
        if (!IsValidDateTime(model.StartTime) || !IsValidDateTime(model.EndTime))
        {
            return BadRequest(new { error = "Invalid date format for StartTime or EndTime." });
        }

        var startTime = model.StartTime.Value;   
        
        var endTime = model.EndTime.Value;

        try
        {
           

            var logs = await _logRepository.GetLogsAsync(startTime, endTime);

            if (!logs.Any())
            {
                return NotFound(new { error = "No logs found." });
            }
            return Ok(new { Logs = logs });
        }
        catch (Exception ex) {
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }

    
    }

    private bool IsValidDateTime(DateTime? input)
    {
        return input is DateTime;
    }
}
