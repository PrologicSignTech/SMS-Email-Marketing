# Configuration Guide

## Important: Security Configuration Required

Before deploying to production, you **MUST** configure the following security settings:

### Webhook Secret (REQUIRED for Production)

The `WebhookSettings:Secret` in `appsettings.json` is intentionally left empty for development.

**⚠️ CRITICAL:** Configure this before enabling webhook endpoints in production!

```json
{
  "WebhookSettings": {
    "Secret": "YOUR_SECURE_RANDOM_SECRET_HERE"
  }
}
```

**How to generate a secure secret:**
```bash
openssl rand -base64 32
```

**Best Practices:**
- Never commit secrets to version control
- Use environment variables or Azure Key Vault in production
- Use different secrets for different environments
- Share the same secret with your webhook providers (Twilio, SendGrid, etc.)

### Other Configuration Items

#### JWT Settings
Already configured with a default secret. **Change this for production!**

#### Stripe Settings
Add your actual Stripe keys before accepting payments.

#### Connection Strings
Update with your production database connection string.

## Environment-Specific Configuration

Create `appsettings.Production.json` for production-specific settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_PRODUCTION_DB_CONNECTION_STRING"
  },
  "WebhookSettings": {
    "Secret": "YOUR_PRODUCTION_WEBHOOK_SECRET"
  },
  "JwtSettings": {
    "Secret": "YOUR_PRODUCTION_JWT_SECRET"
  }
}
```

## Security Checklist

Before deploying to production:

- [ ] Configure `WebhookSettings:Secret` with a secure random value
- [ ] Update `JwtSettings:Secret` with a production secret
- [ ] Configure production database connection string
- [ ] Add real Stripe API keys if using payments
- [ ] Review all configuration values
- [ ] Test webhook signature validation
- [ ] Enable HTTPS in production
- [ ] Configure proper CORS policies
- [ ] Set up logging and monitoring

## Configuration Validation

The application will:
- Reject webhook requests with signatures if `WebhookSettings:Secret` is not configured
- Allow webhook requests without signatures during development (but log warnings)
- Require proper configuration before production deployment

## For More Information

See `API_INTEGRATION_DOCUMENTATION.md` for complete API and webhook documentation.
