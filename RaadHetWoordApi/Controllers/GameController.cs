using Microsoft.AspNetCore.Mvc;

[Route("api/game")]
[ApiController]
public class GameController : ControllerBase
{
    private static readonly List<string> Words = new List<string> { "apple", "banana", "cherry" };  // dit straks vanuit postgesql halen
    private static readonly Dictionary<string, GameState> Games = new();

    [HttpPost("new")]
    public IActionResult StartGame()
    {
        var gameId = Guid.NewGuid().ToString();
        var word = Words[new Random().Next(Words.Count)];
        var maskedWord = new string('_', word.Length); // "______" voor "apple" bv.

        Games[gameId] = new GameState { Word = word, MaskedWord = maskedWord, AttemptsLeft = 7, GameOver = false };

        return Ok(new { gameId, maskedWord, attemptsLeft = 7 });
    }

    [HttpPost("{gameId}/guess")]
    public IActionResult MakeGuess(string gameId, [FromBody] char letter)
    {
        if (!Games.TryGetValue(gameId, out var game)) return NotFound("Game not found");

        if (game.GameOver) return BadRequest("Game is already over!");

        if (game.Word.Contains(letter))
        {
            var maskedWordArray = game.MaskedWord.ToCharArray();
            for (int i = 0; i < game.Word.Length; i++)
                if (game.Word[i] == letter) maskedWordArray[i] = letter;
            game.MaskedWord = new string(maskedWordArray);
        }
        else
        {
            game.AttemptsLeft--;
        }

        // Controleer of de game voorbij is na een gok, als een van de twee voorwaarden true is dan wordt game.Gameover = true;
        game.GameOver = game.MaskedWord == game.Word || game.AttemptsLeft <= 0;

        return Ok(new { game.MaskedWord, game.AttemptsLeft, game.GameOver });
    }

    [HttpGet("{gameId}/isGameOver")]
    public IActionResult IsGameOver(string gameId)
    {
        if (!Games.TryGetValue(gameId, out var game)) return NotFound("Game not found");

        bool isWon = game.MaskedWord == game.Word;
        bool isLost = game.AttemptsLeft <= 0;
        game.GameOver = isWon || isLost; // Zet de status als de game voorbij is

        var response = new
        {
            gameId = gameId,
            isGameOver = game.GameOver,
            isWon = isWon,
            isLost = isLost,
            message = isWon ? "You won! 🎉" : (isLost ? "Game over! ❌" : "Keep guessing!")
        };

        return Ok(response);
    }

    private class GameState
    {
        public string Word { get; set; }
        public string MaskedWord { get; set; }
        public int AttemptsLeft { get; set; }
        public bool GameOver { get; set; }
    }
}
