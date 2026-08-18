using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        /// <summary>
        /// Authorize : Everyone (Anonymous)
        /// input : Route — hotelId (Guid), city, HotelName
        /// output : HotelDetailDto
        /// Description :  دریافت جزئیات کامل یک هتل به همراه اتاق‌ها و امکانات 
        /// بر اساس ایدی هتل یا اسم هتل یا شهر هتل
        /// </summary>
        [HttpGet("{hotelId:guid}")]
        public async Task<IActionResult> Get()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Authorize : Admin
        /// input : CreateHotelRequest (Body)
        /// output : HotelDto (201 Created)
        /// Description : ساخت هتل جدید
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Authorize : Admin
        /// input : Route — hotelId + UpdateHotelRequest (Body)
        /// output : HotelDto
        /// Description : ویرایش اطلاعات یک هتل
        /// </summary>
        [HttpPut("{hotelId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Authorize : Admin
        /// input : Route — hotelId (Guid)
        /// output : 204 No Content
        /// Description : حذف نرم (Soft Delete) هتل
        /// </summary>
        [HttpDelete("{hotelId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid hotelId)
        {
            throw new NotImplementedException();
        }
    }
}
