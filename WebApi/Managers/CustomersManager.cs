using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using RocketStoreApi.Entities;
using RocketStoreApi.Models;
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

            if (this.Context.Customers.Any(i => i.Email == entity.Email))
            {
                this.Logger.LogWarning($"A customer with email '{entity.Email}' already exists.");

                return Result<Guid>.Failure(
                    ErrorCodes.CustomerAlreadyExists,
                    $"A customer with email '{entity.Email}' already exists.");
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
                query = query.Where(c => c.Name.Contains(name) || c.Email.Contains(email));
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    query = query.Where(c => c.Name.Contains(name));
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    query = query.Where(c => c.Email.Contains(email));
                }
            }

            List<Entities.Customer> customerslist = await query.ToListAsync().ConfigureAwait(true);
            return Result<List<Entities.Customer>>.Success(customerslist);
        }

        /// <inheritdoc />
        public async Task<Result<Entities.Customer>> GetCustomerIdAsync(string id)
        {
            IQueryable<Entities.Customer> query = this.Context.Customers.AsQueryable();

            // Filter by name or email if provided
            if (!string.IsNullOrWhiteSpace(id))
            {
                query = query.Where(c => c.Id.Equals(id));
            }

            Entities.Customer resultcustomer = await query.FirstOrDefaultAsync().ConfigureAwait(true);

            if (resultcustomer != null) 
            {
                return Result<Entities.Customer>.Success(resultcustomer);
            }
            else
            {
                return Result<Entities.Customer>.Failure(
                       ErrorCodes.CustomerDoesNotExists,
                       $"There is no customer with Id '{id}' in the database.");
            }
        }

        #endregion
    }
}
