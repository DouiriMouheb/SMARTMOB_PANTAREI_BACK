using SMARTMOB_PANTAREI_BACK.DTOs;

namespace SMARTMOB_PANTAREI_BACK.Services
{
    public interface IPostazioniPerLineaService
    {
        Task<IEnumerable<PostazioniPerLineaDto>> GetAllAsync();
        Task<PostazioniPerLineaDto?> GetByLineaAsync(string codLinea);
    }
}