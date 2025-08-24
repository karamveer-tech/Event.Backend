using eventManager.Dtos;
using eventManager.Model;
using eventManager.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using MySql.Data.MySqlClient;

namespace eventManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly EventService _eventService;
        private readonly IWebHostEnvironment _env;
        public EventController(EventService eventService, IWebHostEnvironment env)
        {
            _eventService = eventService;
            _env = env;
        }
        [HttpGet("get-all-events")]
        public async Task<IActionResult> GetAllEvents()
        {
            var res = await _eventService.GetAllEvents(_env);
            return Ok(res);
        }
        [HttpPost("create-event")]
        public async Task<IActionResult> CreateEvent([FromForm] events input, IWebHostEnvironment env)
        {
            // 0️⃣  Validate
            if (input.event_Template == null || input.event_Template.Length == 0)
                return BadRequest("event_Template (file) is required.");

            // 1️⃣  Ensure uploads folder exists
            var uploadsDir = Path.Combine(env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsDir);

            // 2️⃣  Save the file with a unique name
            var uniqueName = Guid.NewGuid() + Path.GetExtension(input.event_Template.FileName);
            var absPath = Path.Combine(uploadsDir, uniqueName);

            await using (var stream = new FileStream(absPath, FileMode.Create))
            {
                await input.event_Template.CopyToAsync(stream);
            }

            // 3️⃣  Store relative path in DB
            input.template_path = $"uploads/{uniqueName}";

            // 4️⃣  Persist the event (your service)
            var res = await _eventService.AddUpdateEvent(input);

            return Ok(res);
        }

        [HttpGet("get-event-by-id/{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var result = await _eventService.GetEventById(id, _env);
            if (result == null)
                return NotFound($"Event with ID {id} not found.");

            return Ok(result);
        }
        [HttpPost("update-event")]
        public async Task<IActionResult> UpdateEvent([FromForm] events input)
        {
            // 0️⃣ Check if the event exists
            var existingEvent = await _eventService.GetEventById(input.id, _env);
            if (existingEvent == null)
                return NotFound("Event not found.");

            // 1️⃣ If new file uploaded, handle it
            if (input.event_Template != null && input.event_Template.Length > 0)
            {
                // Delete old file if exists
                if (!string.IsNullOrEmpty(existingEvent.template_path))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, existingEvent.template_path);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                // Save new file
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsDir);

                var uniqueName = Guid.NewGuid() + Path.GetExtension(input.event_Template.FileName);
                var absPath = Path.Combine(uploadsDir, uniqueName);

                await using (var stream = new FileStream(absPath, FileMode.Create))
                {
                    await input.event_Template.CopyToAsync(stream);
                }

                input.template_path = $"uploads/{uniqueName}";
            }
            else
            {
                // Preserve old template path if file not updated
                input.template_path = existingEvent.template_path;
            }

            var res = await _eventService.AddUpdateEvent(input);
            return Ok(res);
        }
        [HttpDelete("delete-event/{id}")]
        public async Task<IActionResult> DeleteEvent(int id, IWebHostEnvironment env)
        {
            // 0️⃣ Get event by ID
            var existingEvent = await _eventService.GetEventById(id, _env);
            if (existingEvent == null)
                return NotFound("Event not found.");

            // 1️⃣ Delete file from disk
            if (!string.IsNullOrEmpty(existingEvent.template_path))
            {
                var filePath = Path.Combine(env.WebRootPath, existingEvent.template_path);
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
