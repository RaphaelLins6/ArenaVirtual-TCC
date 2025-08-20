

namespace ArenaVirtual.Services;

public class ImagemService
{
    public async Task<string> SalvarImagemAsync(string caminhoOriginal)
    {
        string nomeArquivo = $"{Guid.NewGuid()}.jpg";
        string destino = Path.Combine(FileSystem.AppDataDirectory, "Imagens");

        if (!Directory.Exists(destino))
            Directory.CreateDirectory(destino);

        string caminhoCompleto = Path.Combine(destino, nomeArquivo);

        File.Copy(caminhoOriginal, caminhoCompleto, overwrite: true);

        return caminhoCompleto;
    }
}
