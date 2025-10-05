namespace ProductCatalog.Domain.Exceptions
{
    /// <summary>
    /// Represents errors that occur within the product domain.
    /// </summary>
    public class ProductDomainException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductDomainException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ProductDomainException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductDomainException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ProductDomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when a product name is invalid.
    /// </summary>
    public class InvalidProductNameException : ProductDomainException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidProductNameException"/> class.
        /// </summary>
        /// <param name="name">The invalid product name.</param>
        public InvalidProductNameException(string name)
            : base($"Product name '{name}' is invalid. Name cannot be empty and must be less than 200 characters.")
        {
        }
    }

    /// <summary>
    /// Exception thrown when a product price is invalid.
    /// </summary>
    public class InvalidPriceException : ProductDomainException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPriceException"/> class.
        /// </summary>
        /// <param name="amount">The invalid price amount.</param>
        public InvalidPriceException(decimal amount)
            : base($"Price '{amount}' is invalid. Price cannot be negative.")
        {
        }
    }

    /// <summary>
    /// Exception thrown when a product stock value is invalid.
    /// </summary>
    public class InvalidStockException : ProductDomainException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStockException"/> class.
        /// </summary>
        /// <param name="stock">The invalid stock value.</param>
        public InvalidStockException(int stock)
            : base($"Stock '{stock}' is invalid. Stock cannot be negative.")
        {
        }
    }
}
