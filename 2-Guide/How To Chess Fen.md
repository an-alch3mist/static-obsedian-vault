```cs
	public static string B_to_FEN(List<List<char>> B, char side = 'b')
	{
		// 1) Generate only the piece‑placement
		var placement = C_E.B_to_FEN_arrangement(B);
		// 2) Statically append the four “dummy” fields:
		return $"{placement} {side} - - 0 1";
	}
```