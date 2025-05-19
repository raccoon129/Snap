using COMMON.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Interfaces
{
    public interface IDB<T>
    {
        /// <summary>
        /// Interfaz que define el comportamiento de una clase para la comunicacion coon una base de datos 
        /// </summary>

        string Error { get; }

        List<T> ObtenerTodas();

        T ObtenerPorID(int id);
        
        T ObtenerPorID(string id);
        
        bool Eliminar(T entidad);
        
        T Insertar(T entidad);
        
        T Actualizar(T entidad);

        List<M> EjecutarProcedimiento<M>
            (string nombre, Dictionary<string, string> parametros)
            where M : class;

    }
}
