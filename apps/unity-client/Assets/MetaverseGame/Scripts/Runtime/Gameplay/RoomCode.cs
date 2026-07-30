using System;
using System.Text.RegularExpressions;

namespace MetaverseGame.Gameplay
{
    public static class RoomCode
    {
        private static readonly Regex Pattern =
            new("^[A-Z0-9]{4,8}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Room code is required.", nameof(value));
            }

            string normalized = value.Trim().ToUpperInvariant();
            if (!Pattern.IsMatch(normalized))
            {
                throw new ArgumentException(
                    "Room codes contain four to eight ASCII letters or digits.",
                    nameof(value));
            }
            return normalized;
        }
    }
}
