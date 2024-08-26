using HRM_SK.Contracts;
using HRM_SK.Database;
using Newtonsoft.Json;

namespace HRM_SK.Serivices.Notification_Service
{

    public class Notify<TValue> where TValue : class
    {
        private readonly DatabaseContext _dbContext;

        public Notify(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string getNotificationClassName()
        {
            return typeof(TValue).Name;
        }


        public async Task dispatch(NotificationRecord<TValue> notificationData)
        {
            var dataToJSON = Newtonsoft.Json.JsonConvert.SerializeObject(notificationData.Data, Newtonsoft.Json.Formatting.None,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            var newNotificcation = new Notification
            {
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow,
                data = dataToJSON,
                notifiableId = notificationData.NotifiableId,
                type = getNotificationClassName(),
                notifiableType = notificationData.NotifiableType,
                readAt = null
            };

            _dbContext.Notification.Add(newNotificcation);
            await _dbContext.SaveChangesAsync();
        }


    }
}
