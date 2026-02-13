//-----------------------------------------------------------------------
// <copyright file="StandardRulebook.cs">
//     Copyright (c) Michael Szvetits. All rights reserved.
// </copyright>
// <author>Michael Szvetits</author>
//-----------------------------------------------------------------------
namespace Chess.Model.Rule
{
    using Chess.Model.Command;
    using Chess.Model.Data;
    using Chess.Model.Game;
    using Chess.Model.Piece;
    using Chess.Model.Visitor;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents the standard chess rulebook.
    /// </summary>
    public class StandardRulebook : IRulebook
    {
        /// <summary>
        /// Represents the check rule of a standard chess game.
        /// </summary>
        private readonly CheckRule checkRule;

        /// <summary>
        /// Represents the end rule of a standard chess game.
        /// </summary>
        private readonly EndRule endRule;

        /// <summary>
        /// Represents the movement rule of a standard chess game.
        /// </summary>
        private readonly MovementRule movementRule;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardRulebook"/> class.
        /// </summary>
        public StandardRulebook()
        {
            var threatAnalyzer = new ThreatAnalyzer();
            var castlingRule = new CastlingRule(threatAnalyzer);
            var enPassantRule = new EnPassantRule();
            var promotionRule = new PromotionRule();

            this.checkRule = new CheckRule(threatAnalyzer);
            this.movementRule = new MovementRule(castlingRule, enPassantRule, promotionRule, threatAnalyzer);
            this.endRule = new EndRule(this.checkRule, this.movementRule);
        }

        /// <summary>
        /// Creates a new chess game according to the standard rulebook.
        /// </summary>
        /// <returns>The newly created chess game.</returns>
        public ChessGame CreateGame(bool isChess960 = false)
        {
            IEnumerable<PlacedPiece> makeBaseLine960(int row, Color color, int[] baseOrder)
            {
                if (baseOrder == null)
                {
                    foreach (var piece in makeBaseLine(row, color))
                    {
                        yield return piece;
                    }
                    yield break;
                }

                for (int col = 0; col < 8; col++)
                {
                    int pieceEnum = baseOrder[col];

                    ChessPiece piece = pieceEnum switch
                    {
                        (int)PieceEnum.Rook => new Rook(color),
                        (int)PieceEnum.Knight => new Knight(color),
                        (int)PieceEnum.Bishop => new Bishop(color),
                        (int)PieceEnum.Queen => new Queen(color),
                        (int)PieceEnum.King => new King(color),
                        _ => throw new InvalidOperationException("Invalid piece enum value.")
                    };
                    yield return new PlacedPiece(new Position(row, col), piece);
                }
            }

            IEnumerable<PlacedPiece> makeBaseLine(int row, Color color)
            {
                yield return new PlacedPiece(new Position(row, 0), new Rook(color));
                yield return new PlacedPiece(new Position(row, 1), new Knight(color));
                yield return new PlacedPiece(new Position(row, 2), new Bishop(color));
                yield return new PlacedPiece(new Position(row, 3), new Queen(color));
                yield return new PlacedPiece(new Position(row, 4), new King(color));
                yield return new PlacedPiece(new Position(row, 5), new Bishop(color));
                yield return new PlacedPiece(new Position(row, 6), new Knight(color));
                yield return new PlacedPiece(new Position(row, 7), new Rook(color));
            }

            int[] makeBaseOrder960(int whiteRow, int blackRow)
            {
                int[] baseOrder = new int[8];

                Random random = new Random((int)DateTime.Now.Ticks);

                int bishopColumn1 = random.Next(0, 4) * 2;
                int bishopColumn2 = random.Next(0, 4) * 2 + 1;

                int rooksPlaced = 0;
                bool kingPlaced = false;
                int knightsPlaced = 0;
                bool queenPlaced = false;
                for (int col = 0; col < 8; col++)
                {
                    if (col == bishopColumn1 || col == bishopColumn2)
                    {
                        baseOrder[col] = (int)PieceEnum.Bishop;
                        continue;
                    }

                    int selectedPiece = -1;
                    bool canPlacePiece = false;
                    while (!canPlacePiece)
                    {
                        selectedPiece = random.Next(1, 6);
                        if (selectedPiece != (int)PieceEnum.Bishop)
                        {
                            if (selectedPiece == (int)PieceEnum.Knight && knightsPlaced < 2)
                            {
                                knightsPlaced++;
                                canPlacePiece = true;
                            }
                            else if (selectedPiece == (int)PieceEnum.Queen && !queenPlaced)
                            {
                                queenPlaced = true;
                                canPlacePiece = true;
                            }
                            else if (selectedPiece == (int)PieceEnum.Rook && 
                                ((rooksPlaced == 0 && !kingPlaced) || (rooksPlaced == 1 && kingPlaced)))
                            {
                                rooksPlaced++;
                                canPlacePiece = true;
                            }
                            else if (selectedPiece == (int)PieceEnum.King && !kingPlaced && rooksPlaced == 1)
                            {
                                kingPlaced = true;
                                canPlacePiece = true;
                            }
                        }
                    }

                    baseOrder[col] = selectedPiece;
                }
                return baseOrder;
            }

            IEnumerable<PlacedPiece> makePawns(int row, Color color) =>
                Enumerable.Range(0, 8).Select(
                    i => new PlacedPiece(new Position(row, i), new Pawn(color))
                );

            IImmutableDictionary<Position, ChessPiece> makePieces(int pawnRow, int baseRow, Color color, int[] baseOrder = null)
            {
                var pawns = makePawns(pawnRow, color);
                var baseLine = makeBaseLine960(baseRow, color, baseOrder);
                var pieces = baseLine.Union(pawns);
                var empty = ImmutableSortedDictionary.Create<Position, ChessPiece>(PositionComparer.DefaultComparer);
                return pieces.Aggregate(empty, (s, p) => s.Add(p.Position, p.Piece));
            }

            var baseOrder960 = isChess960 ? makeBaseOrder960(0, 7) : null;

            var whitePlayer = new Player(Color.White);
            var whitePieces = makePieces(1, 0, Color.White, baseOrder960);
            var blackPlayer = new Player(Color.Black);
            var blackPieces = makePieces(6, 7, Color.Black, baseOrder960);
            var board = new Board(whitePieces.AddRange(blackPieces));

            return new ChessGame(board, whitePlayer, blackPlayer);
        }

