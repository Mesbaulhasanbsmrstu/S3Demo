using System.ComponentModel.DataAnnotations;

namespace S3Demo.Models
{
    public class S3FileDetails
    {
        [Key]
        public int Id { get; set; }
        public DateTime FileDate { get; set; }
        public string FileName { get; set; }
    }
}
