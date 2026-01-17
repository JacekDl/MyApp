namespace MyApp.Domain.Common
{
    public static class FormatStringHelper
    {
        public static (string Code, string Name) FormatCodeAndText(string? code, string? text)
        {
            var formatedCode = (code ?? string.Empty).Trim().ToUpper();

            var formatedText = (text ?? string.Empty).Trim();
            formatedText = formatedText.Length switch
            {
                0 => string.Empty,
                1 => formatedText.ToUpper(),
                _ => char.ToUpper(formatedText[0]) + formatedText[1..].ToLower()
            };

            return (formatedCode, formatedText);
        }
    }
}
