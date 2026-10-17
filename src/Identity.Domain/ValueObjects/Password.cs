using System.Text;
using Cypherly.Domain.Common;
using Identity.Domain.Common;

namespace Identity.Domain.ValueObjects
{
    public class Password : ValueObject
    {
        private const int MinLength = 8;
        private const int MaxBytes = 72;
        private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());

        public string HashedPassword { get; private set; } = string.Empty;

        public Password() { } //For EF Core

        private Password(string hashedPassword)
        {
            HashedPassword = hashedPassword;
        }

        /// <summary>
        /// Validates and creates a password
        /// </summary>
        /// <param name="plainPassword"></param>
        /// <returns>Result containing the password, if param is valid</returns>
        public static Result<Password> Create(string plainPassword)
        {
            try
            {
                IsPasswordValidFormat(plainPassword);
                return Result.Ok(new Password(BCrypt.Net.BCrypt.HashPassword(plainPassword)));
            }
            catch (Exception ex)
            {
                return Result.Fail<Password>(Errors.General.UnspecifiedError(ex.Message));
            }
        }

        /// <summary>
        /// Compares the plain password to the hashed password
        /// </summary>
        /// <param name="plainPassword"></param>
        /// <returns>Boolean value representing the result of the comparison</returns>
        public bool Verify(string plainPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, HashedPassword);
        }

        public static void VerifyAgainstDummy(string plainPassword)
        {
            BCrypt.Net.BCrypt.Verify(plainPassword, DummyHash);
        }

        private static void IsPasswordValidFormat(string pw)
        {
            if (pw.Length < MinLength || Encoding.UTF8.GetByteCount(pw) > MaxBytes)
            {
                throw new ArgumentException(
                    $"Incorrect password: Must be at least {MinLength} characters and at most {MaxBytes} bytes.");
            }
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HashedPassword;
        }
    }
}