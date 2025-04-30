using AutoMapper;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Domain.Entities;

// AutoMapper Profile
namespace DocumentManagementML.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Document mappings
            CreateMap<Document, DocumentDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DocumentId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.DocumentName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.FilePath, opt => opt.MapFrom(src => src.FileLocation))
                .ForMember(dest => dest.FileType, opt => opt.MapFrom(src => src.FileType))
                .ForMember(dest => dest.FileSize, opt => opt.MapFrom(src => src.FileSizeBytes))
                .ForMember(dest => dest.UploadDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.LastModifiedDate, opt => opt.MapFrom(src => src.LastModifiedDate))
                .ForMember(dest => dest.DocumentTypeId, opt => opt.MapFrom(src => src.DocumentTypeId))
                .ForMember(dest => dest.DocumentTypeName, opt => opt.MapFrom(src => src.DocumentType != null ? src.DocumentType.Name : null))
                .ForMember(dest => dest.UploadedById, opt => opt.MapFrom(src => src.UploadedById))
                .ForMember(dest => dest.UploadedByName, opt => opt.MapFrom(src => src.UploadedBy != null ? src.UploadedBy.Username : null))
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.MetadataDictionary))
                .ForMember(dest => dest.MetadataItems, opt => opt.MapFrom(src => src.MetadataItems));
            
            CreateMap<DocumentCreateDto, Document>()
                .ForMember(dest => dest.DocumentName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.DocumentTypeId, opt => opt.MapFrom(src => src.DocumentTypeId))
                .ForMember(dest => dest.MetadataDictionary, opt => opt.Ignore());
            
            CreateMap<DocumentUpdateDto, Document>()
                .ForMember(dest => dest.DocumentName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.DocumentTypeId, opt => opt.MapFrom(src => src.DocumentTypeId))
                .ForMember(dest => dest.MetadataDictionary, opt => opt.Ignore());
            
            // DocumentType mappings
            CreateMap<DocumentType, DocumentTypeDto>();
            CreateMap<DocumentTypeCreateDto, DocumentType>();
            CreateMap<DocumentTypeUpdateDto, DocumentType>();
            
            // ML mappings
            CreateMap<ClassificationResult, DocumentClassificationResultDto>()
                .ForMember(dest => dest.PredictedDocumentTypeName, opt => opt.MapFrom(src => src.PredictedDocumentType != null ? src.PredictedDocumentType.Name : null))
                .ForMember(dest => dest.DocumentTypeScores, opt => opt.MapFrom(src => src.AllScores));
            
            CreateMap<DocumentTypeScore, DocumentTypeScoreDto>()
                .ForMember(dest => dest.DocumentTypeName, opt => opt.MapFrom(src => src.DocumentType != null ? src.DocumentType.Name : null));
            
            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username));
            
            CreateMap<UserCreateDto, User>();
            CreateMap<UserUpdateDto, User>();
        }
    }
}