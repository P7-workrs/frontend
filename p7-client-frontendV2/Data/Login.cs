using System.ComponentModel.DataAnnotations;

namespace p7_client_frontendV2.Data;

public class User
{
    [Required]
    public string? Name { get; set; }
}
