using Microsoft.AspNetCore.Mvc;
using SMARTMOB_PANTAREI_BACK.DTOs;
using SMARTMOB_PANTAREI_BACK.Services;

namespace SMARTMOB_PANTAREI_BACK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostazioniController : ControllerBase
    {
        private readonly IPostazioniService _postazioniService;
        private readonly ILogger<PostazioniController> _logger;

        public PostazioniController(IPostazioniService postazioniService, ILogger<PostazioniController> logger)
        {
            _postazioniService = postazioniService;
            _logger = logger;
        }

        /// <summary>
        /// Get all postazioni
        /// </summary>
        /// <returns>List of all postazioni</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PostazioniDto>>> GetAll()
        {
            try
            {
                var postazioni = await _postazioniService.GetAllAsync();
                return Ok(postazioni);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all postazioni");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get postazione by composite key (CodLineaProd and CodPostazione)
        /// </summary>
        /// <param name="codLineaProd">Codice linea produzione</param>
        /// <param name="codPostazione">Codice postazione</param>
        /// <returns>Postazione details</returns>
        [HttpGet("{codLineaProd}/{codPostazione}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostazioniDto>> GetById(string codLineaProd, string codPostazione)
        {
            try
            {
                var postazione = await _postazioniService.GetByIdAsync(codLineaProd, codPostazione);
                if (postazione == null)
                {
                    return NotFound($"Postazione with CodLineaProd '{codLineaProd}' and CodPostazione '{codPostazione}' not found");
                }
                return Ok(postazione);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving postazione with CodLineaProd {CodLineaProd} and CodPostazione {CodPostazione}", codLineaProd, codPostazione);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new postazione
        /// </summary>
        /// <param name="createDto">Postazione data</param>
        /// <returns>Created postazione</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<PostazioniDto>> Create([FromBody] CreatePostazioniDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if postazione already exists
                var existing = await _postazioniService.GetByIdAsync(createDto.CodLineaProd, createDto.CodPostazione);
                if (existing != null)
                {
                    return Conflict($"Postazione with CodLineaProd '{createDto.CodLineaProd}' and CodPostazione '{createDto.CodPostazione}' already exists");
                }

                var postazione = await _postazioniService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetById), 
                    new { codLineaProd = postazione.CodLineaProd, codPostazione = postazione.CodPostazione }, 
                    postazione);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating postazione");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update an existing postazione
        /// </summary>
        /// <param name="codLineaProd">Codice linea produzione</param>
        /// <param name="codPostazione">Codice postazione</param>
        /// <param name="updateDto">Updated postazione data</param>
        /// <returns>Updated postazione</returns>
        [HttpPut("{codLineaProd}/{codPostazione}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PostazioniDto>> Update(string codLineaProd, string codPostazione, [FromBody] UpdatePostazioniDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var postazione = await _postazioniService.UpdateAsync(codLineaProd, codPostazione, updateDto);
                if (postazione == null)
                {
                    return NotFound($"Postazione with CodLineaProd '{codLineaProd}' and CodPostazione '{codPostazione}' not found");
                }

                return Ok(postazione);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating postazione with CodLineaProd {CodLineaProd} and CodPostazione {CodPostazione}", codLineaProd, codPostazione);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a postazione
        /// </summary>
        /// <param name="codLineaProd">Codice linea produzione</param>
        /// <param name="codPostazione">Codice postazione</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{codLineaProd}/{codPostazione}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(string codLineaProd, string codPostazione)
        {
            try
            {
                var deleted = await _postazioniService.DeleteAsync(codLineaProd, codPostazione);
                if (!deleted)
                {
                    return NotFound($"Postazione with CodLineaProd '{codLineaProd}' and CodPostazione '{codPostazione}' not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting postazione with CodLineaProd {CodLineaProd} and CodPostazione {CodPostazione}", codLineaProd, codPostazione);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get postazioni by linea produzione
        /// </summary>
        /// <param name="codLineaProd">Codice linea produzione</param>
        /// <returns>List of postazioni for the specified linea produzione</returns>
        [HttpGet("by-linea/{codLineaProd}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PostazioniDto>>> GetByLineaProd(string codLineaProd)
        {
            try
            {
                var postazioni = await _postazioniService.GetByLineaProdAsync(codLineaProd);
                return Ok(postazioni);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving postazioni for linea produzione {CodLineaProd}", codLineaProd);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}