        /// <summary>
        /// Gets the status of a chess game, according to the standard rulebook.
        /// </summary>
        /// <param name="game">The game state to be analyzed.</param>
        /// <returns>The current status of the game.</returns>
        public Status GetStatus(ChessGame game)
        {
            return this.endRule.GetStatus(game);
        }

        /// <summary>
        /// Gets all possible updates (i.e., future game states) for a chess piece on a specified position,
        /// according to the standard rulebook.
        /// </summary>
        /// <param name="game">The current game state.</param>
        /// <param name="position">The position to be analyzed.</param>
        /// <returns>A sequence of all possible updates for a chess piece on the specified position.</returns>
        public IEnumerable<Update> GetUpdates(ChessGame game, Position position)
        {
            var piece = game.Board.GetPiece(position, game.ActivePlayer.Color);
            var updates = piece.Map(
                p =>
                {
                    var moves = this.movementRule.GetCommands(game, p);
                    var turnEnds = moves.Select(c => new SequenceCommand(c, EndTurnCommand.Instance));
                    var records = turnEnds.Select
                    (
                        c => new SequenceCommand(c, new SetLastUpdateCommand(new Update(game, c)))
                    );
                    var futures = records.Select(c => c.Execute(game).Map(g => new Update(g, c)));
                    return futures.FilterMaybes().Where
                    (
                        e => !this.checkRule.Check(e.Game, e.Game.PassivePlayer)
                    );
                }
            );

            return updates.GetOrElse(Enumerable.Empty<Update>());
        }
    }
}