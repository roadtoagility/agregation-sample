using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Aggregation
{
    public class ClientesAggregationTests
    {
        public ClientesAggregationTests()
        {

        }

        [Fact]
        public void DeveCriarClienteValido()
        {
            var clientAgg = ClienteAggregation.From("Douglas Ramalho", "25766896314");
            var clienteGerado = clientAgg.GetData();

            Assert.Equal("Douglas Ramalho", clienteGerado.Nome);
        }

        [Fact]
        public void NaoDeveCriarClienteComNomeIncompleto()
        {
            var clientAgg = ClienteAggregation.From("Douglas", "25766896314");
            Assert.True(clientAgg.Erros.Any());
        }

        [Fact]
        public void DeveAdicionarCartaoCliente()
        {
            var clientAgg = ClienteAggregation.From("Douglas Ramalho", "25766896314");
            var cmd = new AddCartaoCreditoCommand("1111111111111111", DateTime.Now.AddYears(10));
            clientAgg.Execute(cmd);

            Assert.Equal("1111111111111111", clientAgg.GetData().CartaoCredito.Numero);
        }

        [Fact]
        public void DeveCriarAgregacaoValida()
        {
            var clientAgg = ClienteAggregation.From("Douglas Ramalho", "25766896314");
            Assert.NotNull(clientAgg.GetData());
        }
    }

    public class Result<T>
    {
        public Exception[] Exceptions { get; private set; }
        public T Value { get; private set; }
        public bool HasExceptions { get { return Exceptions.Any(); } }

        public Result(T data, params Exception[] exceptions)
        {
            Value = data;
            Exceptions = exceptions;
        }
    }

    public interface ICommand
    {

    }


    public class ClienteAggregation
    {
        private Cliente _data;
        public List<Exception> Erros { get; private set; } = new List<Exception>();

        private ClienteAggregation(string nome, string cpf)
        {
            var result = Execute(new AddClienteCommand(nome, cpf));

            if (result.HasExceptions)
            {
                Erros.Add(new Exception("Erro na criação da agregação"));
            }
        }

        public static ClienteAggregation From(string nome, string cpf)
        {
            var agg = new ClienteAggregation(nome, cpf);

            return agg;
        }

        public Result<Cliente> Execute(ICommand cmd)
        {
            return ((dynamic)this).Apply((dynamic)cmd);
        }

        private Result<Cliente> Apply(AddClienteCommand cmd)
        {
            if (cmd.Nome.Split(" ").Length > 1) // deve ser informado o nome completo
            {
                return CriarCliente(cmd);
            }
            else
            {
                var erro = new Exception("Deve ser informado o nome completo do cliente.");
                return new Result<Cliente>(null, erro);
            }
        }

        private Result<Cliente> Apply(AddCartaoCreditoCommand cmd)
        {
            if (cmd.Validade >= DateTime.Now)
            {
                return AdicionarCartaoCredito(cmd);
            }
            else
            {
                var erro = new Exception("O cartão de crédito precisa estar dentro de sua data de validade");
                return new Result<Cliente>(null, erro);
            }
        }

        private Result<Cliente> CriarCliente(AddClienteCommand cmd)
        {
            _data = Cliente.From(cmd.Nome, cmd.Cpf);
            return new Result<Cliente>(_data);
        }

        private Result<Cliente> AdicionarCartaoCredito(AddCartaoCreditoCommand cmd)
        {
            _data.AdicionarCartao(new CartaoCredito(cmd.Numero, cmd.Validade));
            return new Result<Cliente>(_data);
        }

        public Cliente GetData()
        {
            return _data;
        }
    }

    public class AddClienteCommand : ICommand
    {
        public string Nome { get; private set; }
        public string Cpf { get; private set; }

        public AddClienteCommand(string nome, string cpf)
        {
            Nome = nome;
            Cpf = cpf;
        }
    }

    public class AddCartaoCreditoCommand : ICommand
    {
        public string Numero { get; private set; }
        public DateTime Validade { get; private set; }

        public AddCartaoCreditoCommand(string numero, DateTime validade)
        {
            Numero = numero;
            Validade = validade;
        }
    }


    public class Cliente
    {
        public string Nome { get; private set; }
        public string Cpf { get; private set; }

        public CartaoCredito CartaoCredito { get; private set; }

        private Cliente(string nome, string cpf)
        {
            Nome = nome;
            Cpf = cpf;
        }

        public static Cliente From(string nome, string cpf)
        {
            if (string.IsNullOrEmpty(nome))
                throw new Exception("O nome deve ser informado");

            if (string.IsNullOrEmpty(cpf))
                throw new Exception("O cpf deve ser informado");

            return new Cliente(nome, cpf);
        }

        public void AdicionarCartao(CartaoCredito cartao)
        {
            CartaoCredito = cartao;
        }
    }

    public class CartaoCredito
    {
        public string Numero { get; private set; }
        public DateTime Validade { get; private set; }

        public CartaoCredito(string numero, DateTime validade)
        {
            Numero = numero;
            Validade = validade;
        }
    }
}
