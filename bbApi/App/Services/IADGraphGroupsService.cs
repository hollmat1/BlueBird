using bbApi.App.Models;
using Microsoft.Graph;

namespace bbApi.App.Services
{
    public interface IADGraphGroupsService
    {
        Task<GroupDTO> CreateGroupAsync(NewGroupDTO newGroup);
        Task DeleteGroupAsync(string id);
        Task<GroupDTO?> GetGroupAsync(string DisplayName);
        Task<UserDTO?> GetUserAsync(string UserPrincipalName);
        Task AddGroupMemberAsync(string GroupId, string MemberId);
    }
}