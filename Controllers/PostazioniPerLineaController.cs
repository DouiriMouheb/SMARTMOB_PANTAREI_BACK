using Microsoft.AspNetCore.Mvc;
using SMARTMOB_PANTAREI_BACK.DTOs;
using SMARTMOB_PANTAREI_BACK.Services;

namespace SMARTMOB_PANTAREI_BACK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostazioniPerLineaController : ControllerBase
    {
        private readonly IPostazioniPerLineaService _postazioniPerLineaService;
        private readonly ILogger<PostazioniPerLineaController> _logger;

        public PostazioniPerLineaController(
            IPostazioniPerLineaService postazioniPerLineaService,
            ILogger<PostazioniPerLineaController> logger)
        {
            _postazioniPerLineaService = postazioniPerLineaService;
            _logger = logger;
        }

        /// <summary>
        /// Get all postazioni grouped by linea
        /// </summary>
        /// <returns>List of postazioni per linea</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostazioniPerLineaDto>>> GetAll()
        {
            try
            {
                var result = await _postazioniPerLineaService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all postazioni per linea");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get postazioni for a specific linea
        /// </summary>
        /// <param name="codLinea">Production line code</param>
        /// <returns>Postazioni for the specified linea</returns>
        [HttpGet("{codLinea}")]
        public async Task<ActionResult<PostazioniPerLineaDto>> GetByLinea(string codLinea)
        {
            try
            {
                var result = await _postazioniPerLineaService.GetByLineaAsync(codLinea);
                
                if (result == null)
                {
                    return NotFound($"Postazioni for linea {codLinea} not found");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving postazioni per linea for {CodLinea}", codLinea);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}