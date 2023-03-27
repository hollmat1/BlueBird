using AutoMapper;
using AutoMapper.Execution;
using Azure.Core;
using Azure.Core.Serialization;
using bbApi.App.Infrastructure;
using bbApi.App.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace bbApi.App.Services
{
    public class ADGraphGroupsService : ADGraphServiceBase, IADGraphGroupsService
    {
        public ADGraphGroupsService(ITokenAcquisition tokenAcquisition, IMapper mapper) :
            base(tokenAcquisition, mapper)
        {

        }

        public async Task<UserDTO?> GetUserAsync(string UserPrincipalName)
        {
            var user = await graphServiceClient.Users.Request().Filter($"userprincipalname eq '{UserPrincipalName}'").GetAsync();

            if (user == null)
                return default(UserDTO?);

            return mapper.Map<UserDTO>(user.First());
        }

        public async Task<GroupDTO?> GetGroupAsync(string DisplayName)
        {
            var group = await graphServiceClient.Groups.Request().Filter($"displayname eq '{DisplayName}'").GetAsync();

            if (group.Count == 0)
                return default(GroupDTO?);

            return mapper.Map<GroupDTO>(group.First());
        }

        public async Task<bool> IsAdminUnitMemberAsync(string AdminUnitid, string Memberid)
        {
            try
            {
                var Result = await graphServiceClient.Directory.AdministrativeUnits[AdminUnitid].Members[Memberid].Request().GetAsync();
                return Result != null;
            }
            catch (ServiceException se)
            {
                if (se.StatusCode == HttpStatusCode.NotFound)
                    return false;
            }

            return false;
        }

        public async Task<bool> IsAADRoleMemberAsync(string roleId, string Memberid)
        {
            try
            {
                var Result = await graphServiceClient.DirectoryRoles[roleId].Members[Memberid].Request().GetAsync();
                return Result != null;
            }
            catch (ServiceException se)
            {
                if (se.StatusCode == HttpStatusCode.NotFound)
                    return false;
            }

            return false;
        }

        public async Task<bool> IsGroupMemberAsync(string groupId, string Memberid)
        {
            try
            {
                var Result = await graphServiceClient.Groups[groupId].Members[Memberid].Request().GetAsync();
                return Result != null;
            }
            catch (ServiceException se)
            {
                if (se.StatusCode == HttpStatusCode.NotFound )
                    return false;
            }

            return false;
        }

        public async Task<GroupDTO?> GetGroupMembersAsync(string DisplayName)
        {
            throw new NotImplementedException();
            var group = await graphServiceClient.Groups.Request().Filter($"displayname eq '{DisplayName}'").GetAsync();

            if (group.Count == 0)
                return default(GroupDTO?);

            return mapper.Map<GroupDTO>(group.First());
        }

        public async Task<GroupDTO?> GetAdminUnitMembersAsync(string DisplayName)
        {
            throw new NotImplementedException();
            var group = await graphServiceClient.Groups.Request().Filter($"displayname eq '{DisplayName}'").GetAsync();

            if (group.Count == 0)
                return default(GroupDTO?);

            return mapper.Map<GroupDTO>(group.First());
        }

        public async Task<GroupDTO?> GetAadRoleMembersAsync(string DisplayName)
        {
            throw new NotImplementedException();
            var group = await graphServiceClient.Groups.Request().Filter($"displayname eq '{DisplayName}'").GetAsync();

            if (group.Count == 0)
                return default(GroupDTO?);

            return mapper.Map<GroupDTO>(group.First());
        }

        public async Task<GroupDTO> CreateGroupAsync(NewGroupDTO newGroup)
        {
            var group = mapper.Map<Group>(newGroup);

            group.SecurityEnabled = true;
            group.MailEnabled = false;
            group.MailNickname = newGroup.DisplayName.Replace(" ", "");

            var result = await graphServiceClient.Groups.Request().AddAsync(group);

            return mapper.Map<GroupDTO>(result) ;
        }

        public async Task<GroupDTO> CreateM365GroupAsync(NewGroupDTO newGroup)
        {
            var group = mapper.Map<Group>(newGroup);

            group.IsAssignableToRole = false;
            group.SecurityEnabled = true;
            group.MailEnabled = true;
            group.MailNickname = newGroup.DisplayName.Replace(" ", "");
            group.GroupTypes = new string[] { "Unified" };
            var result = await graphServiceClient.Groups.Request().AddAsync(group);
            return mapper.Map<GroupDTO>(result);
        }

        public async Task<GroupDTO?> CreateM365GroupAsync(NewGroupDTO newGroup, string AdminUnitId)
        {
            var group = mapper.Map<Group>(newGroup);
            group.MailEnabled = true;
            group.MailNickname = newGroup.DisplayName.Replace(" ", "");
            group.GroupTypes = new string[] { "Unified" };

            var GroupMember = new Dictionary<string, object>
            {
                {
                    "@odata.type" , "#microsoft.graph.group"
                },
                {
                    "description" , group.Description
                },
                {
                    "displayName" , group.DisplayName
                },
                {
                    "groupTypes" , new List<string>
                    {
                        "Unified"
                    }
                },
                {
                    "mailEnabled" , group.MailEnabled
                },
                {
                    "mailNickname" , group.MailNickname
                },
                {
                    "securityEnabled" , false
                },
                {
                    "IsAssignableToRole", false
                }
            };

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"https://graph.microsoft.com/v1.0/directory/administrativeUnits/{AdminUnitId}/members"),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonSerializer.Serialize(GroupMember), Encoding.UTF8, "application/json")
            };

            var token = tokenAcquisition.GetAuthenticationResultForAppAsync("https://graph.microsoft.com/.default").Result;

            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.AccessToken);

            var response = graphServiceClient.HttpProvider.SendAsync(request).Result;

            var result = await JsonSerializer.DeserializeAsync<Group>(response.Content.ReadAsStream());

            if (result == null)
                return default(GroupDTO);

            return mapper.Map<GroupDTO>(result);

        }

        public async Task<GroupDTO?> CreateGroupAsync(NewGroupDTO newGroup, string AdminUnitId)
        {
            var group = mapper.Map<Group>(newGroup);

            group.SecurityEnabled = true;
            group.MailEnabled = false;
            group.MailNickname = newGroup.DisplayName.Replace(" ", "");

            var GroupMember = new Dictionary<string, object>
            {
                {
                    "@odata.type" , "#microsoft.graph.group"
                },
                {
                    "description" , group.Description
                },
                {
                    "displayName" , group.DisplayName
                },
                //{
                //    "groupTypes" , new List<string>
                //    {
                        //"Unified",
                //        "Security"
                //    }
                //},
                {
                    "mailEnabled" , false
                },
                {
                    "mailNickname" , group.MailNickname
                },
                {
                    "securityEnabled" , true
                },
                {
                    "IsAssignableToRole", group.IsAssignableToRole.HasValue && group.IsAssignableToRole.Value
                }
            };

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"https://graph.microsoft.com/v1.0/directory/administrativeUnits/{AdminUnitId}/members"),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonSerializer.Serialize(GroupMember), Encoding.UTF8, "application/json")
            };

            var token = tokenAcquisition.GetAuthenticationResultForAppAsync("https://graph.microsoft.com/.default").Result;

            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.AccessToken);

            var response = graphServiceClient.HttpProvider.SendAsync(request).Result;

            var result = await JsonSerializer.DeserializeAsync<Group>(response.Content.ReadAsStream());

            if (result == null)
                return default(GroupDTO);

            return mapper.Map<GroupDTO>(result);

        }

        public async Task DeleteGroupAsync(string id)
        {
            await graphServiceClient.Groups[id].Request().DeleteAsync();
        }

        public async Task AddGroupMemberAsync(string GroupId, string MemberId)
        {
            var obj = graphServiceClient.DirectoryObjects[MemberId].Request().GetAsync();

            if (obj == null || obj.Result == null)
                throw new ObjectNotFoundException();

            await graphServiceClient.Groups[GroupId].Members.References.Request().AddAsync(obj.Result);

        }

        public async Task<RoleDTO?>  GetAADRoleAsync(string DisplayName)
        {
            var group = await graphServiceClient.DirectoryRoles.Request().Filter($"displayname eq '{DisplayName}'").GetAsync();

            if (group == null)
                return default(RoleDTO);

            return mapper.Map<RoleDTO>(group.First());
        }

        public async Task<IEnumerable<RoleDTO>> GetAADRoles()
        {
            var groups = await graphServiceClient.DirectoryRoles.Request().GetAsync();
            return mapper.Map<IEnumerable<RoleDTO>>(groups.ToList());
        }

        public async Task RemoveGroupMemberAsync(string GroupId, string MemberId)
        {
            var obj = graphServiceClient.DirectoryObjects[MemberId].Request().GetAsync();

            if (obj == null || obj.Result == null)
                throw new ObjectNotFoundException();

            await graphServiceClient.Groups[GroupId].Members[MemberId].Reference.Request().DeleteAsync();
        }

        public async Task AssignGroupToRoleAsync(string RoleId, string GroupId)
        {
            var obj = graphServiceClient.DirectoryObjects[GroupId].Request().GetAsync();

            await graphServiceClient.DirectoryRoles[RoleId].Members.References.Request().AddAsync(obj.Result);

        }

        public async Task<IEnumerable<DirectoryObjectDTO>> GetAADRoleMembersAsync(string RoleId)
        {
            var members = await graphServiceClient.DirectoryRoles[RoleId].Members.Request().GetAsync();

            var DirectoryObjects = new List<DirectoryObjectDTO>();

            foreach (var member in members.CurrentPage)
            {
                var memberDTO = mapper.Map<DirectoryObjectDTO>(member);

                if (member is Group)
                {
                    memberDTO.DisplayName = ((Group)member).DisplayName;
                    memberDTO.MemberType = "Group";
                    DirectoryObjects.Add(memberDTO);
                }
                else if (member is User)
                {
                    memberDTO.DisplayName = ((User)member).DisplayName;
                    memberDTO.MemberType = "User";
                    DirectoryObjects.Add(memberDTO);
                }
                else 
                {
                    memberDTO.MemberType = "Unknown";
                    DirectoryObjects.Add(memberDTO);
                }
            }

            return DirectoryObjects;
        }

        public async Task RemoveAADRoleMemberAsync(string RoleId, string MemberId)
        {
            await graphServiceClient.DirectoryRoles[RoleId].Members[MemberId].Reference.Request().DeleteAsync();
        }

        public async Task<AdminUnitDTO?> GetAADAdminUnit(string adminUnitName)
        {
            var result = await graphServiceClient.Directory.AdministrativeUnits.Request().Filter($"DisplayName eq '{adminUnitName}'").GetAsync();

            if (result == null || result.Count != 1)
                return default(AdminUnitDTO);

            return mapper.Map<AdminUnitDTO>(result.First());
        }

        public async Task AddAdminUnitMemberAsync(string AdminUnitId, string MemberId)
        {
            var obj = graphServiceClient.DirectoryObjects[MemberId].Request().GetAsync();

            await graphServiceClient.Directory.AdministrativeUnits[AdminUnitId].Members.References.Request().AddAsync(obj.Result);

        }

        public async Task RemoveAdminUnitMemberAsync(string AdminUnitId, string MemberId)
        {
            await graphServiceClient.Directory.AdministrativeUnits[AdminUnitId].Members[MemberId].Reference.Request().DeleteAsync();
        }
    }
}
