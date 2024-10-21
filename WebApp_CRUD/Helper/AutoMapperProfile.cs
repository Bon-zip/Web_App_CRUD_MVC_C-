using AutoMapper;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using WebApp_CRUD.Data;
using WebApp_CRUD.Models.ViewModels;

namespace WebApp_CRUD.Helper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() 
        {
            CreateMap<RegisterViewModel, User>();
        }
    }
}
