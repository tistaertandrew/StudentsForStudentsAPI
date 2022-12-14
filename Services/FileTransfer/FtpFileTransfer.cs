using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using System.Text;

namespace StudentsForStudentsAPI.Services.FileTransfer
{
    public class FtpFileTransfer : IDisposable
    {
        private readonly SftpClient _client;
        private bool _connected = false;

        /// <summary>
        /// Must be greater than 4
        /// </summary>
        private int _timeout = 5;

        /// <summary>
        /// Create client and try to connect with given connexion info
        /// </summary>
        /// <param name="connexionInfo"></param>
        /// <exception cref="Exception">An exception could be thrown on connexion failure</exception>
        public FtpFileTransfer(ClientConnectionInfo connexionInfo)
        {
            var sshNetConnexionInfo = new Renci.SshNet.ConnectionInfo
                (connexionInfo.host,
                connexionInfo.username,
                new PasswordAuthenticationMethod(
                    connexionInfo.username,
                    connexionInfo.password));

            sshNetConnexionInfo.Timeout = TimeSpan.FromSeconds(_timeout);

            _client = new SftpClient(sshNetConnexionInfo);
        }

        /// <summary>
        /// Download a file from client server.
        /// </summary>
        /// <param name="filePath">the full path of file to download from remote server</param>
        /// <returns>StreamReader of the downloaded file</returns>
        public string DownloadFile(string filePath)
        {
            TryToConnect();
            return ReadFromRemote(filePath);
        }

        /// <summary>
        /// Upload the given content to the remote server.
        /// </summary>
        /// <param name="content">The content to upload</param>
        /// <param name="remoteFilePath">The remote path where to upload the file</param>
        public void UploadFile(string content, string remoteFilePath)
        {
            TryToConnect();
            var directory = Path.GetDirectoryName(remoteFilePath);
            if (directory == null) throw new Exception("Could not get the directory from the given remote file path");
            directory = directory.Replace("\\", @"/");

            CreateRemoteDirectoryRecursively(directory);

            MemoryStream stream = WriteToMemoryStream(content);
            _client.UploadFile(stream, remoteFilePath);
        }

        /// <summary>
        /// Delete file from remote server
        /// </summary>
        /// <param name="filePath"></param>
        public void DeleteFile(string filePath)
        {
            TryToConnect();
            _client.DeleteFile(filePath);
        }

        /// <summary>
        /// List all remote working directory filenames
        /// </summary>
        /// <param name="path"></param>
        /// <returns>List of fullname (with path) of all files</returns>
        public List<string> ListFilesName(string path)
        {
            TryToConnect();
            var sftpFiles = _client.ListDirectory(path);
            var filesName = new List<string>();
            foreach (var file in sftpFiles) filesName.Add(file.Name);
            return filesName;
        }

        /// <summary>
        /// Demand client connexion status
        /// </summary>
        /// <returns>true if the client is connected, otherwise false</returns>
        public bool ClientIsConnected() => _client.IsConnected;

        /// <summary>
        /// Demand if a remote file exists
        /// </summary>
        /// <param name="filePath">The tested path</param>
        /// <returns>true if the file exists, otherwise false</returns>
        public bool FileExists(string filePath)
        {
            TryToConnect();
            return _client.Exists(filePath);
        }

        /// <summary>
        /// Try to connect to server
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void TryToConnect()
        {
            try
            {
                if (!_connected)
                {
                    _client.Connect();
                    bool a = _client.IsConnected;
                    _connected = true;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error while trying to connect to server: {e.Message}");
            }
        }

        /// <summary>
        /// Read a file at given path from the remote server. The read file content be returned.
        /// </summary>
        /// <param name="remoteFilePath">the path of the file at remote server</param>
        /// <returns>The content the read file</returns>
        private string ReadFromRemote(string remoteFilePath)
        {
            var memoryStream = ReadFromRemoteAsMemoryStream(remoteFilePath);
            using var content = new StreamReader(memoryStream);
            return content.ReadToEnd();
        }

        /// <summary>
        /// Read a file at given path from the remote server. The read file content be returned as a MemoryStream.
        /// </summary>
        /// <param name="remoteFilePath">the path of the file at remote server</param>
        /// <returns>The content the read file as a MemoryStream</returns>
        private MemoryStream ReadFromRemoteAsMemoryStream(string remoteFilePath)
        {
            SftpFileStream streamOfRemoteFile = _client.OpenRead(remoteFilePath);
            var memoryStream = new MemoryStream();
            streamOfRemoteFile.CopyTo(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        /// <summary>
        /// Writes the given content to a memory stream and returns it
        /// </summary>
        /// <param name="content">The content to write</param>
        /// <returns></returns>
        private MemoryStream WriteToMemoryStream(string content)
        {
            var bytesArray = Encoding.ASCII.GetBytes(content);
            return new MemoryStream(bytesArray);
        }

        /// <summary>
        /// Create a remote directory recursively
        /// </summary>
        /// <param name="remoteDirectoryPath"></param>
        /// <exception cref="Exception"></exception>
        private void CreateRemoteDirectoryRecursively(string remoteDirectoryPath)
        {
            if (remoteDirectoryPath[0] == '/') remoteDirectoryPath = remoteDirectoryPath.Substring(1);
            string remainingDirectoryPath = remoteDirectoryPath;
            string currentDirectoryPath = "";
            int indexOfNextDirectory;

            bool isThereMoreDirectory() => indexOfNextDirectory >= 0;

            bool isLastDirectory() => string.IsNullOrEmpty(remainingDirectoryPath);

            while (!isLastDirectory())
            {
                indexOfNextDirectory = remainingDirectoryPath.IndexOf('/');
                currentDirectoryPath += '/';

                if (isThereMoreDirectory())
                {
                    currentDirectoryPath += remainingDirectoryPath.Substring(0, indexOfNextDirectory);
                    remainingDirectoryPath = remainingDirectoryPath.Substring(indexOfNextDirectory + 1);
                }
                else
                {
                    currentDirectoryPath += remainingDirectoryPath;
                    remainingDirectoryPath = "";
                }

                try
                {
                    SftpFileAttributes attrs = _client.GetAttributes(currentDirectoryPath);

                    if (!attrs.IsDirectory)
                    {
                        throw new Exception("Can't create directory, current is a file");
                    }
                }
                catch (SftpPathNotFoundException)
                {
                    _client.CreateDirectory(currentDirectoryPath);
                }
            }
        }

        public int Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                _timeout = value > 5 ? value : _timeout;
            }
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_client.IsConnected) try { _client.Disconnect(); } catch (ObjectDisposedException) { }
                _client.Dispose();
            }
        }
    }
}