using System.ComponentModel.DataAnnotations;

namespace p7_client_frontendV2.Data
{
    public class Job
    {
        [Required]
        public string? JobName { get; set; }
    }
}
