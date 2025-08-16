using ArenaVirtual.Models;
using MvvmHelpers;

namespace ArenaVirtual.ViewModels.Atleta {
    public class ConviteViewModel : BaseViewModel {
        public Convite ConviteOriginal { get; }
        public Usuario UsuarioSolicitante { get; }
        public string ImagemUrl => UsuarioSolicitante?.ImagemPath;

        public ConviteViewModel(Convite convite, Usuario usuario) {
            ConviteOriginal = convite;
            UsuarioSolicitante = usuario;
        }
    }
}