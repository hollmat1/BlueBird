using bbApi.App.Models;
using bbApi.App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace bbApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MSGraphController : ControllerBase
    {
        private readonly IADGraphGroupsService graphService;
        private readonly ITokenAcquisition tokenaquisition;

        public MSGraphController(IADGraphGroupsService graphService, ITokenAcquisition tokenaquisition)
        {
            this.graphService = graphService;
            this.tokenaquisition = tokenaquisition;
        }

        // GET: api/<MSGraphController>
        [HttpGet("users/{UserPrincipalName}")]
        [ProducesResponseType(typeof(UserDTO), 200)]
        public async Task<IActionResult> GetUserAsync(string UserPrincipalName)
        {
            var res = graphService.GetUserAsync(UserPrincipalName).Result;

            if (res == null)
                return new NotFoundResult();

            return Ok(res);
        }

        // GET: api/<MSGraphController>
        [HttpGet("groups/{DisplayName}")]
        [ProducesResponseType(typeof(GroupDTO), 200)]
        public async Task<IActionResult> GetGroupAsync(string DisplayName)
        {
            var res = await graphService.GetGroupAsync(DisplayName);

            if (res == null)
                return new NotFoundResult();

            return Ok(res);
        }


        // POST api/<MSGraphController>
        [HttpPost("groups")]
        [ProducesResponseType(typeof(GroupDTO), (int)HttpStatusCode.Created)]
        public async Task<IActionResult> PostCreateGroup([FromBody] NewGroupDTO newGroup)
        {
            await graphService.CreateGroupAsync(newGroup);
            return Ok();
        }

        // POST api/<MSGraphController>
        [HttpPost("groups/{GroupId}/members")]
        [ProducesResponseType(typeof(GroupDTO), (int)HttpStatusCode.Created)]
        public async Task<IActionResult> PostAddMember(string GroupId, [FromBody] string memberid)
        {
            await graphService.AddGroupMemberAsync(GroupId, memberid);
            return Ok();
        }

        // PUT api/<MSGraphController>/5
        [HttpPut("groups/{id}")]
        public void Put(string id, [FromBody] string value)
        {

        }

        // DELETE api/<MSGraphController>/5
        [HttpDelete("groups/{id}")]
        [ProducesResponseType(typeof(GroupDTO), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Delete(string id)
        {
            graphService.DeleteGroupAsync(id);
            return Ok();
        }
    }
}
