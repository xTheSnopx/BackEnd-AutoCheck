using AutoMapper;
using AutoCheckAML.Api.Entity;
using AutoCheckAML.Api.Web.DTOs;

namespace AutoCheckAML.Api.Web.Mapping
{
    /// <summary>
    /// Perfil de mapeo AutoMapper - Define conversiones entre entidades y DTOs
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, LoginResponse>()
                .ForMember(dest => dest.Token, opt => opt.Ignore());

            CreateMap<User, RegisterResponse>();

            // FormSubmission mappings
            CreateMap<FormSubmissionRequest, FormSubmission>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pendiente"))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<FormSubmission, FormSubmissionResponse>();
        }
    }
}
