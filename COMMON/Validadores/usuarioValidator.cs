using COMMON.Entidades;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Validadores
{
    //Es abtract ya que como no hay campos control donde heredar
    public class usuarioValidator : AbstractValidator<usuario>
    {
        public usuarioValidator()
        {
            RuleFor(x => x.nombre_usuario)
                .NotEmpty().WithMessage("El nombre de usuario es obligatorio")
                .MaximumLength(100).WithMessage("El nombre de usuario no puede exceder los 100 caracteres");

            RuleFor(x => x.biografia)
                .MaximumLength(200).WithMessage("La biografía no puede exceder los 200 caracteres");

            RuleFor(x => x.email)
                .NotEmpty().WithMessage("El email es obligatorio")
                .EmailAddress().WithMessage("El formato del email no es válido")
                .MaximumLength(150).WithMessage("El email no puede exceder los 150 caracteres");

            //RuleFor(x => x.Telefono)
            //    .NotEmpty().WithMessage("El teléfono es obligatorio")
            //    .MaximumLength(20).WithMessage("El teléfono no puede exceder los 20 caracteres");

            //RuleFor(x => x.Pais)
            //    .NotEmpty().WithMessage("El país es obligatorio")
            //    .MaximumLength(50).WithMessage("El país no puede exceder los 50 caracteres");

            //Esto es así porque por default se colocará una a todos los usuarios si el campo es null.
            //RuleFor(x => x.FotoPerfil)
            //    .NotEmpty().WithMessage("La foto de perfil es obligatoria");

            //Como en WhatsApp, siempre existirán estados predefinidos.
            RuleFor(x => x.estado)
                .NotEmpty().WithMessage("El estado es obligatorio")
                .MaximumLength(50).WithMessage("El estado no puede exceder los 50 caracteres");

            RuleFor(x => x.pin_contacto)
                .NotEmpty().WithMessage("El PIN de contacto es obligatorio")
                .Length(5).WithMessage("El PIN de contacto debe tener exactamente 5 caracteres");
        }
    }
}
