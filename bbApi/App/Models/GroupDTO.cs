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

        public string Mail { get; set; }
        public string MailNickName { get; set; }
        public bool MailEnabled { get; set; }
        public string[] ProxyAddressses { get; set; }
    }
}
