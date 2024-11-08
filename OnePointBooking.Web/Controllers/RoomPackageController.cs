using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnePointBooking.Application.Common.Utility;
using OnePointBooking.Application.Services.Interface;
using OnePointBooking.Domain.Entities;
using OnePointBooking.Web.ViewModels;

namespace OnePointBooking.Web.Controllers
{
    [Authorize]
    public class RoomPackageController : Controller
    {
        private readonly IRoomPackageService _roomPackageService;
        private readonly IRoomService _roomService;

        public RoomPackageController(IRoomPackageService roomPackageService, IRoomService roomService)
        {
            _roomPackageService = roomPackageService;
            _roomService = roomService;
        }
        public IActionResult Index()
        {
            var roomPackages = _roomPackageService.GetAllRoomPackages();
            return View(roomPackages);
        }
        public IActionResult Create()
        {
            RoomPackageVM roomPackageVM = new()
            {
                RoomList = _roomService.GetAllRooms().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),

            };
            return View(roomPackageVM);
        }
        [HttpPost]
        public IActionResult Create(RoomPackageVM obj)
        {
            bool roomNumberExists = _roomPackageService.CheckRoomPackageExists(obj.RoomPackage.RoomPackageId);

            if (ModelState.IsValid && !roomNumberExists)
            {
                obj.RoomPackage.CreatedBy = User.Identity.Name;
                obj.RoomPackage.CreatedDate = DateTime.Now;

                // TotalPrice is calculated dynamically in the model, no need to assign it
                _roomPackageService.CreateRoomPackage(obj.RoomPackage);
                TempData["success"] = "The Room Package has been created successfully.";
                return RedirectToAction(nameof(Index));
            }

            // If already exists
            if (roomNumberExists)
            {
                TempData["error"] = "The Room Package already exists.";
            }

            // Repopulate RoomList and StatusList
            obj.RoomList = _roomService.GetAllRooms().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });

               obj.StatusList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Active", Value = SD.StatusActive },
                new SelectListItem { Text = "Inactive", Value = SD.StatusInactive }
            };

            return View(obj);
        }
        private decimal CalculateTotalAmenitiesPrice(IEnumerable<Amenity> amenities)
        {
            if (amenities == null || !amenities.Any())
            {
                return 0;
            }

            // Hitung total harga fasilitas
            return amenities.Sum(amenity => amenity.Price);
        }
        public IActionResult Update(int roomPackageID)
        {
            RoomPackageVM roomPackageVM = new()
            {
                RoomList = _roomService.GetAllRooms().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                RoomPackage = _roomPackageService.GetRoomPackageById(roomPackageID)
            };
            if (roomPackageVM.RoomPackage == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(roomPackageVM);
        }
        [HttpPost]
        public IActionResult Update(RoomPackageVM roomPackageVM)
        {
            if (ModelState.IsValid)
            {
                roomPackageVM.RoomPackage.UpdatedBy = User.Identity.Name;
                roomPackageVM.RoomPackage.UpdatedDate = DateTime.Now;

                // No need to assign TotalPrice, it's computed dynamically
                _roomPackageService.UpdateRoomPackage(roomPackageVM.RoomPackage);
                TempData["success"] = "The Room Package has been updated successfully.";
                return RedirectToAction("Index");
            }

            // Repopulate RoomList and StatusList
            roomPackageVM.RoomList = _roomService.GetAllRooms().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });

            roomPackageVM.StatusList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Active", Value = SD.StatusActive },
                new SelectListItem { Text = "Inactive", Value = SD.StatusInactive }
            };

            return View(roomPackageVM);
        }

        public IActionResult Delete(int roomPackageId)
        {
            RoomPackageVM roomPackageVM = new()
            {
                RoomList = _roomService.GetAllRooms().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                RoomPackage = _roomPackageService.GetRoomPackageById(roomPackageId)
            };
            if (roomPackageVM.RoomPackage == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(roomPackageVM);
        }
        [HttpPost]
        public IActionResult Delete(RoomPackageVM roomPackageVM)
        {

            RoomPackage? objFromDb = _roomPackageService.GetRoomPackageById(roomPackageVM.RoomPackage.RoomPackageId);
            if (objFromDb is not null)
            {
                _roomPackageService.DeleteRoomPackage(objFromDb.RoomPackageId);
                TempData["success"] = "The Room Package has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The Room Package could not be deleted .";

            return View();
        }

    }
}
