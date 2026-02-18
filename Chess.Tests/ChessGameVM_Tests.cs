using Chess.ViewModel.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Chess.Tests
{
    public class ChessGameVM_Tests
    {
        [Fact]
        public void ChangeTypeCommand_TogglesType()
        {
            var game = new ChessGameVM(null, Model.Game.GameType.Chess);
            var initialType = game.SelectedGameType;
            var initialBoardLayout = game.Board.Pieces;

            game.ChangeGameTypeCommand.Execute(null);
            var newType = game.SelectedGameType;
            var newBoardLayout = game.Board.Pieces;

            Assert.NotEqual(initialType, newType);
            Assert.NotEqual(initialBoardLayout, newBoardLayout);
        }

        [Fact]
        public void NewCommand_GeneratesNewGame_WithDifferentBoard960()
        {
            var game = new ChessGameVM(null, Model.Game.GameType.Chess960);
            var initialBoardLayout = game.Board.Pieces;
            game.NewCommand.Execute(null);
            var newBoardLayout = game.Board.Pieces;
            Assert.NotEqual(initialBoardLayout, newBoardLayout);
        }
    }
}
