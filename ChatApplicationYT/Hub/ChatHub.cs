using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;

namespace ChatApplicationYT.Hub
{
	public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
	{
		private readonly IDictionary<string, UserRoomConnection> _connection;

		public ChatHub (IDictionary<string, UserRoomConnection> connection)
		{
			_connection = connection;
		}

		public async Task JoinRoom(UserRoomConnection userConnection)
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.room!);
			_connection[Context.ConnectionId] = userConnection;
			await Clients.Groups(userConnection.room!).SendAsync("ReceiveMessage", "Lets Program Bot", $"{userConnection.user} has Joined the Groups", DateTime.Now);
			await SendConnectedUser(userConnection.room!);
		}

		public async Task SendMessage(string message)
		{
			if(_connection.TryGetValue(Context.ConnectionId, out UserRoomConnection userRoomConnection))
			{
				await Clients.Group(userRoomConnection.room!).SendAsync("ReceiveMessage", userRoomConnection.user, message, DateTime.Now);
			}
		}

		public override Task OnDisconnectedAsync(Exception? exception)
		{
			if (_connection.TryGetValue(Context.ConnectionId, out UserRoomConnection connection))
			{
				return base.OnDisconnectedAsync(exception);

			}
			_connection.Remove(Context.ConnectionId);
			Clients.Group(connection.room!)
				.SendAsync("ReceiveMessage", "Let program Bot", $"{connection.user} has left the group", DateTime.Now);
			SendConnectedUser(connection.room);
			return base.OnDisconnectedAsync(exception);
		}

		public Task SendConnectedUser(string room)
		{
			var users = _connection.Values.Where(u => u.room == room).Select(s => s.user);
			return Clients.Group(room).SendAsync("ConnectedUser", users);
		}
	}
}
