using Microsoft.EntityFrameworkCore;
using SMARTMOB_PANTAREI_BACK.Data;
using SMARTMOB_PANTAREI_BACK.DTOs;
using SMARTMOB_PANTAREI_BACK.Models;

namespace SMARTMOB_PANTAREI_BACK.Services
{
    public interface IAcquisizioniService
    {
        Task<IEnumerable<AcquisizioniDto>> GetAllAsync();
        Task<AcquisizioniDto?> GetByIdAsync(int id);
        Task<IEnumerable<AcquisizioniDto>> GetByLineaAndPostazioneAsync(string codLinea, string codPostazione);
    }

    public class AcquisizioniService : IAcquisizioniService
    {
        private readonly ApplicationDbContext _context;

        public AcquisizioniService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AcquisizioniDto>> GetAllAsync()
        {
            var acquisizioni = await _context.Acquisizioni.ToListAsync();
            return acquisizioni.Select(MapToDto);
        }

        public async Task<AcquisizioniDto?> GetByIdAsync(int id)
        {
            var acquisizione = await _context.Acquisizioni.FindAsync(id);
            return acquisizione != null ? MapToDto(acquisizione) : null;
        }

        public async Task<IEnumerable<AcquisizioniDto>> GetByLineaAndPostazioneAsync(string codLinea, string codPostazione)
        {
            var acquisizioni = await _context.Acquisizioni
                .Where(a => a.CodLinea == codLinea && a.CodPostazione == codPostazione)
                .ToListAsync();
            return acquisizioni.Select(MapToDto);
        }

        private static AcquisizioniDto MapToDto(Acquisizioni acquisizione)
        {
            bool? EsitoToBool(int? val)
            {
                if (!val.HasValue) return null;
                return val.Value != 0;
            }

            return new AcquisizioniDto
            {
                Id = acquisizione.Id,
                CodLinea = acquisizione.CodLinea,
                CodPostazione = acquisizione.CodPostazione,
                FotoAcquisizione = acquisizione.FotoAcquisizione,
                CodiceArticolo = acquisizione.CodiceArticolo,
                IdCatasta = acquisizione.IdCatasta,
                AbilitaCq = acquisizione.AbilitaCq,
                EsitoCqArticolo = EsitoToBool(acquisizione.EsitoCqArticolo),
                NumSpineContate = acquisizione.NumSpineContate,
                NumSpineAttese = acquisizione.NumSpineAttese,
                DataInserimento = acquisizione.DataInserimento,
                DataAggiornamento = acquisizione.DataAggiornamento,
                Descrizione = acquisizione.Descrizione
            };
        }
    }
}