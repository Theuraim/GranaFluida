using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Security.Policy;

namespace GranaFluida.Models
{
    [Table("META")]
    public class Metas
    {
        [Key]
        [Display(Name = "ID da Movimentacao")]
        public int IDMETA { get; set; }

        [Required(ErrorMessage = "Campo Obrigatório!")]
        [Display(Name = "Descrição da meta")]
        public string DESCRICAOMETA { get; set; }

        //VALORMETA
        [Required(ErrorMessage = "Campo Obrigatório!")]
        [Display(Name = "Valor da Meta")]
        public float VALORMETA{ get; set; }

        //DATACADASTRO
        [Required(ErrorMessage = "Campo Obrigatório!")]
        [Display(Name = "Data da Final da Meta")]
        public DateTime DATACADASTRO { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

        //DATAFINALMETA
        [Required(ErrorMessage = "Campo Obrigatório!")]
        [Display(Name = "Data da Final da Meta")]
        public DateTime DATAFINALMETA { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

        //ATIVA?
        [Display(Name = "Ativa?")]
        public bool ATIVA { get; set; } = true;

        //USUARIO - FK
        [Required]
        [Column("IDUSUARIO")]
        public int? IDUSUARIO { get; set; }

        [ForeignKey("IDUSUARIO")]
        public Usuarios? Usuario { get; set; }
    }
}
