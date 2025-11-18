using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Options;

using CheckMachAPI.Settings;

namespace CheckMachAPI.Services
{

    public class NotificationService
    {
        private readonly NotificationHubClient _hubClient;

        public NotificationService(IOptions<NotificationSettings> options)
        {
            var settings = options.Value;
            _hubClient = NotificationHubClient.CreateClientFromConnectionString(
                settings.ConnectionString,
                settings.HubName
            );
        }

        public async Task SendPushNotificationAsync(string title, string body)
        {
            var payload = new
            {
                notification = new
                {
                    title,
                    body
                }
            };

            string json = System.Text.Json.JsonSerializer.Serialize(payload);
            await _hubClient.SendFcmNativeNotificationAsync(json);
        }
    }
}
