using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace fnValidaCPFAPI
{

    /// <summary>
    /// Classe de Azure Function para Valida��o de CPF
    /// </summary>
    public class FnValidaCPFClass
    {
        private readonly ILogger _logger;

        public FnValidaCPFClass(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FnValidaCPFClass>();
        }

        /// <summary>
        /// M�todo Principal da Azure Function de Valida��o de CPF
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Function("fnvalidacpf")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation("Iniciando a valida��o do CPF");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject<dynamic>(requestBody);
             
            if (data == null) return new BadRequestObjectResult("Por favor, informe o CPF");
       
            string cpf = data.cpf;

            if (FnValidaCPFClass.ValidaCPF(cpf) == false) return new BadRequestObjectResult("CPF inv�lido!");
     
            var responseMessage = "CPF v�lido!";

            return new OkObjectResult(responseMessage);
        }

        /// <summary>
        /// M�todo de apoio de valida��o de CPF
        /// </summary>
        /// <param name="cpf">CPF</param>
        /// <returns>Indica��o se o CPF � v�lido ou n�o</returns>
        private static bool ValidaCPF(string cpf)
        {
            // Remove caracteres n�o num�ricos
            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            if (cpf.Length != 11 || cpf.All(c => c == cpf[0]))
            {
                return false;
            }

            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
            {
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            }

            int resto = soma % 11;
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            string digito = resto.ToString();
            tempCpf += digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
            {
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            }

            resto = soma % 11;
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            digito += resto.ToString();

            return cpf.EndsWith(digito);
        }
    }
}
