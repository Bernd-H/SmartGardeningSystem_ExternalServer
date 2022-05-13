namespace ExternalServer.Common.Utilities {
    public static class CommunicationCodes {

        public static byte[] ACK = new byte[] { 0xFF };

        // codes concerning the connection astablishment over the internet
        public static byte[] SendPeerToPeerEndPoint = new byte[1] { 86 };

        public static byte[] NoPeerToPeerPossible = new byte[1] { 87 };
    }
}
