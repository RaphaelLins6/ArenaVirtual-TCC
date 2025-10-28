using ArenaVirtualAPI.Models;
using System.Threading.Tasks;

namespace ArenaVirtualAPI.Services {
    public interface IPatrocinioDetalheService {
        Task<PatrocinioDetalhe?> GetDetalheByPropostaIdAsync(int propostaId);
    }
}