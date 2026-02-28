using System.IdentityModel.Tokens.Jwt;

namespace SdJwt.Net.Samples.Shared;

/// <summary>
/// Sample data and claim sets for tutorials.
/// Provides realistic but fictional data for demonstration purposes.
/// </summary>
public static class SampleData
{
    // =========================================================================
    // PERSON IDENTITIES
    // =========================================================================

    public static class People
    {
        public static JwtPayload Alice => new()
        {
            ["given_name"] = "Alice",
            ["family_name"] = "Johnson",
            ["email"] = "alice.johnson@example.com",
            ["birthdate"] = "1995-03-15",
            ["address"] = new Dictionary<string, object>
            {
                ["street_address"] = "123 Main Street",
                ["locality"] = "San Francisco",
                ["region"] = "CA",
                ["postal_code"] = "94102",
                ["country"] = "US"
            }
        };

        public static JwtPayload Bob => new()
        {
            ["given_name"] = "Bob",
            ["family_name"] = "Smith",
            ["email"] = "bob.smith@example.com",
            ["birthdate"] = "1988-07-22",
            ["address"] = new Dictionary<string, object>
            {
                ["street_address"] = "456 Oak Avenue",
                ["locality"] = "New York",
                ["region"] = "NY",
                ["postal_code"] = "10001",
                ["country"] = "US"
            }
        };
    }

    // =========================================================================
    // EDUCATION CREDENTIALS
    // =========================================================================

    public static class Education
    {
        public static JwtPayload UniversityDegree(string studentName = "Alice Johnson") => new()
        {
            [JwtRegisteredClaimNames.Iss] = "https://university.example.edu",
            [JwtRegisteredClaimNames.Sub] = $"student_{Guid.NewGuid():N}"[..20],
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddYears(100).ToUnixTimeSeconds(),
            ["student_name"] = studentName,
            ["degree"] = "Bachelor of Science",
            ["major"] = "Computer Science",
            ["graduation_date"] = "2024-06-15",
            ["gpa"] = 3.8,
            ["honors"] = "magna cum laude"
        };

        public static JwtPayload ProfessionalCertification => new()
        {
            [JwtRegisteredClaimNames.Iss] = "https://certifications.example.org",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddYears(3).ToUnixTimeSeconds(),
            ["certification_name"] = "Cloud Solutions Architect",
            ["certification_id"] = $"CERT-{Guid.NewGuid():N}"[..16],
            ["issue_date"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            ["expiry_date"] = DateTime.UtcNow.AddYears(3).ToString("yyyy-MM-dd"),
            ["level"] = "Professional",
            ["score"] = 92
        };
    }

    // =========================================================================
    // EMPLOYMENT CREDENTIALS
    // =========================================================================

    public static class Employment
    {
        public static JwtPayload EmploymentVerification(string employeeName = "Alice Johnson") => new()
        {
            [JwtRegisteredClaimNames.Iss] = "https://hr.techcorp.example.com",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddMonths(6).ToUnixTimeSeconds(),
            ["employee_name"] = employeeName,
            ["employee_id"] = $"EMP-{Guid.NewGuid():N}"[..12],
            ["job_title"] = "Senior Software Engineer",
            ["department"] = "Engineering",
            ["start_date"] = "2022-01-15",
            ["employment_status"] = "Active",
            ["salary_band"] = "L5"
        };
    }

    // =========================================================================
    // FINANCIAL CREDENTIALS
    // =========================================================================

    public static class Financial
    {
        public static JwtPayload BankAccountSummary => new()
        {
            [JwtRegisteredClaimNames.Iss] = "https://bank.example.com",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
            ["account_holder"] = "Alice Johnson",
            ["account_type"] = "Checking",
            ["account_status"] = "Good Standing",
            ["balance_range"] = "50000-100000",
            ["account_age_years"] = 5,
            ["credit_score_band"] = "750-799"
        };

        public static JwtPayload SuperannuationStatement => new()
        {
            [JwtRegisteredClaimNames.Iss] = "https://super.example.com.au",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds(),
            ["member_name"] = "Alice Johnson",
            ["member_number"] = $"SUP-{Guid.NewGuid():N}"[..12],
            ["fund_name"] = "Example Super Fund",
            ["balance"] = 185000.00m,
            ["investment_option"] = "Balanced Growth",
            ["contribution_ytd"] = 12500.00m,
            ["insurance_cover"] = new Dictionary<string, object>
            {
                ["death_cover"] = 500000,
                ["tpd_cover"] = 500000,
                ["income_protection"] = true
            }
        };
    }

    // =========================================================================
    // HEALTHCARE CREDENTIALS
    // =========================================================================

