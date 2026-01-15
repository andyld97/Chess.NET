# Chess.NET

**Chess.NET** is a modern chess application game in **C# / .NET 10**, built completely from scratch.  
It includes full game rules, a WPF-based GUI, custom bot logic, and even the ability to **play against Stockfish** via UCI.

> ⚠️ **ATTENTION**  This game isn't finished yet and it's still very rudimental!

![Chess.NET Screenshot](Assets/screenshot.png)

---

## Features

###  Game Logic
- (Almost) full chess rule validation
- Check and checkmate detection
- **Castling (king-side & queen-side)** fully implemented
- Illegal moves are reliably prevented

###  Bots
- **Custom built-in bot**  
  - Simple, material-focused, very greedy  
  - This bot is so stupid, it's called Stupido!
- **Stockfish integration (UCI)**
  - Play directly against Stockfish
  - Difficulty adjustable via search depth / engine options (TODO currently just static in code)

### 🖥️ GUI (WPF)
- Dynamic 8×8 chess board
- Drag & Drop piece movement
- Clean layered rendering (board + pieces)
- Helpful debug output for development

### Requirements
- `stockfish.exe` placed anywhere if you want to play against the engine!
