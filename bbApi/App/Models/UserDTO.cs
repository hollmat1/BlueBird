using Microsoft.Graph;

namespace bbApi.App.Models
{
    public class UserDTO
    {
        public string DisplayName { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public string Mail { get; set; }
        public string UserPrincipalName { get; set; }
        public string OnPremisesUserPrincipalName { get; set; }
    }
}
