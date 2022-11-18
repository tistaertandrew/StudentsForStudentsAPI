using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Models.ViewModels;
using StudentsForStudentsAPI.Services.FileTransfer;
using System.Text.RegularExpressions;

namespace StudentsForStudentsAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FileController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly UserManager<User> _userManager;

        private readonly FtpFileTransfer _fileTransfer;

        // TODO  Move this into env file
        private readonly string _username = "d180106";
        const string _host = "192.168.128.13";
        const string _password = "cgfqdH2N";

        private readonly string _rootRemoteDirectory = "/home";
        private readonly string _workingRemoteDirectory = "public_html/StudentsForStudentsAPI/Files";

        private readonly string _home;
        private readonly string _filesRemotePath;

        /// <summary>
        /// Regex to remove files like (. or ..) from list of files
        /// </summary>
        private readonly Regex _regexToRemoveRootFilesFromFileList;

        public FileController(DatabaseContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;

            _fileTransfer = new FtpFileTransfer(new ClientConnexionInfo(_host, _username, _password));

            _home = $"{_rootRemoteDirectory}/{_username}";
            _filesRemotePath = $"{_home}/{_workingRemoteDirectory}";
            _regexToRemoveRootFilesFromFileList = new Regex(@"^(\.+)$");
        }

        [HttpGet("Download/{fileName}")]
        [Produces("application/json")]
        public IActionResult DownloadFile(string fileName)
        {
            var dbFile = _context.Files.Where(file => file.Name == fileName).First();

            var errors = new List<string>();

            if (dbFile == null)
            {
                errors.Add($"File '{fileName}' not found in database");
                return Ok(new FileResponseViewModel<string>(isError: true, errors));
            }

            string filePath = $"{_filesRemotePath}/{dbFile.Name}.{dbFile.Extension}";

            if (!IsConnected())
            {
                
                errors.Add("Client is not connected");
                return Ok(new FileResponseViewModel<string>(isError: true, errors));
            }

            if (!FileExists(filePath))
            {
                errors.Add("File does not exist in remote");
                return Ok(new FileResponseViewModel<string>(isError: true, errors));
            }

            using var stream = _fileTransfer.DownloadFile(filePath);

            try
            {
                var content = stream.ReadToEnd();
                return Ok(new FileResponseViewModel<string>(content));
            }
            catch (Exception e)
            {
                errors.Add(e.Message);
                return Ok(new FileResponseViewModel<string>(isError: true, errors));
            }
        }

        [HttpPost("Upload")]
        [Produces("application/json")]
        public IActionResult UploadFile(UploadFileViewModel request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest("Informations invalides");
            }

            var errors = new List<string>();

            var remoteDestinationPath = $"{_filesRemotePath}/{request.FileName}";

            if (!IsConnected())
            { 
                errors.Add("Client is not connected");
                return Ok(new FileResponseViewModel<string>(isError: true, errors));
            }

            if (FileExists(remoteDestinationPath))
            {
                errors.Add("File already exist in remote");
                return Ok(new FileResponseViewModel<string>(isError: true, errors));
            }

            try
            {
                _fileTransfer.UploadFile(request.Content, remoteDestinationPath);
                _context.Files.Add(new Models.File
                {
                    Name = request.FileName,
                    Extension = request.Extension,
                    
                }); ;
            }
            catch(Exception e)
            {
                errors.Add(e.Message);
                return Ok(new FileResponseViewModel<string>(isError: true, errors));
            }

            return Ok(new FileResponseViewModel<string>(isError: false, errors));
        }

        [HttpGet("List")]
        [Produces("application/json")]
        public IActionResult ListFiles()
        {
            try
            {
                if (!IsConnected()) return Ok("client is not connected");
                if (!FileExists(_filesRemotePath)) return Ok("path does not exist");
                var names = _fileTransfer.ListFilesName(_filesRemotePath);
                names.RemoveAll(name => _regexToRemoveRootFilesFromFileList.IsMatch(name));
                return Ok(new FileResponseViewModel<List<string>>(content: names));
            } catch(Exception e)
            {
                var errors = new List<string>();
                errors.Add(e.Message);
                return Ok(new FileResponseViewModel<string>(isError: true, errors));
            }
        }

        [HttpGet("Delete/{filename}")]
        public IActionResult DeleteFile(string filename)
        {
            // is he the owner of the file
            //this.GetFileFromDbByName();

            return Ok();
        }

        private Models.File GetFileFromDbByName(string filename)
        {
            return _context.Files.Where(file => file.Name == filename).First();
        }

        private bool IsConnected()
        {
            return _fileTransfer.ClientIsConnected();
        }

        private bool FileExists(string filePath)
        {
            return _fileTransfer.FileExists(filePath);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing) _fileTransfer.Dispose();
        }
    }
}
