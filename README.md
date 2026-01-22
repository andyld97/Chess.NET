# Chess.NET

**Chess.NET** is a modern chess application game in **C# / .NET 10**, built completely from scratch.  
It includes (almost) full game rules, a WPF-based GUI, custom bot logic, and even the ability to **play against Stockfish** via UCI.

> ⚠️ **ATTENTION**  This game isn't finished yet and it's still very rudimental!

![Chess.NET Screenshot](Assets/screenshot.png)

## Features

### Missing Features
- Time Control

###  Game Logic
- (Almost) full chess rule validation
- EN PASSANT!
- Choose promotion!
- Check, checkmate and stalemate detection
- **Castling (king-side & queen-side)** fully implemented
- Illegal moves are reliably prevented

###  Bots
- **Custom built-in bot**  
  - Simple, material-focused, very greedy  
  - This bot is so stupid, it's called Stupido!
- **Stockfish integration (UCI)**
  - Play directly against Stockfish
  - Difficulty adjustable via search depth / engine options (settings)

### GUI (WPF)
- Drag & Drop chess board
- Localized (DE/EN)
- Settings
- Puzzles

### Requirements
- `stockfish.exe` if you want to play against the engine!
