using eventManager.Dtos;
using eventManager.Model;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using MySqlX.XDevAPI.Common;
using static eventManager.Enums.Enums;

namespace eventManager.Service
{
    public class EventService
    {
        private readonly IConfiguration _configuration;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _ctx;
        private readonly IWebHostEnvironment _env;
        private readonly DataBaseUtil _db;
        public EventService(IConfiguration configuration, IConfiguration config,
                        IHttpContextAccessor ctx,
                        IWebHostEnvironment env)
        {
            _configuration = configuration;
            _config = config;
            _ctx = ctx;
            _env = env;
        }
        public async Task<IReadOnlyList<events>> GetAllEvents(IWebHostEnvironment ev)
        {
            DataBaseUtil _db = new DataBaseUtil(_configuration);
            List<events> events = new List<events>();
            var req = _ctx.HttpContext?.Request;
            var baseUrl = req != null ? $"{req.Scheme}://{req.Host}/" : "";
            string query = @"select * from events";
            events = await _db.GetMultipleRecordFromQuery<events>(query);
            var list = events.Select(e => new events
            {
                id = e.id,
                organiser_id = e.organiser_id,
                title = e.title,
                description = e.description,
                location = e.location,
                start_datetime = e.start_datetime,
                end_datetime = e.end_datetime,
                status = e.status,
                created_at = e.created_at,
                template_path = string.IsNullOrEmpty(e.template_path)
                                ? null
                                : baseUrl + e.template_path
            })
        .ToList()
        .AsReadOnly();
            return list;
        }

        public async Task<Response> AddUpdateEvent(events newEvent)
        {
            DataBaseUtil _db = new DataBaseUtil(_configuration);
            var result = new Response();
            try
            {

                Dictionary<string, string> values = new Dictionary<string, string>();
                Dictionary<string, string> where = new Dictionary<string, string>();
                string passwordKey = _configuration["password:Key"];
                if (newEvent.id == 0)
                {
                    values.Add("title", newEvent.title);
                    values.Add("organiser_id", newEvent.organiser_id.ToString());
                    values.Add("description", newEvent.description);
                    values.Add("location", newEvent.location);
                    values.Add("start_datetime", newEvent.start_datetime.ToString("yyyy-MM-dd HH:mm:ss"));
                    values.Add("end_datetime", newEvent.end_datetime.ToString("yyyy-MM-dd HH:mm:ss"));
                    values.Add("status", newEvent.status);
                    values.Add("template_path", newEvent.template_path);
                    var res = _db.SaveExecuteNonQuery<events>(values);
                    if (res == 1)
                    {
                        result.status = 1;
                        result.message = "Saved Succefully.";
                    }
                    else
                    {
                        result.status = 0;
                        result.message = "Error in inset user.";
                    }
                }
                else
                {
                    values.Add("title", newEvent.title);
                    values.Add("organiser_id", newEvent.organiser_id.ToString());
                    values.Add("description", newEvent.description);
                    values.Add("location", newEvent.location);
                    values.Add("start_datetime", newEvent.start_datetime.ToString());
                    values.Add("end_datetime", newEvent.end_datetime.ToString());
                    values.Add("status", newEvent.status);
                    var res = await _db.UpdateRecord<events>(values, where);
                    if (res == 1)
                    {
                        result.status = 1;
                        result.message = "Updated Succefully.";
                    }
                    else
                    {
                        result.status = 0;
                        result.message = "Error in update user.";
                    }
                }
            }
            catch (Exception ex)
            {
                result.status = 0;
                result.message = "Error in update user.";
            }
            return result;
        }
        public async Task<eventDto?> GetEventById(long id, IWebHostEnvironment ev)
        {
            DataBaseUtil _db = new DataBaseUtil(_configuration);
            var req = _ctx.HttpContext?.Request;
            var baseUrl = req != null ? $"{req.Scheme}://{req.Host}/" : "";

            // Get event by id
            string eventQuery = @"SELECT * FROM events WHERE id = '" + id + "'";
            var result = await _db.GetSingleRecordFromQuery<events>(eventQuery);

            if (result == null)
                return null;

            // Fetch ticket types for the event
            string ticketQuery = @"SELECT * FROM ticket_type WHERE event_id = '" + id + "'";
            var ticketTypes = await _db.GetMultipleRecordFromQuery<ticket_type>(ticketQuery);

            // Map to DTO
            var eventDto = new eventDto
            {
                id = result.id,
                organiser_id = result.organiser_id,
                title = result.title,
                description = result.description,
                location = result.location,
                start_datetime = result.start_datetime,
                end_datetime = result.end_datetime,
                status = result.status,
                created_at = result.created_at,
                template_path = string.IsNullOrEmpty(result.template_path) ? null : baseUrl + result.template_path,
                ticket_Types = ticketTypes ?? new List<ticket_type>()
            };

            return eventDto;
        }

