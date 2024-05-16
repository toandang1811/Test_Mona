using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Data
{
    public class Key
    {
        [Key]
        public string TableName { get; set; }
        public int LastKey { get; set; }
        public string KeyName { get; set; }
    }
}
