using COMMON.Entidades;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Validadores
{
    public class amigoValidator : AbstractValidator<amigo>
    {
        public amigoValidator()
        {
            RuleFor(x => x.id_usuario)
                .NotEmpty().WithMessage("El ID del usuario es obligatorio")
                .GreaterThan(0).WithMessage("El ID del usuario debe ser mayor que 0");

            RuleFor(x => x.id_amigo_usuario)
                .NotEmpty().WithMessage("El ID del amigo es obligatorio")
                .GreaterThan(0).WithMessage("El ID del amigo debe ser mayor que 0")
                .NotEqual(x => x.id_usuario).WithMessage("El usuario y el amigo no pueden ser el mismo");

            RuleFor(x => x.estado)
                .NotEmpty().WithMessage("El estado es obligatorio")
                .Must(estado => estado == "pendiente" || estado == "aceptado" || estado == "rechazado")
                .WithMessage("El estado debe ser 'pendiente', 'aceptado' o 'rechazado'");

            RuleFor(x => x.fecha_aceptacion)
                .Must((amigo, fechaAceptacion) => !fechaAceptacion.HasValue || amigo.estado == "aceptado")
                .WithMessage("La fecha de aceptación solo debe establecerse cuando el estado es 'aceptado'");
        }

    }
}
