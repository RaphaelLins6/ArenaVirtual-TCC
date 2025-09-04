# 🏀 Arena Virtual

Bem-vindo ao **Arena Virtual**! Este é um projeto de TCC do curso de Ciência da Computação, desenvolvido com o objetivo de criar um **gerenciador de campeonatos amadores de basquete** utilizando **C# .NET MAUI**. 🎓

## 📋 Sobre o Projeto

O **Arena Virtual** é uma aplicação multiplataforma que permite:
- 🏆 **Gerenciamento de Campeonatos:** Criação, edição e exclusão de torneios.
- 📊 **Acompanhamento de Partidas:** Registro de resultados, estatísticas e histórico de jogos.
- ⛹️‍ **Gestão de Equipes:** Cadastro de times e jogadores, edição e exclusão.
- 📈 **Ranking em Tempo Real:** Visualização da classificação dos times.
- 📱💻 **Interface Multiplataforma:** Experiência unificada em Android, iOS, Windows e MacCatalyst.
- 🌐 **API REST:** Permite integração com sistemas externos, sincronização de dados e acesso remoto às informações do campeonato.

A aplicação foi projetada para funcionar em **Android**, **iOS**, **Windows** e **MacCatalyst**, aproveitando o poder do .NET MAUI para criar uma experiência unificada.

## 🚀 Tecnologias Utilizadas

- 👨‍💻 **C# .NET MAUI**: Framework para desenvolvimento multiplataforma.
- 🗄️ **SQLite**: Banco de dados local para armazenamento de informações.
- 🏗️ **MVVM**: Arquitetura para separação de responsabilidades.
- 🎨 **XAML**: Para criação de interfaces gráficas.
- 🌐 **API REST:** ASP.NET Core Web API (para integração e sincronização de dados)
- 🛠️ **Ferramentas:** Visual Studio 2022, .NET 8 SDK, Postman (para testes de API), swagger (para documentação de API)

---

## Como Iniciar o Projeto

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 (com suporte ao .NET MAUI)
- Emulador Android/iOS ou ambiente Windows/MacCatalyst

### Passos

1. **Clonar o repositório:**
`git clone https://github.com/RaphaelLins6/ArenaVirtual-TCC.git`

2. **Acessar a pasta do projeto:**
`cd ArenaVirtual-TCC`

3. **Restaurar dependências:**
- Abra o projeto no Visual Studio 2022
- Execute o comando __Build > Restore NuGet Packages__ ou pressione `Ctrl+Shift+B`

4. **Configurar variáveis de ambiente (se necessário):**
- O projeto utiliza SQLite local, não requer configuração adicional para ambiente de desenvolvimento.
- Para a API, configure o arquivo `appsettings.json` conforme necessário (porta, string de conexão, etc).

5. **Executar o projeto:**
- Selecione a plataforma desejada (Android, iOS, Windows, MacCatalyst) e pressione `F5` para iniciar o app.
- Para iniciar a API, selecione o projeto da Web API e pressione `F5` ou execute:
  `dotnet run --project ArenaVirtual.Api`

---

## 📜 Licença

Este projeto está licenciado sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## Autor

- Raphael Lins – [GitHub](https://github.com/RaphaelLins6)

## 🙏 Agradecimentos

Agradecimentos especiais aos professores orientadores, colegas de curso e à comunidade .NET MAUI pelo suporte e recursos disponibilizados.

---