using eventManager.Dtos;
using eventManager.Model;
using eventManager.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using MySql.Data.MySqlClient;
using System.Text.Json;

namespace eventManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly EventService _eventService;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _ctx;
        public EventController(EventService eventService, IWebHostEnvironment env, IHttpContextAccessor ctx)
        {
            _eventService = eventService;
            _env = env;
            _ctx = ctx;
        }
        [HttpGet("get-all-events")]
        public async Task<IActionResult> GetAllEvents()
        {
            var res = await _eventService.GetAllEvents();
            return Ok(res);
        }
        [HttpGet("get-user-events")]
        public async Task<IActionResult> GetUserEvents()
        {
            var res = await _eventService.GetUserEvents();
            return Ok(res);
        }
        [HttpPost("create-event")]
        public async Task<IActionResult> CreateEvent([FromForm] events input, IWebHostEnvironment env)
        {
            // 0️⃣  Validate
            if (input.banner == null || input.banner.Length == 0)
                return BadRequest("event_Template (file) is required.");

            var paidTickets = string.IsNullOrEmpty(input.paidTicketsJson) ? new List<paid_tickets>() : JsonSerializer.Deserialize<List<paid_tickets>>(input.paidTicketsJson);

            var req = _ctx.HttpContext?.Request;
            var baseUrl = req != null ? $"{req.Scheme}://{req.Host}/" : string.Empty;

            var uploadsDir = Path.Combine(env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsDir);

            // 2️⃣  Save the file with a unique name
            var uniqueName_banner = Guid.NewGuid() + Path.GetExtension(input.banner.FileName);
            //var uniqueName_images = Guid.NewGuid() + Path.GetExtension(input.images.FileName);
            var absPath = Path.Combine(uploadsDir, uniqueName_banner);
            //var absPath_csvFile = Path.Combine(uploadsDir, uniqueName_images);

            await using (var stream = new FileStream(absPath, FileMode.Create))
            {
                await input.banner.CopyToAsync(stream);
            }
            //await using (var stream = new FileStream(absPath_csvFile, FileMode.Create))
            //{
            //    await input.images.CopyToAsync(stream);
            //}

            // 3️⃣  Store relative path in DB
            input.banner_path = $"{baseUrl}uploads/{uniqueName_banner}";
            //input.csvFile_path = $"uploads/{uniqueName_images}";

            input.paidTickets = paidTickets;
            // 4️⃣  Persist the event (your service)
            var res = await _eventService.AddUpdateEvent(input);

            return Ok(res);
        }

        [HttpGet("get-event-by-id/{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var result = await _eventService.GetEventById(id);
            if (result == null)
                return NotFound($"Event with ID {id} not found.");

            return Ok(result);
        }


        [HttpPost("update-event")]
        public async Task<IActionResult> UpdateEvent([FromForm] events input)
        {
            // 0️⃣ Check if the event exists
            var existingEvent = await _eventService.GetEventById(input.id);
            if (existingEvent == null)
                return NotFound("Event not found.");

            // Get baseUrl from request (https://localhost:44315)
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";

            // 1️⃣ Handle Banner Upload
            if (input.banner != null && input.banner.Length > 0)
            {
                // Delete old file if exists
                if (!string.IsNullOrEmpty(existingEvent.banner_path))
                {
                    // Strip baseUrl if old path is full URL
                    var oldRelativePath = existingEvent.banner_path.Replace(baseUrl + "/", "");
                    var oldPath = Path.Combine(_env.WebRootPath, oldRelativePath);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                // Save new file
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsDir);

                var uniqueName = Guid.NewGuid() + Path.GetExtension(input.banner.FileName);
                var absPath = Path.Combine(uploadsDir, uniqueName);

                await using (var stream = new FileStream(absPath, FileMode.Create))
                {
                    await input.banner.CopyToAsync(stream);
                }

                // Store full URL
                input.banner_path = $"{baseUrl}/uploads/{uniqueName}";
            }
            else
            {
                // Preserve old banner full URL
                input.banner_path = existingEvent.banner_path;
            }

            // 2️⃣ Ticket Logic
            if (input.ticketType?.ToLower() == "free")
            {
                input.freeSeats = input.freeSeats > 0 ? input.freeSeats : existingEvent.freeSeats;
                input.paidTickets = new List<paid_tickets>();
            }
            else if (input.ticketType?.ToLower() == "paid")
            {
                if (!string.IsNullOrEmpty(input.paidTicketsJson))
                {
                    try
                    {
                        input.paidTickets = Newtonsoft.Json.JsonConvert.DeserializeObject<List<paid_tickets>>(input.paidTicketsJson)
                                            ?? new List<paid_tickets>();
                    }
                    catch
                    {
                        return BadRequest("Invalid paid tickets JSON format.");
                    }
                }
            }

            var res = await _eventService.AddUpdateEvent(input);
            return Ok(res);
        }


        [HttpDelete("delete-event/{id}")]
        public async Task<IActionResult> DeleteEvent(int id, IWebHostEnvironment env)
        {
            // 0️⃣ Get event by ID
            var existingEvent = await _eventService.GetEventById(id);
            if (existingEvent == null)
                return NotFound("Event not found.");

            // 1️⃣ Delete file from disk
            if (!string.IsNullOrEmpty(existingEvent.banner_path))
            {
                var filePath = Path.Combine(env.WebRootPath, existingEvent.banner_path);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            // 2️⃣ Delete event from DB
            var result = await _eventService.DeleteEvent(id);
            return Ok(result);
        }
        //[HttpGet("users-by-event/{eventId}")]
        //public async Task<IActionResult> GetUsersByEvent(int eventId)
        //{
        //    var users = new List<EventUserDto>();

        //    using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        //    {
        //        await connection.OpenAsync();
        //        string query = @"SELECT user_id, no_of_events, purchase_at 
        //                     FROM event_user 
        //                     WHERE event_id = @eventId";

        //        using (var command = new MySqlCommand(query, connection))
        //        {
        //            command.Parameters.AddWithValue("@eventId", eventId);

        //            using (var reader = await command.ExecuteReaderAsync())
        //            {
        //                while (await reader.ReadAsync())
        //                {
        //                    users.Add(new EventUserDto
        //                    {
        //                        UserId = reader.GetInt32(0),
        //                        NoOfEvents = reader.GetInt32(1),
        //                        PurchaseAt = reader.GetDateTime(2)
        //                    });
        //                }
        //            }
        //        }
        //    }

        //    return Ok(users);
        //}

        [HttpPost("add-ticket-type")]
        public async Task<Response> AddTicketTypes([FromBody] List<ticket_type> inputList)
        {
            var res = await _eventService.AddUpdateTicketTypes(inputList);
            return res;
        }
        [HttpPost("add-event-user")]
        public async Task<Response> AddUpdateEventUser([FromBody] event_user input)
        {
            var res = await _eventService.AddUpdateEventUsers(input);
            return res;
        }

    }
}
