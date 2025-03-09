using System.ComponentModel.DataAnnotations;

namespace RaadHetWoordApi.Models
{
    public class Woord
    {
        [Key]
        public int Id { get; set; }  // Unieke ID (primaire sleutel)
        public string Tekst { get; set; }  // Het woord zelf
    }
}
