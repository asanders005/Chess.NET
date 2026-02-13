using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.View.Window
{
    using Chess.Model.Game;
    using Chess.View.Selector;
    using Chess.ViewModel.Game;
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public partial class LaunchWindow : Window
    {
        GameType selectedGame = GameType.Chess;

        public LaunchWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Shows the window and returns the selected game type.
        /// </summary>
        /// <param name="gameTypes">The possible game types to choose from</param>
        /// <returns>The game selected by the user</returns>
        public GameType Show(IEnumerable<GameType> gameTypes)
        {
            this.GameControl.ItemsSource = gameTypes;
            this.ShowDialog();
            return selectedGame;
        }

        /// <summary>
        /// Closes the window after the user has selected a game type.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Additional information about the event</param>
        private void StartClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is GameType gameType)
            {
                selectedGame = gameType;
                this.Close();
            }
        }
    }
}
