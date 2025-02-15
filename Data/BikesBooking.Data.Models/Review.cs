﻿namespace BikesBooking.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using BikesBooking.Data.Common.Models;

    public class Review : BaseDeletableModel<int>
    {
        [Required]
        [MaxLength(20)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        public int Vote { get; set; }

        public DateTime DateRelease { get; set; }

        public int? MotorcycleId { get; set; }

        public virtual Motorcycle Motorcycle { get; set; }
    }
}
