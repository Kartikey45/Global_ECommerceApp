using ECommerceApp.Order.DTOs.Request;
using ECommerceApp.Order.Enums;
using ECommerceApp.Order.Services.Interfaces;
using ECommerceApp.Shared.Authorization;
using ECommerceApp.Shared.Constants;
using ECommerceApp.Shared.Helpers;
using ECommerceApp.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Order.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Groups.All)]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrdersController(IOrderService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get logged-in user's orders (paginated)
        /// </summary>
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.GetUserId();
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 50) pageSize = 10;

            var orders = await _service.GetMyOrdersAsync(
                userId, page, pageSize);

            return Ok(ApiResponse<object>.Ok(orders));
        }

        /// <summary>
        /// Get all orders — Admin only
        /// </summary>
        [HttpGet]
        [Authorize(Roles = Roles.Groups.Admins)]
        [HasPermission(Permission.View)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 50) pageSize = 10;

            var orders = await _service.GetAllOrdersAsync(
                page, pageSize);

            return Ok(ApiResponse<object>.Ok(orders));
        }

        /// <summary>
        /// Get single order by ID
        /// Customer sees own orders only.
        /// Admin sees all.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.GetUserId();
            var isAdmin = User.IsAdmin();

            var order = await _service.GetByIdAsync(
                id, userId, isAdmin);

            if (order == null)
                return NotFound(
                    ApiResponse<object>.Fail(
                        $"Order {id} not found."));

            return Ok(ApiResponse<object>.Ok(order));
        }

        /// <summary>
        /// Place a new order
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrder(
            [FromBody] CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "Validation failed.",
                        ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()));

            var userId = User.GetUserId();
            var userEmail = User.GetEmail();

            var order = await _service.CreateOrderAsync(
                dto, userId, userEmail);

            return CreatedAtAction(
                nameof(GetById),
                new { id = order.Id },
                ApiResponse<object>.Ok(
                    order,
                    "Order placed successfully."));
        }

        /// <summary>
        /// Cancel an order
        /// Customer can cancel own Pending/Confirmed orders.
        /// Admin can cancel any order.
        /// </summary>
        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> CancelOrder(
            int id, [FromBody] CancelOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "Reason is required."));

            var userId = User.GetUserId();
            var isAdmin = User.IsAdmin();

            var result = await _service.CancelOrderAsync(
                id, userId, isAdmin, dto);

            if (!result)
                return NotFound(
                    ApiResponse<object>.Fail(
                        $"Order {id} not found."));

            return Ok(ApiResponse<object>.Ok(
                null!,
                "Order cancelled successfully."));
        }

        /// <summary>
        /// Force update order status — Admin only
        /// </summary>
        [HttpPut("{id:int}/status")]
        [Authorize(Roles = Roles.Groups.Admins)]
        [HasPermission(Permission.Edit)]
        public async Task<IActionResult> UpdateStatus(
            int id, [FromQuery] OrderStatus status)
        {
            var result = await _service
                .UpdateStatusAsync(id, status);

            if (!result)
                return NotFound(
                    ApiResponse<object>.Fail(
                        $"Order {id} not found."));

            return Ok(ApiResponse<object>.Ok(
                null!,
                $"Order status updated to {status}."));
        }
    }
}