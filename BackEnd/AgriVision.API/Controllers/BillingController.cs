using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AgriVision.Application.DTOs.Subscriptions;
using AgriVision.Domain.Entities;
using AgriVision.Domain.Enums;
using AgriVision.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace AgriVision.API.Controllers
{
    [ApiController]
    [Route("api/v1/billing")]
    public class BillingController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AgriVisionDbContext _context;

        public BillingController(IConfiguration configuration, AgriVisionDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }

        private string GetFrontendBaseUrl()
        {
            var baseUrl = _configuration["Frontend:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new Exception("Frontend:BaseUrl is not configured.");
            return baseUrl.TrimEnd('/');
        }

        private string GetStripeSecretKey()
        {
            var key = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(key))
                throw new Exception("Stripe:SecretKey is not configured.");
            return key;
        }

        private string GetStripeWebhookSecret()
        {
            var secret = _configuration["Stripe:WebhookSecret"];
            if (string.IsNullOrWhiteSpace(secret))
                throw new Exception("Stripe:WebhookSecret is not configured.");
            return secret;
        }

        private string GetPriceId(SubscriptionPlan plan)
        {
            return plan switch
            {
                SubscriptionPlan.Basic => _configuration["Stripe:Prices:Basic"] ?? string.Empty,
                SubscriptionPlan.Advanced => _configuration["Stripe:Prices:Advanced"] ?? string.Empty,
                _ => string.Empty
            };
        }

        private static SubscriptionPlan? GetPlanFromPriceId(string? priceId, IConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(priceId)) return null;
            if (priceId == config["Stripe:Prices:Basic"]) return SubscriptionPlan.Basic;
            if (priceId == config["Stripe:Prices:Advanced"]) return SubscriptionPlan.Advanced;
            return null;
        }

        private async Task<string?> ResolveStripeCustomerIdAsync(string userId)
        {
            var existingCustomerId = await _context.Subscriptions
                .AsNoTracking()
                .Where(s => s.UserId == userId && s.StripeCustomerId != null)
                .OrderByDescending(s => s.StartDate)
                .Select(s => s.StripeCustomerId)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(existingCustomerId))
                return existingCustomerId;

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return null;

            StripeConfiguration.ApiKey = GetStripeSecretKey();
            var customerService = new CustomerService();
            var customers = await customerService.ListAsync(new CustomerListOptions
            {
                Email = user.Email,
                Limit = 1
            });

            return customers.Data.FirstOrDefault()?.Id;
        }

        private async Task ApplySubscriptionAsync(string userId, Stripe.Subscription stripeSub, string? priceId)
        {
            var plan = GetPlanFromPriceId(priceId, _configuration);
            if (plan == null) return;

            var existing = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSub.Id);

            if (existing == null)
            {
                existing = new AgriVision.Domain.Entities.Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = userId
                };
                _context.Subscriptions.Add(existing);
            }

            existing.Plan = plan.Value;
            existing.Price = stripeSub.Items.Data.FirstOrDefault()?.Price?.UnitAmountDecimal / 100m ?? 0m;
            existing.StartDate = stripeSub.StartDate.ToUniversalTime();
            var periodEnd = existing.StartDate.AddDays(30);
            existing.EndDate = periodEnd;
            existing.IsActive = stripeSub.Status == "active";
            existing.Status = stripeSub.Status;
            existing.StripeCustomerId = stripeSub.CustomerId;
            existing.StripeSubscriptionId = stripeSub.Id;
            existing.StripePriceId = priceId;
            existing.CurrentPeriodEnd = periodEnd;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                user.SubscriptionPlan = plan.Value;
            }

            await _context.SaveChangesAsync();
        }

        [Authorize]
        [HttpPost("checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequestDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId not found in token.");

            if (!Enum.TryParse<SubscriptionPlan>(dto.Plan, true, out var plan))
                return BadRequest("Invalid plan.");

            if (plan == SubscriptionPlan.Free)
                return BadRequest("Free plan does not require checkout.");

            var priceId = GetPriceId(plan);
            if (string.IsNullOrWhiteSpace(priceId))
                return BadRequest("Stripe price is not configured.");

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Unauthorized("User not found.");

            StripeConfiguration.ApiKey = GetStripeSecretKey();

            var customerService = new CustomerService();
            var existingCustomerId = await _context.Subscriptions
                .AsNoTracking()
                .Where(s => s.UserId == userId && s.StripeCustomerId != null)
                .OrderByDescending(s => s.StartDate)
                .Select(s => s.StripeCustomerId)
                .FirstOrDefaultAsync();

            var customerId = existingCustomerId;
            if (string.IsNullOrWhiteSpace(customerId))
            {
                var customer = await customerService.CreateAsync(new CustomerCreateOptions
                {
                    Email = user.Email,
                    Name = user.FullName,
                    Metadata = new Dictionary<string, string>
                    {
                        { "userId", userId }
                    }
                });
                customerId = customer.Id;
            }

            var frontendBase = GetFrontendBaseUrl();
            var options = new SessionCreateOptions
            {
                Mode = "subscription",
                Customer = customerId,
                SuccessUrl = $"{frontendBase}/Dashboard/user/SubscriptionSuccess.html?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{frontendBase}/Dashboard/user/SubscriptionCheckout.html?canceled=true",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1
                    }
                },
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId },
                    { "plan", plan.ToString() }
                },
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "userId", userId },
                        { "plan", plan.ToString() }
                    }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return Ok(new CreateCheckoutSessionResponseDto { Url = session.Url });
        }

        [Authorize]
        [HttpPost("checkout-session/{sessionId}/complete")]
        public async Task<IActionResult> CompleteCheckoutSession(string sessionId)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId not found in token.");

            StripeConfiguration.ApiKey = GetStripeSecretKey();
            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(sessionId);
            if (session == null || string.IsNullOrWhiteSpace(session.SubscriptionId))
                return BadRequest("Invalid session.");

            if (session.Metadata == null || !session.Metadata.TryGetValue("userId", out var sessionUserId))
                return BadRequest("Session user not found.");

            if (!string.Equals(sessionUserId, userId, StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var subService = new SubscriptionService();
            var stripeSub = await subService.GetAsync(session.SubscriptionId);
            var priceId = stripeSub.Items.Data.FirstOrDefault()?.Price?.Id;
            await ApplySubscriptionAsync(userId, stripeSub, priceId);

            return Ok();
        }

        [Authorize]
        [HttpGet("payments")]
        public async Task<IActionResult> GetPayments()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId not found in token.");

            var payments = await _context.Payments
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(20)
                .ToListAsync();

            if (payments.Any())
                return Ok(payments);

            var customerId = await ResolveStripeCustomerIdAsync(userId);

            if (string.IsNullOrWhiteSpace(customerId))
                return Ok(payments);

            try
            {
                StripeConfiguration.ApiKey = GetStripeSecretKey();
                var invoiceService = new InvoiceService();
                var invoices = await invoiceService.ListAsync(new InvoiceListOptions
                {
                    Customer = customerId,
                    Limit = 20
                });

                var mapped = invoices.Data.Select(i =>
                {
                    DateTime? createdAt = null;
                    if (i.Created != default)
                    {
                        var created = i.Created;
                        createdAt = created.Kind == DateTimeKind.Utc ? created : created.ToUniversalTime();
                    }

                    return new
                    {
                        CreatedAt = createdAt,
                        Amount = (i.AmountPaid > 0 ? i.AmountPaid : i.AmountDue) / 100m,
                        Currency = i.Currency?.ToUpperInvariant() ?? "USD",
                        Status = i.Status ?? "Unknown",
                        StripeInvoiceId = i.Id,
                        HostedInvoiceUrl = i.HostedInvoiceUrl,
                        InvoicePdf = i.InvoicePdf
                    };
                });

                return Ok(mapped);
            }
            catch (StripeException ex)
            {
                return BadRequest($"Stripe error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("cancel-subscription")]
        public async Task<IActionResult> CancelSubscription()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId not found in token.");

            var subscription = await _context.Subscriptions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            StripeConfiguration.ApiKey = GetStripeSecretKey();

            string? stripeSubscriptionId = subscription?.StripeSubscriptionId;
            var customerId = await ResolveStripeCustomerIdAsync(userId);

            async Task<string?> FindLatestStripeSubscriptionIdAsync()
            {
                if (string.IsNullOrWhiteSpace(customerId))
                    return null;

                var subService = new SubscriptionService();
                var subs = await subService.ListAsync(new SubscriptionListOptions
                {
                    Customer = customerId,
                    Limit = 5,
                    Status = "all"
                });
                return subs.Data
                    .OrderByDescending(s => s.Created)
                    .FirstOrDefault()?.Id;
            }

            if (string.IsNullOrWhiteSpace(stripeSubscriptionId))
            {
                stripeSubscriptionId = await FindLatestStripeSubscriptionIdAsync();
            }

            if (string.IsNullOrWhiteSpace(stripeSubscriptionId))
                return NotFound("Active subscription not found.");

            var service = new SubscriptionService();
            try
            {
                await service.CancelAsync(stripeSubscriptionId);
            }
            catch (StripeException ex) when (ex.StripeError?.Code == "resource_missing"
                || ex.Message.Contains("No such subscription", StringComparison.OrdinalIgnoreCase))
            {
                var fallbackId = await FindLatestStripeSubscriptionIdAsync();
                if (!string.IsNullOrWhiteSpace(fallbackId) && fallbackId != stripeSubscriptionId)
                {
                    await service.CancelAsync(fallbackId);
                    stripeSubscriptionId = fallbackId;
                }
                else
                {
                    // Stripe does not know the subscription; mark local subscription as canceled.
                }
            }

            if (subscription == null)
            {
                subscription = new AgriVision.Domain.Entities.Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    StripeSubscriptionId = stripeSubscriptionId,
                    StartDate = DateTime.UtcNow
                };
                _context.Subscriptions.Add(subscription);
            }

            subscription.Plan = SubscriptionPlan.Free;
            subscription.Price = 0m;
            subscription.IsActive = false;
            subscription.Status = "canceled";
            subscription.EndDate = null;
            subscription.CurrentPeriodEnd = null;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                user.SubscriptionPlan = SubscriptionPlan.Free;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Subscription canceled." });
        }

        [AllowAnonymous]
        [HttpPost("stripe/webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    GetStripeWebhookSecret());
            }
            catch (Exception ex)
            {
                return BadRequest($"Webhook Error: {ex.Message}");
            }

            StripeConfiguration.ApiKey = GetStripeSecretKey();

            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                {
                    var session = stripeEvent.Data.Object as Session;
                    if (session == null) break;

                    var userId = session.Metadata != null && session.Metadata.TryGetValue("userId", out var id)
                        ? id
                        : null;
                    if (string.IsNullOrWhiteSpace(userId)) break;

                    var subscriptionId = session.SubscriptionId;
                    if (string.IsNullOrWhiteSpace(subscriptionId)) break;

                    var subService = new SubscriptionService();
                    var subscription = await subService.GetAsync(subscriptionId);
                    var priceId = subscription.Items.Data.FirstOrDefault()?.Price?.Id;
                    await ApplySubscriptionAsync(userId, subscription, priceId);
                    break;
                }
                case "invoice.paid":
                {
                    var invoice = stripeEvent.Data.Object as Invoice;
                    if (invoice == null) break;

                    var subscriptionId = invoice.Lines?.Data?.FirstOrDefault()?.SubscriptionId;
                    var subscription = await _context.Subscriptions
                        .FirstOrDefaultAsync(s => s.StripeSubscriptionId == subscriptionId);
                    if (subscription == null && !string.IsNullOrWhiteSpace(invoice.CustomerId))
                    {
                        subscription = await _context.Subscriptions
                            .FirstOrDefaultAsync(s => s.StripeCustomerId == invoice.CustomerId);
                    }

                    if (subscription != null)
                    {
                        subscription.IsActive = true;
                        subscription.Status = "active";
                        subscription.EndDate = invoice.PeriodEnd.ToUniversalTime();
                        subscription.CurrentPeriodEnd = subscription.EndDate;
                    }

                    if (subscription != null && !string.IsNullOrWhiteSpace(subscription.UserId))
                    {
                        var payment = new Payment
                        {
                            Id = Guid.NewGuid(),
                            UserId = subscription.UserId,
                            SubscriptionId = subscription.Id,
                            Amount = invoice.AmountPaid / 100m,
                            Currency = invoice.Currency?.ToUpperInvariant() ?? "USD",
                            Status = "Paid",
                            Provider = "Stripe",
                            TransactionId = invoice.Id,
                            StripeInvoiceId = invoice.Id,
                            StripePaymentIntentId = null,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Payments.Add(payment);
                        await _context.SaveChangesAsync();
                    }
                    break;
                }
                case "invoice.payment_failed":
                {
                    var invoice = stripeEvent.Data.Object as Invoice;
                    if (invoice == null) break;
                    var subscriptionId = invoice.Lines?.Data?.FirstOrDefault()?.SubscriptionId;
                    var subscription = await _context.Subscriptions
                        .FirstOrDefaultAsync(s => s.StripeSubscriptionId == subscriptionId);
                    if (subscription != null)
                    {
                        subscription.IsActive = false;
                        subscription.Status = "past_due";
                        await _context.SaveChangesAsync();
                    }
                    break;
                }
                case "customer.subscription.deleted":
                {
                    var stripeSub = stripeEvent.Data.Object as Stripe.Subscription;
                    if (stripeSub == null) break;
                    var existing = await _context.Subscriptions
                        .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSub.Id);
                    if (existing != null)
                    {
                        existing.IsActive = false;
                        existing.Status = "canceled";
                        existing.EndDate = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                    break;
                }
            }

            return Ok();
        }
    }
}
