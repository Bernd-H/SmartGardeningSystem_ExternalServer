using System.Net.Sockets;

namespace ExternalServer.Common.Utilities {
    public static class SocketExtensions {
        public static bool IsConnected_UsingPoll(this Socket socket) {
            try {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }

        /// <summary>
        /// This Method is for Sockets were keep alives get send very frequently and the state must not be 100% accurate.
        /// If that is not the case use IsConnected_UsingPoll()
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool IsConnected(this Socket socket) {
            return socket.Connected;
        }
    }
}
