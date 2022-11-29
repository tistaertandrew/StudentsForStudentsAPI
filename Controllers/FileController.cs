using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Models.ViewModels;
using StudentsForStudentsAPI.Services;
using StudentsForStudentsAPI.Services.FileTransfer;

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

        public FileController(DatabaseContext context, UserManager<User> userManager, IUserService userService)
        {
            _context = context;
            _userManager = userManager;
            _userService = userService;

            _fileTransfer = new FtpFileTransfer(new ClientConnexionInfo(_host, _username, _password));

            _home = $"{_rootRemoteDirectory}/{_username}";
            _remoteWorkingDirectory = $"{_home}/{_filesRemoteDirectory}";
        }

        [AllowAnonymous]
        [HttpGet("Count")]
        [Produces("application/json")]
        public ActionResult GetFilesCount()
        {
            return Ok(_context.Files.Count());
        }

        [HttpGet("{filename}")]
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
        [HttpPost]
        [Produces("application/json")]
        public IActionResult UploadFile(UploadFileViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Informations invalides");
            }

            var errors = new List<string>();
            var file = new Models.File { Name = request.Filename, Extension = request.Extension, CreationDate = DateTime.Now };
            User user;
            bool isError = false;

            try
            {
                ThrowExceptionIfFileEntryAlreadyExists(file);
                user = GetCurrentUserFromToken();
                file.Owner = user;
                UploadFileToRemoteServer(file, request.Content);
                SaveFileEntryToDatabase(file);
            }
            catch (Exception e)
            {
                errors.Add(e.Message);
                isError = true;
            }

            return Ok(new FileResponseViewModel<string>(isError: isError, errors));
        }

        [HttpDelete]
        [Produces("application/json")]
        public IActionResult DeleteFile(string filename)
        {
            Models.File dbFile;
            User user;
            var errors = new List<string>();
            bool isError = false;

            try
            {
                dbFile = GetFileEntryFromDatabaseByName(filename);
                user = GetCurrentUserFromToken();
                ThrowExceptionIfCurrentUserDontOwnFile(dbFile);
                DeleteFileFromRemoteServer(dbFile);
                RemoveFromDatabaseIfExists(dbFile);
            }
            catch (Exception e)
            {
                errors.Add(e.Message);
                isError = true;
            }

            return Ok(new FileResponseViewModel<string>(isError: isError, errors));
        }

        [HttpGet]
        [Produces("application/json")]
        public IActionResult GetFilesMetadata()
        {
            List<Models.File> files = new List<Models.File>();
            var isError = false;
            var errors = new List<string>();

            try
            {
                files = _context.Files.Include(file => file.Owner).ToList();

                var mapped = files.Select(file => new FileViewModel
                {
                    FileId = file.Id,
                    Filename = file.Name,
                    CreationDate = file.CreationDate,
                    OwnerId = file?.Owner.Id,
                    OwnerName = file.Owner?.UserName
                });

                return Ok(new FileResponseViewModel<IEnumerable<FileViewModel>>(content: mapped, isError: isError, errors));
            }
            catch (Exception e)
            {
                errors.Add(e.Message);
                isError = true;
            }

            return Ok(new FileResponseViewModel<List<Models.File>>(content: files, isError: isError, errors));
        }

        private void ThrowExceptionIfFileEntryAlreadyExists(Models.File file)
        {
            try
            {
                if (IsFileEntryExistsInDatabase(file))
                {
                    throw new Exception("File name already taken");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error while testing file's name existence: {e.Message}");
            }
        }

        private void RemoveFromDatabaseIfExists(Models.File file)
        {
            if (IsFileEntryExistsInDatabase(file))
            {
                
                _context.Files.Remove(file);
                _context.SaveChanges();
            }
        }

        private bool IsFileEntryExistsInDatabase(Models.File file)
        {
            return _context.Files.Any(current => current.Name.Equals(file.Name));
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
                var query = _context.Files.Where(file => file.Name == filename).Include(file => file.Owner);
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
                if (user.Id != file.Owner.Id)
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
            catch (Exception e)
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
        /// Demand if a file exists in remote server
        /// </summary>
        /// <param name="file"></param>
        /// <returns>true if the file exists, otherwise false</returns>
        private bool FileExistsInRemoteServer(Models.File file) => _fileTransfer.FileExists(GetPathToFileInRemoteServer(file));

        protected override void Dispose(bool disposing)
        {
            if (disposing) _fileTransfer.Dispose();
        }
    }
}