    public static class Healthcare
    {
        public static JwtPayload PatientSummary => new()
        {
            [JwtRegisteredClaimNames.Iss] = "https://hospital.example.com",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddDays(90).ToUnixTimeSeconds(),
            ["patient_id"] = $"PAT-{Guid.NewGuid():N}"[..12],
            ["patient_name"] = "Alice Johnson",
            ["date_of_birth"] = "1995-03-15",
            ["blood_type"] = "O+",
            ["allergies"] = new[] { "Penicillin" },
            ["current_medications"] = new[] { "Lisinopril 10mg", "Vitamin D" },
            ["primary_physician"] = "Dr. Sarah Chen",
            ["insurance_verified"] = true
        };

        public static JwtPayload VaccinationRecord => new()
        {
            [JwtRegisteredClaimNames.Iss] = "https://health.gov.example",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddYears(10).ToUnixTimeSeconds(),
            ["patient_name"] = "Alice Johnson",
            ["vaccine_type"] = "COVID-19",
            ["manufacturer"] = "Pfizer-BioNTech",
            ["dose_number"] = 3,
            ["vaccination_date"] = "2024-01-15",
            ["batch_number"] = "EL9261",
            ["administering_facility"] = "City Health Clinic"
        };
    }

    // =========================================================================
    // GOVERNMENT CREDENTIALS
    // =========================================================================

    public static class Government
    {
        public static JwtPayload NationalId => new()
        {
            [JwtRegisteredClaimNames.Iss] = "https://identity.gov.example",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddYears(10).ToUnixTimeSeconds(),
            ["document_type"] = "National ID Card",
            ["document_number"] = $"NID-{Guid.NewGuid():N}"[..16],
            ["given_name"] = "Alice",
            ["family_name"] = "Johnson",
            ["date_of_birth"] = "1995-03-15",
            ["nationality"] = "US",
            ["issue_date"] = DateTime.UtcNow.AddYears(-2).ToString("yyyy-MM-dd"),
            ["expiry_date"] = DateTime.UtcNow.AddYears(8).ToString("yyyy-MM-dd")
        };

        public static JwtPayload DriversLicense => new()
        {
            [JwtRegisteredClaimNames.Iss] = "https://dmv.state.gov.example",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddYears(5).ToUnixTimeSeconds(),
            ["license_number"] = $"DL{new Random().Next(100000, 999999)}",
            ["full_name"] = "Alice Marie Johnson",
            ["date_of_birth"] = "1995-03-15",
            ["address"] = "123 Main Street, San Francisco, CA 94102",
            ["license_class"] = "C",
            ["restrictions"] = "Corrective Lenses",
            ["endorsements"] = Array.Empty<string>(),
            ["issue_date"] = DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd"),
            ["expiry_date"] = DateTime.UtcNow.AddYears(4).ToString("yyyy-MM-dd"),
            ["organ_donor"] = true
        };
    }

    // =========================================================================
    // RETAIL CREDENTIALS
    // =========================================================================

    public static class Retail
    {
        public static JwtPayload PurchaseReceipt => new()
        {
            [JwtRegisteredClaimNames.Iss] = "https://store.example.com",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddDays(90).ToUnixTimeSeconds(),
            ["receipt_id"] = $"RCP-{Guid.NewGuid():N}"[..16],
            ["purchase_date"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            ["store_location"] = "Downtown Store #42",
            ["total_amount"] = 299.99m,
            ["payment_method"] = "Credit Card",
            ["items"] = new[]
            {
                new Dictionary<string, object> { ["sku"] = "SKU-001", ["name"] = "Wireless Headphones", ["price"] = 199.99 },
                new Dictionary<string, object> { ["sku"] = "SKU-002", ["name"] = "USB-C Cable", ["price"] = 29.99 },
                new Dictionary<string, object> { ["sku"] = "SKU-003", ["name"] = "Phone Case", ["price"] = 49.99 }
            },
            ["return_eligible"] = true,
            ["return_deadline"] = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd")
        };
    }

    // =========================================================================
    // TELECOM CREDENTIALS
    // =========================================================================

    public static class Telecom
    {
        public static JwtPayload MobileSubscription => new()
        {
            [JwtRegisteredClaimNames.Iss] = "https://carrier.example.com",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddMonths(1).ToUnixTimeSeconds(),
            ["subscriber_name"] = "Alice Johnson",
            ["phone_number"] = "+1-555-123-4567",
            ["imsi"] = $"310150{new Random().Next(100000000, 999999999)}",
            ["plan_type"] = "Unlimited Premium",
            ["account_status"] = "Active",
            ["contract_end_date"] = DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-dd"),
            ["esim_eligible"] = true,
            ["port_out_authorized"] = false
        };
    }
}
