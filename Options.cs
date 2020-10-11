
namespace CryptUtils
{
    interface IOptions
    {
        string DIR { get; set; }

        string OUTPUTDIR { get; set; }

        string Password { get; set; }
    }

    class EncryptOptions : IOptions
    {
        public string DIR { get; set; }

        public string OUTPUTDIR { get; set; }

        public string Password { get; set; }
    }

   
    class DecryptOptions : IOptions
    {
        public string DIR { get; set; }

        public string OUTPUTDIR { get; set; }

        public string Password { get; set; }
    }
}
