using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Models.ViewModels;
using StudentsForStudentsAPI.Services;
using StudentsForStudentsAPI.Services.FileTransfer;
using System.Text.RegularExpressions;

namespace StudentsForStudentsAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class FileController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;

        private readonly FtpFileTransfer _fileTransfer;

        // TODO  Move this into env file
        private readonly string _username = "d180106";
        const string _host = "192.168.128.13";
        const string _password = "cgfqdH2N";

        private readonly string _rootRemoteDirectory = "/home";
        private readonly string _filesRemoteDirectory = "public_html/StudentsForStudentsAPI/Files";
        
        private readonly string _home;
        private readonly string _remoteWorkingDirectory;

        /// <summary>
        /// Regex to remove files like (. or ..) from list of files
        /// </summary>
        private readonly Regex _regexToRemoveRootFilesFromFileList;

        public FileController(DatabaseContext context, UserManager<User> userManager, IUserService userService)
        {
            _context = context;
            _userManager = userManager;
            _userService = userService;

            _fileTransfer = new FtpFileTransfer(new ClientConnexionInfo(_host, _username, _password));

            _home = $"{_rootRemoteDirectory}/{_username}";
            _remoteWorkingDirectory = $"{_home}/{_filesRemoteDirectory}";
            _regexToRemoveRootFilesFromFileList = new Regex(@"^(\.+)$");
        }

        [HttpGet("Download/{filename}")]
        [Produces("application/json")]
        public IActionResult DownloadFile(string filename)
        {
            Models.File dbFile;
            var errors = new List<string>();
            User user;
            bool isError = false;
            string content = string.Empty;
            
            try
            {
                dbFile = GetFileEntryFromDatabaseByName(filename);
                ThrowExceptionIfFileDoesNotExistsInRemoteServer(dbFile);
                user = GetCurrentUserFromToken();
                content = GetFileContentFromRemoteServer(dbFile);
            }
            catch (Exception e)
            {
                errors.Add(e.Message);
                isError = true;
            }

            return Ok(new FileResponseViewModel<string>(content: content, isError: isError, errors));
        }

        [Authorize(Roles = "Member, Admin")]
        [HttpPost("Upload")]
        [Produces("application/json")]
        public IActionResult UploadFile(UploadFileViewModel request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest("Informations invalides");
            }

            var errors = new List<string>();
            var file = new Models.File { Name = request.FileName, Extension = request.Extension };
            User user;
            bool isError = false;

            try
            {
                ThrowExceptionIfFileAlreadyExistsInRemoteServer(file);
                user = GetCurrentUserFromToken();
                file.Onwer = user;
                UploadFileToRemoteServer(file, request.Content);
                SaveFileEntryToDatabase(file);
            }
            catch(Exception e)
            {
                DeleteFileFromRemoteServerIfExists(file, errors);
                errors.Add(e.Message);
                isError = true;
            }

            return Ok(new FileResponseViewModel<string>(isError: isError, errors));
        }

        [HttpGet("List")]
        [Produces("application/json")]
        public IActionResult ListFiles()
        {
            try
            {
                ThrowExceptionIfWorkingDirectoryDoesNotExistsInRemoteServer();
                List<string> names = ListFileFromRemoteServer();
                names.RemoveAll(name => _regexToRemoveRootFilesFromFileList.IsMatch(name));
                return Ok(new FileResponseViewModel<List<string>>(content: names));
            }
            catch (Exception e)
            {
                var errors = new List<string>();
                errors.Add($"Error while listing remote files: {e.Message}");
                return Ok(new FileResponseViewModel<string>(isError: true, errors));
            }
        }

        [HttpGet("Delete/{filename}")]
        [Produces("application/json")]
        public IActionResult DeleteFile(string filename)
        {
            Models.File dbFile;
            User user;
            var errors = new List<string>();
            bool isError = false;

            try
            {
                //ThrowExceptionIfClientIsNotConnectedToRemoteServer();
                dbFile = GetFileEntryFromDatabaseByName(filename);
                user = GetCurrentUserFromToken();
                ThrowExceptionIfCurrentUserDontOwnFile(dbFile);
                DeleteFileFromRemoteServer(dbFile);
            }
            catch(Exception e)
            {
                errors.Add(e.Message);
                isError = true;
            }

            return Ok(new FileResponseViewModel<string>(isError: isError, errors));
        }

        /// <summary>
        /// Delete file from remote server if exists. If errors param is given then, on exception, errors messages will be added to without throwing exception.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="errors"></param>
        /// <exception cref="Exception">If errors happpens while testing existence and deleting. If errors param is given then errors message be added to without throwing exception</exception>
        private void DeleteFileFromRemoteServerIfExists(Models.File file, List<string>? errors = default)
        {
            try
            {
                if (FileExistsInRemoteServer(file))
                {
                    DeleteFileFromRemoteServer(file);
                }
            }
            catch (Exception e)
            {
                if (errors != null) errors.Add(e.Message);
                else throw new Exception($"Error while deleting file if exists from remote server: {e.Message}");
            }
        }

        /// <summary>
        /// Demand to save file entry to database
        /// </summary>
        /// <param name="file"></param>
        /// <param name="user"></param>
        /// <exception cref="Exception">errors could happens while saving</exception>
        private void SaveFileEntryToDatabase(Models.File file)
        {
            try
            {
                _context.Files.Add(file);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw new Exception($"Error while saving file entry to database: {e.Message}");
            }
        }
        
        /// <summary>
        /// Demand if working directory exists in remote server, if false throw exeption
        /// </summary>
        /// <exception cref="Exception">if working directory does nnot exists in remote server</exception>
        private void ThrowExceptionIfWorkingDirectoryDoesNotExistsInRemoteServer()
        {
            if (!FileExistsInRemoteServer(_remoteWorkingDirectory)) throw new Exception("Working directory does not exists in remote server");
        }

        /// <summary>
        /// Demand if a file path exists in remote server, if true throw exception
        /// </summary>
        /// <param name="file">the path to the file to test existence in remote</param>
        /// <exception cref="Exception">if the file does exists in remote</exception>
        private void ThrowExceptionIfFileAlreadyExistsInRemoteServer(Models.File file)
        {
            if (FileExistsInRemoteServer(file))
            {
                throw new Exception("File already exists in remote");
            }
        }

        /// <summary>
        /// Demand if a file path exists in remote server, if false throw exception
        /// </summary>
        /// <param name="file">the path to the file to test existence in remote</param>
        /// <exception cref="Exception">if the file does not exists in remote</exception>
        private void ThrowExceptionIfFileDoesNotExistsInRemoteServer(Models.File file)
        {
            if (!FileExistsInRemoteServer(file))
            {
                throw new Exception("File does not exists in remote");
            }
        }

        /// <summary>
        /// Demand the file entry correcponding to a filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        /// <exception cref="Exception">If any entry haev the given filename or if an exception occure while getting the entry from the database</exception>
        private Models.File GetFileEntryFromDatabaseByName(string filename)
        {
            try
            {
                var query = _context.Files.Where(file => file.Name == filename).Include(file => file.Onwer);
                if (!query.Any()) throw new Exception($"No file with name '{filename}' was found");
                return query.First();
            }
            catch (Exception e)
            {
                throw new Exception($"Error while getting file '{filename}' from database : {e.Message}");
            }
        }

        /// <summary>
        /// Demand the current user from the http context
        /// </summary>
        /// <returns>the user found</returns>
        /// <exception cref="Exception">if could not find the user</exception>
        private User GetCurrentUserFromToken()
        {
            var user = _userManager.FindByIdAsync(_userService.GetUserIdFromToken()).Result;
            if (user == null)
            {
                throw new Exception("Current user not found with token");
            };
            return user;
        }

        /// <summary>
        /// Demand if the current user own the file
        /// </summary>
        /// <param name="file"></param>
        /// <exception cref="Exception">if the current user don't own the file</exception>
        /// <see cref="GetCurrentUserFromToken"/>
        private void ThrowExceptionIfCurrentUserDontOwnFile(Models.File file)
        {
            try
            {
                var user = GetCurrentUserFromToken();
                if (user.Id != file.Onwer.Id)
                    throw new Exception($"Current user don't own the file '{file.Name}'");
            }
            catch (Exception e)
            {
                throw new Exception($"Error while testing file ownership : {e.Message}");
            }
        }

        /// <summary>
        /// Upload file to remote server
        /// </summary>
        /// <param name="file"></param>
        /// <param name="remoteDestinationPath"></param>
        /// <exception cref="Exception"></exception>
        private void UploadFileToRemoteServer(Models.File file, string content)
        {
            try
            {
                _fileTransfer.UploadFile(content, GetPathToFileInRemoteServer(file));
            }
            catch (Exception e)
            {
                throw new Exception($"Error while uploading file to remote server: {e.Message}");
            }
        }

        /// <summary>
        /// List files from remote server
        /// </summary>
        /// <returns>List of files name</returns>
        private List<string> ListFileFromRemoteServer()
        {
            return _fileTransfer.ListFilesName(_remoteWorkingDirectory);
        }

        /// <summary>
        /// Download file from remote server and return his content
        /// </summary>
        /// <param name="file"></param>
        /// <returns>the file's content</returns>
        /// <exception cref="Exception"></exception>
        private string GetFileContentFromRemoteServer(Models.File file)
        {
            try
            {
                var stream = _fileTransfer.DownloadFile(GetPathToFileInRemoteServer(file));
                return stream.ReadToEnd();
            }
            catch (Exception e)
            {
                throw new Exception($"Error while getting file content from server : {e.Message}");
            }
        }

        /// <summary>
        /// Comand to delete a file from remote server
        /// </summary>
        /// <param name="dbFile">the entry file to delete from remote server</param>
        /// <returns></returns>
        private void DeleteFileFromRemoteServer(Models.File dbFile)
        {
            try
            {
                _fileTransfer.DeleteFile(GetPathToFileInRemoteServer(dbFile));
            }
            catch(Exception e)
            {
                throw new Exception($"Error while deleting file from remote server : {e.Message}");
            }
        }

        /// <summary>
        /// Builds path to remote server from file
        /// </summary>
        /// <param name="dbFile">the file to build the path from</param>
        /// <returns>the path to file in remote server</returns>
        private string GetPathToFileInRemoteServer(Models.File dbFile)
        {
            return $"{_remoteWorkingDirectory}/{dbFile.Name}.{dbFile.Extension}";
        }

        /// <summary>
        /// Demand if the client is connected to remote server
        /// </summary>
        /// <returns>true if the client is connected, otherwise false</returns>
        private bool IsClientConnectedToRemotetServer() => _fileTransfer.ClientIsConnected();
        
        /// <summary>
        /// Demand if a file exists in remote server
        /// </summary>
        /// <param name="file"></param>
        /// <returns>true if the file exists, otherwise false</returns>
        private bool FileExistsInRemoteServer(Models.File file) => _fileTransfer.FileExists(GetPathToFileInRemoteServer(file));

        /// <summary>
        /// Demand if a file path exists in reomte server
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>true if the file exists, otherwise false</returns>
        private bool FileExistsInRemoteServer(string filePath) => _fileTransfer.FileExists(filePath);

        protected override void Dispose(bool disposing)
        {
            if (disposing) _fileTransfer.Dispose();
        }
    }
}
