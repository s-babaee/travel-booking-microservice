using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        /// <summary>
        /// Authorize : Everyone (Anonymous)
        /// input : Route — hotelId (Guid)
        /// output : List<RoomDto>
        /// Description : دریافت لیست اتاق‌های یک هتل
        /// </summary>
        [HttpGet("hotels/{hotelId:guid}/rooms")]
        public async Task<IActionResult> GetByHotel(Guid hotelId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Authorize : Admin
        /// input : Route — hotelId + CreateRoomRequest (Body)
        /// output : RoomDto (201 Created)
        /// Description : افزودن اتاق جدید به یک هتل
        /// </summary>
        [HttpPost("hotels/{hotelId:guid}/rooms")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Authorize : Admin
        /// input : Route — roomId + UpdateRoomRequest (Body)
        /// output : RoomDto
        /// Description : ویرایش اطلاعات اتاق
        /// </summary>
        [HttpPut("rooms/{roomId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Authorize : Admin
        /// input : Route — roomId (Guid)
        /// output : 204 No Content
        /// Description : حذف اتاق
        /// </summary>
        [HttpDelete("rooms/{roomId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid roomId)
        {
            throw new NotImplementedException();
        }
    }
}
