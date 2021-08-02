﻿namespace BikesBooking.Services.Data.User
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;

    public interface IUserService
    {
        int GetTotalDeales();

        int GetTotalUsers();

        Task<IdentityResult> AssignRole(IServiceProvider services, string email, string role);
    }
}
