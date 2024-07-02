﻿using signalR.DTO.Request;
using signalR.Models.Local;
using signalR.Models.StoredProcedures;

namespace signalR.Services.Interfaces
{
    public interface INotificationsService
    {
        Task<List<NotificationPending>> GetNotitificationsPending(string serialTerminal);       
        Task<SentTerminalsStatus> SendNotification(SendNotification sendNotificationRequest);
    }
}
