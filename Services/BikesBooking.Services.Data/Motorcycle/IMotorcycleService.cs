﻿namespace BikesBooking.Services.Data.Motorcycle
{
    using System;
    using System.Threading.Tasks;

    using BikesBooking.Services.Data.DTO.Clients;
    using BikesBooking.Services.Data.DTO.MotorcycleModels;

    public interface IMotorcycleService
    {
        Task<int> CreateMotorcycleAsync(MotorcycleServiceDto createMotorcycle, int dealerId);

        Task<bool> Edit(MotorcycleServiceDto motorcycle, int id);

        Task<MotorcycleQueryServiceModel> GetCollectionOfMotorsAsync(
            int currentPage,
            int motorcyclesPerPage,
            int dealerId);

        Task<MotorcycleQueryServiceModel> GetFreeMotors(
            int currentPage,
            int motorcyclesPerPage,
            SearchMotorcycleInputModel inputModel);

        Task<OfferSigleMotorcycleDto> GetMotorcycleByIdAsync(int id);

        Task ChangeStateOfMotorcycle(bool available, int id);

        Task OfferCurrentMotor(OfferPeriodForMotorDto offerPeriodForMotorDto, int id);

        Task<bool> CancelCurrentOffer(int motorcycleId);

        Task RemoveMotorcycleAsync(int id);

        bool IsInActiveOffer(int motorcycleId);

        MotorcycleDetailViewModel Details(int id);

        MotorcycleDetailViewModel DetailsForEdit(int id);

        bool IsByDealer(int motorId, int dealerId);

        public int GetMotorcycleCount();

        public int GetNotAvailableMotorcycleCount();
    }
}
