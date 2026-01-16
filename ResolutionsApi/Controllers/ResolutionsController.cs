using Microsoft.AspNetCore.Mvc;
using ResolutionsApi.Models;

namespace ResolutionsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResolutionsController : ControllerBase
    {
        private static List<Resolution> _items = new();
        private static int _nextId = 1;

        // --------------------
        // GET ALL (FILTER + SEARCH)
        // --------------------
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? isDone, [FromQuery] string? title)
        {
            IEnumerable<Resolution> result = _items;

            if (!string.IsNullOrEmpty(isDone))
            {
                if (!bool.TryParse(isDone, out bool done))
                {
                    return ErrorResponse(
                        "BadRequest",
                        "Validation failed.",
                        new[] { "isDone must be true or false" },
                        400
                    );
                }

                result = result.Where(r => r.IsDone == done);
            }

            if (!string.IsNullOrWhiteSpace(title))
            {
                result = result.Where(r => r.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            }

            return Ok(new { items = result });
        }

        // --------------------
        // GET BY ID
        // --------------------
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            if (id <= 0)
                return ErrorResponse("BadRequest", "Validation failed.", new[] { "id must be greater than 0" }, 400);

            var item = _items.FirstOrDefault(r => r.Id == id);
            if (item == null)
                return ErrorResponse("NotFound", "Resolution not found.", new[] { $"id {id} not found" }, 404);

            return Ok(item);
        }

        // --------------------
        // CREATE
        // --------------------
        [HttpPost]
        public IActionResult Create([FromBody] Resolution? input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Title))
            {
                return ErrorResponse(
                    "BadRequest",
                    "Validation failed.",
                    new[] { "title is required" },
                    400
                );
            }

            var item = new Resolution
            {
                Id = _nextId++,
                Title = input.Title.Trim(),
                IsDone = false,
                CreatedAt = DateTime.UtcNow
            };

            _items.Add(item);
            return Created("", item);
        }

        // --------------------
        // UPDATE (FULL REPLACE)
        // --------------------
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Resolution? input)
        {
            if (id <= 0)
                return ErrorResponse("BadRequest", "Validation failed.", new[] { "route id must be greater than 0" }, 400);

            if (input == null)
                return ErrorResponse("BadRequest", "Validation failed.", new[] { "body is required" }, 400);

            if (input.Id == 0)
                return ErrorResponse("BadRequest", "Validation failed.", new[] { "body id is required" }, 400);

            if (id != input.Id)
            {
                return ErrorResponse(
                    "BadRequest",
                    "Route id does not match body id.",
                    new[] { $"route id: {id}", $"body id: {input.Id}" },
                    400
                );
            }

            if (string.IsNullOrWhiteSpace(input.Title))
                return ErrorResponse("BadRequest", "Validation failed.", new[] { "title is required" }, 400);

            var item = _items.FirstOrDefault(r => r.Id == id);
            if (item == null)
                return ErrorResponse("NotFound", "Resolution not found.", new[] { $"id {id} not found" }, 404);

            item.Title = input.Title.Trim();
            item.IsDone = input.IsDone;
            item.UpdatedAt = DateTime.UtcNow;

            return Ok(item);
        }

        // --------------------
        // DELETE
        // --------------------
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
                return ErrorResponse("BadRequest", "Validation failed.", new[] { "id must be greater than 0" }, 400);

            var item = _items.FirstOrDefault(r => r.Id == id);
            if (item == null)
                return ErrorResponse("NotFound", "Resolution not found.", new[] { $"id {id} not found" }, 404);

            _items.Remove(item);
            return NoContent();
        }

        // --------------------
        // ERROR FORMAT HELPER
        // --------------------
        private IActionResult ErrorResponse(string error, string message, IEnumerable<string> details, int statusCode)
        {
            return StatusCode(statusCode, new
            {
                error,
                message,
                details
            });
        }
    }
}
