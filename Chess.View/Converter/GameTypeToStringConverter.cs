using System;
using System.Globalization;
using System.Windows.Data;
using Chess.Model.Game;
using Chess.Model.Piece;

namespace Chess.View.Converter
{
    public class GameTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GameType gameType)
            {
                // Example display: "White Bishop". Change formatting here if you prefer short codes or Unicode symbols.
                return gameType.ToString();
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}