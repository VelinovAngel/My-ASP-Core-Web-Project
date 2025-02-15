﻿namespace BikesBooking.Services.Data.DTO.MotorcycleModels
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using BikesBooking.Common;
    using BikesBooking.Services.Data.Motorcycle;

    public class AllFreeMotorcylesQueryDto
    {
        public const int MotorcyclesPerPage = GlobalConstants.MaxPageFreeMotors;

        public int CurrentPage { get; set; } = GlobalConstants.CurrentPage;

        public int TotalMotorcycle { get; set; }

        public IEnumerable<MotorcycleDetailsModel> Motors { get; set; }

        public int CountryId { get; set; }

        public int CityId { get; set; }

        public DateTime PickUpDate { get; set; }

        public DateTime DropOffDate { get; set; }

        public int CityCount { get; set; }

        public int ManufacturerId { get; set; }

        public BikesBooking.Data.Models.Enum.Type Type { get; set; }

        public IEnumerable<KeyValuePair<string, string>> CountriesItems { get; set; }

        public IEnumerable<KeyValuePair<string, string>> ManufacturerItems { get; set; }
    }
}
