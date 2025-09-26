using Microsoft.AspNetCore.SignalR;

namespace SMARTMOB_PANTAREI_BACK.Hubs
{
    public class AcquisizioniHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Join a group for a specific production line
        /// </summary>
        /// <param name="codLinea">Production line code</param>
        public async Task JoinLineGroup(string codLinea)
        {
            string groupName = $"line_{codLinea}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("JoinedGroup", groupName);
        }

        /// <summary>
        /// Leave a group for a specific production line
        /// </summary>
        /// <param name="codLinea">Production line code</param>
        public async Task LeaveLineGroup(string codLinea)
        {
            string groupName = $"line_{codLinea}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("LeftGroup", groupName);
        }

        /// <summary>
        /// Join a group for a specific workstation
        /// </summary>
        /// <param name="codPostazione">Workstation code</param>
        public async Task JoinStationGroup(string codPostazione)
        {
            string groupName = $"station_{codPostazione}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("JoinedGroup", groupName);
        }

        /// <summary>
        /// Leave a group for a specific workstation
        /// </summary>
        /// <param name="codPostazione">Workstation code</param>
        public async Task LeaveStationGroup(string codPostazione)
        {
            string groupName = $"station_{codPostazione}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("LeftGroup", groupName);
        }

        /// <summary>
        /// Join a group for a specific line AND station combination
        /// </summary>
        /// <param name="codLinea">Production line code</param>
        /// <param name="codPostazione">Workstation code</param>
        public async Task JoinLineStationGroup(string codLinea, string codPostazione)
        {
            string groupName = $"line_{codLinea}_station_{codPostazione}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("JoinedGroup", groupName);
        }

        /// <summary>
        /// Leave a group for a specific line AND station combination
        /// </summary>
        /// <param name="codLinea">Production line code</param>
        /// <param name="codPostazione">Workstation code</param>
        public async Task LeaveLineStationGroup(string codLinea, string codPostazione)
        {
            string groupName = $"line_{codLinea}_station_{codPostazione}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("LeftGroup", groupName);
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
