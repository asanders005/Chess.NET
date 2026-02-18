using Chess.Model.Command;
using Chess.Model.Game;
using Chess.Model.Piece;
using Chess.Model.Rule;
using System;
using System.Linq;
using Xunit;

namespace Chess.Tests
{
    public class ChessModel_CreationTests
    {
        [Fact]
        public void CreateGame_Chess960_DoesNotMatchStandardSetup_MultipleRuns()
        {
            // Run multiple times to reduce the chance of a false positive (randomly generating the standard setup)
            var rulebook = new StandardRulebook();
            const int runs = 100;

            var standardGame = rulebook.CreateGame(false);
            bool[] standardMatches = new bool[runs];

            for (int i = 0; i < runs; i++)
            {
                var chess960Game = rulebook.CreateGame(true);
                // Compare the back rank pieces of the chess960 game to the standard game
                bool matchesStandard = true;
                for (int col = 0; col < 8; col++)
                {
                    var standardPiece = standardGame.Board.GetPieces(Color.White).Single(p => p.Position.Row == 0 && p.Position.Column == col);
                    var chess960Piece = chess960Game.Board.GetPieces(Color.White).Single(p => p.Position.Row == 0 && p.Position.Column == col);
                    if (standardPiece.Piece.GetType() != chess960Piece.Piece.GetType())
                    {
                        matchesStandard = false;
                        break;
                    }
                }
                standardMatches[i] = matchesStandard;
            }

            // Assert that not all runs matched the standard setup (allowing for the possibility of a random match)
            Assert.False(standardMatches.All(m => m), $"All {runs} runs matched the standard setup, which is highly unlikely.");
        }

        [Fact]
        public void CreateGame_Chess960_InvariantsHold_MultipleRuns()
        {
            var rulebook = new StandardRulebook();
            const int runs = 100;

            for (int i = 0; i < runs; i++)
            {
                var game = rulebook.CreateGame(true);

                var white = game.Board.GetPieces(Color.White).ToList();
                var black = game.Board.GetPieces(Color.Black).ToList();

                // piece counts
                Assert.Equal(16, white.Count);
                Assert.Equal(16, black.Count);

                Assert.Equal(1, white.Count(p => p.Piece is King));
                Assert.Equal(1, black.Count(p => p.Piece is King));

                Assert.Equal(1, white.Count(p => p.Piece is Queen));
                Assert.Equal(1, black.Count(p => p.Piece is Queen));

                Assert.Equal(2, white.Count(p => p.Piece is Bishop));
                Assert.Equal(2, black.Count(p => p.Piece is Bishop));

                Assert.Equal(2, white.Count(p => p.Piece is Knight));
                Assert.Equal(2, black.Count(p => p.Piece is Knight));

                Assert.Equal(2, white.Count(p => p.Piece is Rook));
                Assert.Equal(2, black.Count(p => p.Piece is Rook));

                // bishops on opposite colored squares (row+col parity different)
                var whiteBishopCols = white.Where(p => p.Piece is Bishop).Select(p => p.Position.Column).ToList();
                Assert.Equal(2, whiteBishopCols.Count);
                Assert.NotEqual(whiteBishopCols[0] % 2, whiteBishopCols[1] % 2);

                var blackBishopCols = black.Where(p => p.Piece is Bishop).Select(p => p.Position.Column).ToList();
                Assert.Equal(2, blackBishopCols.Count);
                Assert.NotEqual(blackBishopCols[0] % 2, blackBishopCols[1] % 2);

                // king must be between the rooks
                var whiteKingCol = white.Single(p => p.Piece is King).Position.Column;
                var whiteRookCols = white.Where(p => p.Piece is Rook).Select(p => p.Position.Column).OrderBy(c => c).ToArray();
                Assert.True(whiteRookCols[0] < whiteKingCol && whiteKingCol < whiteRookCols[1]);

                var blackKingCol = black.Single(p => p.Piece is King).Position.Column;
                var blackRookCols = black.Where(p => p.Piece is Rook).Select(p => p.Position.Column).OrderBy(c => c).ToArray();
                Assert.True(blackRookCols[0] < blackKingCol && blackKingCol < blackRookCols[1]);

                // pawns on correct rows
                Assert.All(white.Where(p => p.Piece is Pawn), p => Assert.Equal(1, p.Position.Row));
                Assert.All(black.Where(p => p.Piece is Pawn), p => Assert.Equal(6, p.Position.Row));

                // uniqueness of positions and total pieces
                var allPositions = white.Concat(black).Select(p => (p.Position.Row, p.Position.Column)).ToList();
                Assert.Equal(32, allPositions.Distinct().Count());
            }
        }

        [Fact]
        public void CreateGame_Chess960_BackRankTypesMatchBetweenSides()
        {
            // StandardRulebook uses the same base order array for both sides.
            var rulebook = new StandardRulebook();
            var game = rulebook.CreateGame(true);

            // For each column, the piece type on white's back rank (row 0) should match black's back rank (row 7)
            for (int col = 0; col < 8; col++)
            {
                var whitePiece = game.Board.GetPieces(Color.White).Single(p => p.Position.Row == 0 && p.Position.Column == col);
                var blackPiece = game.Board.GetPieces(Color.Black).Single(p => p.Position.Row == 7 && p.Position.Column == col);

                Assert.Equal(whitePiece.Piece.GetType(), blackPiece.Piece.GetType());
            }
        }
    }
}