        public async Task<bool> DeleteEvent(int id)
        {
            DataBaseUtil _db = new DataBaseUtil(_configuration);

            string query = @"DELETE FROM events WHERE id = '" + id + "'";
            var parameters = new { id };

            var rowsAffected = await _db.ExecuteDelete(query);
            return rowsAffected > 0;
        }

        public async Task<Response> AddUpdateTicketTypes(List<ticket_type> ticketTypes)
        {
            DataBaseUtil _db = new DataBaseUtil(_configuration);
            var result = new Response();
            try
            {
                int successCount = 0;
                int failureCount = 0;

                foreach (var newTicketType in ticketTypes)
                {
                    Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "ticket_type_name", newTicketType.ticket_type_name },
                { "ticket_price", newTicketType.ticket_price.ToString() },
                { "event_id", newTicketType.event_id.ToString() },
                { "no_of_tickets", newTicketType.no_of_tickets.ToString() }
            };

                    if (newTicketType.id == 0)
                    {
                        // Insert new ticket
                        var res = _db.SaveExecuteNonQuery<ticket_type>(values);
                        if (res == 1)
                            successCount++;
                        else
                            failureCount++;
                    }
                    else
                    {
                        // Update existing ticket
                        Dictionary<string, string> where = new Dictionary<string, string>
                {
                    { "id", newTicketType.id.ToString() }
                };

                        var res = await _db.UpdateRecord<ticket_type>(values, where);
                        if (res == 1)
                            successCount++;
                        else
                            failureCount++;
                    }
                }

                result.status = 1;
                result.message = $"Processed: {ticketTypes.Count}, Success: {successCount}, Failed: {failureCount}";
            }
            catch (Exception ex)
            {
                result.status = 0;
                result.message = "Error: " + ex.Message;
            }

            return result;
        }
        public async Task<Response> AddUpdateEventUsers(event_user newEventUsers)
        {
            DataBaseUtil _db = new DataBaseUtil(_configuration);
            var result = new Response();
            try
            {

                Dictionary<string, string> values = new Dictionary<string, string>();
                Dictionary<string, string> where = new Dictionary<string, string>();
                if (newEventUsers.id == 0)
                {
                    values.Add("event_id", newEventUsers.event_id.ToString());
                    values.Add("user_id", newEventUsers.user_id.ToString());
                    values.Add("no_of_events", newEventUsers.no_of_events.ToString());
                    values.Add("ticket_type_id", newEventUsers.ticket_type_id.ToString());
                    var res = _db.SaveExecuteNonQuery<event_user>(values);
                    if (res == 1)
                    {
                        result.status = 1;
                        result.message = "Saved Succefully.";
                    }
                    else
                    {
                        result.status = 0;
                        result.message = "Error in inset user.";
                    }
                }
                else
                {
                    values.Add("event_id", newEventUsers.event_id.ToString());
                    values.Add("user_id", newEventUsers.user_id.ToString());
                    values.Add("no_of_events", newEventUsers.no_of_events.ToString());
                    values.Add("ticket_type_id", newEventUsers.ticket_type_id.ToString());
                    where.Add("id", newEventUsers.id.ToString());
                    var res = await _db.UpdateRecord<event_user>(values, where);
                    if (res == 1)
                    {
                        result.status = 1;
                        result.message = "Updated Succefully.";
                    }
                    else
                    {
                        result.status = 0;
                        result.message = "Error in update user.";
                    }
                }
            }
            catch (Exception ex)
            {
                result.status = 0;
                result.message = "Error in update user.";
            }
            return result;
        }

    }
}
