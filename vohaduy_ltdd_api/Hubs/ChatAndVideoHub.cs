using Microsoft.AspNetCore.SignalR;
using vohaduy_ltdd_api.DTOs;

namespace vohaduy_ltdd_api.Hubs
{
    public class ChatAndVideoHub : Hub
    {
        private static readonly Dictionary<string, HashSet<string>> RoomParticipants = new();

        public async Task JoinVideoRoom(string roomCode, string employeeName, string peerId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Room_{roomCode}");

            lock (RoomParticipants)
            {
                if (!RoomParticipants.ContainsKey(roomCode))
                {
                    RoomParticipants[roomCode] = new HashSet<string>();
                }
                RoomParticipants[roomCode].Add(peerId);
            }

            // Notify others in room
            await Clients.OthersInGroup($"Room_{roomCode}").SendAsync("UserJoinedRoom", new
            {
                RoomCode = roomCode,
                EmployeeName = employeeName,
                PeerId = peerId
            });

            // Send list of current peers in room to new joiner
            List<string> currentPeers;
            lock (RoomParticipants)
            {
                currentPeers = RoomParticipants[roomCode].Where(p => p != peerId).ToList();
            }

            await Clients.Caller.SendAsync("CurrentRoomPeers", currentPeers);
        }

        public async Task LeaveVideoRoom(string roomCode, string peerId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Room_{roomCode}");

            lock (RoomParticipants)
            {
                if (RoomParticipants.ContainsKey(roomCode))
                {
                    RoomParticipants[roomCode].Remove(peerId);
                    if (RoomParticipants[roomCode].Count == 0)
                    {
                        RoomParticipants.Remove(roomCode);
                    }
                }
            }

            await Clients.OthersInGroup($"Room_{roomCode}").SendAsync("UserLeftRoom", new
            {
                RoomCode = roomCode,
                PeerId = peerId
            });
        }

        /// <summary>
        /// Relays WebRTC signaling data (Offer, Answer, ICE Candidates) between peers
        /// </summary>
        public async Task SendSignal(WebRtcSignalDto signal)
        {
            if (!string.IsNullOrEmpty(signal.TargetPeerId))
            {
                // Send directly to targeted peer in room
                await Clients.OthersInGroup($"Room_{signal.RoomCode}").SendAsync("ReceiveSignal", signal);
            }
            else
            {
                // Broadcast to all peers in room
                await Clients.OthersInGroup($"Room_{signal.RoomCode}").SendAsync("ReceiveSignal", signal);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
