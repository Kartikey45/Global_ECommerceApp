using ECommerceApp.Product.DTOs.Request;
using ECommerceApp.Product.Services.Interfaces;
using ECommerceApp.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Product.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoriesController(
            ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _service.GetAllAsync();
            return Ok(ApiResponse<object>.Ok(categories));
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _service.GetByIdAsync(id);

            if (category == null)
                return NotFound(
                    ApiResponse<object>.Fail(
                        $"Category {id} not found."));

            return Ok(ApiResponse<object>.Ok(category));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "Validation failed.",
                        ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()));

            var category =
                await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = category.Id },
                ApiResponse<object>.Ok(
                    category,
                    "Category created successfully."));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound(
                    ApiResponse<object>.Fail(
                        $"Category {id} not found."));

            return Ok(ApiResponse<object>.Ok(
                null!,
                "Category deleted successfully."));
        }
    }
}