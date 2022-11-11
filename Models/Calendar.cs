namespace StudentsForStudentsAPI.Models
{
    public static class Calendar
    {
        public static async Task<string?> GetCalendar(string calendarUrl)
        {
            try
            {
                using HttpClient client = new HttpClient();
                using HttpResponseMessage resp = await client.GetAsync(calendarUrl);
                using HttpContent content = resp.Content;
                return content.ReadAsStringAsync().Result;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
