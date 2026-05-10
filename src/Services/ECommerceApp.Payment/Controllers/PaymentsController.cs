using ECommerceApp.Payment.Services.Interfaces;
using ECommerceApp.Shared.Helpers;
using ECommerceApp.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Payment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _service;

        public PaymentsController(
            IPaymentService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get payment details by payment ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var payment = await _service.GetByIdAsync(id);

            if (payment == null)
                return NotFound(
                    ApiResponse<object>.Fail(
                        $"Payment {id} not found."));

            // Customer can only see own payments
            if (!User.IsAdmin() &&
                payment.UserId != User.GetUserId())
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "Not authorized to view " +
                        "this payment."));

            return Ok(ApiResponse<object>.Ok(payment));
        }

        /// <summary>
        /// Get payment by Order ID
        /// </summary>
        [HttpGet("order/{orderId:int}")]
        public async Task<IActionResult> GetByOrderId(
            int orderId)
        {
            var payment =
                await _service.GetByOrderIdAsync(orderId);

            if (payment == null)
                return NotFound(
                    ApiResponse<object>.Fail(
                        $"Payment for Order {orderId} " +
                        $"not found."));

            if (!User.IsAdmin() &&
                payment.UserId != User.GetUserId())
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "Not authorized to view " +
                        "this payment."));

            return Ok(ApiResponse<object>.Ok(payment));
        }

        /// <summary>
        /// Process refund for a payment — Admin only
        /// </summary>
        [HttpPost("{id:int}/refund")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Refund(int id)
        {
            var payment =
                await _service.ProcessRefundAsync(id);

            if (payment == null)
                return NotFound(
                    ApiResponse<object>.Fail(
                        $"Payment {id} not found."));

            return Ok(ApiResponse<object>.Ok(
                payment,
                "Refund processed successfully."));
        }
    }
}