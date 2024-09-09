using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MinimalChatApp.DomainModel.Data;
using MinimalChatApp.DomainModel.Models;

namespace MinimalChatApp.Repository.ApiLogs;
public class LogRepository : ILogRepository
{
    private readonly ApplicationDbContext _context;
    public LogRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<ApiLog>> GetLogsAsync(DateTime startTime, DateTime endTime)
    {
        return await _context.ApiLogs
           .Where(log => log.TimeOfCall >= startTime && log.TimeOfCall <= endTime)
           .OrderByDescending(log => log.TimeOfCall)
           .ToListAsync();
    }
}
