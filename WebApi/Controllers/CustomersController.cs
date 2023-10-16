using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RocketStoreApi.Managers;
using RocketStoreApi.Models;

namespace RocketStoreApi.Controllers
{
    /// <summary>
    /// Defines the customers controller.
    /// This controller provides actions on customers.
    /// </summary>
    [ControllerName("Customers")]
    [ApiController]
    public partial class CustomersController : ControllerBase
    {
        // Ignore Spelling: api

        #region Public Methods

        /// <summary>
        /// Creates the specified customer.
        /// </summary>
        /// <param name="customer">The customer that should be created.</param>
        /// <returns>
        /// The new customer identifier.
        /// </returns>
        [HttpPost("api/customers")]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Conflict)]
        [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
        public async Task<IActionResult> CreateCustomerAsync(Customer customer)
        {
            Result<Guid> result = await this.HttpContext.RequestServices.GetRequiredService<ICustomersManager>()
                .CreateCustomerAsync(customer).ConfigureAwait(true);

            if (result.FailedWith(ErrorCodes.CustomerAlreadyExists))
            {
                return this.Conflict(
                    new ProblemDetails()
                    {
                        Status = (int)HttpStatusCode.Conflict,
                        Title = result.ErrorCode,
                        Detail = result.ErrorDescription
                    });
            }
            else if (result.Failed)
            {
                return this.BadRequest(
                    new ProblemDetails()
                    {
                        Status = (int)HttpStatusCode.BadRequest,
                        Title = result.ErrorCode,
                        Detail = result.ErrorDescription
                    });
            }

            return this.Created(
                this.GetUri("customers", result.Value),
                result.Value);
        }

        /// <summary>
        /// Gets a list of existing customers.
        /// </summary>
        /// <param name="name">The customer name to be filtered.</param>
        /// <param name="email">The customer email to be filtered.</param>
        /// <returns>
        /// A collection with all the customers, including the identifier, name, and email address for each customer.
        /// </returns>
        [HttpGet("api/customers")]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(List<Entities.Customer>), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> GetCustomerAsync(string name = null, string email = null)
        {
            Result<List<Entities.Customer>> result = await this.HttpContext.RequestServices.GetRequiredService<ICustomersManager>()
                .GetCustomerAsync(name, email).ConfigureAwait(true);

            if (result.Failed)
            {
                return this.BadRequest(
                    new ProblemDetails()
                    {
                        Status = (int)HttpStatusCode.BadRequest,
                        Title = result.ErrorCode,
                        Detail = result.ErrorDescription
                    });
            }

            return this.Accepted(result.Value);
        }

        /// <summary>
        /// Retrieve a single existing customer.
        /// </summary>
        /// <param name="id">The id from the customer to be retrieved.</param>
        /// <returns>
        /// All the information available for the customer.
        /// </returns>
        [HttpGet("api/customers/{id}")]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(Entities.Customer), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> GetCustomerIdAsync(string id)
        {
            Result<Entities.Customer> result = await this.HttpContext.RequestServices.GetRequiredService<ICustomersManager>()
                .GetCustomerIdAsync(id).ConfigureAwait(true);

            if (result.FailedWith(ErrorCodes.CustomerDoesNotExists))
            {
                return this.BadRequest(
                    new ProblemDetails()
                    {
                        Status = (int)HttpStatusCode.InternalServerError,
                        Title = result.ErrorCode,
                        Detail = result.ErrorDescription
                    });
            }

            return this.Accepted(result.Value);
        }

        #endregion

        #region Private Methods

        private Uri GetUri(params object[] parameters)
        {
            string result = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

            foreach (object pathParam in parameters)
            {
                result = $"{result}/{pathParam}";
            }

            return new Uri(result);
        }

        #endregion
    }
}
