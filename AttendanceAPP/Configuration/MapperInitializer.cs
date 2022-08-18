using AttendanceAPP.DTOs;
using AttendanceAPP.Model;
using AutoMapper;

namespace AttendanceAPP.Configuration
{
    public class MapperInitializer : Profile
    {
        public MapperInitializer()
        {
            CreateMap<UserModel, UserDTO>().ReverseMap();
            CreateMap<UserModel, UserCreateDTO>().ReverseMap();
        }
    }
}
