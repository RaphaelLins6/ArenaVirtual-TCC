namespace ArenaVirtualAPI.Models {
    // PatrocinioDetalhe não é uma entidade de banco de dados, é um modelo de agregação
    public class PatrocinioDetalhe {
        // CampanhaPatrocinio e PropostaPatrocinio devem ser classes existentes
        public CampanhaPatrocinio Campanha { get; set; }
        public PropostaPatrocinio Proposta { get; set; }

        // Você pode adicionar um construtor se precisar inicializar esses valores
        public PatrocinioDetalhe() { }
    }
}
