using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BancoDeDados.Deadlock
{
    public static class Programa
    {
        private static readonly object BloqueioGlobal = new();
        private static readonly Dictionary<int, List<int>> GrafoEspera = new();
        private static readonly Dictionary<int, Transacao> Transacoes = new();
        private static int ContadorCarimboDeTempo = 0;

        public static void Main()
        {
            var recursoX = new Recurso("X");
            var recursoY = new Recurso("Y");

            var tarefas = new List<Task>();

            for (int i = 0; i <= 5; i++)
            {
                int id = i;
                tarefas.Add(Task.Run(() => { FuncaoTransacao(id, recursoX, recursoY); }));
            }

            Task.WaitAll(tarefas.ToArray());
        }

        public static void FuncaoTransacao(int id, Recurso primeiroRecurso, Recurso segundoRecurso)
        {
            int carimboDeTempo;
            lock (BloqueioGlobal)
            {
                carimboDeTempo = ++ContadorCarimboDeTempo;
                Transacoes[id] = new Transacao(id, carimboDeTempo);
                Console.WriteLine($"Thread T({id}) entra em execução com carimbo de tempo {carimboDeTempo}.");
            }

            Thread.Sleep(new Random().Next(10, 20)); // Aumento no tempo de espera antes de tentar o primeiro recurso

            // Tentar adquirir o primeiro recurso com lógica Wait-Die
            while (!primeiroRecurso.TentarAdquirir(id))
            {
                int idProprietario = primeiroRecurso.Proprietario;

                lock (BloqueioGlobal)
                {
                    GrafoEspera[id] = new List<int> { idProprietario };
                }

                // Se a transação atual tem um carimbo de tempo menor que o proprietário, espera
                if (Transacoes[id].CarimboDeTempo < Transacoes[idProprietario].CarimboDeTempo)
                {
                    Console.WriteLine($"Thread T({id}) está esperando pelo recurso {primeiroRecurso.Nome} (Wait-Die).");
                    Thread.Sleep(new Random().Next(200, 400)); // Aumento no tempo de espera antes de nova tentativa
                }
                else
                {
                    Console.WriteLine($"Thread T({id}) é finalizada devido a deadlock detectado (Wait-Die).");
                    GrafoEspera.Remove(id);
                    Transacoes.Remove(id);
                    FuncaoTransacao(id, primeiroRecurso, segundoRecurso);
                    return;
                }
            }

            Console.WriteLine($"Thread T({id}) obteve o bloqueio do recurso {primeiroRecurso.Nome}.");

            Thread.Sleep(new Random().Next(1000)); // Aumento no tempo de espera antes de tentar o segundo recurso

            // Lógica para adquirir o segundo recurso
            while (!segundoRecurso.TentarAdquirir(id))
            {
                int idProprietario = segundoRecurso.Proprietario;

                lock (BloqueioGlobal)
                {
                    GrafoEspera[id] = new List<int> { idProprietario };
                }

                // Implementação do algoritmo Wait-Die
                if (Transacoes[id].CarimboDeTempo < Transacoes[idProprietario].CarimboDeTempo)
                {
                    Console.WriteLine($"Thread T({id}) está esperando pelo recurso {segundoRecurso.Nome} (Wait-Die).");
                    Thread.Sleep(new Random().Next(200, 400)); // Aumento no tempo de espera antes de nova tentativa
                }
                else
                {
                    Console.WriteLine($"Thread T({id}) é finalizada devido a deadlock detectado (Wait-Die).");
                    primeiroRecurso.Liberar();
                    GrafoEspera.Remove(id);
                    Transacoes.Remove(id);
                    FuncaoTransacao(id, primeiroRecurso, segundoRecurso);
                    return;
                }
            }

            segundoRecurso.Proprietario = id;
            Console.WriteLine($"Thread T({id}) obteve o bloqueio do recurso {segundoRecurso.Nome}.");

            Thread.Sleep(500); // Aumento do tempo de operação crítica

            primeiroRecurso.Liberar();
            Console.WriteLine($"Thread T({id}) liberou o recurso {primeiroRecurso.Nome}.");
            segundoRecurso.Liberar();
            Console.WriteLine($"Thread T({id}) liberou o recurso {segundoRecurso.Nome}.");

            lock (BloqueioGlobal)
            {
                GrafoEspera.Remove(id);
                Transacoes.Remove(id);
            }

            Console.WriteLine($"Thread T({id}) finalizou sua execução.");
        }
    }
}