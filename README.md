# AnaLivros API

AnaLivros é uma API Web em .NET Core para buscar, salvar e analisar livros publicados no Brasil. Integra-se com uma API externa de ISBN e armazena os dados dos livros no MongoDB, incluindo avaliações de usuários.

---

## Funcionalidades

- Buscar informações de um livro pelo ISBN em uma API externa.
- Salvar livros no MongoDB com nota de avaliação.
- Normalização do ISBN (remove traços antes de salvar).
- Calcular a média das avaliações.
- Retornar livros mais bem avaliados.
- Documentação completa com Swagger/OpenAPI.

---

## Tecnologias

- .NET Core 6+
- MongoDB
- HttpClient para chamadas à API externa
- Swashbuckle/Swagger para documentação da API

---

## Começando

### Pré-requisitos

- [.NET 6 SDK](https://dotnet.microsoft.com/download)
- [MongoDB](https://www.mongodb.com/try/download/community) instalado localmente ou remoto

### Configuração

1. Clone o repositório

2. Configure o appsettings.json com a sua conexão do MongoDB:

```bash
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://localhost:27017"
  }
}
```

3. Execute a API e abra o Swagger para testar os Endpoints. Os mais importantes são:

### Banco de Dados

O Banco de dados utilizado foi o MongoDB.

**Informações Extras:**
-   Nome: BookStore
-   Coleção: Books
-   Exemplo de entrada:

```bash
db.Books.insertMany([
  {
    title: "Casas estranhas",
    authors: ["Uketsu", "Jefferson José Teixeira"],
    year: 2025,
    isbn: "9788551013137",
    review: 8.0
  },
  {
    title: "Metamorfose",
    authors: ["Franz Kafka"],
    year: null,
    isbn: "9786580210008",
    review: 9.5
  }
]);

```

### Endpoints

>GET /api/books/{isbn}

- Busca um livro na API Externa pelo ISBN. Teste: 9788545702870

>POST /api/books/{isbn}/save?review={review}

- Busca o livro e o salva no banca com uma avaliação.

>GET /api/books/average-review?top=3

- Retorna a média de todas as notas e os livros mais bem avaliados.
