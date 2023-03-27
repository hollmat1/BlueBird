using AutoMapper;
using bbApi.App.Models;
using Microsoft.Graph;
using Microsoft.Identity.Web;

namespace bbApi.App.Services
{
    public class ADGraphApplicationsService : ADGraphServiceBase
    {
        public ADGraphApplicationsService(ITokenAcquisition tokenAcquisition, IMapper mapper) :
            base(tokenAcquisition, mapper)
        {

        }

        public async Task<ApplicationDTO> CreateAppRegistrationAsync(NewApplicationDTO newApp)
        {
            var newapp = mapper.Map<Application>(newApp);
            var result = await graphServiceClient.Applications.Request().AddAsync(newapp);
            return mapper.Map<ApplicationDTO>(result);
        }

        public async Task<ApplicationDTO> GetAppRegistrationAsync(string AppDisplayName)
        {
            var result = await graphServiceClient.Applications.Request().Filter($"DisplayName eq '{AppDisplayName}'").GetAsync();
            return mapper.Map<ApplicationDTO>(result.FirstOrDefault());
        }
    }
}
