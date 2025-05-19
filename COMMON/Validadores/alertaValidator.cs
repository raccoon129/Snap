using COMMON.Entidades;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Validadores
{
    public class alertaValidator : AbstractValidator<alerta>
    {
        public alertaValidator()
        {
            RuleFor(x => x.id_usuario_origen)
                .NotEmpty().WithMessage("El ID del usuario origen es obligatorio")
                .GreaterThan(0).WithMessage("El ID del usuario origen debe ser mayor que 0");

            RuleFor(x => x.id_usuario_destino)
                .NotEmpty().WithMessage("El ID del usuario destino es obligatorio")
                .GreaterThan(0).WithMessage("El ID del usuario destino debe ser mayor que 0")
                .NotEqual(x => x.id_usuario_origen).WithMessage("El usuario origen y destino no pueden ser el mismo");

            //Para que sea algo breve
            RuleFor(x => x.comentario_alerta)
                .MaximumLength(500).WithMessage("El comentario de la alerta no puede exceder los 500 caracteres");
        }
    }
}
