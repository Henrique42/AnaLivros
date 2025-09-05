using AnaLivros.Services;
using AnaLivros.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;  // Importar Swagger annotations
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BookApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BookService _bookService;

        public BooksController(BookService bookService)
        {
            _bookService = bookService;
        }

        static bool ValidaISBN(string isbn)
        {
            var regex = new Regex(@"^[0-9]{10}([0-9]{3})?$|^[0-9-]{13,17}$");
            if (!regex.IsMatch(isbn))
                return false;
            return true;
        }

        /// <summary>
        /// Retorna um livro publicado no Brasil com base no ISBN.
        /// </summary>
        /// <remarks>
        /// Utilize este endpoint para retornar informações importantes de um livro através do seu ISBN.
        /// Aceita tanto o ISBN no formato de 10 dígitos (obsoleto) quanto no de 13 dígitos (atual). Pode conter ou não traços de formatação.
        /// </remarks>
        /// <param name="isbn">O ISBN do livro que você quer encontrar.</param>
        /// <returns>O registro único de um livro</returns>
        /// <response code="200">Retorna os detalhes do livro</response>
        /// <response code="404">Livro não encontrado</response>
        [HttpGet("{isbn}")]
        [SwaggerOperation(Summary = "Buscar livro pelo ISBN", Description = "Operação que retorna um livro publicado no Brasil a partir do seu códico ISBN.")]
        [SwaggerResponse(200, "Retorna os detalhes do livro", typeof(Book))]
        [SwaggerResponse(404, "Livro não encontrado")]
        public async Task<IActionResult> GetBookByIsbn(string isbn)
        {
            // Valida se o código ISBN é válido
            if (!ValidaISBN(isbn))
            {
                return BadRequest(new { message = "ISBN inválido. Deve ser ISBN-10 ou ISBN-13, com ou sem traços." });
            }

            var book = await _bookService.GetBookByIsbnAsync(isbn);

            if (book == null)
            {
                return NotFound(new { message = "Livro não encontrado." });
            }

            return Ok(book);  // Retorna o livro como JSON.
        }

        /// <summary>
        /// Busca um livro pelo ISBN e salva no banco de dados com uma avaliação (representada por uma nota em decimal).
        /// </summary>
        /// <param name="isbn">O ISBN do livro.</param>
        /// <param name="review">Avaliação do livro (decimal).</param>
        /// <returns>O livro salvo com a avaliação incluída.</returns>
        [HttpPost("{isbn}/save")]
        [SwaggerOperation(Summary = "Buscar e salvar livro com avaliação",
                          Description = "Busca um livro pelo ISBN e o salva no banco de dados junto com uma avaliação.")]
        [SwaggerResponse(200, "Livro salvo com sucesso", typeof(Book))]
        [SwaggerResponse(404, "Livro não encontrado")]
        public async Task<IActionResult> ReviewBook(string isbn, double review)
        {
            // Valida se o código ISBN é válido
            if (!ValidaISBN(isbn))
            {
                return BadRequest(new { message = "ISBN inválido. Deve ser ISBN-10 ou ISBN-13, com ou sem traços." });
            }

            var book = await _bookService.GetBookByIsbnAsync(isbn);

            if (book == null)
                return NotFound(new { message = "Livro não encontrado." });

            // Adiciona o campo Review
            book.review = review;
            book.isbn = book.isbn.Replace("-", "");

            var verify_book = await _bookService.GetByIsbnFromDbAsync(book.isbn);

            if (verify_book is null)
            {
                await _bookService.CreateAsync(book);
            }
            else
            {
                book.Id = verify_book.Id;
                await _bookService.UpdateAsync(book.Id, book);
            }

            return Ok(book);
        }

        /////////////////////////
        // Métodos padrão de CRUD
        /////////////////////////
        [HttpGet]
        public async Task<List<Book>> Get() =>
            await _bookService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Book>> Get(string id)
        {
            var book = await _bookService.GetAsync(id);

            if (book is null)
            {
                return NotFound();
            }

            return book;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Book newBook)
        {
            await _bookService.CreateAsync(newBook);

            return CreatedAtAction(nameof(Get), new { id = newBook.Id }, newBook);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Book updatedBook)
        {
            var book = await _bookService.GetAsync(id);

            if (book is null)
            {
                return NotFound();
            }

            updatedBook.Id = book.Id;

            await _bookService.UpdateAsync(id, updatedBook);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var book = await _bookService.GetAsync(id);

            if (book is null)
            {
                return NotFound();
            }

            await _bookService.RemoveAsync(id);

            return NoContent();
        }

        /// <summary>
        /// Retorna a média das avaliações de todos os livros salvos no banco e a lista dos livros com as maiores avaliações.
        /// </summary>
        /// <param name="top">Quantidade de livros mais bem avaliados a serem retornados (padrão: 3).</param>
        /// <returns>A média das avaliações e a lista dos livros mais bem avaliados.</returns>
        [HttpGet("average-review")]
        [SwaggerOperation(
            Summary = "Obter média de avaliações e livros mais bem avaliados",
            Description = "Calcula a média das avaliações de todos os livros salvos e retorna também os livros mais bem avaliados.")]
        [SwaggerResponse(200, "Média e lista de livros mais bem avaliados retornadas com sucesso")]
        [SwaggerResponse(404, "Nenhum livro encontrado no banco de dados")]
        public async Task<IActionResult> GetAverageReviewAndTopBooks([FromQuery] int top = 3)
        {
            var books = await _bookService.GetAsync();

            if (books == null || books.Count == 0)
                return Ok(new { averageReview = 0.0, topBooks = new List<Book>() });

            var avg = books.Average(b => b.review);
            var topBooks = books.OrderByDescending(b => b.review).Take(top).ToList();

            return Ok(new
            {
                averageReview = avg,
                topBooks = topBooks
            });
        }


    }
}
