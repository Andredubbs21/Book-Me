using Microsoft.AspNetCore.Mvc;

namespace gateway.api.Controllers;


    [Route("api/gateway")]
    [ApiController]
    public class Gatewaycontroller: ControllerBase{
        private readonly IHttpClientFactory _clientFactory;
        private const string bookingRoute = "http://localhost:5280/api/booking";
        public Gatewaycontroller(IHttpClientFactory clientFactory){
            _clientFactory = clientFactory;
        }

        [HttpPost("{username}/{id}/{amount}")]
        public async Task<IActionResult> CreateBookingByParams(string username, int id, int amount)
        {
            var client = _clientFactory.CreateClient();
            Console.WriteLine(username, id, amount);
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
    };

