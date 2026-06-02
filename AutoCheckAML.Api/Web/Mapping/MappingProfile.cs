using AutoMapper;
using AutoCheckAML.Api.Entity;
using AutoCheckAML.Api.Web.DTOs;
using System.Collections.Generic;
using System.Linq;

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

            CreateMap<User, UserDto>()
                .ForMember(dest => dest.CrewName, opt => opt.MapFrom(src => src.Crew != null ? src.Crew.Name : null))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles != null ? src.UserRoles.Select(ur => ur.Role.Name).ToList() : new List<string>()));

            // Role mappings
            CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.RolePermissions != null ? src.RolePermissions.Select(rp => rp.Permission.Name).ToList() : new List<string>()));
            CreateMap<CreateRoleRequest, Role>();
            CreateMap<UpdateRoleRequest, Role>();

            // Permission mappings
            CreateMap<Permission, PermissionDto>();

            // Crew mappings
            CreateMap<Crew, CrewDto>()
                .ForMember(dest => dest.ManagedByUserName, opt => opt.MapFrom(src => src.ManagedByUser != null ? src.ManagedByUser.FullName : null));
            CreateMap<CreateCrewRequest, Crew>();
            CreateMap<UpdateCrewRequest, Crew>();

            // FormTemplate mappings
            CreateMap<FormTemplate, FormTemplateDto>()
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.FullName : null));
            CreateMap<CreateFormTemplateRequest, FormTemplate>();
            CreateMap<UpdateFormTemplateRequest, FormTemplate>();

            // FormField mappings
            CreateMap<FormField, FormFieldDto>();
            CreateMap<CreateFormFieldRequest, FormField>();
            CreateMap<UpdateFormFieldRequest, FormField>();

            // FormSubmission mappings (nueva arquitectura)
            CreateMap<FormSubmission, FormSubmissionDto>()
                .ForMember(dest => dest.FormTemplateName, opt => opt.MapFrom(src => src.FormTemplate != null ? src.FormTemplate.Name : null))
                .ForMember(dest => dest.SubmittedByUserName, opt => opt.MapFrom(src => src.SubmittedByUser != null ? src.SubmittedByUser.FullName : null))
                .ForMember(dest => dest.AssignedToCrewName, opt => opt.MapFrom(src => src.AssignedToCrew != null ? src.AssignedToCrew.Name : null))
                .ForMember(dest => dest.VerifiedByUserName, opt => opt.MapFrom(src => src.VerifiedByUser != null ? src.VerifiedByUser.FullName : null));
            CreateMap<CreateFormSubmissionRequest, FormSubmission>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pendiente"))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // Answer mappings
            CreateMap<Answer, AnswerDto>()
                .ForMember(dest => dest.FormFieldLabel, opt => opt.MapFrom(src => src.FormField != null ? src.FormField.Label : null));
            CreateMap<CreateAnswerRequest, Answer>();

            // Attachment mappings
            CreateMap<Attachment, AttachmentDto>()
                .ForMember(dest => dest.FormFieldLabel, opt => opt.MapFrom(src => src.FormField != null ? src.FormField.Label : null))
                .ForMember(dest => dest.UploadedByUserName, opt => opt.MapFrom(src => src.UploadedByUser != null ? src.UploadedByUser.FullName : null));
            CreateMap<CreateAttachmentRequest, Attachment>();

            // AuditLog mappings
            CreateMap<AuditLog, AuditLogDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null));

            // ExportHistory mappings
            CreateMap<ExportHistory, ExportHistoryDto>()
                .ForMember(dest => dest.ExportedByUserName, opt => opt.MapFrom(src => src.ExportedByUser != null ? src.ExportedByUser.FullName : null));
        }
    }
}

