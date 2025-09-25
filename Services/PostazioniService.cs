using Microsoft.EntityFrameworkCore;
using SMARTMOB_PANTAREI_BACK.Data;
using SMARTMOB_PANTAREI_BACK.DTOs;
using SMARTMOB_PANTAREI_BACK.Models;

namespace SMARTMOB_PANTAREI_BACK.Services
{
    public interface IPostazioniService
    {
        Task<IEnumerable<PostazioniDto>> GetAllAsync();
        Task<PostazioniDto?> GetByIdAsync(string codLineaProd, string codPostazione);
        Task<PostazioniDto> CreateAsync(CreatePostazioniDto createDto);
        Task<PostazioniDto?> UpdateAsync(string codLineaProd, string codPostazione, UpdatePostazioniDto updateDto);
        Task<bool> DeleteAsync(string codLineaProd, string codPostazione);
        Task<IEnumerable<PostazioniDto>> GetByLineaProdAsync(string codLineaProd);
    }

    public class PostazioniService : IPostazioniService
    {
        private readonly ApplicationDbContext _context;

        public PostazioniService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PostazioniDto>> GetAllAsync()
        {
            var postazioni = await _context.Postazioni.ToListAsync();
            return postazioni.Select(MapToDto);
        }

        public async Task<PostazioniDto?> GetByIdAsync(string codLineaProd, string codPostazione)
        {
            var postazione = await _context.Postazioni
                .FirstOrDefaultAsync(p => p.CodLineaProd == codLineaProd && p.CodPostazione == codPostazione);
            return postazione != null ? MapToDto(postazione) : null;
        }

        public async Task<PostazioniDto> CreateAsync(CreatePostazioniDto createDto)
        {
            var postazione = new Postazioni
            {
                CodLineaProd = createDto.CodLineaProd,
                CodPostazione = createDto.CodPostazione,
                // FlgRullieraBox = createDto.FlgRullieraBox,
                DataInserimento = DateTime.Now
            };

            _context.Postazioni.Add(postazione);
            await _context.SaveChangesAsync();

            return MapToDto(postazione);
        }

        public async Task<PostazioniDto?> UpdateAsync(string codLineaProd, string codPostazione, UpdatePostazioniDto updateDto)
        {
            var postazione = await _context.Postazioni
                .FirstOrDefaultAsync(p => p.CodLineaProd == codLineaProd && p.CodPostazione == codPostazione);
            
            if (postazione == null)
                return null;

            // postazione.FlgRullieraBox = updateDto.FlgRullieraBox;

            await _context.SaveChangesAsync();
            return MapToDto(postazione);
        }

        public async Task<bool> DeleteAsync(string codLineaProd, string codPostazione)
        {
            var postazione = await _context.Postazioni
                .FirstOrDefaultAsync(p => p.CodLineaProd == codLineaProd && p.CodPostazione == codPostazione);
            
            if (postazione == null)
                return false;

            _context.Postazioni.Remove(postazione);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PostazioniDto>> GetByLineaProdAsync(string codLineaProd)
        {
            var postazioni = await _context.Postazioni
                .Where(p => p.CodLineaProd == codLineaProd)
                .ToListAsync();
            return postazioni.Select(MapToDto);
        }

        private static PostazioniDto MapToDto(Postazioni postazione)
        {
            return new PostazioniDto
            {
                CodLineaProd = postazione.CodLineaProd,
                CodPostazione = postazione.CodPostazione,
                // FlgRullieraBox = postazione.FlgRullieraBox,
                DataInserimento = postazione.DataInserimento
            };
        }
    }
}