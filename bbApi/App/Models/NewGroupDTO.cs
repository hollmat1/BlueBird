using System.ComponentModel.DataAnnotations;

namespace bbApi.App.Models
{
    public class NewGroupDTO
    {
        [Required]
        public string DisplayName { get; set; }
        [Required]
        public string Description { get; set; }
        public bool IsAssignableToRole { get; set; }
    }
}
