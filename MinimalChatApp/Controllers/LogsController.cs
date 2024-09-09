using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using MinimalChatApp.DomainModel.Data;
using MinimalChatApp.DomainModel.Dtos.Incoming;

namespace MinimalChatApp.Controllers;
[Route("api/")]
[ApiController]
public class LogsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public LogsController(ApplicationDbContext context)
    {
        _context= context;
    }

    [HttpGet("log")]
    [Authorize] 
    public async Task<IActionResult> GetLogs([FromBody] GetLogsReqDto model)
    {
        if (!IsValidDateTime(model.StartTime) || !IsValidDateTime(model.EndTime))
        {
            return BadRequest(new { error = "Invalid date format for StartTime or EndTime." });
        }

        var startTime = model.StartTime;   
        
        var endTime = model.EndTime;

        try
        {
            // Fetch logs from the database
            var logs = await _context.ApiLogs
                .Where(log => log.TimeOfCall >= startTime && log.TimeOfCall <= endTime)
                .OrderByDescending(log => log.TimeOfCall)
                .ToListAsync();

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
