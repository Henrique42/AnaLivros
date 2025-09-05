using AnaLivros.Models;
using Newtonsoft.Json;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace AnaLivros.Services
{
    public class BookService
    {
        private const string api_isbn = "https://brasilapi.com.br/api/isbn/v1";
        private readonly HttpClient _httpClient;
        private readonly IMongoCollection<Book> _booksCollection;

        // Construtor que injeta tanto HttpClient quanto a coleção do Mongo
        public BookService(HttpClient httpClient, IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
        {
            _httpClient = httpClient;
            var mongoClient = new MongoClient(bookStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(bookStoreDatabaseSettings.Value.DatabaseName);

            _booksCollection = mongoDatabase.GetCollection<Book>(bookStoreDatabaseSettings.Value.BooksCollectionName);
        }

        // Método que retorna um livro pelo ISBN (busca externa)
        public async Task<Book> GetBookByIsbnAsync(string isbn)
        {
            var url = $"{api_isbn}/{isbn}";
            var response = await _httpClient.GetStringAsync(url);
            var book = JsonConvert.DeserializeObject<Book>(response);
            return book;
        }

        // Métodos de CRUD padrão
        public async Task<List<Book>> GetAsync() =>
        await _booksCollection.Find(_ => true).ToListAsync();

        public async Task<Book?> GetAsync(string id) =>
            await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Book newBook) =>
            await _booksCollection.InsertOneAsync(newBook);

        public async Task UpdateAsync(string id, Book updatedBook) =>
            await _booksCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

        public async Task RemoveAsync(string id) =>
            await _booksCollection.DeleteOneAsync(x => x.Id == id);

        // (Opcional) Método para buscar direto do Mongo
        public async Task<Book> GetByIsbnFromDbAsync(string isbn)
        {
            return await _booksCollection.Find(b => b.isbn == isbn).FirstOrDefaultAsync();
        }
    }
}
