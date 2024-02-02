using AutoMapper;
using File_Public.DbModels;
using File_Public.Models;

namespace File_Public.MappingProfile
{
    public class AutoMapperProfile : Profile
    {
       public AutoMapperProfile() {
            CreateMap<Document,VmDocument>();
            CreateMap<VmDocument, Document>();

            CreateMap<DocGroupLU, VmDocGroupLU>();
            CreateMap<VmDocGroupLU, DocGroupLU>();

            CreateMap<GetFileDto, VmFileNameAndExtension>()
                .ForMember(m => m.DocGroup, opt => opt.MapFrom(x => x.DocGroup))
                .ForMember(m => m.DocName, opt => opt.MapFrom(x => x.DocName))
                .ForMember(m => m.DocExt, opt => opt.MapFrom(x => x.DocExt))
                .ForMember(m => m.ClientId, opt => opt.MapFrom(x => x.ClientId))
                .ForMember(m => m.DocDate, opt => opt.MapFrom(x => x.DocDate))
                .ForMember(m => m.Isin, opt => opt.MapFrom(x => x.ISIN))
                .ForMember(m => m.Language, opt => opt.MapFrom(x => x.Lang));

            CreateMap<VmFileNameAndExtension,GetFileDto>()
                .ForMember(m => m.DocGroup, opt => opt.MapFrom(x => x.DocGroup))
                .ForMember(m => m.DocName, opt => opt.MapFrom(x => x.DocName))
                .ForMember(m => m.DocExt, opt => opt.MapFrom(x => x.DocExt))
                .ForMember(m => m.ClientId, opt => opt.MapFrom(x => x.ClientId))
                .ForMember(m => m.DocDate, opt => opt.MapFrom(x => x.DocDate))
                .ForMember(m => m.ISIN, opt => opt.MapFrom(x => x.Isin))
                .ForMember(m => m.Lang, opt => opt.MapFrom(x => x.Language));
       }
    }
}
