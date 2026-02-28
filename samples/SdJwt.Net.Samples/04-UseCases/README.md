# Use Case Samples

Production-ready implementation patterns organized by industry vertical.

## Overview

These samples demonstrate complete credential flows for real-world scenarios. Each use case includes:

- Full credential schema design
- Selective disclosure patterns
- Verification requirements
- Security considerations

## Use Cases by Industry

| Industry   | Sample                | Description                             |
| ---------- | --------------------- | --------------------------------------- |
| Education  | UniversityDegree      | Academic credential with GPA protection |
| Finance    | LoanApplication       | Income verification with privacy        |
| Healthcare | PatientConsent        | Medical data sharing consent            |
| Government | CrossBorderIdentity   | Travel document verification            |
| Retail     | FraudResistantReturns | Purchase verification without PII       |
| Telecom    | EsimTransfer          | Device identity portability             |

## Key Patterns Demonstrated

### Minimal Disclosure

Each sample shows how to reveal only necessary claims:

- Education: Degree type without GPA
- Finance: Employment status without salary
- Healthcare: Consent without medical details
- Government: Nationality without address

### Multi-Party Workflows

Several samples involve multiple credential exchanges:

- Finance: Employer credential + Income credential
- Healthcare: Identity + Insurance + Consent
- Government: ID + Travel document + Visa

### Security Levels

Samples demonstrate appropriate HAIP levels:

- Level 1: Retail, Telecom
- Level 2: Finance, Education
- Level 3: Government, Healthcare (where required)

## Running Samples

```bash
# Run all use case samples
dotnet run --project samples/SdJwt.Net.Samples -- usecases

# Run specific industry
dotnet run --project samples/SdJwt.Net.Samples -- usecases education
dotnet run --project samples/SdJwt.Net.Samples -- usecases finance
```

## Related Documentation

- [Financial AI](../../docs/use-cases/financial-ai.md)
- [Cross-Border Government](../../docs/use-cases/crossborder.md)
- [Telecom eSIM](../../docs/use-cases/telco-esim.md)
- [E-Commerce Returns](../../docs/use-cases/retail-ecommerce-returns.md)
- [Automated Compliance](../../docs/use-cases/automated-compliance.md)
- [Incident Response](../../docs/use-cases/incident-response.md)
