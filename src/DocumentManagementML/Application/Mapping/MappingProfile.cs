using AutoMapper;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Domain.Entities;

namespace DocumentManagementML.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Document mappings
            CreateMap<Document, DocumentDto>()
                .ForMember(dest => dest.DocumentTypeName, opt => opt.MapFrom(src => src.DocumentType != null ? src.DocumentType.Name : null))
                .ForMember(dest => dest.UploadedByName, opt => opt.MapFrom(src => src.UploadedBy != null ? src.UploadedBy.Name : null));
            
            CreateMap<DocumentCreateDto, Document>();
            CreateMap<DocumentUpdateDto, Document>();
            
            // DocumentType mappings
            CreateMap<DocumentType, DocumentTypeDto>();
            CreateMap<DocumentTypeCreateDto, DocumentType>();
            CreateMap<DocumentTypeUpdateDto, DocumentType>();
            
            // ML mappings
            CreateMap<ClassificationResult, DocumentClassificationResultDto>()
                .ForMember(dest => dest.PredictedDocumentTypeName, opt => opt.MapFrom(src => src.PredictedDocumentType != null ? src.PredictedDocumentType.Name : null));
            
            CreateMap<DocumentTypeScore, DocumentTypeScoreDto>();
        }
    }
} 