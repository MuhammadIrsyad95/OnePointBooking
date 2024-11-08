using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnePointBooking.Application.Common.Utility;
using OnePointBooking.Domain.Entities;

namespace OnePointBooking.Web.ViewModels
{
    public class RoomPackageVM
    {

        public RoomPackage? RoomPackage { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem>? RoomList { get; set; }

        [ValidateNever]  // Add this attribute to skip validation
        public IEnumerable<SelectListItem>? StatusList { get; set; } // Declare the StatusList property
                                                                     // Tambahkan properti berikut
        public RoomPackageVM()
        {
            // Populate StatusList with Active and Inactive options
            StatusList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Active", Value = SD.StatusActive },
                new SelectListItem { Text = "Inactive", Value = SD.StatusInactive }
            };
        }
    }
}
