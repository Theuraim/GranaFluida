using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace GranaFluida.Models
{
    [Table("USUARIO")]
    public class Usuarios
    {
        [Key]
        [Display(Name = "ID do Usuario")]
        public int IDUSUARIO { get; set; }

        [Required(ErrorMessage = "Campo Obrigatório!")]
        [StringLength(16, ErrorMessage = "Tamanho Máximo de 16 caracteres!")]
        [Display(Name = "Nome de usuário")]
        public string NOMEUSUARIO { get; set; }


        [Required(ErrorMessage = "Campo Obrigatório!")]
        [StringLength(200, ErrorMessage = "Tamanho Máximo de 200 caracteres!")]
        [Display(Name = "Nome Completo")]
        public string NOMECOMPLETO { get; set; }

        [Required(ErrorMessage = "Campo Obrigatório!")]
        [StringLength(16, ErrorMessage = "Tamanho Máximo de 16 caracteres!")]
        [Display(Name = "Senha")]
        public string SENHA { get; set; }

        [Display(Name = "E-mail")]
        public string? EMAIL { get; set; }

        [Display(Name = "Endereço")]
        public string? ENDERECO { get; set; }

        [Display(Name = "Cidade")]
        public string? CIDADE { get; set; }

        [Display(Name = "Estado")]
        public string? ESTADO { get; set; }

        public DateTime? DATACADASTRO { get; set; }

        public DateTime? DATAATUALIZACAO { get; set; }
    }
}
