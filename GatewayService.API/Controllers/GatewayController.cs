using Microsoft.AspNetCore.Mvc;

namespace GatewayService.api.Controllers;


    [Route("api/gateway")]
    [ApiController]
    public class GatewayServicecontroller: ControllerBase{
        private readonly IHttpClientFactory _clientFactory;
        private const string bookingRoute = "http://localhost:5280/api/booking";
        public GatewayServicecontroller(IHttpClientFactory clientFactory){
            _clientFactory = clientFactory;
        }

        [HttpPost("user_id_amount/{username}/{id}/{amount}")]
        public async Task<IActionResult> CreateBookingByParams(string username, int id, int amount)
        {
            var client = _clientFactory.CreateClient();
            // Create the HTTP request with the necessary route
            var bookingResponse = await client.PostAsync($"{bookingRoute}/create_req/{username}/{id}/{amount}", null);

            // Check if the request was successful
            if (bookingResponse.IsSuccessStatusCode)
            {
                // Return the location header from the booking service
                if (bookingResponse.Headers.Location != null)
                {
                    return Created(bookingResponse.Headers.Location, null);
                }
                
                return StatusCode((int)bookingResponse.StatusCode);
            }

            // Return the status code and error message if the request failed
            var errorContent = await bookingResponse.Content.ReadAsStringAsync();
            return StatusCode((int)bookingResponse.StatusCode, errorContent);
        }

        [HttpPost("user_eventname_amount/{username}/{eventname}/{amount}")]
        public async Task<IActionResult> CreateBookingByParamsName(string username, string eventname, int amount)
        {
            var client = _clientFactory.CreateClient();
            // Create the HTTP request with the necessary route
            var bookingResponse = await client.PostAsync($"{bookingRoute}/create_req_name/{username}/{eventname}/{amount}", null);

            // Check if the request was successful
            if (bookingResponse.IsSuccessStatusCode)
            {
                // Return the location header from the booking service
                if (bookingResponse.Headers.Location != null)
                {
                    return Created(bookingResponse.Headers.Location, null);
                }
                
                return StatusCode((int)bookingResponse.StatusCode);
            }

            // Return the status code and error message if the request failed
            var errorContent = await bookingResponse.Content.ReadAsStringAsync();
            return StatusCode((int)bookingResponse.StatusCode, errorContent);
        }
        
        [HttpDelete ("user_eventname/{username}/{eventname}")]
        public async Task<IActionResult> DeleteBookingByParams(string username, string eventname){
            var client = _clientFactory.CreateClient();
            var bookingResponse = await client.DeleteAsync($"{bookingRoute}/user_eventname/{username}/{eventname}");

            if (bookingResponse.IsSuccessStatusCode)
            {
                return NoContent();
            }
            var errorContent = await bookingResponse.Content.ReadAsStringAsync();
            return StatusCode((int)bookingResponse.StatusCode, errorContent);
        }
        [HttpDelete ("user_eventId/{username}/{id}")]
        public async Task<IActionResult> DeleteBookingByIdParams(string username, int id){
            var client = _clientFactory.CreateClient();
            var bookingResponse = await client.DeleteAsync($"{bookingRoute}/user_event/{username}/{id}");

            if (bookingResponse.IsSuccessStatusCode)
            {
                return NoContent();
            }
            var errorContent = await bookingResponse.Content.ReadAsStringAsync();
            return StatusCode((int)bookingResponse.StatusCode, errorContent);
        }
    };

