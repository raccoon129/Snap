using COMMON.Entidades;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Validadores
{
    public class comentarioValidator : AbstractValidator<comentario>
    {
        public comentarioValidator()
        {
            RuleFor(x => x.id_foto)
                .NotEmpty().WithMessage("El ID de la foto es obligatorio")
                .GreaterThan(0).WithMessage("El ID de la foto debe ser mayor que 0");

            RuleFor(x => x.id_usuario)
                .NotEmpty().WithMessage("El ID del usuario es obligatorio")
                .GreaterThan(0).WithMessage("El ID del usuario debe ser mayor que 0");

            RuleFor(x => x.contenido)
                .NotEmpty().WithMessage("El contenido del comentario es obligatorio")
                .MaximumLength(1000).WithMessage("El contenido del comentario no puede exceder los 1000 caracteres");
        }
    }
}
