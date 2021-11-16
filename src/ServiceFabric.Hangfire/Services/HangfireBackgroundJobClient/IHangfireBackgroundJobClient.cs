using HangfireService.Interfaces;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HangfireService.Services.HangfireBackgroundJobClient
{
    public interface IHangfireBackgroundJobClient
    {
        string Enqueue(
            Expression<Func<Task>> operation, 
            QueueNamesEnum queue = QueueNamesEnum.@default);

        string ContinueWith(string jobId, Expression<Func<Task>> operation,
            QueueNamesEnum queue = QueueNamesEnum.@default);
    }
}