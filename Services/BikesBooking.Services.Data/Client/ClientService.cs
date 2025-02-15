﻿namespace BikesBooking.Services.Data.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BikesBooking.Data.Common.Repositories;
    using BikesBooking.Data.Models;
    using BikesBooking.Services.Data.DTO.Clients;
    using BikesBooking.Services.Data.DTO.MotorcycleModels;
    using BikesBooking.Services.Data.Votes;

    public class ClientService : IClientService
    {
        private readonly IRepository<Client> clientRepository;
        private readonly IRepository<Motorcycle> motorcyclesRepository;
        private readonly IRepository<Offer> offerRepository;
        private readonly IRepository<ClientsOffers> clientsOffersRepository;
        private readonly IRepository<ApplicationUser> userRepository;
        private readonly IRepository<Review> reviewRepository;
        private readonly IVoteService votesService;

        public ClientService(
            IRepository<Client> clientRepository,
            IRepository<Motorcycle> motorcyclesRepository,
            IRepository<Offer> offerRepository,
            IRepository<ClientsOffers> clientsOffersRepository,
            IRepository<ApplicationUser> userRepository,
            IRepository<Review> reviewRepository,
            IVoteService votesService)
        {
            this.clientRepository = clientRepository;
            this.motorcyclesRepository = motorcyclesRepository;
            this.offerRepository = offerRepository;
            this.clientsOffersRepository = clientsOffersRepository;
            this.userRepository = userRepository;
            this.reviewRepository = reviewRepository;
            this.votesService = votesService;
        }

        public async Task<bool> BookThisMotorcycleByClient(int clientId, int offerId, DateTime pickUp, DateTime dropOff, int motorcycleId)
        {
            var currMotorcycle = this.motorcyclesRepository.All()
                .FirstOrDefault(x => x.Id == motorcycleId);

            var currOffer = this.offerRepository.AllAsNoTracking()
                .Where(x => x.Id == offerId)
                .FirstOrDefault();

            if (currOffer == null)
            {
                return false;
            }

            if (pickUp < currOffer.PickUpDate && dropOff > currOffer.DropOffDate)
            {
                return false;
            }

            var currClient = this.clientRepository.AllAsNoTracking()
                .FirstOrDefault(x => x.Id == clientId);

            if (currOffer.DropOffDate >= dropOff)
            {
                currOffer.PickUpDate = dropOff.AddDays(1);
                if (currOffer.PickUpDate > currOffer.DropOffDate)
                {
                    currOffer.PickUpDate = currOffer.DropOffDate;
                    currMotorcycle.Available = false;
                    currMotorcycle.OfferId = null;
                }

                currOffer.StatisticsBooked++;
                currOffer.IsFree = true;
            }

            this.offerRepository.Update(currOffer);
            await this.offerRepository.SaveChangesAsync();

            var newOfferTable = new Offer
            {
                StatisticsBooked = 1,
                IsFree = false,
                PickUpDate = pickUp,
                DropOffDate = dropOff,
            };
            await this.offerRepository.AddAsync(newOfferTable);
            await this.offerRepository.SaveChangesAsync();

            var newBookingTimeSpan = new ClientsOffers
            {
                OfferId = newOfferTable.Id,
                ClientId = currClient.Id,
                MotorcycleId = motorcycleId,
            };

            await this.clientsOffersRepository.AddAsync(newBookingTimeSpan);
            await this.clientsOffersRepository.SaveChangesAsync();

            return true;
        }

        public async Task CreateClientAsync(string userId, string address, string city)
        {
            var firstName = this.userRepository.AllAsNoTracking()
                               .Where(x => x.Id == userId)
                               .FirstOrDefault().FirstName;
            var lastName = this.userRepository.AllAsNoTracking()
                               .Where(x => x.Id == userId)
                               .FirstOrDefault().LastName;
            var fullName = $"{firstName} {lastName}";
            var email = this.userRepository.AllAsNoTracking().Where(x => x.Id == userId).FirstOrDefault().Email;
            var completeAddress = $"{city} - {address}";
            var currClient = new Client
            {
                Name = fullName,
                Email = email,
                Address = completeAddress,
                UserId = userId,
            };

            await this.clientRepository.AddAsync(currClient);
            await this.clientRepository.SaveChangesAsync();
        }

        public async Task<bool> CancellationOfBookedMotorcycleByClient(int clientId, DateTime pickUp, DateTime dropOff, int motorcycleId)
        {
            if (pickUp >= DateTime.UtcNow)
            {
                return false;
            }

            var currMotorcycle = this.motorcyclesRepository.All()
                .FirstOrDefault(x => x.Id == motorcycleId);

            var motorcycleOffer = this.offerRepository.All()
                .FirstOrDefault(x => x.Id == currMotorcycle.OfferId);

            motorcycleOffer.PickUpDate = pickUp;
            this.offerRepository.Update(motorcycleOffer);
            await this.offerRepository.SaveChangesAsync();

            var currClientOffer = this.clientsOffersRepository.All()
                .FirstOrDefault(x => x.ClientId == clientId);
            if (currClientOffer == null)
            {
                return false;
            }

            var currOffer = this.offerRepository.All()
                .FirstOrDefault(x => x.Id == currClientOffer.OfferId);
            var currClient = this.clientRepository.All()
                .FirstOrDefault(x => x.Id == currClientOffer.ClientId);

            if (currOffer == null || currClient == null)
            {
                return false;
            }

            this.clientsOffersRepository.Delete(currClientOffer);
            await this.clientsOffersRepository.SaveChangesAsync();

            this.offerRepository.Delete(currOffer);
            await this.offerRepository.SaveChangesAsync();

            return true;
        }

        public IEnumerable<BookedMotorcycleDto> GetAllListOfMotorcycleByClietId(int clientId)
        {
            var currOffer = this.clientsOffersRepository.All()
                .Where(x => x.ClientId == clientId).ToList();

            var listOfmotorcycle = new List<BookedMotorcycleDto>();

            foreach (var bike in currOffer)
            {
                var motorcycle = this.motorcyclesRepository.All()
               .Where(x => x.Id == bike.MotorcycleId)
               .Select(x => new BookedMotorcycleDto
               {
                   Id = x.Id,
                   Manufacturer = x.Manufacturer.Name,
                   Model = x.Model.Name,
                   Type = (TypeOfMotors)x.TypeMotor,
                   Price = x.Price,
                   Color = x.Color.Name,
                   City = x.City.Name,
                   Dealer = x.Dealer.Name,
                   AverageVote = this.votesService.GetAverageVote(bike.MotorcycleId),
                   DealerEmail = x.Dealer.Email,
                   Url = x.Url,
                   Year = x.Manufacturer.Year,
                   Description = x.Description,
               }).FirstOrDefault();
                motorcycle.PickUpDate = bike.Offer.PickUpDate;
                motorcycle.DropOffDate = bike.Offer.DropOffDate;

                if (bike.Offer.DropOffDate >= DateTime.UtcNow)
                {
                    listOfmotorcycle.Add(motorcycle);
                    continue;
                }
            }

            return listOfmotorcycle;
        }

        public BookedMotorcycleDto GetSingleBookedMotorcycleByClientId(int clientId, int motorcycle)
        {
            var allMotors = this.GetAllListOfMotorcycleByClietId(clientId);
            return allMotors.FirstOrDefault(x => x.Id == motorcycle);
        }

        public int GetClientId(string userId)
             => this.clientRepository.All()
            .Where(x => x.UserId == userId)
            .FirstOrDefault().Id;

        public ClientServiceModel GetCurrentClient(string userId)
            => this.clientRepository.AllAsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new ClientServiceModel
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
                Email = x.Email,
                UserId = userId,
            }).FirstOrDefault();

        public string GetCurrentClientEmail(string userId)
            => this.clientRepository.AllAsNoTracking().FirstOrDefault(x => x.UserId == userId).Email;

        public int GetCurrentOfferId(DateTime pickUp, DateTime dropOff)
            => this.offerRepository
                   .AllAsNoTracking()
                   .Where(x => x.PickUpDate == pickUp &&
                               x.DropOffDate == dropOff)
                   .FirstOrDefault().Id;

        public bool IsAlreadyClientExist(string userId)
            => this.clientRepository.AllAsNoTracking().Any(x => x.UserId == userId);

        public async Task CreaterReviewByUser(int motorcycleId, byte value, string username, string description)
        {
            var currMotorcycle = this.reviewRepository.All()
                .Where(x => x.MotorcycleId == motorcycleId).ToList();

            if (currMotorcycle.Any(x => x.Name == username))
            {
                return;
            }

            var review = new Review
            {
                MotorcycleId = motorcycleId,
                DateRelease = DateTime.UtcNow,
                Name = username,
                Description = description,
                Vote = value,
            };
            await this.reviewRepository.AddAsync(review);
            await this.reviewRepository.SaveChangesAsync();
        }

        public string GetClientIdByUser(string userId)
            => this.clientRepository.All().FirstOrDefault(x => x.UserId == userId).UserId;

        public string GetCurrentClientInfo(int id)
            => this.clientRepository.All()
            .FirstOrDefault(x => x.Id == id).Address;

        public async Task<bool> Edit(int clientId, string address, string city)
        {
            var currClient = this.clientRepository.All()
                .FirstOrDefault(x => x.Id == clientId);

            if (currClient == null)
            {
                return false;
            }

            currClient.Address = $"{city} - {address}";
            this.clientRepository.Update(currClient);
            await this.clientRepository.SaveChangesAsync();
            return true;
        }
    }
}
