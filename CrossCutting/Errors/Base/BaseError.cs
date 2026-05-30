namespace CrossCutting.Errors.Base
{
    public record BaseError(string Message, string ErrorClass, int HttpErrorCode, Dictionary<string, string>? ValidationErros = null);
}
