using Microsoft.AspNetCore.Hosting;
using OnePointBooking.Application.Common.Interfaces;
using OnePointBooking.Application.Common.Utility;
using OnePointBooking.Application.Services.Interface;
using OnePointBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePointBooking.Application.Services.Implementation
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public RoomService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public void CreateRoom(Room room)
        {
            if (room.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(room.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\RoomImage");

                using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                room.Image.CopyTo(fileStream);

                room.ImageUrl = @"\images\RoomImage\" + fileName;

            }
            else
            {
                room.ImageUrl = "https://placehold.co/600x400";
            }
            _unitOfWork.Room.Add(room);
            _unitOfWork.Save();
        }

        public bool DeleteRoom(int id)
        {
            try
            {
                Room? objFromDb = _unitOfWork.Room.Get(u => u.Id == id);
                if (objFromDb is not null)
                {
                    if (!string.IsNullOrWhiteSpace(objFromDb.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, objFromDb.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    _unitOfWork.Room.Remove(objFromDb);
                    _unitOfWork.Save();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public IEnumerable<Room> GetAllRooms()
        {
            return _unitOfWork.Room.GetAll(includeProperties: "RoomPackage.RoomAmenity,RoomSetup"); //jangan lupa nanti tambahkan includeproperties untuk roomsetups dan room packages
        }

        public Room GetRoomById(int id)
        {
            return _unitOfWork.Room.Get(u => u.Id == id, includeProperties: "RoomPackage.RoomAmenity,RoomSetup");//jangan lupa nanti tambahkan includeproperties 
        }
        public IEnumerable<Room> GetRoomsAvailabilityByDate(int days, DateOnly startDate)
        {
            // Get all rooms along with their packages and setups
            var rooms = _unitOfWork.Room.GetAll(includeProperties: "RoomPackage,RoomSetup").ToList();

            // Get all bookings that are currently approved or checked in
            var bookings = _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved ).ToList();

            foreach (var room in rooms)
            {
                // Check if the room is available based on the bookings and the given dates
                bool isAvailable = CheckRoomAvailability(room.Id, days, startDate, bookings);
                room.isAvailable = isAvailable; // Set availability status
            }

            return rooms;
        }

        public bool IsRoomAvailableByDate(int roomId, int days, DateOnly startDate)
        {
            // Get all bookings for the specified room that are currently approved or checked in
            var bookings = _unitOfWork.Booking.GetAll(u => (u.Status == SD.StatusApproved ) &&
                                                             u.RoomId == roomId).ToList();

            // Check the room's availability based on the bookings and the given dates
            return CheckRoomAvailability(roomId, days, startDate, bookings);
        }

        private bool CheckRoomAvailability(int roomId, int days, DateOnly startDate, List<Booking> bookings)
        {
            foreach (var booking in bookings)
            {
                // Check if the booking overlaps with the requested dates
                if (booking.RoomId == roomId)
                {
                    var bookingEndDate = booking.StartDate.AddDays(booking.Days);
                    var requestedEndDate = startDate.AddDays(days);

                    if ((startDate < bookingEndDate) && (booking.StartDate < requestedEndDate))
                    {
                        return false; // Room is not available due to overlap
                    }
                }
            }
            return true; // Room is available
        }

        

        public void UpdateRoom(Room room)
        {
            //Update Image
            if (room.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(room.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\RoomImage");

                if (!string.IsNullOrEmpty(room.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, room.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                room.Image.CopyTo(fileStream);

                room.ImageUrl = @"\images\RoomImage\" + fileName;

            }
            _unitOfWork.Room.Update(room);
            _unitOfWork.Save();
        }
        // Implementasi metode lainnya...

        public IEnumerable<RoomPackage> GetAvailableRoomPackages(int roomId)
        {
            return _unitOfWork.RoomPackage.GetAll(u => u.RoomId == roomId && u.Status == "Active");
        }

        public IEnumerable<RoomSetup> GetRoomSetupsByRoomId(int roomId)
        {
            return _unitOfWork.RoomSetup.GetAll(u => u.RoomId == roomId);
        }
    }
}
