﻿namespace BikesBooking.Web.ViewModels.Dealers
{
    using System.ComponentModel.DataAnnotations;

    using Microsoft.AspNetCore.Http;

    using static BikesBooking.Common.GlobalConstants;
    using static BikesBooking.Common.GlobalConstants.Dealer;

    public class BecomeDealerFormModel
    {
        [Required]
        [StringLength(
            NameMaxLength,
            ErrorMessage = "The dealer's Company name must be between {1} and {2} characters.",
            MinimumLength = NameMinLength)]
        [Display(Name ="Name of company")]
        public string Name { get; set; }

        [Required]
        [StringLength(
            Dealer.AddressMaxLength,
            ErrorMessage = "The dealer address must be between {1} and {2} characters.",
            MinimumLength = AddressMinLength)]
        [Display(Name = "Address of company")]
        public string Address { get; set; }

        [Required]
        [StringLength(
            CountryNameLength,
            ErrorMessage = "The dealer country must be between {1} and {2} characters.",
            MinimumLength = CountryNameMinLength)]
        public string Country { get; set; }

        [Required]
        [StringLength(
            CityNameMaxLength,
            ErrorMessage = "The dealer City must be between {1} and {2} characters.",
            MinimumLength = CityNameMinLength)]
        public string City { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(
            DescriptionMaxLength,
            ErrorMessage = "The description must be between {1} and {2} characters.",
            MinimumLength = DescriptionMinLength)]
        public string Description { get; set; }

        [Display(Name = "Put a profile picture")]
        public IFormFile ImageFile { get; set; }
    }
}
