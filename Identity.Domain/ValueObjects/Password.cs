using System.Text.RegularExpressions;
using Cypherly.Domain.Common;
using Identity.Domain.Common;

namespace Identity.Domain.ValueObjects
{
    public class Password : ValueObject
    {
        public string HashedPassword { get; private set; } = string.Empty;

        public Password() { } //For EF Core

        private Password(string hashedPassword)
        {
            HashedPassword = hashedPassword;
        }

        public static Result<Password> Create(string plainPassword)
        {
            try
            {
                IsPasswordValidFormat(plainPassword);
                return Result.Ok(new Password(BCrypt.Net.BCrypt.HashPassword(plainPassword)));
            }
            catch (Exception ex)
            {
                return Result.Fail<Password>(Error.Failure(ex.Message));
            }
        }

        public bool Verify(string plainPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, HashedPassword);
        }

        private static void IsPasswordValidFormat(string pw)
        {
            if (!Regex.IsMatch(pw, @"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[\W_]).{8,36}$"))
            {
                throw new ArgumentException(
                    "Incorrect password: Must be between 8 and 36 characters, contain one uppercase letter, one lowercase letter, one special character, and no spaces.");
            }
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HashedPassword;
        }
    }
}
