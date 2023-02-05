using bbApi.App.Models;

namespace bbApi.App.Services
{
    public interface IADGraphService
    {
        void CreateGroupAsync(NewGroupDTO newGroup);
        void DeleteGroupAsync(string id);
        Task<GroupDTO> GetGroupAsync(string DisplayName);
        Task<UserDTO> GetUserAsync(string UserPrincipalName);
        void AddGroupMembership(string GroupId, string MemberId);
    }
}