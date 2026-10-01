using System;

namespace Gravivore.Platform.Monetization
{
    public readonly struct ProductId : IEquatable<ProductId>
    {
        public ProductId(string value)
        {
            Value = Validate(value, nameof(value));
        }

        public string Value { get; }
        public bool IsValid => !string.IsNullOrEmpty(Value);
        public bool Equals(ProductId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is ProductId other && Equals(other);
        public override int GetHashCode() => Value != null ? StringComparer.Ordinal.GetHashCode(Value) : 0;
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(ProductId left, ProductId right) => left.Equals(right);
        public static bool operator !=(ProductId left, ProductId right) => !left.Equals(right);

        private static string Validate(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value) || !string.Equals(value, value.Trim(), StringComparison.Ordinal))
            {
                throw new ArgumentException("A non-empty stable id without surrounding whitespace is required.", parameterName);
            }

            return value;
        }
    }

    public readonly struct EntitlementId : IEquatable<EntitlementId>
    {
        public EntitlementId(string value)
        {
            Value = Validate(value, nameof(value));
        }

        public string Value { get; }
        public bool IsValid => !string.IsNullOrEmpty(Value);
        public bool Equals(EntitlementId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is EntitlementId other && Equals(other);
        public override int GetHashCode() => Value != null ? StringComparer.Ordinal.GetHashCode(Value) : 0;
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(EntitlementId left, EntitlementId right) => left.Equals(right);
        public static bool operator !=(EntitlementId left, EntitlementId right) => !left.Equals(right);

        private static string Validate(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value) || !string.Equals(value, value.Trim(), StringComparison.Ordinal))
            {
                throw new ArgumentException("A non-empty stable id without surrounding whitespace is required.", parameterName);
            }

            return value;
        }
    }
}
