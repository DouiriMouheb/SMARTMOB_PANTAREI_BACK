using Microsoft.AspNetCore.Mvc;
using SMARTMOB_PANTAREI_BACK.DTOs;
using SMARTMOB_PANTAREI_BACK.Services;

namespace SMARTMOB_PANTAREI_BACK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AcquisizioniController : ControllerBase
    {
        private readonly IAcquisizioniService _acquisizioniService;
        private readonly ILogger<AcquisizioniController> _logger;

        public AcquisizioniController(IAcquisizioniService acquisizioniService, ILogger<AcquisizioniController> logger)
        {
            _acquisizioniService = acquisizioniService;
            _logger = logger;
        }

        /// <summary>
        /// Get all acquisizioni
        /// </summary>
        /// <returns>List of all acquisizioni</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AcquisizioniDto>>> GetAll()
        {
            try
            {
                var acquisizioni = await _acquisizioniService.GetAllAsync();
                return Ok(acquisizioni);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all acquisizioni");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get acquisizione by ID
        /// </summary>
        /// <param name="id">Acquisizione ID</param>
        /// <returns>Acquisizione details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AcquisizioniDto>> GetById(int id)
        {
            try
            {
                var acquisizione = await _acquisizioniService.GetByIdAsync(id);
                if (acquisizione == null)
                {
                    return NotFound($"Acquisizione with ID {id} not found");
                }
                return Ok(acquisizione);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving acquisizione with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}