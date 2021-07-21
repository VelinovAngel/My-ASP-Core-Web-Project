﻿namespace BikesBooking.Services.Data
{
    using System.Threading.Tasks;

    using BikesBooking.Services.Data.DTO.Dealers;

    public interface IDealersService
    {
        bool IsAlreadyPublicDealerExist(string id);

        Task CreatePublicDealer(CreateDealerDto dealer, string userId);
    }
}
