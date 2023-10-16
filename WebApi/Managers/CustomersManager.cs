using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RocketStoreApi.Storage;

namespace RocketStoreApi.Managers
{
    /// <summary>
    /// Defines the default implementation of <see cref="ICustomersManager"/>.
    /// </summary>
    /// <seealso cref="ICustomersManager" />
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Created via dependency injection.")]
    internal partial class CustomersManager : ICustomersManager
    {
        #region Private Properties

        private ApplicationDbContext Context
        {
            get;
        }

        private IMapper Mapper
        {
            get;
        }

        private ILogger Logger
        {
            get;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomersManager" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="logger">The logger.</param>
        public CustomersManager(ApplicationDbContext context, IMapper mapper, ILogger<CustomersManager> logger)
        {
            this.Context = context;
            this.Mapper = mapper;
            this.Logger = logger;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task<Result<Guid>> CreateCustomerAsync(Models.Customer customer, CancellationToken cancellationToken = default)
        {
            customer = customer ?? throw new ArgumentNullException(nameof(customer));

            Entities.Customer entity = this.Mapper.Map<Models.Customer, Entities.Customer>(customer);

            if (this.Context.Customers.Any(i => i.EmailAddress == entity.EmailAddress))
            {
                this.Logger.LogWarning($"A customer with email '{entity.EmailAddress}' already exists.");

                return Result<Guid>.Failure(
                    ErrorCodes.CustomerAlreadyExists,
                    $"A customer with email '{entity.EmailAddress}' already exists.");
            }

            this.Context.Customers.Add(entity);

            await this.Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            this.Logger.LogInformation($"Customer '{customer.Name}' created successfully.");

            return Result<Guid>.Success(
                new Guid(entity.Id));
        }

        /// <inheritdoc />
        public async Task<Result<List<Entities.Customer>>> GetCustomerAsync(string name = null, string email = null)
        {
            IQueryable<Entities.Customer> query = this.Context.Customers.AsQueryable();

            // Filter by name or email if provided
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(email))
            {
                query = query.Where(c => c.Name.Contains(name) || c.EmailAddress.Contains(email));
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    query = query.Where(c => c.Name.Contains(name));
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    query = query.Where(c => c.EmailAddress.Contains(email));
                }
            }

            List<Entities.Customer> customerslist = await query.ToListAsync().ConfigureAwait(true);
            return Result<List<Entities.Customer>>.Success(customerslist);
        }

        /// <inheritdoc />
        public async Task<Result<Entities.Customer>> GetCustomerIdAsync(string id)
        {
            Entities.Customer resultcustomer = await this.Context.Customers.FirstOrDefaultAsync(c => c.Id == id).ConfigureAwait(true);

            if (resultcustomer != null) 
            {
                if (!string.IsNullOrWhiteSpace(resultcustomer.Address))
                {
                    string baseUrl = "http://api.positionstack.com/v1/forward";
                    string accessKey = "15d47496f789474651c34c3f64ee13dd";
                    string query = Uri.EscapeDataString(resultcustomer.Address);
                    Uri fullUrl = new Uri($"{baseUrl}?access_key={accessKey}&query={query}");

                    using (HttpClient httpClient = new HttpClient())
                    {
                        try
                        {
                            HttpResponseMessage response = await httpClient.GetAsync(fullUrl).ConfigureAwait(false);
                            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                            if (response.IsSuccessStatusCode)
                            {
                                var objectjson = JsonConvert.DeserializeObject<JToken>(content);

                                // Could not finish the code because I couldn't make the Positionstack API to provide data (even in their website I couldn't test the API from their own URI)
                                // At this point I should get the latitude and longitude information from the external API based on resultcustomer.Address
                                // and with those values I should update resultcustomer.Latitude and resultcustomer.Longitude
                            }
                        }
                        catch (HttpRequestException e)
                        {
                        }
                    }
                }

                return Result<Entities.Customer>.Success(resultcustomer);
            }
            else
            {
                return Result<Entities.Customer>.Failure(
                       ErrorCodes.CustomerDoesNotExists,
                       $"There is no customer with Id '{id}' in the database.");
            }
        }

        /// <inheritdoc />
        public async Task<Result<string>> DeleteCustomerAsync(string id)
        {
            Entities.Customer customerToDelete = await this.Context.Customers.FirstOrDefaultAsync(c => c.Id == id).ConfigureAwait(true);

            if (customerToDelete != null)
            {
                this.Context.Customers.Remove(customerToDelete);
                this.Context.SaveChanges();

                this.Logger.LogInformation($"Customer '{customerToDelete.Name}' deleted successfully.");

                return Result<string>.Success($"Customer '{customerToDelete.Name}' deleted successfully.");
            }
            else
            {
                return Result<string>.Failure(
                       ErrorCodes.CustomerDoesNotExists,
                       $"There is no customer with Id '{id}' in the database.");
            }
        }
        #endregion
    }
}
