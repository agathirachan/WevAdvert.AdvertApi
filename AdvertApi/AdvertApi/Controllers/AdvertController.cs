using AdvertApi.Models;
using AdvertApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertApi.Controllers
{
    [Route("api/v1/[controller]/[action]")]
    [ApiController]
    public class AdvertController : ControllerBase
    {
        private readonly IAdvertStorageService _advertStorageService;
        public AdvertController(IAdvertStorageService advertStorageService)
        {
            this._advertStorageService = advertStorageService;
        }
       
        [HttpPost]
        [ProducesResponseType(404)]
        [ProducesResponseType(201,Type=typeof(CreateAdvertResponse))]
        public  async Task<IActionResult> Create(AdvertModel model)
        {
            string recordId;
            try
            {
                recordId = await _advertStorageService.Add(model);
            }
            catch(KeyNotFoundException ex)
            {
                return new NotFoundResult();
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            return StatusCode(201, new CreateAdvertResponse() {Id= recordId });
        }

        [HttpPost]
        [ProducesResponseType(404)]
        [ProducesResponseType(201, Type = typeof(CreateAdvertResponse))]
        public async Task<IActionResult> Confirm(ConfimAdvertModel model)
        {
            try
            {
                 await _advertStorageService.Confirm(model);
            }
            catch (KeyNotFoundException ex)
            {
                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            return Ok();
        }
    }
}
