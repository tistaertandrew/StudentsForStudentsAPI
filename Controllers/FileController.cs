using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentsForStudentsAPI.Models;
using StudentsForStudentsAPI.Models.ViewModels;
using StudentsForStudentsAPI.Services;
using StudentsForStudentsAPI.Services.FileTransfer;
using StudentsForStudentsAPI.Services.MailService;
using System.Text.RegularExpressions;
using StudentsForStudentsAPI.Models.DbModels;
using StudentsForStudentsAPI.Models.Mails;
using StudentsForStudentsAPI.Services.UserService;
using File = StudentsForStudentsAPI.Models.DbModels.File;
using Swashbuckle.AspNetCore.Annotations;

namespace StudentsForStudentsAPI.Controllers
{
    /// <summary>
    /// Controller qui permet de gérer les fichiers du système
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class FileController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;
        private readonly IMailService _mailService;
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly IConfiguration _config;
        private readonly FtpFileTransfer _fileTransfer;

        private readonly string _home;
        private readonly string _remoteWorkingDirectory;

        /// <summary>
        /// Constructeur du controller qui permet de gérer les fichiers du système
        /// </summary>
        /// <param name="context">Un objet permettant d'intéragir avec la base de données</param>
        /// <param name="userManager">Un service permettant de gérer l'utilisateur connecté</param>
        /// <param name="userService">Un service permettant d'intéragir avec l'utilisateur connecté</param>
        /// <param name="mailService">Un service qui permet d'envoyer des mails</param>
        /// <param name="hubContext">Un objet permettant d'envoyer des notifications aux différents clients connectés</param>
        /// <param name="config">Un objet contenant la configuration du système</param>
        public FileController(DatabaseContext context, UserManager<User> userManager, IUserService userService, IMailService mailService, IHubContext<SignalRHub> hubContext, IConfiguration config = null)
        {
            _context = context;
            _userManager = userManager;
            _userService = userService;
            _mailService = mailService;
            _hubContext = hubContext;
            _config = config;
            _fileTransfer = new FtpFileTransfer(new ClientConnectionInfo(_config.GetSection("RemoteFileServerSettings:Host").Value, _config.GetSection("RemoteFileServerSettings:UserName").Value, _config.GetSection("RemoteFileServerSettings:UserPassword").Value));

            _home = $"{_config.GetSection("RemoteFileServerSettings:RootRemoteDirectory").Value}/{_config.GetSection("RemoteFileServerSettings:UserName").Value}";
            _remoteWorkingDirectory = $"{_home}/{_config.GetSection("RemoteFileServerSettings:FilesRemoteDirectory").Value}";
        }
        
