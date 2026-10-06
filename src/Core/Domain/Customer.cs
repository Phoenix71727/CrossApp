using Core.Dto;

namespace Core.Domain;

public sealed class Customer
{
    public string Id { get; }
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string? Phone { get; private set; }

    private Customer(string id, string fullName, string email, string? phone)
    {
        Id = id;
        FullName = fullName;
        Email = email;
        Phone = phone;
    }

    public static Customer Create(string id, string fullName, string email, string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор клієнта не може бути порожнім", nameof(id));

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Ім'я клієнта не може бути порожнім", nameof(fullName));

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("Email клієнта має бути непорожнім і містити '@'", nameof(email));

        return new Customer(id.Trim(), fullName.Trim(), email.Trim(), phone?.Trim());
    }

    public CustomerDto ToDto() => new(Id, FullName, Email, Phone);

    public static Customer FromDto(CustomerDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return Create(dto.Id, dto.FullName, dto.Email, dto.Phone);
    }

    public override string ToString() => $"{FullName} (ID: {Id}) <{Email}>";
}
