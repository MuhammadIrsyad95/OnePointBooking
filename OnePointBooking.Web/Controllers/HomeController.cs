using Microsoft.AspNetCore.Mvc;
using OnePointBooking.Application.Services.Interface;
using OnePointBooking.Web.Models;
using OnePointBooking.Web.ViewModels;
using System.Diagnostics;

namespace OnePointBooking.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public HomeController(IRoomService roomService, IWebHostEnvironment webHostEnvironment)
        {
            _roomService = roomService;
            _webHostEnvironment = webHostEnvironment;
        }


        public IActionResult Index()
        {
            HomeVM homeVM = new()
            {
                RoomList = _roomService.GetAllRooms(),
                Days = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
            };
            return View(homeVM);
        }
        [HttpPost]
        public IActionResult GetRoomsByDate(int days, DateOnly startDate)
        {

            HomeVM homeVM = new()
            {
                StartDate = startDate,
                RoomList = _roomService.GetRoomsAvailabilityByDate(days, startDate),
                Days = days
            };
            return PartialView("_RoomList", homeVM);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
