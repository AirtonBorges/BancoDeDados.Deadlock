namespace BancoDeDados.Deadlock;

public class Recurso
{
    public string Nome { get; }
    public int Proprietario { get; set; } = -1; // ID da thread que possui o recurso
    private readonly object _bloqueio = new object();

    public Recurso(string nome)
    {
        Nome = nome;
    }

    public bool TentarAdquirir(int id)
    {
        if (Monitor.TryEnter(_bloqueio))
        {
            Proprietario = id;
            return true;
        }
        return false;
    }

    public void Liberar()
    {
        Proprietario = -1;
        if (Monitor.IsEntered(_bloqueio))
        {
            Monitor.Exit(_bloqueio);
        }
    }
}