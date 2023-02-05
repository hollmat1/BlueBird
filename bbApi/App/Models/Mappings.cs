using AutoMapper;
using Microsoft.Graph;

namespace bbApi.App.Models
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Group, GroupDTO>();
            CreateMap<User, UserDTO>();
            CreateMap<RoleDefinition, RoleDTO>();

            CreateMap<NewGroupDTO, Group>();
            CreateMap<UserDTO, User>();
            CreateMap<RoleDTO, RoleDefinition>();
        }
    }
}
