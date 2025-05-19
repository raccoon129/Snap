using COMMON.Entidades;
using COMMON.Interfaces;
using COMMON.Validadores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class FabricRepository
    {
        private string _cadenaDeConexion;
        private TipoDB _tipoDB;

        public FabricRepository(string cadenaDeConexion, TipoDB tipoDB)
        {
            _cadenaDeConexion = cadenaDeConexion;
            _tipoDB = tipoDB;
        }

        public IDB<alerta> AlertaRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.MySQL:
                    return new MySQL<alerta>(_cadenaDeConexion, new alertaValidator(), "id_alerta", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<amigo> AmigoRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.MySQL:
                    return new MySQL<amigo>(_cadenaDeConexion, new amigoValidator(), "id_amigo", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<comentario> ComentarioRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.MySQL:
                    return new MySQL<comentario>(_cadenaDeConexion, new comentarioValidator(), "id_comentario", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<favorito> FavoritoRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.MySQL:
                    return new MySQL<favorito>(_cadenaDeConexion, new favoritoValidator(), "id_favorito", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<foto> FotoRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.MySQL:
                    return new MySQL<foto>(_cadenaDeConexion, new fotoValidator(), "id_foto", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<publicacion> PublicacionRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.MySQL:
                    return new MySQL<publicacion>(_cadenaDeConexion, new publicacionValidator(), "id_publicacion", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }

        public IDB<usuario> UsuarioRepository()
        {
            switch (_tipoDB)
            {
                case TipoDB.MySQL:
                    return new MySQL<usuario>(_cadenaDeConexion, new usuarioValidator(), "id_usuario", true);
                default:
                    throw new NotImplementedException("Tipo de base de datos no soportado");
            }
        }
    }

    public enum TipoDB
    {
        MySQL
    }
}
