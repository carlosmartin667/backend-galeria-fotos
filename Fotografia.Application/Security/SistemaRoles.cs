namespace Fotografia.Application.Security;

public static class SistemaRoles
{
    public const string Admin = "Admin";
    public const string Usuario = "Usuario";
    public const string Invitado = "Invitado";
    public const string AdminUsuario = Admin + "," + Usuario;
}
