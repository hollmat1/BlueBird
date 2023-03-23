using AutoMapper;
using AutoMapper.Execution;
using bbApi.App.Infrastructure;
using bbApi.App.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;

namespace bbApi.App.Services
{
    public class ADGraphService : IADGraphService
    {
        private GraphServiceClient graphServiceClient;
        private readonly ITokenAcquisition tokenAcquisition;
        private readonly IMapper mapper;

        public ADGraphService(ITokenAcquisition tokenAcquisition, IMapper mapper)
        {
            this.tokenAcquisition = tokenAcquisition;
            this.mapper = mapper;
            PrepareAuthenticatedClient();
        }

        public async Task<UserDTO> GetUserAsync(string UserPrincipalName)
        {
            var user = await graphServiceClient.Users.Request().Filter($"userprincipalname eq '{UserPrincipalName}'").GetAsync();

            if (user == null || user.Count != 1)
                return null;

            return mapper.Map<UserDTO>(user.First());
        }

        public async Task<GroupDTO> GetGroupAsync(string DisplayName)
        {
            var group = await graphServiceClient.Groups.Request().Filter($"displayname eq '{DisplayName}'").GetAsync();

            if (group == null || group.Count != 1)
                return null;

            return mapper.Map<GroupDTO>(group.First());
        }

        public async void CreateGroupAsync(NewGroupDTO newGroup)
        {
            var group = mapper.Map<Group>(newGroup);

            group.SecurityEnabled = true;
            group.MailEnabled = false;
            group.MailNickname = newGroup.DisplayName.Replace(" ", "");

            await graphServiceClient.Groups.Request().AddAsync(group);
        }

        public async void CreateGroupAsync(string AdminId, string MemberId)
        {
 
            //Microsoft.Graph.DirectoryNamespace.AdministrativeUnits.Item.Members.MembersPostRequestBody

            //        var requestBody = new Microsoft.Graph.DirectoryNamespace.AdministrativeUnits.Item.Members.MembersPostRequestBody
            //        {
            //            AdditionalData = new Dictionary<string, object>
            //{
            //    {
            //        "@odata.type" , "#microsoft.graph.group"
            //    },
            //    {
            //        "description" , "Self help community for golf"
            //    },
            //    {
            //        "displayName" , "Golf Assist"
            //    },
            //    {
            //        "groupTypes" , new List<string>
            //        {
            //            "Unified",
            //        }
            //    },
            //    {
            //        "mailEnabled" , true
            //    },
            //    {
            //        "mailNickname" , "golfassist"
            //    },
            //    {
            //        "securityEnabled" , false
            //    },
            //},
            //        };
            //        await graphServiceClient.Directory.AdministrativeUnits["{administrativeUnit-id}"].Members.PostAsync(requestBody);

        }

        public async void DeleteGroupAsync(string id)
        {
            await graphServiceClient.Groups[id].Request().DeleteAsync();
        }

        public async void AddGroupMembership(string GroupId, string MemberId)
        {
            var obj = graphServiceClient.DirectoryObjects[MemberId].Request().GetAsync();

            if (obj == null || obj.Result == null)
                throw new ObjectNotFoundException();

            await graphServiceClient.Groups[GroupId].Members.References.Request().AddAsync(obj.Result);

        }

        public async Task<RoleDTO>  GetAADRoleAsync(string DisplayName)
        {
            var group = await graphServiceClient.DirectoryRoles.Request().Filter($"displayname eq '{DisplayName}'").GetAsync();

            if (group == null || group.Count != 1)
                return null;

            return mapper.Map<RoleDTO>(group.First());
        }

        public async Task<IEnumerable<RoleDTO>> GetAADRoles()
        {
            var groups = await graphServiceClient.DirectoryRoles.Request().GetAsync();
            return mapper.Map<IEnumerable<RoleDTO>>(groups.ToList());
        }

        private void PrepareAuthenticatedClient()
        {
            if (graphServiceClient == null)
            {
                // Create Microsoft Graph client.
                // https://learn.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=CS#client-credentials-provider
                try
                {
                    var token = tokenAcquisition.GetAuthenticationResultForAppAsync("https://graph.microsoft.com/.default").Result;

                    var proxyAddress = "http://localhost:8888";

                    // Create a new System.Net.Http.HttpClientHandler with the proxy
                    var handler = new HttpClientHandler
                    {
                        // Create a new System.Net.WebProxy
                        // See WebProxy documentation for scenarios requiring
                        // authentication to the proxy
                        Proxy = new WebProxy(new Uri(proxyAddress))
                    };

                    //var httpClient = GraphClientFactory.Create(proxy: new WebProxy(new Uri(proxyAddress)));

                    graphServiceClient = new GraphServiceClient("https://graph.microsoft.com/v1.0/",
                                                                         new DelegateAuthenticationProvider(
                                                                             async (requestMessage) =>
                                                                             {
                                                                                 await Task.Run(() =>
                                                                                 {
                                                                                     requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.AccessToken);
                                                                                 });
                                                                             }));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Could not create a graph client {ex}");
                }
            }
        }

        public async void RemoveGroupMembershipAsync(string GroupId, string MemberId)
        {
            var obj = graphServiceClient.DirectoryObjects[MemberId].Request().GetAsync();

            if (obj == null || obj.Result == null)
                throw new ObjectNotFoundException();

            await graphServiceClient.Groups[GroupId].Members[MemberId].Reference.Request().DeleteAsync();
        }

        public async void AssignGroupToRoleAsync(string RoleId, string GroupId)
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

        public async void RemoveAADRoleMembershipAsync(string RoleId, string MemberId)
        {
            await graphServiceClient.DirectoryRoles[RoleId].Members[MemberId].Reference.Request().DeleteAsync();
        }
    }
}
