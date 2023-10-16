using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RocketStoreApi.Managers
{
    /// <summary>
    /// Defines the interface of the customers manager.
    /// The customers manager allows retrieving, creating, and deleting customers.
    /// </summary>
    public partial interface ICustomersManager
    {
        #region Methods

        /// <summary>
        /// Creates the specified customer.
        /// </summary>
        /// <param name="customer">The customer that should be created.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The <see cref="Task{TResult}" /> that represents the asynchronous operation.
        /// The <see cref="Result{T}" /> that describes the result.
        /// The new customer identifier.
        /// </returns>
        Task<Result<Guid>> CreateCustomerAsync(Models.Customer customer, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a list of existing customers.
        /// </summary>
        /// <param name="name">The customer name to be filtered.</param>
        /// <param name="email">The customer email to be filtered.</param>
        /// <returns>
        /// The <see cref="Task{TResult}" /> that represents the asynchronous operation.
        /// The <see cref="Result{T}" /> that describes the result.
        /// A collection with all the customers, including the identifier, name, and email address for each customer.
        /// </returns>
        Task<Result<List<Entities.Customer>>> GetCustomerAsync(string name = null, string email = null);

        #endregion
    }
}
