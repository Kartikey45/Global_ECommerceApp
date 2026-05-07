using ECommerceApp.Product.DTOs.Request;
using ECommerceApp.Product.Services.Interfaces;
using ECommerceApp.Shared.Helpers;
using ECommerceApp.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Product.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductsController(IProductService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get paginated products with optional filters.
        /// Public endpoint — no auth required.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page        = 1,
            [FromQuery] int pageSize    = 10,
            [FromQuery] int? categoryId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? search  = null)
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 100) pageSize = 10;

            var result = await _service.GetAllAsync(
                page, pageSize, categoryId,
                minPrice, maxPrice, search);

            return Ok(ApiResponse<object>.Ok(result));
        }

        /// <summary>Get single product by ID.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _service.GetByIdAsync(id);

            if (product == null)
                return NotFound(
                    ApiResponse<object>.Fail(
                        $"Product {id} not found."));

            return Ok(ApiResponse<object>.Ok(product));
        }

        /// <summary>Get single product by SKU.</summary>
        [HttpGet("sku/{sku}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBySku(string sku)
        {
            var product = await _service.GetBySkuAsync(sku);

            if (product == null)
                return NotFound(
                    ApiResponse<object>.Fail(
                        $"Product with SKU '{sku}' " +
                        $"not found."));

            return Ok(ApiResponse<object>.Ok(product));
        }

        /// <summary>Create product. Admin only.</summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create(
            [FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "Validation failed.",
                        ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()));

            var product = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = product.Id },
                ApiResponse<object>.Ok(
                    product,
                    "Product created successfully."));
        }

        /// <summary>Update product. Admin only.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(
            int id, [FromBody] UpdateProductDto dto)
        {
            var product =
                await _service.UpdateAsync(id, dto);

            if (product == null)
                return NotFound(
                    ApiResponse<object>.Fail(
                        $"Product {id} not found."));

            return Ok(ApiResponse<object>.Ok(
                product,
                "Product updated successfully."));
        }

        /// <summary>Soft delete product. Admin only.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound(
                    ApiResponse<object>.Fail(
                        $"Product {id} not found."));

            return Ok(ApiResponse<object>.Ok(
                null!, "Product deleted successfully."));
        }

        /// <summary>
        /// Deduct stock atomically via SP.
        /// Called by Order Service internally.
        /// </summary>
        [HttpPatch("{id:int}/stock")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateStock(
            int id, [FromBody] UpdateStockDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "Validation failed."));

            var (success, message) =
                await _service.UpdateStockAsync(
                    id, dto.Quantity);

            if (!success)
                return BadRequest(
                    ApiResponse<object>.Fail(message));

            return Ok(ApiResponse<object>.Ok(
                null!, message));
        }
    }
}