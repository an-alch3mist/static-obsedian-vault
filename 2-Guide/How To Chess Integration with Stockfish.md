
A FEN (Forsyth–Edwards Notation) string packs an entire chess position into one line. Let’s break down:

```cs
rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1
```

---

## 1) Piece placement (the part before the first space)

The board is given rank-by-rank from 8 down to 1, with files a→h left to right. Numbers mean “that many empty squares.” Letters are pieces (uppercase = White, lowercase = Black).

```cs
r n b q k b n r   ← rank 8
p p p p p p p p   ← rank 7
. . . . . . . .   ← rank 6
. . . . . . . .   ← rank 5
. . . . . . . .   ← rank 4
. . . . . . . .   ← rank 3
P P P P P P P P   ← rank 2
R N B Q K B N R   ← rank 1
```

Here’s a simple ASCII‐art board:

```cs
8 | r  n  b  q  k  b  n  r
7 | p  p  p  p  p  p  p  p
6 | .  .  .  .  .  .  .  .
5 | .  .  .  .  .  .  .  .
4 | .  .  .  .  .  .  .  .
3 | .  .  .  .  .  .  .  .
2 | P  P  P  P  P  P  P  P
1 | R  N  B  Q  K  B  N  R
    ————————————————
     a  b  c  d  e  f  g  h
```

That’s the **standard initial setup**.

---

## 2) Side to move (`b`)

Immediately after the space you see `b` → it’s Black’s turn. If it were White to move it would be `w`.

---

## 3) Castling availability (`KQkq`)

- `K` = White may castle king-side (with the rook on h1)
    
- `Q` = White may castle queen-side (with the rook on a1)
    
- `k` = Black may castle king-side (with the rook on h8)
    
- `q` = Black may castle queen-side (with the rook on a8)
    

If neither side can castle, this field would be `-`.

---

## 4) En passant target square (`-`)

After a pawn moves two squares, the square “behind” it is a possible en-passant target. Here `-` means **no** en-passant is currently available.

---

## 5) Halfmove clock (`0`)

Counts the number of half-moves (ply) since the last capture or pawn-move, for the fifty-move draw rule. Starting position is `0`.

---

## 6) Fullmove number (`1`)

Counts the full moves. It starts at 1 and increments **after** Black’s move. In the initial position it’s move 1.

---

### Putting it all together In

```cs
rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1
```

| ==Field==                        | ==Meaning on Board==                                       |
| -------------------------------- | ---------------------------------------------------------- |
| small char                       | Black piece                                                |
| Captital char                    | White piece                                                |
| /                                | new row                                                    |
| num                              | number of empty spaces to move forward in same row [1 - 8] |
| k:<br>q:<br>r:<br>n:<br>b:<br>p: | King<br>Queen<br>rook<br>knight<br>bishop<br>pawn          |
| KQRNBP                           | similar meaning with white                                 |
|                                  |                                                            |

| ==**Field**==     | ==**Value**==                                     | ==**Meaning**==                              |
| ----------------- | ------------------------------------------------- | -------------------------------------------- |
| Piece placement   | `rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR`<br> | Standard starting setup<br><br>              |
| Side to move      | `b`                                               | b: Black to move, w: White to move           |
| Castling rights   | `KQkq`                                            | Both sides can castle both ways              |
| En passant square | `-`                                               | No en passant possible                       |
| Halfmove clock    | `0`                                               | Zero half-moves since last pawn move/capture |
| Fullmove number   | `1`                                               | It’s the first full move of the game         |

So that FEN is simply “the standard chess starting position, but **Black** to move on move 1, with all castling rights intact.”

Sure! Let’s unpack those three fields one by one in very simple terms:

---

# Those three fields in a FEN string—

```css
… [piece data] …  <side-to-move>  <castling>  <en-passant>  <halfmove>  <fullmove>
```

—might look cryptic at first (“`- 0 1`”), but they’re actually quite simple. Let’s take them one by one, with concrete examples.

---

## 1) En passant target square (`-`)

### What is “en passant”?

When a pawn moves two squares forward from its starting rank (for White that’s rank 2→4, for Black rank 7→5), it passes through the “adjacent” square. If an enemy pawn sits on a square next to that passed‐through square, it has one turn to capture “as if” the pawn had moved only one square.

### How FEN records it

- If there is an en-passant capture available **on the very next move**, FEN writes the square behind the pawn.
    
- If no pawn double-step just happened, FEN writes `-`.
    

#### Example

1. White plays **1. e2–e4**.
    
2. Black now _could_ capture en passant with a pawn on d4 or f4 (if one existed), because the White pawn “passed through” e3.
    
3. The FEN after White’s 1 e4 would have `e3` in that field.
    
4. If Black makes any other move (or there’s no pawn on d4/f4), that en-passant opportunity disappears—and a subsequent FEN would show `-`.
    

---

## 2) Halfmove clock (`0`)

This counts how many half-moves (ply) have been made **since the last pawn advance or capture**. It’s used to enforce the “50-move rule” draw.

- **Every** time a pawn moves or a piece is captured, this counter resets to **0**.
    
- Otherwise (non-pawn, non-capture move), it increments by **1** each half-move.
    

#### Example sequence

1. **1.e4** → pawn move ⇒ reset to `0`
    
2. **…Nc6** → knight move ⇒ halfmove = `1`
    
3. **2.Nf3** → knight move ⇒ halfmove = `2`
    
4. **…Nf6** → knight move ⇒ halfmove = `3`
    
5. **3.d4** → pawn move ⇒ reset back to `0`
    

---

## 3) Fullmove number (`1`)

This tracks the **full moves** in the game; it starts at **1** and increments **after Black’s move**.

- Before White’s very first move, fullmove = 1.
    
- After Black replies, fullmove becomes 2, and so on.
    

#### Example

- **Initial position** (before any moves): fullmove = **1**
    
- **1.e4 e5** (White’s 1st move and Black’s 1st move) ⇒ fullmove increments to **2**
    
- **2.Nf3 Nc6** ⇒ fullmove increments to **3**
    

---

### Putting it together

Let’s say we start from the very beginning and White plays 1.e4, then Black responds 1…c5. What does the FEN look like?

1. **After White’s 1.e4** (but before Black moves):
    
    ```cs
    rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1
    ```
    
    - Side to move: `b` (Black)
        
    - En passant: `e3` (because White just played e2–e4)
        
    - Halfmove: `0` (pawn move)
        
    - Fullmove: `1` (still in move 1, Black to move)
        
2. **After Black’s 1…c5**:
    
    ```cs
    rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq c6 0 2
    ```
    
    - Side to move: `w` (White)
        
    - En passant: `c6` (because Black just played c7–c5)
        
    - Halfmove: `0` (pawn move)
        
    - Fullmove: `2` (we’ve completed Black’s reply to move 1)
        

---

Now when you see `… - 0 1` in a FEN:

- `-` → no en passant possible right now
    
- `0` → it’s been 0 half-moves since the last pawn-move or capture
    
- `1` → we’re still in fullmove 1 (it’s Black’s turn)