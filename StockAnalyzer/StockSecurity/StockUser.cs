using System.Text;
using System.Xml.Serialization;

namespace StockAnalyzer.StockSecurity
{
    public class StockUser
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Pseudo { get; set; }
        public string EmailAddress { get; set; }
        [XmlIgnore]
        public string Password { private get { return this.Decrypt(this.EncryptedPassword); } set { this.EncryptedPassword = this.Encrypt(value); } }
        public byte[] EncryptedPassword { get; set; }

        public StockUser()
        {
        }
        public StockUser(string pseudo, string password)
        {
            this.Pseudo = pseudo;
            this.Password = password;
        }
        public StockUser(string pseudo, byte[] encryptedPassword)
        {
            this.Pseudo = pseudo;
            this.EncryptedPassword = encryptedPassword;
        }
        public StockUser(int userId, string firstName, string lastName, string pseudo, string emailAddress, string password)
        {
            this.UserId = userId;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Pseudo = pseudo;
            this.EmailAddress = emailAddress;
            this.Password = password;
        }
        public StockUser(int userId, string firstName, string lastName, string pseudo, string emailAddress, byte[] encryptedPassword)
        {
            this.UserId = userId;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Pseudo = pseudo;
            this.EmailAddress = emailAddress;
            this.EncryptedPassword = encryptedPassword;
        }

        private byte[] Encrypt(string data)
        {
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] uid = enc.GetBytes(this.GetUID());
            byte[] dataBytes = enc.GetBytes(data);

            byte[] encoded = new byte[dataBytes.Length];

            for (int i = 0; i < dataBytes.Length; i++)
            {
                encoded[i] = (byte)(dataBytes[i] ^ uid[i]);
            }
            return encoded;
        }
        private string Decrypt(byte[] data)
        {
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] uid = enc.GetBytes(this.GetUID());

            byte[] encoded = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                encoded[i] = (byte)(data[i] ^ uid[i]);
            }
            return enc.GetString(encoded);
        }
        public string GetUID()
        {
            return StockToolKit.GetHash(this.Pseudo);
        }
    }
}
