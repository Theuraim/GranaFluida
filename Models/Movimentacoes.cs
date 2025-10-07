using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Security.Policy;

namespace GranaFluida.Models
{
    [Table("MOVIMENTACAO")]
    public class Movimentacoes
    {
        [Key]
        [Display(Name = "ID da Movimentacao")]
        public int IDMOVIMENTACAO { get; set; }

        [Required(ErrorMessage = "Campo Obrigatório!")]
        [Display(Name = "Descrição da movimentação")]
        public string DESCRICAOMOVIMENTACAO { get; set; }

        //VALORMOVIMENTADO
        [Required(ErrorMessage = "Campo Obrigatório!")]
        [Display(Name = "Valor Movimentado")]
        public float VALORMOVIMENTADO { get; set; }

        //DATAMOVIMENTACAO
        [Required(ErrorMessage = "Campo Obrigatório!")]
        [Display(Name = "Data da Movimentação")]
        public DateTime DATAMOVIMENTACAO { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

        //TIPOMOVIMENTACAO - ENTRADA/SAIDA?
        [Required(ErrorMessage = "Campo Obrigatório!")]
        [Display(Name = "Tipo da Movimentação")]
        public string TIPOMOVIMENTACAO { get; set; }
        //FIXA?
        [Display(Name = "Movimentação fixa?")]
        public bool FIXA { get; set; } = false;

        //USUARIO - FK
        [Required]
        [Column("IDUSUARIO")]
        public int? IDUSUARIO { get; set; }

        [ForeignKey("IDUSUARIO")]
        public Usuarios? Usuario { get; set; }
    }
}
