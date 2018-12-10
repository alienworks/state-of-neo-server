using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StateOfNeo.Server.Hubs;

namespace StateOfNeo.Server.Controllers
{
    public class NotificationController : BaseApiController
    {
        private readonly IHubContext<NotificationHub> notificationHub;

        public NotificationController(IHubContext<NotificationHub> notificationHub)
        {
            this.notificationHub = notificationHub;
        }
    }
}
