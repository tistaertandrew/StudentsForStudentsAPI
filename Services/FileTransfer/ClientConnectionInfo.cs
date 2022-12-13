namespace StudentsForStudentsAPI.Services.FileTransfer
{
    public class ClientConnectionInfo
    {
        public readonly string host;
        public readonly string username;
        public readonly string password;

        public ClientConnectionInfo(string host, string username, string password)
        {
            this.host = host;
            this.username = username;
            this.password = password;
        }
    }
}