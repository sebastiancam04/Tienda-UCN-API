using System.ComponentModel.DataAnnotations;

namespace Tienda_UCN_api.src.Application.Validators
{
    public class RutValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// Valida que el RUT chileno sea correcto.
        /// </summary>
        /// <param name="value">El valor del RUT a validar.</param>
        /// <returns>Un ValidationResult que indica si la validación fue exitosa o no </returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null)
            {
                string rut = (string)value;

                rut = rut.Replace(".", "").Replace(" ", "");

                int rutNumber = int.Parse(rut.Split('-')[0]);
                char digitoVerificador = rut.Split('-')[1].ToLowerInvariant()[0];

                int[] coefficients = { 2, 3, 4, 5, 6, 7 };
                int sum = 0;
                int index = 0;

                while (rutNumber != 0)
                {
                    sum += rutNumber % 10 * coefficients[index];
                    rutNumber /= 10;
                    index = (index + 1) % 6;
                }

                int result = 11 - (sum % 11);
                char verificador;
                if (result == 10)
                {
                    verificador = 'k';
                }
                else
                {
                    verificador = result.ToString()[0];
                }
                if (verificador == digitoVerificador)
                {
                    return ValidationResult.Success;
                }
            }
            return new ValidationResult("El RUT ingresado no es válido.");
        }
    }
}