using Core.DTO;
using Core.Entities;
using FluentValidation;
using Infraestructura.Repositories;
using Infraestructura.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Weather.api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CocktailsController: ControllerBase
    {
        private readonly ILogger<CocktailsController> logger;
        private readonly CocktailClientService cocktailClientService;
        private readonly IHttpContextAccessor httpContext;

        public CocktailsController(ILogger<CocktailsController> logger, CocktailClientService cocktailClientService,
            IHttpContextAccessor httpContext)
        {
            this.logger = logger;
            this.cocktailClientService = cocktailClientService;
            this.httpContext = httpContext;
        }


        // Lista de tipos de cócteles (Alcoholic / Non alcoholic / Optional alcohol).
        [HttpGet("AlcoholTypes")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AlcoholTypeDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<AlcoholTypeDTO>>> GetAlcoholTypesAsync()
        {
            // Extraemos el TraceId para correlación de logs
            var traceId = httpContext.HttpContext?.TraceIdentifier.Split(':')[0] ?? "";

            try
            {
                logger.LogInformation("[{TraceId}] Call: GetTypesAsync()", traceId);

                // Llamada al servicio que envuelve RestClient
                var response = await cocktailClientService.GetAlcoholTypesAsync();

                logger.LogInformation(response != null
                    ? "[{TraceId}] FinishCall: GetTypesAsync – returned {Count} items"
                    : "[{TraceId}] FinishCall: GetTypesAsync – response null",
                    traceId,
                    response?.Count);

                if (response == null || !response.Any())
                    return NotFound();

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{TraceId}] GetTypesAsync error", traceId);
                return StatusCode(500, new
                {
                    Message = "Error llamando a TheCocktailDB",
                    Detail = ex.Message
                });
            }
        }


        // Lista los cócteles filtrados por tipo ('Alcoholic', 'Non alcoholic' o 'Optional alcohol').
        [HttpGet("ByAlcoholType")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CocktailItemDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<CocktailItemDTO>>> GetByTypeAsync([FromQuery(Name = "type")] string type)
        {
            var traceId = httpContext.HttpContext?.TraceIdentifier.Split(':')[0] ?? "";

            if (string.IsNullOrWhiteSpace(type))
            {
                logger.LogWarning("[{TraceId}] GetByTypeAsync called without a type", traceId);
                return BadRequest(new { Message = "El parámetro 'type' es obligatorio." });
            }

            try
            {
                logger.LogInformation("[{TraceId}] Call: GetByTypeAsync(type={Type})", traceId, type);

                var response = await cocktailClientService.GetByAlcoholTypeAsync(type);

                logger.LogInformation(response != null
                    ? "[{TraceId}] FinishCall: GetByTypeAsync – returned {Count} items"
                    : "[{TraceId}] FinishCall: GetByTypeAsync – response null",
                    traceId,
                    response?.Count);

                if (response == null || !response.Any())
                    return NotFound();

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{TraceId}] GetByTypeAsync error", traceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "Error llamando a TheCocktailDB",
                    Detail = ex.Message
                });
            }
        }


        // Detalle completo de un cóctel por su ID.
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CocktailDetailDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CocktailDetailDTO>> GetByIdAsync(string id)
        {
            var traceId = httpContext.HttpContext?.TraceIdentifier.Split(':')[0] ?? "";

            if (string.IsNullOrWhiteSpace(id))
            {
                logger.LogWarning("[{TraceId}] GetByIdAsync called without id", traceId);
                return BadRequest(new { Message = "El parámetro 'id' es obligatorio." });
            }

            try
            {
                logger.LogInformation("[{TraceId}] Call: GetByIdAsync(id={Id})", traceId, id);

                var result = await cocktailClientService.GetByIdAsync(id);

                if (result == null)
                    return NotFound();

                logger.LogInformation("[{TraceId}] FinishCall: GetByIdAsync – found cocktail {Id}", traceId, id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{TraceId}] GetByIdAsync error", traceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "Error llamando a TheCocktailDB",
                    Detail = ex.Message
                });
            }
        }


        // Lista de categorías de cócteles.
        [HttpGet("Categories")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CategoryDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<CategoryDTO>>> GetCategoriesAsync()
        {
            var traceId = httpContext.HttpContext?.TraceIdentifier.Split(':')[0] ?? "";

            try
            {
                logger.LogInformation("[{TraceId}] Call: GetCategoriesAsync()", traceId);

                var response = await cocktailClientService.GetCategoriesAsync();

                logger.LogInformation(response != null
                    ? "[{TraceId}] FinishCall: GetCategoriesAsync – returned {Count} items"
                    : "[{TraceId}] FinishCall: GetCategoriesAsync – response null",
                    traceId,
                    response?.Count);

                if (response == null || !response.Any())
                    return NotFound();

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{TraceId}] GetCategoriesAsync error", traceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "Error llamando a TheCocktailDB",
                    Detail = ex.Message
                });
            }
        }


        // Lista los cócteles filtrados por categoría (por ejemplo "Ordinary Drink").
        [HttpGet("ByCategory")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CocktailItemDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<CocktailItemDTO>>> GetByCategoryAsync([FromQuery(Name = "category")] string category)
        {
            var traceId = httpContext.HttpContext?.TraceIdentifier.Split(':')[0] ?? "";

            if (string.IsNullOrWhiteSpace(category))
            {
                logger.LogWarning("[{TraceId}] GetByCategoryAsync called without category", traceId);
                return BadRequest(new { Message = "El parámetro 'category' es obligatorio." });
            }

            try
            {
                logger.LogInformation("[{TraceId}] Call: GetByCategoryAsync(category={Category})", traceId, category);

                var response = await cocktailClientService.GetByCategoryAsync(category);

                logger.LogInformation(response != null
                    ? "[{TraceId}] FinishCall: GetByCategoryAsync – returned {Count} items"
                    : "[{TraceId}] FinishCall: GetByCategoryAsync – response null",
                    traceId,
                    response?.Count);

                if (response == null || !response.Any())
                    return NotFound();

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{TraceId}] GetByCategoryAsync error", traceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "Error llamando a TheCocktailDB",
                    Detail = ex.Message
                });
            }
        }


        // Lista de tipos de vasos disponibles.
        [HttpGet("Glasses")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<GlassDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<GlassDTO>>> GetGlassesAsync()
        {
            var traceId = httpContext.HttpContext?.TraceIdentifier.Split(':')[0] ?? "";

            try
            {
                logger.LogInformation("[{TraceId}] Call: GetGlassesAsync()", traceId);

                var response = await cocktailClientService.GetGlassesAsync();

                logger.LogInformation(response != null
                    ? "[{TraceId}] FinishCall: GetGlassesAsync – returned {Count} items"
                    : "[{TraceId}] FinishCall: GetGlassesAsync – response null",
                    traceId,
                    response?.Count);

                if (response == null || !response.Any())
                    return NotFound();

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{TraceId}] GetGlassesAsync error", traceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "Error llamando a TheCocktailDB",
                    Detail = ex.Message
                });
            }
        }


        // Lista los cócteles filtrados por tipo de vaso (por ejemplo "Cocktail glass").
        [HttpGet("ByGlass")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CocktailItemDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<CocktailItemDTO>>> GetByGlassAsync([FromQuery(Name = "glass")] string glass)
        {
            var traceId = httpContext.HttpContext?.TraceIdentifier.Split(':')[0] ?? "";

            if (string.IsNullOrWhiteSpace(glass))
            {
                logger.LogWarning("[{TraceId}] GetByGlassAsync called without glass", traceId);
                return BadRequest(new { Message = "El parámetro 'glass' es obligatorio." });
            }

            try
            {
                logger.LogInformation("[{TraceId}] Call: GetByGlassAsync(glass={Glass})", traceId, glass);

                var response = await cocktailClientService.GetByGlassAsync(glass);

                logger.LogInformation(response != null
                    ? "[{TraceId}] FinishCall: GetByGlassAsync – returned {Count} items"
                    : "[{TraceId}] FinishCall: GetByGlassAsync – response null",
                    traceId,
                    response?.Count);

                if (response == null || !response.Any())
                    return NotFound();

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{TraceId}] GetByGlassAsync error", traceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "Error llamando a TheCocktailDB",
                    Detail = ex.Message
                });
            }
        }


        // Devuelve la lista de ingredientes disponibles.
        [HttpGet("Ingredients")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<IngredientSummaryDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<IngredientSummaryDTO>>> GetIngredientsAsync()
        {
            var traceId = httpContext.HttpContext?.TraceIdentifier.Split(':')[0] ?? "";

            try
            {
                logger.LogInformation("[{TraceId}] Call: GetIngredientsAsync()", traceId);

                var response = await cocktailClientService.GetIngredientsAsync();

                logger.LogInformation(response != null
                    ? "[{TraceId}] FinishCall: GetIngredientsAsync – returned {Count} items"
                    : "[{TraceId}] FinishCall: GetIngredientsAsync – response null",
                    traceId,
                    response?.Count);

                if (response == null || !response.Any())
                    return NotFound();

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{TraceId}] GetIngredientsAsync error", traceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "Error llamando a TheCocktailDB",
                    Detail = ex.Message
                });
            }
        }


        // Obtiene el detalle completo de un ingrediente por su ID.
        [HttpGet("Ingredients/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IngredientDetailDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IngredientDetailDTO>> GetIngredientByIdAsync(string id)
        {
            var traceId = httpContext.HttpContext?.TraceIdentifier.Split(':')[0] ?? "";

            if (string.IsNullOrWhiteSpace(id))
            {
                logger.LogWarning("[{TraceId}] GetIngredientByIdAsync called without id", traceId);
                return BadRequest(new { Message = "El parámetro 'id' es obligatorio." });
            }

            try
            {
                logger.LogInformation("[{TraceId}] Call: GetIngredientByIdAsync(id={Id})", traceId, id);

                var result = await cocktailClientService.GetIngredientByIdAsync(id);
                if (result == null)
                    return NotFound();

                logger.LogInformation("[{TraceId}] FinishCall: GetIngredientByIdAsync – found ingredient {Id}", traceId, id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{TraceId}] GetIngredientByIdAsync error", traceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "Error llamando a TheCocktailDB",
                    Detail = ex.Message
                });
            }
        }


        // Detalle de ingrediente por nombre
        [HttpGet("IngredientNamePopup")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IngredientDetailDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IngredientDetailDTO>> GetIngredientByNameAsync([FromQuery] string name)
        {
            // TraceId para correlación de logs
            var traceId = httpContext.HttpContext?.TraceIdentifier?.Split(':')[0] ?? "";

            if (string.IsNullOrWhiteSpace(name))
            {
                logger.LogWarning("[{TraceId}] GetIngredientByNameAsync – parámetro 'name' vacío", traceId);
                return BadRequest(new { Message = "El parámetro de consulta 'name' es obligatorio." });
            }

            try
            {
                logger.LogInformation("[{TraceId}] Call: GetIngredientByNameAsync(name={Name})", traceId, name);

                var dto = await cocktailClientService.GetIngredientByNameAsync(name);

                logger.LogInformation(dto != null
                    ? "[{TraceId}] FinishCall: GetIngredientByNameAsync – encontrado"
                    : "[{TraceId}] FinishCall: GetIngredientByNameAsync – no encontrado",
                    traceId);

                if (dto == null)
                    return NotFound();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{TraceId}] GetIngredientByNameAsync error", traceId);
                return StatusCode(500, new
                {
                    Message = "Error llamando a TheCocktailDB",
                    Detail = ex.Message
                });
            }
        }


        // Obtener random de BD
        [HttpGet("Random/Row")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RandomCocktailDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RandomCocktailDTO>> GetRandomRowAsync(
            [FromServices] RandomCocktailRepository repo)
        {
            try
            {
                var row = await repo.GetAsync();
                if (row is null) return NotFound(new { Message = "No hay fila persistida todavía." });

                return Ok(new RandomCocktailDTO
                {
                    DrinkId = row.DrinkId,
                    Name = row.Name,
                    ThumbUrl = row.ThumbUrl
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetRandomRowAsync error.");
                return StatusCode(500, new { Message = "Error interno.", Detail = ex.Message });
            }
        }


        // Llama a API, upsert en BD y devuelve la fila actualizada
        [HttpPost("Random/RefreshNow")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RandomCocktailDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RandomCocktailDTO>> RefreshRandomNowAsync(
            [FromServices] CocktailClientService client,
            [FromServices] RandomCocktailRepository repo,
            CancellationToken ct = default)
        {
            try
            {
                var dto = await client.GetRandomLiteAsync(ct);
                if (dto is null)
                    return NotFound(new { Message = "No se pudo obtener un cóctel aleatorio." });

                await repo.AddOrUpdateAsync(new RandomCocktail
                {
                    DrinkId = dto.DrinkId ?? "",
                    Name = dto.Name ?? "",
                    ThumbUrl = dto.ThumbUrl ?? ""
                }, ct);

                return Ok(dto);
            }
            catch (DbUpdateException dbex)
            {
                logger.LogError(dbex, "EF error: {Detail}", dbex.InnerException?.Message);
                return StatusCode(500, new { Message = "Error interno.", Detail = dbex.InnerException?.Message ?? dbex.Message });
            }

        }
    }
}
