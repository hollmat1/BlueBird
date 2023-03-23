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
            CreateMap<DirectoryRole, RoleDTO>();
            CreateMap<DirectoryObject, DirectoryObjectDTO>();

            CreateMap<NewGroupDTO, Group>();
            CreateMap<UserDTO, User>();
        }
    }
}
