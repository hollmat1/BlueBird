using AutoMapper;
using bbApi.App.Infrastructure;
using bbApi.App.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Diagnostics;
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
        }

        public async Task<UserDTO> GetUserAsync(string UserPrincipalName)
        {
            PrepareAuthenticatedClient();

            var user = await graphServiceClient.Users.Request().Filter($"userprincipalname eq '{UserPrincipalName}'").GetAsync();

            if (user == null || user.Count != 1)
                return null;

            return mapper.Map<UserDTO>(user.First());
        }

        public async Task<GroupDTO> GetGroupAsync(string DisplayName)
        {
            PrepareAuthenticatedClient();

            var group = await graphServiceClient.Groups.Request().Filter($"displayname eq '{DisplayName}'").GetAsync();

            if (group == null || group.Count != 1)
                return null;

            return mapper.Map<GroupDTO>(group.First());
        }

        public async void CreateGroupAsync(NewGroupDTO newGroup)
        {
            PrepareAuthenticatedClient();

            var group = mapper.Map<Group>(newGroup);

            group.SecurityEnabled = true;
            group.MailEnabled = false;
            group.MailNickname = newGroup.DisplayName.Replace(" ", "");

            await graphServiceClient.Groups.Request().AddAsync(group);
        }

        public async void DeleteGroupAsync(string id)
        {
            PrepareAuthenticatedClient();

            await graphServiceClient.Groups[id].Request().DeleteAsync();
        }

        public async void AddGroupMembership(string GroupId, string MemberId)
        {
            PrepareAuthenticatedClient();

            var obj = graphServiceClient.DirectoryObjects[MemberId].Request().GetAsync();

            if (obj == null || obj.Result == null)
                throw new ObjectNotFoundException();

            await graphServiceClient.Groups[GroupId].Members.References.Request().AddAsync(obj.Result);

        }

        private void PrepareAuthenticatedClient()
        {
            if (graphServiceClient == null)
            {
                // Create Microsoft Graph client.

                try
                {
                    var token = tokenAcquisition.GetAuthenticationResultForAppAsync("https://graph.microsoft.com/.default").Result;

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
    }
}
