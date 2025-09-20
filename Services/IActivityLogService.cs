using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetTestingWebApp.Services
{
    public interface IActivityLogService
    {
        Task LogChangeAsync(
        string? entityName,
        string stringAction,
        string user,
        string? entityId,
        object? changes);
    }
}