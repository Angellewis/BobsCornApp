using AutoMapper;
using BobsCornApp.Application.Dtos;
using BobsCornApp.Domain.Entities;

namespace BobsCornApp.Application.Mapping;

public class CornMappingProfile : Profile
{
    public CornMappingProfile()
    {
        CreateMap<CornPurchase, CornPurchaseResponseDto>()
            .ForMember(destination => destination.Message, options => options.MapFrom(_ => "Corn purchased successfully."));
    }
}
