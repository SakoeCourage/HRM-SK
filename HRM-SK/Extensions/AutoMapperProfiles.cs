using AutoMapper;
using HRM_SK.Entities;
using HRM_SK.Entities.Staff;
using static App_Setup.Grade.AddGrade;
using static HRM_BACKEND_VSA.Domains.Staffs.Staff_Bio.AddStaffBio;
using static HRM_SK.Contracts.StaffContracts;
using static HRM_SK.Contracts.UserContracts;
using static HRM_SK.Features.Staff_Management.UpdateStaffBio;

namespace HRM_SK.Extensions
{
  public class AutoMapperProfiles : Profile
  {
    public AutoMapperProfiles()
    {
      CreateMap<AddGradeRequest, Grade>().ReverseMap();
      CreateMap<Staff, StaffLoginResponse>().ReverseMap();
      CreateMap<Staff, AuthStaffReponse>().ReverseMap();
      CreateMap<User, UserLoginResponse>().ReverseMap();
      CreateMap<StaffAppointment, StaffAppointmentHistory>().ReverseMap();
      CreateMap<StaffAccomodationDetail, StaffAccomodationUpdateHistory>().ReverseMap();
      CreateMap<Staff, StaffBioUpdateHistory>()
          .ForMember(dest => dest.Id, opt => opt.Ignore())
          ;
      CreateMap<StaffBankDetail, StaffBankUpdateHistory>()
          .ForMember(dest => dest.Id, opt => opt.Ignore());

      CreateMap<StaffAccomodationDetail, StaffAccomodationUpdateHistory>()
         .ForMember(dest => dest.Id, opt => opt.Ignore());

      CreateMap<StaffProfessionalLincense, StaffProfessionalLincenseUpdateHistory>()
    .ForMember(dest => dest.Id, opt => opt.Ignore());

      CreateMap<StaffFamilyDetail, StaffFamilyUpdatetHistory>()
    .ForMember(dest => dest.Id, opt => opt.Ignore());

      CreateMap<StaffChildrenDetail, StaffChildrenUpdateHistory>()
    .ForMember(dest => dest.Id, opt => opt.Ignore());

      CreateMap<staffAccomodationResponseDto, StaffAccomodationDetail>().ReverseMap();
      CreateMap<StaffBioUpdateHistory, Staff>();
      CreateMap<Staff, StaffProfileResponse>().ReverseMap();
      CreateMap<Speciality, StaffSpecialityResponseDto>().ReverseMap();
      CreateMap<UnitResponseDto, Unit>().ReverseMap();
      CreateMap<StaffBankDetail, StaffBankResponseDto>().ReverseMap();
      CreateMap<StaffFamilyResponseDto, StaffFamilyDetail>().ReverseMap();
      CreateMap<StaffProfessionalLicenseDto, StaffProfessionalLincense>().ReverseMap();
      CreateMap<staffChildrenResponseDto, StaffChildrenDetail>().ReverseMap();
      CreateMap<StaffAppointmentResponseDto, StaffAppointment>().ReverseMap();
      CreateMap<StaffPosting, staffPostingResponseDto>().ReverseMap();
      CreateMap<AddStaffBioRequest, UpdateStaffBioRequest>().ReverseMap();
    }
  }
}
