using OnePointBooking.Domain.Entities;

namespace OnePointBooking.Web.ViewModels
{
    public class HomeVM
    {
        public IEnumerable<Room>? RoomList { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int Days { get; set; }
        public int RoomSetupId { get; set; }  // Add this line
        public int RoomPackageId { get; set; } // Add this line

    }
}
