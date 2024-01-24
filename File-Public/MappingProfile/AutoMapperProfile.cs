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

            CreateMap<DocTypeLU, VmDocTypeLU>();
            CreateMap<VmDocTypeLU, DocTypeLU>();
       }
    }
}
