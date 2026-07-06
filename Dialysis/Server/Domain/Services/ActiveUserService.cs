using System;
using System.Security.Claims;
using Dialysis.Server.Domain.Services;

namespace Dialysis.Server.Domain.Services
{
    public interface IActiveUserService
    {
        long UserId { get; }
        ClaimsPrincipal User { get; }
        string UserIp { get; }
    }
    public class ActiveUserService : IActiveUserService
    {
        public ActiveUserService(IHttpContextAccessor httpContextAccessor)
        {
            string tempId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!String.IsNullOrEmpty(tempId)) UserId = Convert.ToInt64(tempId);
            ClaimsPrincipal user = httpContextAccessor.HttpContext.User;
            User = user;
            Claims = httpContextAccessor.HttpContext?.User?.Claims.AsEnumerable().Select(item => new KeyValuePair<string, string>(item.Type, item.Value)).ToList();
            UserIp = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "";
        }
        public long UserId { get; }
        public ClaimsPrincipal User { get; }
        public string UserIp { get; }
        public List<KeyValuePair<string, string>> Claims { get; set; }
    }

}

