﻿namespace BikesBooking.Services.Data.Client
{
    using System.Linq;
    using System.Threading.Tasks;

    using BikesBooking.Data.Common.Repositories;
    using BikesBooking.Data.Models;

    public class ClientService : IClientService
    {
        private readonly IRepository<Client> clientRepository;
        private readonly IRepository<ApplicationUser> userRepository;

        public ClientService(
            IRepository<Client> clientRepository,
            IRepository<ApplicationUser> userRepository)
        {
            this.clientRepository = clientRepository;
            this.userRepository = userRepository;
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

        public int GetClientId(string userId)
             => this.clientRepository.AllAsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(d => new { Id = d.Id })
            .FirstOrDefault().Id;

        public string GetCurrentClientEmail(string id)
            => this.clientRepository.AllAsNoTracking().FirstOrDefault(x => x.UserId == id).Email;

        public bool IsAlreadyClientExist(string id)
            => this.clientRepository.AllAsNoTracking().Any(x => x.UserId == id);
    }
}
