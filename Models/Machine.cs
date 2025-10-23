

namespace CheckMachAPI.Models
{
    public class Machine
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Models { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public int TotalHours { get; set; }
        public DateTime LastServiceDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "";
        public bool IsActive { get; set; }    
    }
}