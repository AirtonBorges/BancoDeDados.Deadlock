namespace BancoDeDados.Deadlock;

public class Transacao
{
    public int Id { get; }
    public int CarimboDeTempo { get; }

    public Transacao(int id, int carimboDeTempo)
    {
        Id = id;
        CarimboDeTempo = carimboDeTempo;
    }
}