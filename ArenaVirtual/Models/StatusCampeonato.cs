namespace ArenaVirtual.Models {
    /// <summary>
    /// Define os possíveis status de um Campeonato no sistema.
    /// Os valores inteiros são importantes para uso em consultas SQLite.
    /// </summary>
    public enum StatusCampeonato {
        Pendente = 0,   // Campeonato criado, mas ainda não começou
        Ativo = 1,      // Campeonato em andamento (este é o filtro que você precisa)
        Finalizado = 2, // Campeonato concluído
        Cancelado = 3   // Campeonato cancelado
    }
}
