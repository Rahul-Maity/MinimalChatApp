using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MinimalChatApp.DomainModel.Models;

namespace MinimalChatApp.Repository.ApiLogs;
public interface ILogRepository
{
    Task<IEnumerable<ApiLog>> GetLogsAsync(DateTime startTime, DateTime endTime);
}
