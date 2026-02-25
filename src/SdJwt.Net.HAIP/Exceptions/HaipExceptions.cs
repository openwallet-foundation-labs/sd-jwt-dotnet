using System.Runtime.Serialization;
using SdJwt.Net.HAIP.Models;

namespace SdJwt.Net.HAIP.Exceptions;

/// <summary>
/// Exception thrown when HAIP compliance validation fails
/// </summary>
[Serializable]
public class HaipComplianceException : Exception {
        /// <summary>
        /// Type of compliance violation that caused the exception
        /// </summary>
        public HaipViolationType ViolationType { get; }

        /// <summary>
        /// Detailed compliance result with all violations
        /// </summary>
        public HaipComplianceResult ComplianceResult { get; }

        /// <summary>
        /// Recommended action to fix the compliance issue
        /// </summary>
        public string RecommendedAction { get; }

        /// <summary>
        /// Initializes a new instance of the HaipComplianceException class
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="violationType">Type of violation</param>
        /// <param name="complianceResult">Detailed compliance result</param>
        /// <param name="recommendedAction">Recommended action to fix the issue</param>
        public HaipComplianceException(
            string message,
            HaipViolationType violationType,
            HaipComplianceResult complianceResult,
            string recommendedAction = "")
            : base(message) {
                ViolationType = violationType;
                ComplianceResult = complianceResult;
                RecommendedAction = recommendedAction;
        }

        /// <summary>
        /// Initializes a new instance of the HaipComplianceException class
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="violationType">Type of violation</param>
        /// <param name="complianceResult">Detailed compliance result</param>
        /// <param name="recommendedAction">Recommended action to fix the issue</param>
        /// <param name="innerException">Inner exception</param>
        public HaipComplianceException(
            string message,
            HaipViolationType violationType,
            HaipComplianceResult complianceResult,
            string recommendedAction,
            Exception innerException)
            : base(message, innerException) {
                ViolationType = violationType;
                ComplianceResult = complianceResult;
                RecommendedAction = recommendedAction;
        }

#if NETSTANDARD2_0 || NETFRAMEWORK
    /// <summary>
    /// Initializes a new instance for serialization
    /// </summary>
    protected HaipComplianceException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
        ViolationType = (HaipViolationType)(info.GetValue(nameof(ViolationType), typeof(HaipViolationType)) ?? HaipViolationType.WeakCryptography);
        ComplianceResult = (HaipComplianceResult)(info.GetValue(nameof(ComplianceResult), typeof(HaipComplianceResult)) ?? new HaipComplianceResult());
        RecommendedAction = info.GetString(nameof(RecommendedAction)) ?? string.Empty;
    }

    /// <summary>
    /// Sets serialization data
    /// </summary>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ViolationType), ViolationType);
        info.AddValue(nameof(ComplianceResult), ComplianceResult);
        info.AddValue(nameof(RecommendedAction), RecommendedAction);
    }
#endif

        /// <summary>
        /// Creates a HAIP compliance exception from a compliance result
        /// </summary>
        /// <param name="result">Compliance result with violations</param>
        /// <param name="context">Additional context for the exception</param>
        /// <returns>New HaipComplianceException</returns>
        public static HaipComplianceException FromResult(HaipComplianceResult result, string? context = null) {
                var criticalViolations = result.Violations.Where(v => v.Severity == HaipSeverity.Critical).ToArray();

                if (!criticalViolations.Any()) {
                        throw new ArgumentException("Compliance result must have critical violations to create an exception", nameof(result));
                }

                var primaryViolation = criticalViolations.First();
                var message = context != null
                    ? $"{context}: {primaryViolation.Description}"
                    : primaryViolation.Description;

                return new HaipComplianceException(
                    message,
                    primaryViolation.Type,
                    result,
                    primaryViolation.RecommendedAction);
        }
}

/// <summary>
/// Exception thrown when HAIP configuration is invalid
/// </summary>
[Serializable]
public class HaipConfigurationException : Exception {
        /// <summary>
        /// Configuration errors found
        /// </summary>
        public string[] ConfigurationErrors { get; }

        /// <summary>
        /// Initializes a new instance of the HaipConfigurationException class
        /// </summary>
        public HaipConfigurationException(string message, params string[] configurationErrors)
            : base(message) {
                ConfigurationErrors = configurationErrors ?? Array.Empty<string>();
        }

        /// <summary>
        /// Initializes a new instance of the HaipConfigurationException class
        /// </summary>
        public HaipConfigurationException(string message, Exception innerException, params string[] configurationErrors)
            : base(message, innerException) {
                ConfigurationErrors = configurationErrors ?? Array.Empty<string>();
        }

#if NETSTANDARD2_0 || NETFRAMEWORK
    /// <summary>
    /// Initializes a new instance for serialization
    /// </summary>
    protected HaipConfigurationException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
        ConfigurationErrors = (string[])(info.GetValue(nameof(ConfigurationErrors), typeof(string[])) ?? Array.Empty<string>());
    }

    /// <summary>
    /// Sets serialization data
    /// </summary>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ConfigurationErrors), ConfigurationErrors);
    }
#endif
}

/// <summary>
/// Exception thrown when trust framework validation fails
/// </summary>
[Serializable]
public class HaipTrustFrameworkException : Exception {
        /// <summary>
        /// Trust framework that failed validation
        /// </summary>
        public string TrustFrameworkId { get; }

        /// <summary>
        /// Entity that failed trust validation
        /// </summary>
        public string EntityId { get; }

        /// <summary>
        /// Initializes a new instance of the HaipTrustFrameworkException class
        /// </summary>
        public HaipTrustFrameworkException(string message, string trustFrameworkId, string entityId)
            : base(message) {
                TrustFrameworkId = trustFrameworkId;
                EntityId = entityId;
        }

        /// <summary>
        /// Initializes a new instance of the HaipTrustFrameworkException class
        /// </summary>
        public HaipTrustFrameworkException(string message, string trustFrameworkId, string entityId, Exception innerException)
            : base(message, innerException) {
                TrustFrameworkId = trustFrameworkId;
                EntityId = entityId;
        }

#if NETSTANDARD2_0 || NETFRAMEWORK
    /// <summary>
    /// Initializes a new instance for serialization
    /// </summary>
    protected HaipTrustFrameworkException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
        TrustFrameworkId = info.GetString(nameof(TrustFrameworkId)) ?? string.Empty;
        EntityId = info.GetString(nameof(EntityId)) ?? string.Empty;
    }

    /// <summary>
    /// Sets serialization data
    /// </summary>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(TrustFrameworkId), TrustFrameworkId);
        info.AddValue(nameof(EntityId), EntityId);
    }
#endif
}