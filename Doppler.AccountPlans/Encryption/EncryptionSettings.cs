namespace Doppler.AccountPlans.Encryption
{
    public class EncryptionSettings
    {
        public string InitVectorAsAsciiString { get; set; }
        public string SaltValueAsAsciiString { get; set; }
        public string Password { get; set; }
    }
}
