namespace AuthService.Application.Exceptions
{
    public sealed class WeakPasswordException : ApplicationException
    {
        public WeakPasswordException()
            : base("Mot de passe trop faible.")
        {
        }
    }
}
