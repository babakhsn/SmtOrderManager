namespace SmtOrderManager.Application.Abstractions;

public interface IJsonSerializer
{
    string Serialize<T>(T value);
}
