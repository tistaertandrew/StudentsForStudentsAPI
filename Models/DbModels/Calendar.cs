namespace StudentsForStudentsAPI.Models.DbModels
{
    public static class Calendar
    {
        public static async Task<string?> GetCalendar(string calendarUrl)
        {
            try
            {
                using var client = new HttpClient();
                using var resp = await client.GetAsync(calendarUrl);
                using var content = resp.Content;
                return content.ReadAsStringAsync().Result;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
