namespace StudentsForStudentsAPI.Services
{
    public interface IUserService
    {
        string? GetUserIdFromToken();
        bool IsTokenValid();
    }
}
