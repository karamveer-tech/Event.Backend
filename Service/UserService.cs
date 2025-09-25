using eventManager.Model;

namespace eventManager.Service
{
    public class UserService
    {
        private readonly IConfiguration _configuration;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _ctx;
        private readonly IWebHostEnvironment _env;
        private readonly DataBaseUtil _db;
        private readonly IEmailService _emailService;
        public UserService(IConfiguration configuration, IConfiguration config,
                       IHttpContextAccessor ctx,
                       IWebHostEnvironment env, IEmailService emailService)
        {
            _configuration = configuration;
            _config = config;
            _ctx = ctx;
            _env = env;
            _emailService = emailService;
        }
        public async Task<IReadOnlyList<users>> GetUserDetails(string emailid)
        {
            var _db = new DataBaseUtil(_configuration);
            var req = _ctx.HttpContext?.Request;
            var baseUrl = req != null ? $"{req.Scheme}://{req.Host}/" : string.Empty;

            string query = @"SELECT * FROM users where email= '" + emailid + "'";
            var dbUsers = await _db.GetMultipleRecordFromQuery<users>(query);

            var list = dbUsers.Select(e => new users
            {
                id = e.id,
                name = e.name,
                email = e.email,
                phone = e.phone,
                Status = e.Status,
            })
           .ToList()
           .AsReadOnly();

            return list;
        }
        public async Task<IReadOnlyList<users>> GetUserDetailsById(int userId)
        {
            var _db = new DataBaseUtil(_configuration);
            var req = _ctx.HttpContext?.Request;
            var baseUrl = req != null ? $"{req.Scheme}://{req.Host}/" : string.Empty;

            string query = @"SELECT * FROM users where id= '" + userId + "'";
            var dbUsers = await _db.GetMultipleRecordFromQuery<users>(query);

            var list = dbUsers.Select(e => new users
            {
                id = e.id,
                name = e.name,
                email = e.email,
                phone = e.phone,
                Status = e.Status,
            })
           .ToList()
           .AsReadOnly();

            return list;
        }
        public async Task<Response> AddOrUpdateBooking(bookings newBooking)
        {
            var result = new Response();
            DataBaseUtil _db = new DataBaseUtil(_configuration);

            try
            {
      
                var values = new Dictionary<string, string>
                {
                    { "user_id", newBooking.user_id.ToString() },
                    { "event_id", newBooking.event_id.ToString() },
                    { "booking_date", newBooking.booking_date.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "status", newBooking.status },
                    { "quantity", newBooking.quantity.ToString() },
                    { "total_amount", newBooking.total_amount.ToString() }
                };

                if (newBooking.id == 0)
                {
                    long bookingId = _db.SaveScalarReturnId<bookings>(values);

                    if (bookingId > 0)
                    {
                       // await InsertTickets(newBooking.ticket_Types, bookingId);

                        result.status = 1;
                        result.success = true;
                        result.message = "Booking saved successfully.";
                        string subject = "Booking Confirmation";
                        string body = $@"
                                        <p>Dear Customer,</p>
                                        <p>Your booking for event ID <strong>{newBooking.event_id}</strong> is confirmed.</p>
                                        <p><strong>Booking ID:</strong> {bookingId}</p>
                                        <p><strong>Quantity:</strong> {newBooking.quantity}</p>
                                        <p><strong>Total Amount:</strong> {newBooking.total_amount:C}</p>
                                        <p>Tickets:</p>
                                        <ul>";

                    

                        body += "</ul><p>Thank you!</p>";

                        await _emailService.SendEmailAsync(newBooking.emailId, subject, body);
                    }
                    else
                    {
                        result.status = 0;
                        result.success = false;
                        result.message = "Error inserting booking.";
                    }
                }
                else
                {
                    var where = new Dictionary<string, string> { { "id", newBooking.id.ToString() } };
                    int updated = await _db.UpdateRecord<bookings>(values, where);

                    if (updated == 1)
                    {
                        // Remove old tickets
                        string deleteQuery = $"DELETE FROM booking_tickets WHERE booking_id = '{newBooking.id}'";
                        await _db.ExecuteDelete(deleteQuery);

                        // Insert new tickets
                        await InsertTickets(newBooking.ticket_Types, newBooking.id);

                        result.status = 1;
                        result.success = true;
                        result.message = "Booking updated successfully.";
                    }
                    else
                    {
                        result.status = 0;
                        result.success = false;
                        result.message = "Error updating booking.";
                    }
                }
            }
            catch (Exception ex)
            {
                result.status = 0;
                result.success = false;
                result.message = "Error in booking process: " + ex.Message;
            }

            return result;

        }
        private async Task InsertTickets(List<ticket_type> tickets, long bookingId)
        {
            if (tickets == null || tickets.Count == 0) return;

            foreach (var item in tickets)
            {
                Dictionary<string, string> pairs = new();
                //pairs.Add("booking_id", bookingId.ToString());
                //pairs.Add("ticket_type_id", item.ticket_type_id.ToString());
                pairs.Add("quantity", item.quantity.ToString());
                pairs.Add("price", item.price.ToString());

                _db.SaveScalarReturnId<ticket_type>(pairs);
            }
        }

        public async Task<int> GetBookedTicketCount(int eventId)
        {
            var _db = new DataBaseUtil(_configuration);
            var req = _ctx.HttpContext?.Request;
            var baseUrl = req != null ? $"{req.Scheme}://{req.Host}/" : string.Empty;
            int totalBookedTicketCount = 0;
            
            string query = @"SELECT quantity FROM bookings where event_id= '" + eventId + "'";
            var dbBookings = await _db.GetMultipleRecordFromQuery<totalTicketBookedCount>(query);
            return totalBookedTicketCount = dbBookings.Sum(r => r.quantity);
        }
        public async Task<IReadOnlyList<myBookings>> GetMyBookings(int userId)
        {
            var _db = new DataBaseUtil(_configuration);
            var req = _ctx.HttpContext?.Request;
            var baseUrl = req != null ? $"{req.Scheme}://{req.Host}/" : string.Empty;

            string query = @"SELECT e.id AS event_id,e.organiser_id,e.title,e.description,e.location,e.start_datetime,e.end_datetime,
                            e.status AS event_status,e.created_at,e.banner_path,e.csvFile_path,e.ticketType,e.freeSeats,e.ImagesPath,
                            b.quantity FROM events e INNER JOIN bookings b ON e.id = b.event_id WHERE b.user_id = '" + userId + "'order by b.booking_date desc";
            var dbMyBookings = await _db.GetMultipleRecordFromQuery<myBookings>(query);

            var list = dbMyBookings.Select(e => new myBookings
            {
                id = e.id,
                event_id=e.event_id,
                organiser_id = e.organiser_id,
                title = e.title,
                description = e.description,
                location = e.location,
                start_datetime = e.start_datetime,
                end_datetime = e.end_datetime,
                status = e.status,
                ticketType = e.ticketType,
                created_at = e.created_at,
                quantity=e.quantity,
                banner_path = string.IsNullOrEmpty(e.banner_path) ? null : $"{e.banner_path}"
            })
            .ToList()
            .AsReadOnly();

            return list;
        }

    }
}
