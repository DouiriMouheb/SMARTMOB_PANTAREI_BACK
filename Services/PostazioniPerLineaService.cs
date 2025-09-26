using Microsoft.EntityFrameworkCore;
using SMARTMOB_PANTAREI_BACK.Data;
using SMARTMOB_PANTAREI_BACK.DTOs;
using SMARTMOB_PANTAREI_BACK.Models;

namespace SMARTMOB_PANTAREI_BACK.Services
{
    public class PostazioniPerLineaService : IPostazioniPerLineaService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PostazioniPerLineaService> _logger;

        public PostazioniPerLineaService(ApplicationDbContext context, ILogger<PostazioniPerLineaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<PostazioniPerLineaDto>> GetAllAsync()
        {
            try
            {
                var postazioniPerLinea = await _context.PostazioniPerLinea
                    .AsNoTracking()
                    .ToListAsync();

                return postazioniPerLinea.Select(p => new PostazioniPerLineaDto
                {
                    CoD_LINEA_PROD = p.CodLinea,
                    CoD_POSTAZIONE = new List<string> { p.CodPostazioneList }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving postazioni per linea");
                throw;
            }
        }

        public async Task<PostazioniPerLineaDto?> GetByLineaAsync(string codLinea)
        {
            try
            {
                var postazionePerLinea = await _context.PostazioniPerLinea
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.CodLinea == codLinea);

                if (postazionePerLinea == null)
                    return null;

                return new PostazioniPerLineaDto
                {
                    CoD_LINEA_PROD = postazionePerLinea.CodLinea,
                    CoD_POSTAZIONE = new List<string> { postazionePerLinea.CodPostazioneList }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving postazione per linea for {CodLinea}", codLinea);
                throw;
            }
        }
    }
}