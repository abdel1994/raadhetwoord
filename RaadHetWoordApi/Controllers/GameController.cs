using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaadHetWoordApi.Data;
using RaadHetWoordApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

[Route("api/game")]
[ApiController]
public class GameController : ControllerBase
{
    private static readonly Dictionary<string, GameState> Games = new();
    private readonly AppDbContext _context;

    public GameController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("random")]
    public IActionResult GetRandomWoord()
    {
        var randomWoord = _context.Woorden
            .OrderBy(x => Guid.NewGuid())
            .Select(w => w.Tekst) // Haal alleen de tekst op
            .FirstOrDefault();

        if (randomWoord == null) return NotFound("No words found");

        return Ok(new { woord = randomWoord });
    }

    [HttpPost("new")]
    public IActionResult StartGame()
    {
        var randomWoord = _context.Woorden
            .OrderBy(x => Guid.NewGuid())
            .Select(w => w.Tekst)
            .FirstOrDefault();

        if (randomWoord == null) return NotFound("No words found");

        var gameId = Guid.NewGuid().ToString();
        var maskedWord = new string('_', randomWoord.Length);

        Games[gameId] = new GameState
        {
            Word = randomWoord,
            MaskedWord = maskedWord,
            AttemptsLeft = 7,
            GameOver = false
        };

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

        game.GameOver = game.MaskedWord == game.Word || game.AttemptsLeft <= 0;

        return Ok(new { game.MaskedWord, game.AttemptsLeft, game.GameOver });
    }

    [HttpGet("{gameId}/isGameOver")]
    public IActionResult IsGameOver(string gameId)
    {
        if (!Games.TryGetValue(gameId, out var game)) return NotFound("Game not found");

        bool isWon = game.MaskedWord == game.Word;
        bool isLost = game.AttemptsLeft <= 0;
        game.GameOver = isWon || isLost;
        

        var response = new
        {
            gameId = gameId,
            isGameOver = game.GameOver,
            isWon = isWon,
            isLost = isLost,
            message = isWon ? "You won! 🎉" : (isLost ? "Game over! ❌" + "Het Woord was" + "  " + game.Word  : "Keep guessing!")
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