        /// <summary>
        /// Route (GET) qui permet de récupérer le nombre de fichiers du système
        /// </summary>
        /// <returns>Le nombre de fichiers du système</returns>
        [AllowAnonymous]
        [HttpGet("Count")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Retourne le nombre de fichiers du système")]
        [SwaggerResponse(StatusCodes.Status200OK, "Le nombre a bien été récupéré", typeof(int))]
        public ActionResult<int> GetFilesCount()
        {
            return Ok(_context.Files.Count());
        }

        /// <summary>
        /// Route (GET) qui permet de récupérer un fichier sur base de son nom
        /// </summary>
        /// <param name="filename">le nom du fichier à récupérer</param>
        /// <returns>Le fichier correspondant au nom donné</returns>
        [Authorize(Roles = "Member, Admin")]
        [HttpGet("{filename}")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Retourne un fichier sur base de son nom")]
        [SwaggerResponse(StatusCodes.Status200OK, "Le fichier a bien été récupéré", typeof(FileResponseViewModel<string>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "L'utilisateur n'est pas connecté ou son token est invalide")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "L'utilisateur n'a pas les droits pour accéder à cette ressource")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Le fichier n'a pas été trouvé")]
        public ActionResult<FileResponseViewModel<string>> DownloadFile(string filename)
        {
            File dbFile;
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

        /// <summary>
        /// Route (POST) qui permet d'ajouter un fichier au système
        /// </summary>
        /// <param name="request">Le potentiel nouveau fichier à ajouter</param>
        /// <returns>Un ViewModel spécifiant si oui ou non l'opération s'est bien effectuée</returns>
        [Authorize(Roles = "Member, Admin")]
        [HttpPost]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Ajoute un fichier au système")]
        [SwaggerResponse(StatusCodes.Status200OK, "Le fichier a bien été ajouté", typeof(FileResponseViewModel<string>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Le fichier n'a pas pu être ajouté")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "L'utilisateur n'est pas connecté ou son token est invalide")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "L'utilisateur n'a pas les droits pour accéder à cette ressource")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Les informations transmises sont invalides")]
        public async Task<ActionResult<FileResponseViewModel<string>>> UploadFile(UploadFileViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Informations invalides");
            }

            var errors = new List<string>();
            var file = new File
            {
                Course = _context.Courses.Include(c => c.Cursus).ThenInclude(c => c.Section).Where(c => c.Id == request.CourseId).First(),
                Name = request.Filename,
                Extension = request.Extension,
                CreationDate = DateTime.Now
            };
            User user;
            bool isError = false;

            try
            {
                ThrowExceptionIfFileEntryAlreadyExists(file);
                user = GetCurrentUserFromToken();
                file.Owner = user;
                UploadFileToRemoteServer(file, request.Content);
                SaveFileEntryToDatabase(file);

                await _hubContext.Clients.All.SendAsync("updateFilesCount");
                await _hubContext.Clients.All.SendAsync("updateFilesMetaData");

                _mailService.SendMail(new AddSyntheseMail($"Ajout de votre synthèse \"{file.Name}\"", user.Email, null, new []{ user.UserName, file.Name }));
            }
            catch (Exception e)
            {
                errors.Add(e.Message);
                isError = true;
            }

            return Ok(new FileResponseViewModel<string>(isError: isError, errors));
        }

        /// <summary>
        /// Route (DELETE) qui permet de supprimer un fichier du système sur base de son nom
        /// </summary>
        /// <param name="filename">Le nom du fichier à supprimer</param>
        /// <returns>Un ViewModel spécifiant si oui ou non l'opération s'est bien effectuée</returns>
        [Authorize(Roles = "Member, Admin")]
        [HttpDelete("{filename}")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Supprime un fichier du système sur base de son nom")]
        [SwaggerResponse(StatusCodes.Status200OK, "Le fichier a bien été supprimé", typeof(FileResponseViewModel<string>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "L'utilisateur n'est pas connecté ou son token est invalide")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "L'utilisateur n'a pas les droits pour accéder à cette ressource")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Le fichier n'a pas été trouvé")]
        public async Task<ActionResult<FileResponseViewModel<string>>> DeleteFile(string filename)
        {
            File dbFile;
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

                await _hubContext.Clients.All.SendAsync("updateFilesCount");
                await _hubContext.Clients.All.SendAsync("updateFilesMetaData");

                _mailService.SendMail(new DeleteSyntheseMail($"Suppression de votre synthèse \"{dbFile.Name}\"", user.Email, null, new []{ user.UserName, dbFile.Name }));
            }
            catch (Exception e)
            {
                errors.Add(e.Message);
                isError = true;
            }

            return Ok(new FileResponseViewModel<string>(isError: isError, errors));
        }

        /// <summary>
        /// Route (GET) qui permet de récupérer les métadonnées des fichiers
        /// </summary>
        /// <returns>Une liste composées des métadonnées des fichiers du système</returns>
        [Authorize(Roles = "Member, Admin")]
        [HttpGet]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Retourne les métadonnées des fichiers du système")]
        [SwaggerResponse(StatusCodes.Status200OK, "Les métadonnées des fichiers ont bien été récupérées", typeof(FileResponseViewModel<IEnumerable<FileViewModel>>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "L'utilisateur n'est pas connecté ou son token est invalide")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "L'utilisateur n'a pas les droits pour accéder à cette ressource")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Les métadonnées des fichiers n'ont pas pu être récupérées")]
        public ActionResult<FileResponseViewModel<IEnumerable<FileViewModel>>> GetFilesMetadata()
        {
            List<File> files = new List<File>();
            var isError = false;
            var errors = new List<string>();

            try
            {
                files = _context.Files.Include(file => file.Owner).Include(f => f.Course).ThenInclude(c => c.Cursus).ThenInclude(c => c.Section).ToList();

                var mapped = files.Select(file => new FileViewModel
                {
                    FileId = file.Id,
                    Filename = file.Name,
                    Course = file.Course,
                    CreationDate = file.CreationDate,
                    OwnerId = file?.Owner.Id,
                    OwnerName = file.Owner?.UserName,
                    Extension = file.Extension,
                });

                return Ok(new FileResponseViewModel<IEnumerable<FileViewModel>>(content: mapped, isError: isError, errors));
            }
            catch (Exception e)
            {
                errors.Add(e.Message);
                isError = true;
            }

            return Ok(new FileResponseViewModel<List<File>>(content: files, isError: isError, errors));
        }

        private void ThrowExceptionIfFileEntryAlreadyExists(File file)
        {
            try
            {
                if (IsFileNameExistsInDatabase(file))
                {
                    throw new Exception("File name already taken");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error while testing file's name existence: {e.Message}");
            }
        }

        private void RemoveFromDatabaseIfExists(File file)
        {
            if (IsFileNameExistsInDatabase(file))
            {
                _context.Files.Remove(file);
                _context.SaveChanges();
            }
        }

        private bool IsFileNameExistsInDatabase(File file)
        {
            return _context.Files.Any(current => current.Name.Equals(file.Name));
        }

        /// <summary>
        /// Demand to save file entry to database
        /// </summary>
        /// <param name="file"></param>
        /// <param name="user"></param>
        /// <exception cref="Exception">errors could happens while saving</exception>
        private void SaveFileEntryToDatabase(File file)
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
        private void ThrowExceptionIfFileDoesNotExistsInRemoteServer(File file)
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
        private File GetFileEntryFromDatabaseByName(string filename)
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
        private void ThrowExceptionIfCurrentUserDontOwnFile(File file)
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
        private void UploadFileToRemoteServer(File file, string content)
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
        private string GetFileContentFromRemoteServer(File file)
        {
            try
            {
                var content = _fileTransfer.DownloadFile(GetPathToFileInRemoteServer(file));
                return content;
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
        private void DeleteFileFromRemoteServer(File dbFile)
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
        private string GetPathToFileInRemoteServer(File dbFile)
        {
            return $"{_remoteWorkingDirectory}/{dbFile.Name}.{dbFile.Extension}";
        }

        /// <summary>
        /// Demand if a file exists in remote server
        /// </summary>
        /// <param name="file"></param>
        /// <returns>true if the file exists, otherwise false</returns>
        private bool FileExistsInRemoteServer(File file) => _fileTransfer.FileExists(GetPathToFileInRemoteServer(file));

        protected override void Dispose(bool disposing)
        {
            if (disposing) _fileTransfer.Dispose();
        }
    }
}