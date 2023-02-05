using System.ComponentModel.DataAnnotations;

namespace bbApi.App.Models
{
    public class GroupDTO
    {
        public string Id { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        public string Description { get; set; }
        public bool SecurityEnabled { get; set; }
        public bool IsAssignableToRole { get; set; }
    }
}
