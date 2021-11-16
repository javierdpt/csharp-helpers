namespace HangfireService.Models
{
    public class RecurrentJobDateParam
    {
        // Order here is the parameter's position among the parameters passed
        public int Order { get; set; } = 0;
        public string Format { get; set; } = null;
        public int AddMonths { get; set; } = 0;

        public int DayOfMonth { get; set; } = 0;
    }
}