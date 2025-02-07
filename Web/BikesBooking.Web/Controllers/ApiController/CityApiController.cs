﻿namespace BikesBooking.Web.Controllers.ApiController
{
    using System.Threading.Tasks;

    using BikesBooking.Services.Data.Home;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;

    [ApiController]
    public class CityApiController : BaseController
    {
        private readonly IHomeService homeService;

        public CityApiController(IHomeService homeService)
        {
            this.homeService = homeService;
        }

        [Route("api/GetCity")]
        [HttpGet]
        public async Task<SelectList> GetCityAsync(int id)
        {
            var model = await this.homeService.GetAllCitiesByCountryIdAsync(id);
            return new SelectList(model, "Id", "Name");
        }
    }
}
