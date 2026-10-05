using AutoMapper;
using QuickDesk.Application.Operations.Commands.Requests;
using QuickDesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Application.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile() 
        {
            #region Commands
            CreateMap<AddUserCommand, AddUser>();
            CreateMap<SignInCommand, SignIn>();
            CreateMap<CreateJobCommand, CreateJob>();
            #endregion
        }
    }
}
